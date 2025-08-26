using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;

namespace Odyssey.Converters
{
    public class BoolToBoldFontWeigthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool t && t)
            {
                return FontWeight.Bold;
            }
            return FontWeight.Normal;
            //return BindingOperations.DoNothing;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
