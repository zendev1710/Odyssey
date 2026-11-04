using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Odyssey.Controls.Ships;

/// <summary>
/// Geometry shared by the pre-rendered ship images and the route highlight rings.
///
/// The Magellan ship images are 80x80 cells containing a 64x64 hexagon with an 8 px margin.
/// The visible ship is not exactly on the hex edge: depending on the coast it is drawn a
/// little closer to the centre. These offsets are measured from the alpha bounding box of the
/// actual image, so they stay correct for every ship type and every zoom level: the values are
/// stored as multiples of the map hex size and multiplied by the current (zoomed) hex size when
/// they are used.
/// </summary>
public static class ShipMarkerMetrics
{
    private const double SourceHexSize = 64.0;
    private const double SourceImageSize = 80.0;
    private const double RingMargin = 1.08;
    private const double MinimumRingRadius = 0.40;
    private const double MaximumRingRadius = 0.60;

    /// <summary>Fallback ring radius, as a fraction of the map hex size.</summary>
    public const double FallbackImageRingRadiusFactor = 0.42;

    /// <summary>Base ring radius used by the vector/symbol ship markers.</summary>
    public const double VectorRingRadiusFactor = 0.34;

    private sealed record ImageMetrics(double X, double Y, double Radius);

    private static readonly ConditionalWeakTable<Bitmap, ImageMetrics> MetricsCache = new();
    private static readonly ImageMetrics Fallback = new(0.0, 0.0, FallbackImageRingRadiusFactor);

    /// <summary>
    /// Returns the visible centre of the ship image for the given coast, in map coordinates.
    /// The returned point scales with the current hex size, so it follows zoom changes.
    /// </summary>
    public static Point GetImageAnchor(Bitmap bitmap, Point center, double hexSize)
    {
        ImageMetrics metrics = MetricsCache.GetValue(bitmap, ComputeMetrics);
        return new Point(center.X + metrics.X * hexSize, center.Y + metrics.Y * hexSize);
    }

    /// <summary>
    /// Returns the radius, in map units, that fully encloses the visible part of the ship image.
    /// The radius scales with the current hex size, so it follows zoom changes.
    /// </summary>
    public static double GetImageRingRadius(Bitmap bitmap, double hexSize)
    {
        ImageMetrics metrics = MetricsCache.GetValue(bitmap, ComputeMetrics);
        return metrics.Radius * hexSize;
    }

    private static ImageMetrics ComputeMetrics(Bitmap bitmap)
    {
        try
        {
            PixelSize size = bitmap.PixelSize;
            if (size.Width <= 0 || size.Height <= 0)
            {
                return Fallback;
            }

            // Copy the image into a known BGRA buffer so the alpha channel can be inspected
            // without depending on the platform decoder's native pixel format.
            using var copy = new WriteableBitmap(
                size,
                bitmap.Dpi,
                PixelFormat.Bgra8888,
                AlphaFormat.Unpremul);

            byte[] pixels;
            int stride;
            using (var framebuffer = copy.Lock())
            {
                bitmap.CopyPixels(framebuffer);
                stride = framebuffer.RowBytes;
                pixels = new byte[stride * size.Height];
                Marshal.Copy(framebuffer.Address, pixels, 0, pixels.Length);
            }

            int minX = size.Width;
            int minY = size.Height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < size.Height; y++)
            {
                int row = y * stride;
                for (int x = 0; x < size.Width; x++)
                {
                    if (pixels[row + x * 4 + 3] <= 16)
                    {
                        continue;
                    }

                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return Fallback;
            }

            double sourceCenterX = (minX + maxX) / 2.0 - SourceImageSize / 2.0;
            double sourceCenterY = (minY + maxY) / 2.0 - SourceImageSize / 2.0;

            // Source-to-screen mapping used by DrawShipImage: x spans the hex width, y its height.
            double offsetX = sourceCenterX * Math.Sqrt(3.0) / SourceHexSize;
            double offsetY = sourceCenterY * 2.0 / SourceHexSize;

            double halfWidth = (maxX - minX + 1) * Math.Sqrt(3.0) / (2.0 * SourceHexSize);
            double halfHeight = (maxY - minY + 1) / SourceHexSize;
            double radius = Math.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight) * RingMargin;

            radius = Math.Clamp(radius, MinimumRingRadius, MaximumRingRadius);
            return new ImageMetrics(offsetX, offsetY, radius);
        }
        catch
        {
            // If a platform cannot copy pixels, keep the previous generic behaviour instead of
            // breaking the whole map render.
            return Fallback;
        }
    }
}
