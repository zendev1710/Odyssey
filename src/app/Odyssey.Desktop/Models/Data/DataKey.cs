using Odyssey.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.Models.Data
{
    public enum KeyType
    {
        UNKNOWN,
        EMPTY,
        NAME,
        LOWERCASE_NAME,
        DESCRIPTION,
        NOTES,
        TERRAIN,
        ISLAND,
        ID,
        FACTION,
        OTHER_FACTION,
        TRAITOR,
        FACTIONNAME,
        FACTIONSTEALTH,
        NUMBER,
        BUILDING,
        SHIP,
        WEIGHT,
        GROUP,
        PREFIX,
        STATUS,
        FAMILIARMAGE,
        GUARDING,
        HUNGER,
        HERB,
        HERO,
        HITPOINTS,
        TYPE,
        SIZE,
        RESOURCE_TYPE,
        RESOURCE_SKILL,
        RESOURCE_COUNT,
        KONFIGURATION,
        CHARSET,
        VISIBILITY,
        TURN,
        SILVER,
        RECRUITMENTCOST,
        AURA,
        AURAMAX,
        OPTIONS,
        EMAIL,
        BANNER,
        LOCALE,
        OWNER,
        ORDERS_CONFIRMED,      // ejcOrdersConfirmed, special tag
        DAMAGE,
        CAPTAIN,
        SPEED,
        COAST,
        CAPACITY,
        CARGO,
        PEASANTS,
        TREES,
        SAPLINGS,
        SALARY,
        LOAD,
        MAXLOAD,
        MSG_TYPE,
        MSG_COST,
        MSG_AMOUNT,
        MSG_MODE,
        MSG_REGION,
        LAST,
    }

    public class DataKey
    {
        public const int MASK = (1 << 7) - 1;

        public const int INTEGER = 1 << 7;

        public static readonly string KEYNAME_LEVEL = "level";
        public static readonly string KEYNAME_NAME = "name";

        /////////////////////////////////////
        // Data report internal names as keys

        public static readonly string SILVER_NAME = "Silber";
        public static readonly string SKILL_SAILING_NAME = "Segeln";
        public static readonly string TREES_NAME = "Bauern";
        public static readonly string HORSES_NAME = RegionItem.HORSES_NAME;

        private static readonly Dictionary<KeyType, string> Values = new()
        {
            { KeyType.EMPTY, "" },
            { KeyType.NAME, "Name" },
            { KeyType.LOWERCASE_NAME, "name" },
            { KeyType.NOTES, "privat" },
            { KeyType.DESCRIPTION, "Beschr" },
            //{ KeyType.DESCRIPTION, "Description" },
            { KeyType.HERB, "herb" },
            //{ KeyType.SAPLINGS, "Schößlinge" },
            { KeyType.SAPLINGS, "Schoesslinge" },
            { KeyType.SALARY, "Lohn" },
            //{ KeyType.TREES, "Bäume" },
            { KeyType.TREES, "Baeume" },
            { KeyType.PEASANTS, "Bauern" },
            { KeyType.TERRAIN, "Terrain" },
            { KeyType.ISLAND, "Insel" },
            { KeyType.ID, "id" },
            { KeyType.FACTION, Strings.DE_UNIT_FACTION_ID },
            { KeyType.TRAITOR, "Verraeter" },
            { KeyType.OTHER_FACTION, "Anderepartei" },
            { KeyType.FACTIONNAME, "Parteiname" },
            { KeyType.FACTIONSTEALTH, "Parteitarnung" },
            { KeyType.NUMBER, "Anzahl" },
            { KeyType.BUILDING, "Burg" },
            { KeyType.SHIP, "Schiff" },
            { KeyType.TYPE, "Typ" },
            { KeyType.SIZE, "Groesse" },
            { KeyType.RESOURCE_TYPE, "type" },
            { KeyType.RESOURCE_SKILL, "skill" },
            { KeyType.RESOURCE_COUNT, "number" },
            { KeyType.KONFIGURATION, "Konfiguration" },
            { KeyType.WEIGHT, "weight" },
            { KeyType.GROUP, "gruppe" },
            { KeyType.PREFIX, "typprefix" },
            { KeyType.HITPOINTS, "hp" },
            { KeyType.STATUS, "Kampfstatus" },
            { KeyType.FAMILIARMAGE, "familiarmage" },
            { KeyType.GUARDING, Strings.DE_GUARDING },
            { KeyType.HUNGER, "hunger" },
            { KeyType.HERO, "hero" },
            { KeyType.CHARSET, "charset" },
            { KeyType.VISIBILITY, "visibility" },
            { KeyType.TURN, "Runde" },
            { KeyType.SILVER, "Silber" },
            { KeyType.RECRUITMENTCOST, "Rekrutierungskosten" },
            { KeyType.AURA, "Aura" },
            { KeyType.AURAMAX, "Auramax" },
            { KeyType.OPTIONS, "Optionen" },
            //{ KeyType.OPTIONS, "Options" },
            { KeyType.EMAIL, "email" },
            { KeyType.BANNER, "banner" },
            { KeyType.LOCALE, "locale" },
            { KeyType.OWNER, "Besitzer" },
            { KeyType.ORDERS_CONFIRMED, "ejcOrdersConfirmed" },
            { KeyType.DAMAGE, "Schaden" },
            { KeyType.CAPTAIN, "Kapitaen" },
            { KeyType.SPEED, "speed" },
            { KeyType.COAST, "Kueste" },
            { KeyType.CAPACITY, "capacity" },
            { KeyType.CARGO, "cargo" },
            { KeyType.LOAD, "Ladung" },
            { KeyType.MAXLOAD, "MaxLadung" },
            { KeyType.MSG_TYPE, "type" },
            { KeyType.MSG_REGION, "region" },
            { KeyType.MSG_AMOUNT, "amount" },
            { KeyType.MSG_COST, "cost" },
            { KeyType.MSG_MODE, "mode" }
        };

        public DataKey()
        {
            KeyType = KeyType.UNKNOWN;
            Key = string.Empty;
            Value = string.Empty;
        }

        public DataKey(string key, string value)
        {
            Key = key;
            Value = value;
            KeyType = KeyType.UNKNOWN;
        }

        public DataKey(KeyType type, string value)
        {
            KeyType = type;
            Value = value;
            Key = string.Empty;
        }

        public DataKey(string type, BlockType btype, string value)
        {
            Set(type, btype);
            Value = value;
        }

        public KeyType GetKeyType()
        {
            // Extract the lower 7 bits
            return (KeyType)((int)KeyType & MASK);
        }

        public static KeyType ParseType(string type, BlockType btype)
        {
            return (type, btype) switch
            {
                // FIXME: "amount" is not always an integer (e.g. "sehr viele";amount => sehr viele means "very very much")
                ("amount", BlockType.MESSAGE) => (KeyType)((int)KeyType.MSG_AMOUNT | INTEGER),
                ("Anderepartei", _) => KeyType.OTHER_FACTION,
                ("Anzahl", _) => KeyType.NUMBER,
                ("Aura", _) => KeyType.AURA,
                ("Auramax", _) => KeyType.AURAMAX,
                ("Baeume", _) or ("Bäume", _) => KeyType.TREES,
                ("banner", _) => KeyType.BANNER,
                ("Bauern", _) => KeyType.PEASANTS,
                ("Beschr", _) => KeyType.DESCRIPTION,
                (Strings.DE_GUARDING, _) => KeyType.GUARDING,
                ("Burg", _) => KeyType.BUILDING,
                ("cargo", _) => KeyType.CARGO,
                ("capacity", _) => KeyType.CAPACITY,
                ("charset", _) => KeyType.CHARSET,
                ("cost", BlockType.MESSAGE) => (KeyType)((int)KeyType.MSG_COST | INTEGER),
                ("email", _) => KeyType.EMAIL,
                ("ejcOrdersConfirmed", _) => KeyType.ORDERS_CONFIRMED,
                ("familiarmage", _) => KeyType.FAMILIARMAGE,
                ("Groesse", _) => KeyType.SIZE,
                ("gruppe", _) => KeyType.GROUP,
                ("herb", _) => KeyType.HERB,
                ("hero", _) => KeyType.HERO,
                ("hp", _) => KeyType.HITPOINTS,
                ("hunger", _) => KeyType.HUNGER,
                // CGN FIXME ? id is not an integer (i.e. "s50j";id)
                // in CsMapFx it is considered as an integer
                //("id", _) => KeyType.ID,
                ("id", _) => (KeyType)((int)KeyType.ID | INTEGER),
                ("Insel", _) => KeyType.ISLAND,
                ("Konfiguration", _) => KeyType.KONFIGURATION,
                ("Kampfstatus", _) => KeyType.STATUS,
                ("Kapitaen", _) => KeyType.CAPTAIN,
                ("Kueste", _) => KeyType.COAST,
                ("Ladung", _) => KeyType.LOAD,
                ("locale", _) => KeyType.LOCALE,
                ("Lohn", _) => KeyType.SALARY,
                ("MaxLadung", _) => KeyType.MAXLOAD,
                ("mode", BlockType.MESSAGE) => (KeyType)((int)KeyType.MSG_MODE | INTEGER),
                ("name", _) => KeyType.LOWERCASE_NAME,
                ("Name", BlockType.COMBATSPELL) or ("Name", BlockType.GROUP) => KeyType.LOWERCASE_NAME,
                ("Name", _) => KeyType.NAME,
                ("number", BlockType.RESOURCE) => KeyType.RESOURCE_COUNT,
                ("Optionen", _) => KeyType.OPTIONS,
                (Strings.DE_UNIT_FACTION_ID, _) => KeyType.FACTION,
                ("Parteiname", _) => KeyType.FACTIONNAME,
                ("Parteitarnung", _) => KeyType.FACTIONSTEALTH,
                ("privat", _) => KeyType.NOTES,
                ("region", BlockType.MESSAGE) => KeyType.MSG_REGION,
                ("Rekrutierungskosten", _) => KeyType.RECRUITMENTCOST,
                ("Runde", _) => (KeyType)((int)KeyType.TURN | INTEGER),
                ("Schoesslinge", _) or ("Schößlinge", _) => KeyType.SAPLINGS,
                ("Schaden", _) => KeyType.DAMAGE,
                ("Schiff", _) => KeyType.SHIP,
                ("Silber", _) => KeyType.SILVER,
                ("speed", _) => KeyType.SPEED,
                ("Terrain", _) => KeyType.TERRAIN,
                ("type", BlockType.MESSAGE) => (KeyType)((int)KeyType.MSG_TYPE | INTEGER),
                ("type", BlockType.RESOURCE) => KeyType.RESOURCE_TYPE,
                ("Typ", _) => KeyType.TYPE,
                ("typprefix", _) => KeyType.PREFIX,
                ("Verraeter", _) => KeyType.TRAITOR,
                ("visibility", _) => KeyType.VISIBILITY,
                ("weight", _) => KeyType.WEIGHT,
                ("", _) => KeyType.EMPTY,
                _ => KeyType.UNKNOWN,
            };
        }

        public string GetKey()
        {
            return Key;
        }

        public static string GetTypeValue(KeyType keyType, bool useGermanChars)
        {
            if (Values.TryGetValue(keyType, out var value))
            {
                if (useGermanChars)
                {
                    // Schoesslinge => Schößlinge
                    // Baeume => Bäume

                    // Convert to German characters if needed
                    // Remplacer les substitutions "ae", "oe", "ue", "ss" par les caractères allemands correspondants
                    value = value.Replace("ae", "ä")
                                 //.Replace("oe", "ö")
                                 //.Replace("ue", "ü")
                                 .Replace("ss", "ß");
                }
                return value;
            }
            return string.Empty;
        }

        public string GetKeyFromType()
        {
            var keytype = GetKeyType();
            if (Values.TryGetValue(keytype, out var name))
            {
                return name;
            }

            if (keytype == KeyType.UNKNOWN)
            {
                // Unknowns keys :
                // - wahrertype
                // - Besitzer
                // - temp
                return Key;
            }
            return string.Empty;
        }
        public string GetTranslatedKey(string? loc = null)
        {
            // OPTIMIZE LATER: use inside .CR file TRANSLATION block data

            switch (GetKeyType())
            {
                case KeyType.BUILDING:
                    return "Castle";
                case KeyType.OWNER:
                    return "Owner";
                case KeyType.CAPTAIN:
                    return "Captain";
                case KeyType.SPEED:
                    return "Speed";
                case KeyType.SAPLINGS:
                    return "Saplings";
                // CGN - translation added
                case KeyType.SALARY:
                    return "Salary";
                case KeyType.NAME:
                    return "Name";
                case KeyType.LOWERCASE_NAME:
                    return "name";
                case KeyType.NOTES:
                    return "private";
                case KeyType.DESCRIPTION:
                    return "Description";
                case KeyType.HERB:
                    return "Herb";
                case KeyType.TREES:
                    return "Trees";
                case KeyType.PEASANTS:
                    return "Peasants";
                case KeyType.TERRAIN:
                    return "Terrain";
                case KeyType.ISLAND:
                    return "Island";
                case KeyType.FACTION:
                    return "Faction";
                case KeyType.TRAITOR:
                    return "Traitor";
                case KeyType.OTHER_FACTION:
                    return "Other faction";
                case KeyType.FACTIONNAME:
                    return "Faction name";
                case KeyType.FACTIONSTEALTH:
                    return "Faction disguised"; // TODO : better translation ?
                case KeyType.NUMBER:
                    return "Number";
                case KeyType.SHIP:
                    return "Ship";
                case KeyType.TYPE:
                    return "Type";
                case KeyType.SIZE:
                    return "Size";
                case KeyType.RESOURCE_TYPE:
                    return "type";
                case KeyType.RESOURCE_SKILL:
                    return "skill";
                case KeyType.RESOURCE_COUNT:
                    return "number";
                case KeyType.KONFIGURATION:
                    return "Configuration";
                case KeyType.WEIGHT:
                    return "weight";
                case KeyType.GROUP:
                    return "group";
                case KeyType.PREFIX:
                    return "typprefix";
                case KeyType.HITPOINTS:
                    return "hp";
                case KeyType.STATUS:
                    return "Combat status";
                case KeyType.FAMILIARMAGE:
                    return "magician's pet";
                case KeyType.GUARDING:
                    return "Guarded";
                case KeyType.HUNGER:
                    return "hunger";
                case KeyType.HERO:
                    return "hero";
                case KeyType.CHARSET:
                    return "charset";
                case KeyType.VISIBILITY:
                    return "visibility";
                case KeyType.TURN:
                    return "round";
                case KeyType.SILVER:
                    return "Silver";
                case KeyType.RECRUITMENTCOST:
                    return "Recruitment costs";
                case KeyType.AURA:
                    return "Aura";
                case KeyType.AURAMAX:
                    return "Auramax";
                case KeyType.OPTIONS:
                    return "Options";
                case KeyType.EMAIL:
                    return "email";
                case KeyType.BANNER:
                    return "banner";
                case KeyType.LOCALE:
                    return "locale";
                case KeyType.ORDERS_CONFIRMED:
                    return "ejcOrdersConfirmed";
                case KeyType.DAMAGE:
                    return "Damage";
                case KeyType.COAST:
                    return "Coast";
                case KeyType.CAPACITY:
                    return "capacity";
                case KeyType.CARGO:
                    return "cargo";
                case KeyType.LOAD:
                    return "Loading"; // TODO : better translation ?
                case KeyType.MAXLOAD:
                    return "MaxLadung";
                case KeyType.MSG_TYPE:
                    return "type";
                case KeyType.MSG_REGION:
                    return "region";
                case KeyType.MSG_AMOUNT:
                    return "amount";
                case KeyType.MSG_COST:
                    return "cost";

                case KeyType.UNKNOWN:
                default:
                    return GetKeyFromType();
            }
        }


        public string GetValue()
        {
            return Value;
        }

        public void Set(string type, BlockType btype)
        {
            KeyType = ParseType(type, btype);
            Key = KeyType == KeyType.UNKNOWN ? type : string.Empty;
        }

        protected KeyType KeyType { get; private set; }

        protected string Key { get; private set; }

        protected string Value { get; private set; }

        // Parses str and returns created datakey object or null
        public bool Parse(string strToParse, BlockType blockType, bool utf8)
        {
            if (string.IsNullOrEmpty(strToParse))
            {
                return false;
            }

            // skip indentation
            string str = strToParse.TrimStart();

            int mask = 0;
            bool startsWithQuote = str.StartsWith('"');

            // samples of lines :
            // "Charset";utf-8
            // 1;field
            // Charset;other
            // "a strinjg with :";special 

            // if first char is '\"' set mask to 1
            if (startsWithQuote)
            {
                str = str.Substring(1);
            }
            else
            {
                // if the first word is an integer (negative possible) set mask to 1
                if (StringUtils.IsFirstWordNumeric(str))
                {
                    mask = INTEGER;
                }
                else
                {
                    return false;
                }
            }
            int lastSemiColumn = str.LastIndexOf(';');
            int quote = str.IndexOf('"');

            // ok, so assign new values
            if (lastSemiColumn >= 0 && lastSemiColumn >= quote)
            {
                Set(str.Substring(lastSemiColumn + 1)/*.Trim()*/, blockType);
            }
            else
            {
                Set("", blockType);
                // To keep ';' inside a COMMANDS block part "...;..." (';' is a comment separator)
                lastSemiColumn = -1;
            }

            KeyType = (KeyType)((int)KeyType | mask);

            // set value (what is before the last ';')
            if (lastSemiColumn != -1)
            {
                str = str[..lastSemiColumn];
            }
            int lastQuote = str.LastIndexOf('"');

            if (quote != -1 && lastQuote != -1)
            {
                str = str[..lastQuote];
            }

            str = str.Replace("\\\\", "\\");
            str = str.Replace("\\\"", "\"");

            // TODO LATER :convert str from ISO to UTF8 if utf8 is false
            Value = str;

            // TODO LATER: return false if value is not specified (nothing after ':')
            return true;
        }

        public bool IsInt()
        {
            return ((int)KeyType & INTEGER) != 0;
        }

        public int GetInt()
        {
            return Utils.Converters.StringToInt(Value);
        }

        public int GetValueInt(int section = 0)
        {
            string[]? parts = Value?.Split(" ");
            if (parts?.Length > section && Int32.TryParse(parts[section], out int v))
            {
                return v;
            }
            return 0;
        }
    }
}
