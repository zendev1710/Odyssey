using System;

namespace Cropper;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: cropper <input.gif> <output.png>");
            return;
        }

        try
        {
            ImageConvert.ConvertGifToPngWithTransparency(args[0], args[1]);
            Console.WriteLine($"Converted and cropped: {args[1]}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}