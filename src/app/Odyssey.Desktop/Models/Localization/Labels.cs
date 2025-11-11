using Odyssey.Assets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.Models.Localization
{
    public static class Labels
    {
        private const string contextPrefix = "emapdetailspanel";

        ///////////////////  
        // Labels constants. Used as resource translation keys.

        public const string AURA_FROM_AURAMAX = "aura_from_auramax";
        public const string BATTLE = "battle";
        public const string BUILDING = "building";
        public const string BUILDINGS = "buildings";
        public const string CAPACITY_ON_FOOT_INFO = "capacity_on_foot_info";
        public const string CAPACITY_ON_HORSE_INFO = "capacity_on_horse_info";
        public const string CAPACITY_SHIP_INFO = "capacity_ship_info";
        public const string CAPTAIN = "captain";
        public const string COAST = "coast";
        public const string COMBAT_SPELLS = "combat_spells";
        public const string COMBAT_SPELL_PERIOD = "combat_spell_period";
        public const string DIALOG_TITLE_BOOKMARKS_LOAD = "dlg_title_bookmarks_load";
        public const string DIALOG_TITLE_BOOKMARKS_SAVE_AS = "dlg_title_bookmarks_save_as";
        public const string DIRECTION = "direction";
        public const string DIRECTION_ABBREVIATED = "direction_abbreviated";
        public const string DISGUISED = "disguised";
        public const string EFFECTS = "effects";
        public const string FACTION_DISGUISED = "faction_disguised";
        public const string FACTION_DISGUISED_AS = "faction_disguised_as";
        public const string FACTION_DISGUISED_AS_FACTION = "faction_disguised_as_faction";
        public const string FACTION_ALL = "faction_all";
        public const string FAMILIAR_INFO = "familiar_info";
        public const string FAMILIAR_MAGE_INFO = "familiar_mage_info";
        public const string GAME_DATE_INFO = "game_date_info";
        public const string GROUP = "group";
        public const string GUARDING = "guarding";
        public const string GUARDS = "guards";
        public const string HEROES = "heroes";
        public const string IN_SEASON = "in_season";
        public const string INSIDE_INFO = "inside_info";
        public const string ITEMS = "items";
        public const string MALLORN = "Mallorn";
        public const string MALLORN_SAPLINGS = "mallornsaplings";
        public const string MALLORN_TREES = "mallorntrees";
        public const string MESSAGES = "messages";
        public const string OWNER = "owner";
        public const string PERSONS = "persons";
        public const string RACE_INFO = "race_info";
        public const string ROADS = "roads";
        public const string ROAD_INFO = "road_info";
        public const string ROAD_IN_PROGRESS_INFO = "road_in_progress_info";
        public const string SAPLINGS = "saplings";
        public const string SEARCH_EVERYTHING = "search_everything";
        public const string SEARCH_REGIONS = "search_regions";
        public const string SEARCH_UNITS = "search_units";
        public const string SEARCH_BUILDINGS = "search_buildings";
        public const string SEARCH_SHIPS = "search_ships";
        public const string SEARCH_ORDERS = "search_orders";
        public const string SEASON = "season";
        public const string SHIP_DAMAGE = "ship_damage";
        public const string SHIPS = "ships";
        public const string SIZE = "size";
        public const string SKILLS = "skills";
        public const string SPELLS = "spells";
        public const string TOTAL_SAILING_SKILL = "sailing_skill";
        public const string TRAITOR = "traitor";
        public const string TRANSIT = "transit";
        public const string TREES = "trees";
        public const string UNITS = "units";
        public const string UNKNOWN_ID = "unknown_id";
        public const string WEIGHT = "weight";
        public const string WEIGHT_INFO = "weight_info";

        public static string Localize(string name, string[]? arguments = null)
        {
            return Localize(Categories.None, name, arguments);
        }

        public static string Localize(Categories category, string name, string[]? arguments = null)
        {
            if (string.IsNullOrEmpty(name)) { return string.Empty; }
            string? label = string.Empty;
            string localizationKey = CategoryToLocalizationKey(category, name);
            if (!string.IsNullOrEmpty(localizationKey))
            {
                bool translationFailed = false;
                label = Lang.ResourceManager.GetString(localizationKey, Lang.Culture);
                if (string.IsNullOrEmpty(label))
                {
                    Debug.WriteLine($"[NOT FOUND] {label} / {localizationKey}");
                    int index = name.LastIndexOf('_');
                    label = index != -1 ? name.Substring(index + 1) : $"--- {name} ---";
                    translationFailed = true;
                }
                if (arguments?.Length > 0)
                {
                    label = translationFailed ? $"{label} {string.Join(" ", arguments)}" : string.Format(label, arguments);
                }
            }
            return label ?? string.Empty;
        }

        public static string LocalizeCountable(Categories category, string name, bool needSingular, int pluralSuffixLengthg = 1, string[]? arguments = null)
        {
            if (needSingular)
            {
                string localizationKey = CategoryToLocalizationKey(category, name);
                name = localizationKey.IndexOf('_') != -1 ? name.Insert(0, "1_") : name.Substring(0, name.Length - pluralSuffixLengthg);
            }
            return Labels.Localize(category, name, arguments);
        }

        private static string CategoryToLocalizationKey(Categories category, string name)
        {
            string localizationKey = name;
            switch (category)
            {
                case Categories.Node:
                    localizationKey = $"{contextPrefix}_node_{name}";
                    break;
                case Categories.CombatSpell:
                    localizationKey = $"{contextPrefix}_combatspell_{name}";
                    break;
                case Categories.Skill:
                    localizationKey = $"rules_skill_{name}";
                    break;
                case Categories.Item:
                    localizationKey = $"rules_item_{name}";
                    break;
                case Categories.Building:
                    localizationKey = $"rules_building_{name}";
                    break;
                case Categories.Ship:
                    localizationKey = $"rules_ship_{name}";
                    break;
                case Categories.Race:
                    localizationKey = $"rules_race_{name}";
                    break;
                case Categories.Spell:
                    localizationKey = $"rules_spell_{name}";
                    break;
                case Categories.Effect:
                    localizationKey = $"rules_effect_{name}";
                    break;
                case Categories.Status:
                    localizationKey = $"status_{name}";
                    break;
                case Categories.CombatStatus:
                    localizationKey = $"combatstatus_{name}";
                    break;
                default: break;
            }
            return localizationKey;
        }

        /// <summary>
        /// Get the name of the coast for the specified id (original name coastToString).
        /// </summary>
        /// <param name="id">the coast id</param>
        /// <returns>the name of the coast, or unknown</returns>
        public static string GetCoastName(int id)
        {
            return Localize($"{Labels.COAST}_{id}");
        }

        public static string GetDirectionName(int id)
        {
            return Localize($"{Labels.DIRECTION}_{id}");
        }

        public static string GetDirectionAbbreviation(int id)
        {
            return Localize($"{Labels.DIRECTION_ABBREVIATED}_{id}");
        }
    }
}
