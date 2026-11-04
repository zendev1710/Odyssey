using System;
using System.Collections.Generic;
using System.Text;

namespace Odyssey.Models.Data
{
    using System;
    using System.Drawing;

    namespace Odyssey.Models.Data
    {
        /// <summary>
        /// Immutable replacement for System.Drawing.Rectangle with similar API.
        /// Mutation-like operations return a new instance.
        /// </summary>
        public readonly record struct Bounds(int X, int Y, int Width, int Height)
        {
            public static readonly Bounds Empty = new(0, 0, 0, 0);

            public int Left => X;
            public int Top => Y;
            public int Right => X + Width;
            public int Bottom => Y + Height;
            public bool IsEmpty => X == 0 && Y == 0 && Width == 0 && Height == 0;

            public Bounds WithOffset(int offsetX, int offsetY) =>
                new(X + offsetX, Y + offsetY, Width, Height);

            public Bounds WithInflate(int horizontalAmount, int verticalAmount)
            {
                int nx = X - horizontalAmount;
                int ny = Y - verticalAmount;
                int nw = Width + 2 * horizontalAmount;
                int nh = Height + 2 * verticalAmount;
                if (nw < 0) nw = 0;
                if (nh < 0) nh = 0;
                return new Bounds(nx, ny, nw, nh);
            }

            public bool Contains(int x, int y) =>
                x >= X && x < X + Width && y >= Y && y < Y + Height;

            public bool Contains(Point p) => Contains(p.X, p.Y);

            public bool IntersectsWith(Bounds rect) =>
                !(rect.X + rect.Width <= X || rect.X >= X + Width ||
                  rect.Y + rect.Height <= Y || rect.Y >= Y + Height);

            public static Bounds Intersect(Bounds a, Bounds b)
            {
                int x1 = Math.Max(a.X, b.X);
                int y1 = Math.Max(a.Y, b.Y);
                int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
                int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);
                if (x2 <= x1 || y2 <= y1) return Empty;
                return new Bounds(x1, y1, x2 - x1, y2 - y1);
            }

            public static Bounds Union(Bounds a, Bounds b)
            {
                if (a.IsEmpty) return b;
                if (b.IsEmpty) return a;
                int x1 = Math.Min(a.X, b.X);
                int y1 = Math.Min(a.Y, b.Y);
                int x2 = Math.Max(a.X + a.Width, b.X + b.Width);
                int y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);
                return new Bounds(x1, y1, x2 - x1, y2 - y1);
            }

            public static Bounds FromLTRB(int left, int top, int right, int bottom) =>
                new(left, top, right - left, bottom - top);

            public void Deconstruct(out int x, out int y, out int width, out int height)
            {
                x = X; y = Y; width = Width; height = Height;
            }

            public override string ToString() =>
                $"{{X={X},Y={Y},Width={Width},Height={Height}}}";

            // convenience conversions to interop with System.Drawing.Rectangle
            public static implicit operator Rectangle(Bounds r) => new Rectangle(r.X, r.Y, r.Width, r.Height);
            public static implicit operator Bounds(Rectangle r) => new Bounds(r.X, r.Y, r.Width, r.Height);
        }
    }
}
