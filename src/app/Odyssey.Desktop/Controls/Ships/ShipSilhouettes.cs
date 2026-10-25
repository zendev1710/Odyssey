using Avalonia;
using Avalonia.Media;
using System;

namespace Odyssey.Controls.Ships;

/// <summary>
/// Draws the vector ship silhouettes used by the "Symbol" and "Vector" map styles.
/// The shape is built in a unit coordinate system where +X is the bow, then rotated so
/// that the bow points toward the coast (outward from the region center).
/// </summary>
public static class ShipSilhouettes
{
    public static void DrawSymbol(
        DrawingContext context,
        ShipVisual visual,
        Point center,
        double size,
        double angleDegrees,
        bool underConstruction = false)
    {
        Draw(context, visual, center, size * 0.92, angleDegrees, detailed: false, underConstruction);
    }

    public static void DrawVector(
        DrawingContext context,
        ShipVisual visual,
        Point center,
        double size,
        double angleDegrees,
        bool underConstruction = false)
    {
        Draw(context, visual, center, size, angleDegrees, detailed: true, underConstruction);
    }

    private static void Draw(
        DrawingContext context,
        ShipVisual visual,
        Point center,
        double size,
        double angleDegrees,
        bool detailed,
        bool underConstruction)
    {
        if (size <= 1.0)
        {
            return;
        }

        double scale = size * 0.5;
        var transform = Matrix.CreateScale(scale, scale)
            * Matrix.CreateRotation(angleDegrees * Math.PI / 180.0)
            * Matrix.CreateTranslation(center.X, center.Y);

        using (context.PushTransform(transform))
        {
            if (underConstruction)
            {
                using (context.PushOpacity(0.65))
                {
                    DrawShip(context, visual, scale, detailed);
                }

                DrawScaffold(context, scale);
            }
            else
            {
                DrawShip(context, visual, scale, detailed);
            }
        }
    }

    /// <summary>
    /// Draws a light scaffold around the hull so a ship under construction stays identifiable
    /// without relying on a separate image asset.
    /// </summary>
    private static void DrawScaffold(DrawingContext context, double scale)
    {
        var brush = new SolidColorBrush(Color.FromRgb(0xE0, 0xA0, 0x30));
        var pen = new Pen(brush, Math.Max(0.035, 2.4 / scale));

        // Two uprights and two crossbeams around the hull.
        context.DrawLine(pen, new Point(-0.84, 0.30), new Point(-0.84, -0.62));
        context.DrawLine(pen, new Point(0.62, 0.30), new Point(0.62, -0.62));
        context.DrawLine(pen, new Point(-0.88, -0.52), new Point(0.66, -0.52));
        context.DrawLine(pen, new Point(-0.88, 0.14), new Point(0.66, 0.14));
    }

    private static void DrawShip(DrawingContext context, ShipVisual visual, double scale, bool detailed)
    {
        var hullBrush = new SolidColorBrush(visual.HullColor);
        var sailBrush = new SolidColorBrush(visual.SailColor);
        var trimBrush = new SolidColorBrush(visual.TrimColor);
        var outline = new Pen(new SolidColorBrush(Color.FromArgb(220, 24, 20, 16)), Math.Max(0.012, 1.8 / scale));
        var rigging = new Pen(trimBrush, Math.Max(0.025, 1.6 / scale));

        // Hull: deck line at y ~ 0, rounded belly below. +X is the bow.
        var hull = Polygon(
            new Point(-0.95, -0.10),
            new Point(-0.88, 0.16),
            new Point(-0.52, 0.32),
            new Point(0.42, 0.32),
            new Point(0.88, 0.16),
            new Point(1.06, 0.00),
            new Point(0.88, -0.12),
            new Point(-0.55, -0.16));
        context.DrawGeometry(hullBrush, outline, hull);

        // A painted strake along the hull.
        context.DrawLine(rigging, new Point(-0.80, 0.06), new Point(0.82, 0.04));

        if (detailed)
        {
            if (visual.HasSternCastle)
            {
                DrawSternCastle(context, visual, hullBrush, outline);
            }

            if (visual.HasShields)
            {
                DrawShields(context, visual, outline);
            }

            if (visual.HasStableStern())
            {
                // Curled stern, typical of northern ships.
                context.DrawLine(rigging, new Point(-0.92, -0.08), new Point(-1.06, -0.40));
                context.DrawLine(rigging, new Point(-1.06, -0.40), new Point(-0.98, -0.52));
            }
        }

        if (visual.HasOars)
        {
            DrawOars(context, rigging, detailed);
        }

        DrawMasts(context, visual, sailBrush, trimBrush, outline, rigging, detailed);

        if (visual.HasDragonHead)
        {
            DrawDragonHead(context, visual, trimBrush, outline);
        }
        else if (detailed)
        {
            // Bowsprit for the sailing ships.
            context.DrawLine(rigging, new Point(0.96, -0.04), new Point(1.32, -0.32));
        }

        if (detailed)
        {
            // A small dark waterline shadow, to detach the ship from bright terrain.
            context.DrawLine(
                new Pen(new SolidColorBrush(Color.FromArgb(80, 0, 0, 0)), Math.Max(0.04, 3.0 / scale)),
                new Point(-0.55, 0.34),
                new Point(0.50, 0.34));
        }
    }

    private static void DrawMasts(
        DrawingContext context,
        ShipVisual visual,
        IBrush sailBrush,
        IBrush trimBrush,
        IPen outline,
        IPen rigging,
        bool detailed)
    {
        double[] mastXs = visual.Masts switch
        {
            2 => [-0.42, 0.30],
            3 => [-0.55, 0.02, 0.50],
            _ => [0.02],
        };

        double sailHalfWidth = visual.Masts switch
        {
            2 => 0.30,
            3 => 0.25,
            _ => 0.40,
        };

        for (int i = 0; i < mastXs.Length; i++)
        {
            double x = mastXs[i];
            context.DrawLine(rigging, new Point(x, -0.10), new Point(x, -0.96));

            if (visual.SailStyle == SailStyle.Lateen)
            {
                var sail = Polygon(
                    new Point(x, -0.94),
                    new Point(x + sailHalfWidth * 1.55, -0.30),
                    new Point(x, -0.20));
                context.DrawGeometry(sailBrush, outline, sail);

                if (detailed)
                {
                    context.DrawLine(rigging, new Point(x, -0.96), new Point(x + sailHalfWidth * 1.65, -0.28));
                }
            }
            else
            {
                var sail = Polygon(
                    new Point(x - sailHalfWidth * 0.85, -0.92),
                    new Point(x + sailHalfWidth * 0.85, -0.92),
                    new Point(x + sailHalfWidth, -0.24),
                    new Point(x - sailHalfWidth * 0.85, -0.24));
                context.DrawGeometry(sailBrush, outline, sail);

                if (detailed)
                {
                    context.DrawLine(rigging, new Point(x - sailHalfWidth * 0.85, -0.62), new Point(x + sailHalfWidth * 0.92, -0.62));
                    context.DrawLine(rigging, new Point(x - sailHalfWidth * 0.85, -0.42), new Point(x + sailHalfWidth * 0.96, -0.42));
                }
            }

            if (detailed)
            {
                // Pennant at the mast top.
                var pennant = Polygon(
                    new Point(x, -0.96),
                    new Point(x + 0.20, -0.88),
                    new Point(x, -0.82));
                context.DrawGeometry(trimBrush, null, pennant);
            }
        }
    }

    private static void DrawOars(DrawingContext context, IPen rigging, bool detailed)
    {
        int rows = detailed ? 5 : 3;
        for (int i = 0; i < rows; i++)
        {
            double x = -0.62 + i * 0.31;
            context.DrawLine(rigging, new Point(x, 0.28), new Point(x + 0.22, 0.66));
        }
    }

    private static void DrawShields(DrawingContext context, ShipVisual visual, IPen outline)
    {
        var brush = new SolidColorBrush(visual.TrimColor);
        for (int i = 0; i < 5; i++)
        {
            double x = -0.62 + i * 0.30;
            double y = -0.08;
            var shield = new EllipseGeometry(new Rect(x - 0.07, y - 0.10, 0.14, 0.20));
            context.DrawGeometry(brush, outline, shield);
        }
    }

    private static void DrawSternCastle(DrawingContext context, ShipVisual visual, IBrush hullBrush, IPen outline)
    {
        var castle = Polygon(
            new Point(-0.98, -0.12),
            new Point(-0.98, -0.52),
            new Point(-0.62, -0.60),
            new Point(-0.52, -0.12));
        context.DrawGeometry(hullBrush, outline, castle);

        var trim = new SolidColorBrush(visual.TrimColor);
        var window = new RectangleGeometry(new Rect(-0.90, -0.46, 0.10, 0.12));
        var window2 = new RectangleGeometry(new Rect(-0.74, -0.48, 0.10, 0.12));
        context.DrawGeometry(trim, null, window);
        context.DrawGeometry(trim, null, window2);
    }

    private static void DrawDragonHead(DrawingContext context, ShipVisual visual, IBrush trimBrush, IPen outline)
    {
        var head = Polygon(
            new Point(0.98, -0.06),
            new Point(1.16, -0.34),
            new Point(1.32, -0.46),
            new Point(1.44, -0.38),
            new Point(1.34, -0.24),
            new Point(1.18, -0.20),
            new Point(1.08, -0.04));
        context.DrawGeometry(trimBrush, outline, head);

        // Horn and eye.
        context.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(230, 30, 24, 18)), 0.05),
            new Point(1.27, -0.44), new Point(1.36, -0.58));
        context.DrawGeometry(Brushes.Black, null, new EllipseGeometry(new Rect(1.25, -0.38, 0.05, 0.05)));
    }

    private static bool HasStableStern(this ShipVisual visual)
    {
        return visual.Shape is ShipShape.Longboat or ShipShape.Dragonship or ShipShape.Trireme;
    }

    private static Geometry Polygon(params Point[] points)
    {
        var geometry = new StreamGeometry();
        using (var gc = geometry.Open())
        {
            gc.BeginFigure(points[0], true);
            for (int i = 1; i < points.Length; i++)
            {
                gc.LineTo(points[i]);
            }
            gc.EndFigure(true);
        }
        return geometry;
    }
}
