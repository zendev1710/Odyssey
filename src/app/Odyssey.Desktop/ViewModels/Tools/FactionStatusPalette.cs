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

}
