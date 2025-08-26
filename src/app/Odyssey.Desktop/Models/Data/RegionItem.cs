using Prism.DryIoc.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.Models.Data
{
    public enum RegionItemCategory
    {
        //UNKNOWN = 0,
        NATURAL_RESOURCE,
        TRADE,
        ACTIVITY,
        POPULATION,
        INCOME
    }

    public enum RegionItemType
    {
        UNKNOWN = 0,
        // Natural resources items
        HORSES,
        TREES,
        SAPLINGS,
        IRON,
        STONES,
        HERBS,
        MALLORN, // trees became mallorn
        MALLORN_SAPLINGS, // young trees became mallorn young trees
        // Rare natural resources items,
        LAEN,
        ADAMANTIUM,
        // Income and cost items
        REVENUE,
        ENTERTAINMENT,
        STEER,
        TRADE_INCOME,
        TRADE_DUTIES,
        THEFT,
        SORCERY,
        LEARNING_COSTS,
        // Trade category items
        TRADE,
        BALM,
        SPICE,
        GEM,
        MYRRH,
        OIL,
        SILK,
        INCENSE,
        // Activity resources
        RECRUITS,
        PEASANTS_WAGES,
        ENTERTAINMENT_MAX,
        SILVER,
        // Population
        PEASANTS,
        PEOPLE,
        FACTION_PEOPLE,
        FACTION_SILVER,
    }
    /*
    public enum RegionItemType
    {
        UNKNOWN = 0,
        // Natural resources
        TREES,
        SAPLINGS,
        IRON = KeyType.LAST + 1,
        STONES = KeyType.LAST + 2,
        HERBS = KeyType.LAST + 3,
        // Rare natural resources,
        LAEN = KeyType.LAST + 4,
        MALLORN = KeyType.LAST + 5, // trees became mallorn
        MALLORN_SAPLINGS = KeyType.LAST + 6, // young trees became mallorn young trees
                                             // Animal natural resources,
        HORSES = KeyType.LAST + 7,
        // Trade
        TRADE = KeyType.LAST + 8,
        BALM = KeyType.LAST + 9,
        SPICE = KeyType.LAST + 10,
        GEM = KeyType.LAST + 11,
        MYRRH = KeyType.LAST + 12,
        OIL = KeyType.LAST + 13,
        SILK = KeyType.LAST + 14,
        INCENSE = KeyType.LAST + 15,
        // Activity resources
        RECRUITS = KeyType.LAST + 16,
        PEASANTS_WAGES = KeyType.LAST + 17,
        ENTERTAINMENT_MAX = KeyType.LAST + 18,
        SILVER = KeyType.LAST + 20,
        // Population
        PEASANTS = KeyType.LAST + 21,
        PEOPLE = KeyType.LAST + 22,
        FACTION_PEOPLE = KeyType.LAST + 23,
        FACTION_SILVER = KeyType.LAST + 24
    }
    */

    public class RegionItem
    {
        public string Name { get; set; }
        public ulong Value { get; set; }
        public RegionItemCategory Category { get; set; }
        public int Skill { get; set; }

        ///////////////////////////////////////
        // Activity items report internal names

        public static readonly string RECRUITS_NAME = "Rekruten";
        public static readonly string ENTERTAINMENT_NAME = "Unterh";
        public static readonly string ENTERTAINMENT_MAX_NAME = "Unterh.max";

        ////////////////////////////////////////////////
        // Natural resources items report internal names

        public static readonly string HORSES_NAME = "Pferde";
        public static readonly string IRON_NAME = "Eisen";
        public static readonly string STONES_NAME = "Steine";
        public static readonly string LAEN_NAME = "Laen";
        public static readonly string MALLORN_NAME = "Mallorn";
        public static readonly string MALLORN_SAPLINGS_NAME = "Mallornschößlinge";

        //////////////////////////////////////////////
        // Luxuries/trade items report internal names

        public static readonly string INCENSE_NAME = "Weihrauch";
        public static readonly string SILK_NAME = "Seide";
        public static readonly string OIL_NAME = "Öl";
        public static readonly string MYRRH_NAME = "Myrrhe";
        public static readonly string GEM_NAME = "Juwel";
        public static readonly string SPICE_NAME = "Gewürz";
        public static readonly string BALM_NAME = "Balsam";

        private static readonly Dictionary<string, RegionItemType> DataAdditionalNames = new()
        {
            { RECRUITS_NAME, RegionItemType.RECRUITS },
            { ENTERTAINMENT_MAX_NAME, RegionItemType.ENTERTAINMENT_MAX },
            // Resources
            { HORSES_NAME, RegionItemType.HORSES },
            { IRON_NAME, RegionItemType.IRON },
            { STONES_NAME, RegionItemType.STONES },
            { LAEN_NAME, RegionItemType.LAEN },
            { MALLORN_NAME, RegionItemType.MALLORN },
            { MALLORN_SAPLINGS_NAME, RegionItemType.MALLORN_SAPLINGS },
            // Luxuries
            { INCENSE_NAME, RegionItemType.INCENSE },
            { SILK_NAME, RegionItemType.SILK },
            { OIL_NAME, RegionItemType.OIL },
            { MYRRH_NAME, RegionItemType.MYRRH },
            { GEM_NAME, RegionItemType.GEM },
            { SPICE_NAME, RegionItemType.SPICE },
            { BALM_NAME, RegionItemType.BALM },
        };

        /// <summary>
        /// RegionInfo struct in original C++ code.
        /// </summary>
        public RegionItem(string name, ulong value, RegionItemCategory category, int skill = 0)
        {
            Debug.WriteLine($"item {name}[{value}] skill[{skill}]");
            Name = name;
            Value = value;
            Category = category;
            Skill = skill;
        }

        public static RegionItemType GetId(string name)
        {
            if (DataAdditionalNames.TryGetValue(name, out var type))
            {
                return type;
            }
            return RegionItemType.UNKNOWN;
        }

        public static RegionItemType ConvertToItemType(KeyType key) { 
            switch (key)
            {
                case KeyType.TREES: return RegionItemType.TREES;
                case KeyType.SAPLINGS: return RegionItemType.SAPLINGS;
                default: break;
            }
            return RegionItemType.UNKNOWN;
        }
    }
}
