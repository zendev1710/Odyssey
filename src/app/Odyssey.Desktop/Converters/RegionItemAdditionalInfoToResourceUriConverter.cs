using Avalonia.Data.Converters;
using Odyssey.Models.Data;
using System;

namespace Odyssey.Converters
{
    public class RegionItemAdditionalInfoToResourceUriConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int t)
            {
                return GetTerrainIcon(t);
            }
            if (value is FactionStatus status)
            {
                return status switch
                {
                    // RED bullet for unkown or traitor faction
                    // BLUE bullet for active faction
                    // GREEN bullet for allied faction
                    // GRAY bullet for anonymous faction
                    FactionStatus.ACTIVE => "/Assets/FactionStatus/blue.gif",
                    FactionStatus.ALLIED => "/Assets/FactionStatus/green.gif",
                    FactionStatus.ANONYMOUS => "/Assets/FactionStatus/gray.gif",
                    FactionStatus.TRAITOR => "/Assets/FactionStatus/red.gif",
                    FactionStatus.UNKNOWN => "/Assets/FactionStatus/red.gif",
                    _ => null,
                };
            }
            if (value is string uri)
            {
                return uri;
            }

            return null;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        private static string? GetTerrainIcon(int t)
        {
            string? iconName;
            switch (t)
            {
                case Terrains.OCEAN:
                    iconName = "ocean";
                    break;
                case Terrains.SWAMP:
                    iconName = "sumpf"; // Marais
                    break;
                case Terrains.PLAINS:
                    iconName = "plain";
                    break;
                case Terrains.DESERT:
                    iconName = "desert";
                    break;
                case Terrains.FOREST:
                    iconName = "forest";
                    break;
                case Terrains.HIGHLAND:
                    iconName = "highland";
                    break;
                case Terrains.MOUNTAIN:
                    iconName = "mountain";
                    break;
                case Terrains.GLACIER:
                    iconName = "glacier";
                    break;
                case Terrains.VOLCANO:
                    iconName = "volcano";
                    break;
                case Terrains.VOLCANO_ACTIVE:
                    iconName = "volcano";
                    break;
                case Terrains.ICEBERG:
                    iconName = "iceberg";
                    break;
                case Terrains.CORRIDOR:
                    iconName = "corridor";
                    break;
                case Terrains.WALL:
                    iconName = "wall";
                    break;
                case Terrains.HALL:
                    iconName = "hall";
                    break;
                case Terrains.FOG:
                    iconName = "fog";
                    break;
                case Terrains.THICKFOG:
                    iconName = "thickfog";
                    break;
                case Terrains.FIREWALL:
                    iconName = "firewall";
                    break;
                case Terrains.MAHLSTROM:
                    iconName = "mahlstrom";
                    break;
                // CHECK: wanted icon for the folowing values
                case Terrains.UNKNOWN:
                case Terrains.PACKICE:
                case Terrains.ICEFLOE:
                case Terrains.LAST:
                default: return null;
            }
            return $"/Assets/Terrains/{iconName}.gif";
        }
    }
}
