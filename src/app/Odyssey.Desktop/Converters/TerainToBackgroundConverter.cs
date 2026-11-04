using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Odyssey.Models.Data;
using Odyssey.Settings;
using System;
using System.Dynamic;

namespace Odyssey.Converters
{
    public class TerainToBackgroundConverter: IValueConverter
    {
        private static object? ToBackgroundColor(string prefix)
        {
            if (GlobalSettings.EnableTerrainBackgroundColor && !string.IsNullOrEmpty(prefix) && App.Current != null && App.Current.TryFindResource($"{prefix}TerrainBackgroundColor", out object? result))
            {
                return result;
            }
            return BindingOperations.DoNothing;
        }

        /// <summary>
        /// Returns the background color for the given terrain type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int terrain)
            {
                switch (terrain)
                {
                    case Terrains.UNKNOWN: return ToBackgroundColor("Unknown");
                    case Terrains.OCEAN: return ToBackgroundColor("Ocean");
                    case Terrains.SWAMP: return ToBackgroundColor("Swamp");
                    case Terrains.PLAINS: return ToBackgroundColor("Plains");
                    case Terrains.DESERT: return ToBackgroundColor("Desert");
                    case Terrains.FOREST: return ToBackgroundColor("Forest");
                    case Terrains.HIGHLAND: return ToBackgroundColor("Highland");
                    case Terrains.MOUNTAIN: return ToBackgroundColor("Mountain");
                    case Terrains.GLACIER: return ToBackgroundColor("Glacier");
                    case Terrains.VOLCANO: return ToBackgroundColor("Volcano");
                    case Terrains.VOLCANO_ACTIVE: return ToBackgroundColor("VolcanoActive");
                    case Terrains.PACKICE: return ToBackgroundColor("Packice");
                    case Terrains.ICEBERG: return ToBackgroundColor("Iceberg");
                    case Terrains.ICEFLOE: return ToBackgroundColor("Icefloe");
                    case Terrains.CORRIDOR: return ToBackgroundColor("Corridor");
                    case Terrains.WALL: return ToBackgroundColor("Wall");
                    case Terrains.HALL: return ToBackgroundColor("Hall");
                    case Terrains.FOG: return ToBackgroundColor("Fog");
                    case Terrains.THICKFOG: return ToBackgroundColor("Thickfog");
                    case Terrains.FIREWALL: return ToBackgroundColor("Firewall");
                    default: break;
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
