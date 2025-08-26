using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Odyssey.Models.Data
{
    public static class Terrains
    {
        // Terrain typess.
        // DO NOT CHANGE these values as they are stored in report files.

        public const int UNKNOWN = 0;
        public const int OCEAN = 1;
        public const int SWAMP = 2;
        public const int PLAINS = 3;
        public const int DESERT = 4;
        public const int FOREST = 5;
        public const int HIGHLAND = 6;
        public const int MOUNTAIN = 7;
        public const int GLACIER = 8;
        public const int VOLCANO = 9;
        public const int VOLCANO_ACTIVE = 10;
        // What are these below terrains exactly?
        public const int PACKICE = 11;
        public const int ICEBERG = 12;
        public const int ICEFLOE = 13;
        public const int CORRIDOR = 14;
        public const int WALL = 15;
        public const int HALL = 16;
        public const int FOG = 17;
        public const int THICKFOG = 18;
        public const int FIREWALL = 19;

        // MAHLSTROM is not public !?
        public const int MAHLSTROM = 20;
        public const int LASTPUBLIC = FIREWALL;
        public const int LAST = 21;

        /// <summary>
        /// Internal names of the terrain types.
        /// German names are used as internal names (to be cautious with special german characters).
        /// They are used in the app and stored in report files as is.
        /// </summary>
        private static readonly string[] INTERNAL_NAMES =
        [
            "Unbekannt",
            "Ozean",
            "Sumpf",
            "Ebene",
            "W\u00fcste", // TODO: better handle special german characters. "Wüste"
            "Wald",
            "Hochland",
            "Berge",
            "Gletscher",
            "Vulkan",
            "Aktiver Vulkan",
            "Packeis",
            "Eisberg",
            "Eisscholle",
            "Gang",
            "Wand",
            "Halle",
            "Nebel",
            "Dichter Nebel",
            "Feuerwand",
            "Mahlstrom"
        ];

        /// <summary>
        /// Translated names of the terrain types.
        /// </summary>
        private static readonly string[] TERRAIN_TRANSLATED_NAMES =
        [
            "Unknown",
            "Ocean",
            "Swamp",
            "Plains", // was "Level" !?
            "Desert",
            "Forest",
            "Highland",
            "Mountain",
            "Glacier",
            "Volcano",
            "Active volcano",
            "Pack ice",
            "Iceberg",
            "Ice floe",
            "Gear", // Corridor ???
            "Wall",
            "Hall",
            "Fog",
            "Dense fog",
            "Wall of fire",
            "Maelstrom"
        ];

        /// <summary>
        /// Returns true if the terrain type can have a region name.
        /// </summary>
        /// <param name="terrainType"></param>
        /// <returns></returns>
        public static bool CanBeNamed(int terrainType)
        {
            // TODO: check if this is correct (can a firewall be optionally named ? Is there some other terrain with no name ?)
            return terrainType != OCEAN && terrainType != FIREWALL;
        }

        /// <summary>
        /// Returns the internal name of the terrain type.
        /// </summary>
        /// <param name="terrainType">type of the terrain.</param>
        /// <returns></returns>
        public static string GetInternalName(int terrainType)
        {
            if (terrainType >= 0 && terrainType <= LAST)
            {
                return INTERNAL_NAMES[terrainType];
            }
            return "Unknown";
        }

        /// <summary>
        /// Returns the translated name of the terrain type.
        /// </summary>
        /// <param name="terrainType">type of the terrain</param>
        /// <param name="translated">Indicates if the returned value must be a translation.</param>
        /// <returns></returns>
        public static string GetLabel(int terrainType, bool translated = true)
        {
            if (terrainType >= 0 && terrainType <= LAST)
            {
                return TERRAIN_TRANSLATED_NAMES[terrainType];
            }
            return "Unknown";
        }

        public static bool CanBeGuarded(int terrainType)
        {
            return terrainType != OCEAN;
        }

        public static bool CanHaveRoad(int terrainType)
        {
            // Rule : a road can be build in Plain, Forest, Highland, Mountain, Volcano/Active Volcano, Swamp, Desert, Glacier
            return terrainType >= SWAMP && terrainType <= VOLCANO_ACTIVE;
        }
    }
}
