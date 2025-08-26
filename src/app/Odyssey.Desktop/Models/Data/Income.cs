using System;

namespace Odyssey.Models.Data
{
    public class Income
    {
        public enum Kind
        {
            MISC = 0,
            ENTERTAIN,
            TAXES,
            TRADE,
            TARIFFS,
            THEFT,
            MAGIC,
            MAX
        }

        private static readonly int[] workPerRegion = {
            0,	    // TERRAIN_UNKNOWN,
		    0,	    // TERRAIN_OCEAN,
		    2000,	// TERRAIN_SWAMP,
		    10000,	// TERRAIN_PLAINS,
		    500,	// TERRAIN_DESERT,
		    10000,	// TERRAIN_FOREST,
		    4000,	// TERRAIN_HIGHLAND,
		    1000,	// TERRAIN_MOUNTAIN,
		    100,	// TERRAIN_GLACIER,
		    500,	// TERRAIN_VOLCANO,
		    0,	    // TERRAIN_ICEBERG,
		    0,	    // TERRAIN_FIREWALL,
		    0,	    // TERRAIN_MAHLSTROM,
		    0	    // TERRAIN_LAST
        };

        public static int ComputeSurplus(int terrain, int trees, int saplings, int peasants, int salary)
        {
            int workstations = terrain < workPerRegion.Length ? workPerRegion[terrain] : 0;
            workstations -= 8 * trees;
            workstations -= 4 * saplings;
            if (workstations < 0)
            {
                workstations = 0;
            }
            int surplus = Math.Min(workstations, peasants);
            if (salary != 0)
            {
                surplus = salary * surplus - 10 * peasants;
            }
            return surplus;
        }
    }
}
