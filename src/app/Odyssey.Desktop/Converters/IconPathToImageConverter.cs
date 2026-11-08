using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;

namespace Odyssey.Converters;

public sealed class IconPathToImageConverter : IValueConverter, IMultiValueConverter
{
    private const string SvgPreviewBackground = "#EFEFEF";
    private const string SvgPreviewForeground = "#212121";
    private static readonly ConcurrentDictionary<string, Lazy<IImage>> ImageCache = new(StringComparer.OrdinalIgnoreCase);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return GetCachedImage(path, () => LoadImage(path));
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 3 || values[0] is not string path || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        Uri uri = ToAssetUri(path);
        if (!Path.GetExtension(uri.AbsolutePath).Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return GetCachedImage(path, () => LoadImage(path));
        }

        string backgroundColor = GetSvgColor(values[1], "theme background");
        string foregroundColor = GetSvgColor(values[2], "theme foreground");
        string cacheKey = $"{uri}|{backgroundColor}|{foregroundColor}";
        return GetCachedImage(cacheKey, () => LoadThemedSvg(uri, backgroundColor, foregroundColor));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static IImage LoadImage(string path)
    {
        Uri uri = ToAssetUri(path);
        if (Path.GetExtension(uri.AbsolutePath).Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return new SvgImage { Source = SvgSource.Load(uri.ToString()) };
        }

        using Stream stream = AssetLoader.Open(uri);
        return new Bitmap(stream);
    }

    private static IImage LoadThemedSvg(Uri uri, string backgroundColor, string foregroundColor)
    {
        using Stream stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        string svg = reader.ReadToEnd()
            .Replace(SvgPreviewBackground, backgroundColor, StringComparison.Ordinal)
            .Replace(SvgPreviewForeground, foregroundColor, StringComparison.Ordinal);
        return new SvgImage { Source = SvgSource.LoadFromSvg(svg) };
    }

    private static IImage GetCachedImage(string key, Func<IImage> loadImage) =>
        ImageCache.GetOrAdd(
            key,
            _ => new Lazy<IImage>(loadImage, LazyThreadSafetyMode.ExecutionAndPublication)).Value;

    private static string GetSvgColor(object? value, string colorName)
    {
        if (value is not SolidColorBrush brush)
        {
            throw new InvalidOperationException($"The {colorName} resource must be a solid color brush.");
        }

        Color color = brush.Color;
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static Uri ToAssetUri(string path)
    {
        if (path.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri(path, UriKind.Absolute);
        }

        string normalized = path.Replace('\\', '/');
        if (normalized.StartsWith("/Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri($"avares://Odyssey{normalized}");
        }

        if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri($"avares://Odyssey/{normalized}");
        }

        throw new ArgumentException($"Unsupported icon asset path '{path}'.", nameof(path));
    }
}