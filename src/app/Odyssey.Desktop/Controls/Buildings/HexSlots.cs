using Avalonia;
using System;

namespace Odyssey.Controls.Buildings;

/// <summary>
/// Computes the positions where buildings can be drawn inside a region hexagon.
/// There is one center slot (index 0) and six perimeter slots (indices 1..6) placed along the
/// hexagon's six edges, in the same order as the six neighbour directions of an axial grid.
/// </summary>
public static class HexSlots
{
    /// <summary>Index of the center slot.</summary>
    public const int CenterIndex = 0;

    /// <summary>Number of perimeter slots (one per hexagon edge).</summary>
    public const int PerimeterCount = 6;

    /// <summary>Total number of slots (center + perimeter).</summary>
    public const int SlotCount = PerimeterCount + 1;

    /// <summary>Distance of the perimeter slots from the center, as a fraction of the hex size.</summary>
    public const double PerimeterRadiusFactor = 0.58;

    /// <summary>
    /// Distance of the midpoint of a hexagon edge from the center, as a fraction of the hex size
    /// (cosine of 30 degrees). Use this to place an object exactly on an edge.
    /// </summary>
    public const double EdgeRadiusFactor = 0.8660254037844386;

    /// <summary>
    /// Neighbour axial deltas, in the same order as the perimeter slot indices (slot = index + 1):
    /// E, SE, SW, W, NW, NE.
    /// </summary>
    public static readonly (int Dq, int Dr)[] NeighborDeltas =
    {
        (1, 0),   // E  -> slot 1
        (1, -1),  // SE -> slot 2
        (0, -1),  // SW -> slot 3
        (-1, 0),  // W  -> slot 4
        (-1, 1),  // NW -> slot 5
        (0, 1),   // NE -> slot 6
    };

    // Unit offsets (in screen space, y grows downward) matching the edge midpoint directions.
    private static readonly (double X, double Y)[] PerimeterOffsets =
    {
        (1.0, 0.0),           // E
        (0.5, 0.8660254),     // SE
        (-0.5, 0.8660254),    // SW
        (-1.0, 0.0),          // W
        (-0.5, -0.8660254),   // NW
        (0.5, -0.8660254),    // NE
    };

    /// <summary>Returns the screen position of the given slot inside a hexagon.</summary>
    public static Point GetSlotCenter(int slotIndex, Point center, double hexSize)
    {
        return GetSlotCenter(slotIndex, center, hexSize, PerimeterRadiusFactor);
    }

    /// <summary>
    /// Returns the screen position of the given slot inside a hexagon, at the given distance
    /// (as a fraction of the hex size) from the center. Use <see cref="EdgeRadiusFactor"/> for
    /// positions exactly on the hexagon edges.
    /// </summary>
    public static Point GetSlotCenter(int slotIndex, Point center, double hexSize, double radiusFactor)
    {
        if (slotIndex <= CenterIndex)
        {
            return center;
        }

        var offset = PerimeterOffsets[(slotIndex - 1) % PerimeterCount];
        double radius = hexSize * radiusFactor;
        return new Point(center.X + offset.X * radius, center.Y + offset.Y * radius);
    }

    /// <summary>
    /// Returns the angle (in degrees, screen space) of the direction from the hexagon center
    /// toward the given perimeter slot. 0 is east and angles grow clockwise.
    /// </summary>
    public static double GetSlotAngle(int slotIndex)
    {
        if (slotIndex <= CenterIndex)
        {
            return 0;
        }

        var offset = PerimeterOffsets[(slotIndex - 1) % PerimeterCount];
        return Math.Atan2(offset.Y, offset.X) * 180.0 / Math.PI;
    }

    /// <summary>
    /// Returns a unit vector tangent to the hexagon edge of the given perimeter slot.
    /// This can be used to spread several objects along the same edge.
    /// </summary>
    public static (double X, double Y) GetSlotTangent(int slotIndex)
    {
        if (slotIndex <= CenterIndex)
        {
            return (1.0, 0.0);
        }

        var offset = PerimeterOffsets[(slotIndex - 1) % PerimeterCount];
        return (-offset.Y, offset.X);
    }

    /// <summary>Returns the perimeter slot index (1..6) for a neighbour delta, or -1 when unknown.</summary>
    public static int SlotForNeighborDelta(int dq, int dr)
    {
        for (int i = 0; i < NeighborDeltas.Length; i++)
        {
            if (NeighborDeltas[i].Dq == dq && NeighborDeltas[i].Dr == dr)
            {
                return i + 1;
            }
        }
        return -1;
    }
}
