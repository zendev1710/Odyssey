using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Odyssey.Models.Tools;
using System;

namespace Odyssey.Converters
{
    public class RegionItemAdditionalInfoToForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            // FIXME: when switching theme, already expanded items are not refreshed with the theme foreground color.
            if (value is BelongsToStatus s)
            {
                string resourceName = "";
                switch (s)
                {
                    case BelongsToStatus.UnitInShip:
                        resourceName = "UnitInShipForegroundColor";
                        break;
                    case BelongsToStatus.UnitInBuilding:
                        resourceName = "UnitInBuildingForegroundColor";
                        break;
                    case BelongsToStatus.ShipInNotActiveFaction:
                        resourceName = "ShipInNotActiveFactionForegroundColor";
                        break;
                    case BelongsToStatus.BuildingInNotActiveFaction:
                        resourceName = "UnitInBuildingForegroundColor";
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(resourceName) && App.Current != null && App.Current.TryFindResource(resourceName, out object? result))
                {
                    return result;
                }
            }
            return BindingOperations.DoNothing;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            // Not needed
            return BindingOperations.DoNothing;
        }
        
    }
}
