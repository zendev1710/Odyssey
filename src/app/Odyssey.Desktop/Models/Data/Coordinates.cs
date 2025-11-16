using System;

namespace Odyssey.Models.Data
{
    public struct Coordinates(int x, int y, int plane)
    {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public int Plane { get; set; } = plane;

        public override readonly bool Equals(object? obj)
        {
            if (obj is Coordinates other)
            {
                return X == other.X && Y == other.Y && Plane == other.Plane;
            }
            return false;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(X, Y, Plane);
        }

        public static int GetId(int x, int y, PlaneType plane = PlaneType.WORLD)
        {
            return HashCode.Combine(x, y, plane);
        }

        public override readonly string ToString()
        {
            return $"{X}, {Y}, {Plane}";
        }

        public static bool operator ==(Coordinates left, Coordinates right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Coordinates left, Coordinates right)
        {
            return !(left == right);
        }
    }
}
