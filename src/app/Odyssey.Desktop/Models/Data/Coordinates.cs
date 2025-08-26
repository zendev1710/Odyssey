using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using Odyssey.Models.Documents;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        // Define an explicit cast to int
        public static explicit operator int(Coordinates coordinates)
        {
            return HashCode.Combine(coordinates.X, coordinates.Y, coordinates.Plane);
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
