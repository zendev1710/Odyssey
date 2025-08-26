using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;

namespace Odyssey.Converters
{
    public class ItemAmountToString : IValueConverter
    {
        private const string NoItem = "---";

        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int n && n == -1)
            {
                return NoItem;
            }
            if (value is string s && string.IsNullOrEmpty(s))
            {
                return NoItem;
            }
            return value;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
