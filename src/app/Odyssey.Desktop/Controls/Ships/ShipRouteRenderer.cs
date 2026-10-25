using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace Odyssey.Controls.Ships;

/// <summary>Values of the "ship route display" map setting.</summary>
public static class ShipRouteModes
{
    public const int All = 0;
    public const int SelectedShip = 1;
    public const int SelectedRegion = 2;
    public const int None = 3;
}

/// <summary>Relationship between a ship owner and the active faction(s).</summary>
public enum ShipFactionRelation
{
    Unknown,
    Active,
    Allied,
    Enemy,
}

/// <summary>
/// Geometry and drawing helpers for ship routes. The route is built from region centres and
/// shared-edge midpoints, offset into parallel lanes, then smoothed with a centripetal
/// Catmull-Rom spline so it reads as a travelled path rather than a chain of straight segments.
/// </summary>
public static class ShipRouteRenderer
{
    private const double Epsilon = 0.0001;

    /// <summary>
    /// Base faction colours. Green is kept for the player's own routes, violet for allies and
    /// red/orange for enemies, so the colour code stays meaningful on the map.
    /// </summary>
    public static Color GetRelationColor(ShipFactionRelation relation)
    {
        return relation switch
        {
            ShipFactionRelation.Active => Color.FromRgb(0x3F, 0xD0, 0x58),
            ShipFactionRelation.Allied => Color.FromRgb(0xA7, 0x8B, 0xFA),
            ShipFactionRelation.Enemy => Color.FromRgb(0xFF, 0x6B, 0x35),
            _ => Color.FromRgb(0x9C, 0xA3, 0xAF),
        };
    }

    /// <summary>
    /// Brightens the faction colour for the selected route. Selection is therefore shown by a
    /// lighter shade plus a higher opacity, without introducing a colour unrelated to the faction.
    /// </summary>
    public static Color GetSelectedRouteColor(Color baseColor)
    {
        const double whiteAmount = 0.28;
        return Color.FromRgb(
            (byte)(baseColor.R + (255 - baseColor.R) * whiteAmount),
            (byte)(baseColor.G + (255 - baseColor.G) * whiteAmount),
            (byte)(baseColor.B + (255 - baseColor.B) * whiteAmount));
    }

    /// <summary>
    /// Builds the path anchors: centre of each region, then midpoint of the shared edge, then
    /// next centre, etc. Each anchor is shifted perpendicular to its segment according to the
    /// lane assigned to that segment, so overlapping routes stay distinguishable.
    /// </summary>
    public static List<Point> BuildParallelAnchors(
        IReadOnlyList<Point> centers,
        IReadOnlyList<double> segmentLanes,
        IReadOnlyList<bool> reversedSegments,
        double laneSpacing)
    {
        var anchors = new List<Point>();
        if (centers.Count == 0)
        {
            return anchors;
        }

        if (centers.Count == 1)
        {
            anchors.Add(centers[0]);
            return anchors;
        }

        int segmentCount = centers.Count - 1;
        var segmentOffsets = new Vector[segmentCount];
        var vertexOffsets = new Vector[centers.Count];
        var vertexOffsetCounts = new int[centers.Count];

        for (int i = 0; i < segmentCount; i++)
        {
            Point a = centers[i];
            Point b = centers[i + 1];
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length < Epsilon)
            {
                continue;
            }

            var normal = new Vector(-dy / length, dx / length);
            if (i < reversedSegments.Count && reversedSegments[i])
            {
                normal = -normal;
            }

            double lane = i < segmentLanes.Count ? segmentLanes[i] : 0.0;
            Vector offset = normal * (lane * laneSpacing);
            segmentOffsets[i] = offset;

            vertexOffsets[i] += offset;
            vertexOffsetCounts[i]++;
            vertexOffsets[i + 1] += offset;
            vertexOffsetCounts[i + 1]++;
        }

        for (int i = 0; i < vertexOffsets.Length; i++)
        {
            if (vertexOffsetCounts[i] > 0)
            {
                vertexOffsets[i] /= vertexOffsetCounts[i];
            }
        }

        anchors.Add(Offset(centers[0], vertexOffsets[0]));
        for (int i = 0; i < segmentCount; i++)
        {
            var midpoint = new Point(
                (centers[i].X + centers[i + 1].X) / 2.0,
                (centers[i].Y + centers[i + 1].Y) / 2.0);

            anchors.Add(Offset(midpoint, segmentOffsets[i]));
            anchors.Add(Offset(centers[i + 1], vertexOffsets[i + 1]));
        }

        return anchors;
    }

    /// <summary>Converts the anchor points into a smooth stream geometry (centripetal Catmull-Rom).</summary>
    public static StreamGeometry BuildSmoothGeometry(IReadOnlyList<Point> points)
    {
        var geometry = new StreamGeometry();
        using StreamGeometryContext context = geometry.Open();

        context.BeginFigure(points[0], false);

        if (points.Count == 1)
        {
            context.LineTo(points[0]);
        }
        else if (points.Count == 2)
        {
            context.LineTo(points[1]);
        }
        else
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                Point p0 = points[Math.Max(i - 1, 0)];
                Point p1 = points[i];
                Point p2 = points[i + 1];
                Point p3 = points[Math.Min(i + 2, points.Count - 1)];
                (Point c1, Point c2) = GetBezierControls(p0, p1, p2, p3);
                context.CubicBezierTo(c1, c2, p2);
            }
        }

        context.EndFigure(false);
        return geometry;
    }

    /// <summary>Draws the route with a dark keyline and, for the selected ship, an outer glow.</summary>
    public static void DrawRoute(
        DrawingContext context,
        StreamGeometry geometry,
        Color color,
        double thickness,
        bool selected)
    {
        if (selected)
        {
            var glow = new SolidColorBrush(Color.FromArgb(65, color.R, color.G, color.B));
            context.DrawGeometry(null, new Pen(glow, thickness + 6.0), geometry);
        }

        var keyline = new SolidColorBrush(Color.FromArgb(190, 8, 12, 16));
        context.DrawGeometry(null, new Pen(keyline, thickness + 2.6), geometry);

        var brush = new SolidColorBrush(color);
        context.DrawGeometry(null, new Pen(brush, thickness), geometry);
    }

    /// <summary>
    /// Returns direction arrows evenly spaced along the smoothed path. <paramref name="stride"/>
    /// is expressed in anchor segments; because anchors alternate region centres and shared-edge
    /// midpoints, a stride of 4 places one arrow roughly every two regions.
    /// </summary>
    public static List<(Point Point, Vector Tangent)> GetDirectionArrows(
        IReadOnlyList<Point> points,
        int stride = 4)
    {
        var arrows = new List<(Point Point, Vector Tangent)>();
        if (points.Count < 2)
        {
            return arrows;
        }

        int segmentCount = points.Count - 1;
        stride = Math.Max(1, stride);

        // Keep the first and last half-region free so arrows do not collide with the departure
        // dot or the destination ring.
        int first = segmentCount > 2 ? 1 : 0;
        int last = segmentCount > 2 ? segmentCount - 2 : segmentCount - 1;

        for (int segment = first; segment <= last; segment += stride)
        {
            if (TryGetDirectionArrow(points, segment, out var arrow))
            {
                arrows.Add(arrow);
            }
        }

        if (arrows.Count == 0 && TryGetDirectionArrow(points, segmentCount / 2, out var middle))
        {
            arrows.Add(middle);
        }

        return arrows;
    }

    /// <summary>
    /// Shortens the end of a route so it stops on the circumference of the destination ring.
    /// Anchors inside the circle are discarded and replaced with the first crossing point of the
    /// last segment that enters the circle.
    /// </summary>
    public static void TrimEndToCircle(List<Point> points, Point center, double radius)
    {
        if (points.Count < 2 || radius <= 0)
        {
            return;
        }

        int lastOutside = -1;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            if (Distance(points[i], center) > radius + Epsilon)
            {
                lastOutside = i;
                break;
            }
        }

        // The whole route is already inside the ring, or it never reaches it.
        if (lastOutside < 0 || lastOutside >= points.Count - 1)
        {
            return;
        }

        Point outside = points[lastOutside];
        Point inside = points[lastOutside + 1];
        if (!TryFindCircleIntersection(outside, inside, center, radius, out Point intersection))
        {
            return;
        }

        points.RemoveRange(lastOutside + 1, points.Count - lastOutside - 1);
        points.Add(intersection);
    }

    private static bool TryGetDirectionArrow(
        IReadOnlyList<Point> points,
        int segmentIndex,
        out (Point Point, Vector Tangent) arrow)
    {
        arrow = default;
        if (points.Count < 2)
        {
            return false;
        }

        segmentIndex = Math.Clamp(segmentIndex, 0, points.Count - 2);
        Point p0 = points[Math.Max(segmentIndex - 1, 0)];
        Point p1 = points[segmentIndex];
        Point p2 = points[segmentIndex + 1];
        Point p3 = points[Math.Min(segmentIndex + 2, points.Count - 1)];
        (Point c1, Point c2) = GetBezierControls(p0, p1, p2, p3);

        const double t = 0.5;
        Point point = EvaluateBezier(p1, c1, c2, p2, t);
        Vector tangent = EvaluateBezierTangent(p1, c1, c2, p2, t);
        if (tangent.Length < Epsilon)
        {
            tangent = new Vector(p2.X - p1.X, p2.Y - p1.Y);
        }

        if (tangent.Length < Epsilon)
        {
            return false;
        }

        arrow = (point, tangent / tangent.Length);
        return true;
    }

    private static bool TryFindCircleIntersection(
        Point from,
        Point to,
        Point center,
        double radius,
        out Point intersection)
    {
        intersection = default;

        double dx = to.X - from.X;
        double dy = to.Y - from.Y;
        double fx = from.X - center.X;
        double fy = from.Y - center.Y;

        double a = dx * dx + dy * dy;
        if (a < Epsilon)
        {
            return false;
        }

        double b = 2.0 * (fx * dx + fy * dy);
        double c = fx * fx + fy * fy - radius * radius;
        double discriminant = b * b - 4.0 * a * c;
        if (discriminant < 0)
        {
            return false;
        }

        double root = Math.Sqrt(discriminant);
        double t1 = (-b - root) / (2.0 * a);
        double t2 = (-b + root) / (2.0 * a);

        // Prefer the first crossing in the direction of travel.
        double t = t1 is >= 0 and <= 1 ? t1 : t2;
        if (t is < 0 or > 1)
        {
            return false;
        }

        intersection = new Point(from.X + dx * t, from.Y + dy * t);
        return true;
    }

    private static Point Offset(Point point, Vector offset)
    {
        return new Point(point.X + offset.X, point.Y + offset.Y);
    }

    private static (Point C1, Point C2) GetBezierControls(Point p0, Point p1, Point p2, Point p3)
    {
        double d01 = Distance(p0, p1);
        double d12 = Distance(p1, p2);
        double d23 = Distance(p2, p3);

        double t0 = 0.0;
        double t1 = t0 + Math.Sqrt(d01);
        double t2 = t1 + Math.Sqrt(d12);
        double t3 = t2 + Math.Sqrt(d23);

        // Degenerate segment: fall back to a straight line between the two anchors.
        if (t2 - t1 < Epsilon)
        {
            return (p1, p2);
        }

        // Endpoint duplicates give a zero interval; shrink it so the tangent remains defined.
        if (t1 - t0 < Epsilon)
        {
            t0 = t1 - Epsilon;
        }

        if (t3 - t2 < Epsilon)
        {
            t3 = t2 + Epsilon;
        }

        Vector m1 = new Vector(p2.X - p0.X, p2.Y - p0.Y) * (1.0 / (t2 - t0));
        Vector m2 = new Vector(p3.X - p1.X, p3.Y - p1.Y) * (1.0 / (t3 - t1));

        double dt = (t2 - t1) / 3.0;
        Point c1 = new Point(p1.X + m1.X * dt, p1.Y + m1.Y * dt);
        Point c2 = new Point(p2.X - m2.X * dt, p2.Y - m2.Y * dt);
        return (c1, c2);
    }

    private static Point EvaluateBezier(Point p0, Point p1, Point p2, Point p3, double t)
    {
        double u = 1.0 - t;
        double a = u * u * u;
        double b = 3.0 * u * u * t;
        double c = 3.0 * u * t * t;
        double d = t * t * t;

        return new Point(
            a * p0.X + b * p1.X + c * p2.X + d * p3.X,
            a * p0.Y + b * p1.Y + c * p2.Y + d * p3.Y);
    }

    private static Vector EvaluateBezierTangent(Point p0, Point p1, Point p2, Point p3, double t)
    {
        double u = 1.0 - t;
        double a = 3.0 * u * u;
        double b = 6.0 * u * t;
        double c = 3.0 * t * t;

        return new Vector(
            a * (p1.X - p0.X) + b * (p2.X - p1.X) + c * (p3.X - p2.X),
            a * (p1.Y - p0.Y) + b * (p2.Y - p1.Y) + c * (p3.Y - p2.Y));
    }

    private static double Distance(Point a, Point b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
