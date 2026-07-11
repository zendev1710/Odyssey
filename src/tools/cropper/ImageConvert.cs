using System.Drawing;
using System.Drawing.Imaging;

public static class ImageConvert
{
    /// <summary>
    /// Converts a GIF image to a PNG, crops to 64x64 from (8,8), and makes black transparent.
    /// </summary>
    public static void ConvertGifToPngWithTransparency(string inputGifPath, string outputPngPath)
    {
        using var src = new Bitmap(inputGifPath);

        // Crop 64x64 from (8,8)
        Rectangle cropRect = new Rectangle(8, 8, 64, 64);
        using var cropped = src.Clone(cropRect, PixelFormat.Format32bppArgb);

        // Make black transparent
        Color black = Color.FromArgb(0, 0, 0);
        for (int y = 0; y < cropped.Height; y++)
        {
            for (int x = 0; x < cropped.Width; x++)
            {
                Color pixel = cropped.GetPixel(x, y);
                if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
                {
                    cropped.SetPixel(x, y, Color.FromArgb(0, pixel.R, pixel.G, pixel.B));
                }
            }
        }

        cropped.Save(outputPngPath, ImageFormat.Png);
    }
}