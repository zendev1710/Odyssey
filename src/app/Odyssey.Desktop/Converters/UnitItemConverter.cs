using Avalonia.Data.Converters;
using Odyssey.Models.Tools;
using System;

namespace Odyssey.Converters
{
    public class UnitItemConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            string pathName = string.Empty;
            if (value is DataProperty data)
            {
                string name = data.Name.ToLower().
                    Replace(' ', '_').
                    Replace("ä", "ae").
                    Replace("ö", "oe").
                    Replace("ü", "ue").
                    Replace("ß", "ss");
                switch (data.Category)
                {
                    case DataProperty.Categories.Skill:
                        pathName = $"skills/de/{name}";
                        break;
                    case DataProperty.Categories.Item:
                        pathName = $"items/de/{name}";
                        break;
                    case DataProperty.Categories.Effect:
                        pathName = $"effects/de/{name}";
                        break;
                    case DataProperty.Categories.Status:
                    case DataProperty.Categories.CombatStatus:
                    case DataProperty.Categories.CombatSpell:
                    case DataProperty.Categories.Spell:
                    case DataProperty.Categories.Building:
                    case DataProperty.Categories.Ship:
                        break;
                }

            }
            if (!string.IsNullOrEmpty(pathName))
            {
                return $"/Assets/{pathName}.gif";
            }
            return null;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
