using Avalonia.Media;
using Odyssey.Models.Data;

namespace Odyssey.ViewModels.Tools;

internal static class FactionStatusPalette
{
    public static IBrush GetBorderBrush(FactionStatus status)
    {
        string color = status switch
        {
            FactionStatus.ACTIVE => "#3B82F6",
            FactionStatus.ALLIED => "#22C55E",
            FactionStatus.ANONYMOUS => "#9CA3AF",
            _ => "#EF4444",
        };
        return new SolidColorBrush(Color.Parse(color));
    }

    public static IBrush GetBackgroundBrush(FactionStatus status)
    {
        string color = status switch
        {
            FactionStatus.ACTIVE => "#DCEBFF",
            FactionStatus.ALLIED => "#DDF4E3",
            FactionStatus.ANONYMOUS => "#E5E7EB",
            _ => "#FCE0E0",
        };
        return new SolidColorBrush(Color.Parse(color));
    }

    public static IBrush GetSelectedBackgroundBrush(FactionStatus status)
    {
        string color = status switch
        {
            FactionStatus.ACTIVE => "#1D4ED8",
            FactionStatus.ALLIED => "#15803D",
            FactionStatus.ANONYMOUS => "#4B5563",
            _ => "#B91C1C",
        };
        return new SolidColorBrush(Color.Parse(color));
    }

    public static IBrush Darken(IBrush brush)
    {
        var baseColor = ((SolidColorBrush)brush).Color;
        return new SolidColorBrush(Color.FromArgb(
            baseColor.A,
            (byte)(baseColor.R / 2),
            (byte)(baseColor.G / 2),
            (byte)(baseColor.B / 2)));
    }
}
