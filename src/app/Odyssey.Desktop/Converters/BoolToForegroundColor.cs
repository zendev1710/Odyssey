using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;

namespace Odyssey.Converters
{
    public class BoolToForegroundColor : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            // LATER: solve bugs
            // - color does not change when switching theme mode
            // - when selecting region with mallorn in light mode, switching to dark mode, then selecting another region => forecolor is da rk not white !?
            // Specific foreground color for rare item (laen, mallorn)
            // Orange SolidColourBrush (something like "#FF9800")
            // TextForegroundColor or TextControlForeground ?
            string resourceName = (value is bool t && t) ? "RegionRareItemForegroundColor" : "TextControlForeground";
            if (!string.IsNullOrEmpty(resourceName) && App.Current != null)
            {
                if (App.Current.TryFindResource(resourceName, out object? result))
                {
                    return result;
                }
            }
            return BindingOperations.DoNothing;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
