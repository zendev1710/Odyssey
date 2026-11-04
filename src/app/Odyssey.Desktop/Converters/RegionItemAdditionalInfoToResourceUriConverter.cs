using Avalonia.Data.Converters;
using Odyssey.Icons;
using Odyssey.Models.Data;
using Odyssey.Services;
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
                    FactionStatus.ACTIVE => "/Assets/factionstatus/blue.gif",
                    FactionStatus.ALLIED => "/Assets/factionstatus/green.gif",
                    FactionStatus.ANONYMOUS => "/Assets/factionstatus/gray.gif",
                    FactionStatus.TRAITOR => "/Assets/factionstatus/red.gif",
                    FactionStatus.UNKNOWN => "/Assets/factionstatus/red.gif",
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
                    iconName = "swamp";
                    break;
                case Terrains.PLAINS:
                    iconName = "plains";
                    break;
                case Terrains.DESERT:
                    iconName = "desert";
                    break;
                case Terrains.FOREST:
                    iconName = "forest";
                    break;
                case Terrains.HIGHLAND:
                    iconName = "highlands";
                    break;
                case Terrains.MOUNTAIN:
                    iconName = "mountains";
                    break;
                case Terrains.GLACIER:
                    iconName = "glacier";
                    break;
                case Terrains.VOLCANO:
                    iconName = "volcano";
                    break;
                case Terrains.VOLCANO_ACTIVE:
                    iconName = "active_volcano";
                    break;
                case Terrains.ICEBERG:
                    iconName = "iceberg";
                    break;
                case Terrains.CORRIDOR:
                    iconName = "passage";
                    break;
                case Terrains.WALL:
                    iconName = "wall_cliff";
                    break;
                case Terrains.HALL:
                    iconName = "hall";
                    break;
                case Terrains.FOG:
                    iconName = "fog";
                    break;
                case Terrains.THICKFOG:
                    iconName = "dense_fog";
                    break;
                case Terrains.FIREWALL:
                    iconName = "firewall";
                    break;
                case Terrains.MAHLSTROM:
                    iconName = "maelstrom";
                    break;
                case Terrains.PACKICE:
                    iconName = "pack_ice";
                    break;
                case Terrains.ICEFLOE:
                    iconName = "ice_floe";
                    break;
                case Terrains.UNKNOWN:
                case Terrains.LAST:
                default:
                    iconName = "undefined";
                    break;
            }
            return IconCatalogService.ResolvePath(IconCategories.Terrain, iconName, "undefined");
        }
    }
}
