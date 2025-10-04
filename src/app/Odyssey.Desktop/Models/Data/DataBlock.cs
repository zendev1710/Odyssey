using Odyssey.Utils;
using DryIoc.FastExpressionCompiler.LightExpression;
using MicroCom.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Odyssey.Models.Data
{
    public enum BlockType
    {
        UNKNOWN,
        VERSION,
        OPTIONS,
        FACTION,
        GROUP,
        ALLIANCE,
        REGION,
        ISLAND,
        SCHEMEN,
        RESOURCE,
        PRICES,
        DURCHREISE,
        DURCHSCHIFFUNG,
        BORDER,
        SHIP,
        BUILDING,
        UNIT,
        UNITMESSAGES,
        TALENTS,
        SPELLS,
        COMBATSPELL,
        ZAUBER,
        KOMPONENTEN,
        TRANK,
        ZUTATEN,
        ITEMS,
        COMMANDS, // ORDERS
        EFFECTS,
        MESSAGE,
        BATTLE,
        MESSAGETYPE,
        TRANSLATION,
        LAST
    }

    // Flags for map icons
    [Flags]
    public enum Flag
    {
        CASTLE = 1 << 0,              // there is a building/tower/castle in this region
        REGION_TAXES = 1 << 1,        // we got taxes in this region (E3 only)
        SHIP = 1 << 2,                // there are ships in this region
        SHIPTRAVEL = 1 << 3,          // ships travelled the region

        LIGHTHOUSE = 1 << 4,          // region is seen by lighthouse
        TRAVEL = 1 << 5,              // region is seen by traveling throu

        MONSTER = 1 << 6,             // monster in region
        SEASNAKE = 1 << 7,            // sea snake in region
        DRAGON = 1 << 8,              // dragon/wyrm in region

        WORMHOLE = 1 << 9,            // wurmloch in region

        TROOPS = 1 << 10,             // units are in this region

        GUARD_OWN = 1 << 11,          // region is guarded by *
        GUARD_ALLY = 1 << 12,
        GUARD_ENEMY = 1 << 13,

        REGION_OWN = 1 << 14,         // region is owned by * (E3 only)
        REGION_ALLY = 1 << 15,
        REGION_ENEMY = 1 << 16,

        STREET = 1 << 17,             // street in region (first)
        STREET_NW = 1 << 17,          // street to north west
        STREET_NO = 1 << 18,          // street to north east
        STREET_O = 1 << 19,           // street to east
        STREET_SO = 1 << 20,          // street to south east
        STREET_SW = 1 << 21,          // street to south west
        STREET_W = 1 << 22,           // street to west

        STREET_UNDONE = 1 << 23,      // incomplete street in region (first)
        STREET_UNDONE_NW = 1 << 23,   // incomplete street to north west
        STREET_UNDONE_NO = 1 << 24,   // incomplete street to north east
        STREET_UNDONE_O = 1 << 25,    // incomplete street to east
        STREET_UNDONE_SO = 1 << 26,   // incomplete street to south east
        STREET_UNDONE_SW = 1 << 27,   // incomplete street to south west
        STREET_UNDONE_W = 1 << 28,    // incomplete street to west

        BLOCKID_BIT0 = 1 << 29,       // number of ids: +1
        BLOCKID_BIT1 = 1 << 30,       // +2

        REGION_SEEN = 1 << 31,        // region is seen by own units

        None = 0                        // no flags are set
    }

    /// <summary>
    /// region_info struct in original C++ code
    /// </summary>
    public class RegionInfos
    {
        public int People { get; set; }
        public int FactionPeople { get; set; }
        public int FactionSilver { get; set; }
        public Dictionary<RegionItemType, RegionItem> Items { get; set; }

        public RegionInfos()
        {
            People = 0;
            FactionPeople = 0;
            FactionSilver = 0;
            Items = [];
        }
    }

    // =========================
    // === attachment base class

    // base class for block attachment classes
    // these classes could contain additional information
    // like new commands of a unit, region symbol flags (map) or
    // pointer to previous' turns unit.
    public interface IAttachment
    {
    }
    public class RegionAttachment : IAttachment
    {
        public List<float> People { get; private set; }
        // Name of island
        public string Island { get; set; }
        public long LearnCost { get; set; }
        public RegionInfos? RegionInfos { get; private set; }
        public long[] Incomes { get; private set; }
        // Number of unconfirmed units
        public int Unconfirmed { get; set; }

        public RegionAttachment()
        {
            Island = "";
            LearnCost = 0;
            Unconfirmed = 0;
            People = [];
            Incomes = new long[(int)Income.Kind.MAX];
            for (int i = 0; i < Incomes.Length; i++)
            {
                Incomes[i] = 0;
            }
        }
        public RegionInfos SetRegionInfos(RegionInfos regionInfo)
        {
            RegionInfos = regionInfo;
            return RegionInfos;
        }
        public void ResetIncomes()
        {
            for (int i = 0; i < Incomes.Length; i++)
            {
                Incomes[i] = 0;
            }
        }
        public long GetIncome(Income.Kind type)
        {
            int index = (int)type;
            if (index >= 0 && index < Incomes.Length)
            {
                return Incomes[index];
            }
            return 0;
        }
        public void AddIncome(Income.Kind type, long value)
        {
            int index = (int)type;
            if (index >= 0 && index < Incomes.Length)
            {
                Incomes[index] += value;
            }
        }
    }

    public class DataBlock
    {
        // Node to be able to iterate and navigate in the list of entities (known regions and their entity children)
        //public LinkedListNode<DataBlock>? EntityNode { get; private set; }
        public LinkedListNode<DataBlock>? Node { get; private set; }

        private DataBlock? PreviousBlock { get; set; }

        private DataBlock? NextBlock { get; set; }

        public void SetNode(LinkedListNode<DataBlock>? node) { 
            Node = node;
        }

        public DataBlock? GetPreviousBlock()
        {
            return PreviousBlock ?? Node?.Previous?.Value;
        }

        public DataBlock? GetNextBlock()
        {
            return NextBlock ?? Node?.Next?.Value;
        }

        public void SetPreviousBlock(DataBlock? block)
        {
            PreviousBlock = block;
        }

        public DataBlock? SetNextBlock(DataBlock? block)
        {
            return NextBlock = block;
        }

        public List<DataKey> GetData() => Data;
        public BlockType GetBlockType() => Type;
        public int GetX() => X;
        public int GetY() => Y;
        /// <summary>
        /// Get the Id.
        /// Original method name was info(). 
        /// </summary>
        /// <returns></returns>
        public int GetId() => Id;
        public int GetFlags() => Flags;
        public int GetDepth() => Depth;
        public int GetTerrain() => Terrain;
        public IAttachment GetAttachment() =>  _attachment ??= CreateAttachment();
        public void SetTerrain(int terrain) => Terrain = terrain;
        public void SetDepth(int depth) => Depth = depth;

        protected BlockType Type { get; set; }
        protected int Id { get; private set; }
        protected int Depth { get; set; }
        protected int Flags { get; private set; }
        protected int X { get; private set; }
        protected int Y { get; private set; }

        // string representation of the block type when its type is BlockType.UNKNOWN
        private string? TypeLabel { get; set; }
        protected int Terrain { get; set; }
        protected List<DataKey> Data { get; set; }
        private IAttachment? _attachment { get; set; }

    public static readonly string[] UNITKEYS = ["target", "unit", "mage", "spy", "teacher", "student"];

        public static readonly string[] TRANSLATED_NAMES =
        [
            "Unknown",
            "Ocean",
            "Swamp",
            "Level",
            "Desert",
            "Forest",
            "highland",
            "Mountain",
            "Glacier",
            "Volcano",
            "Active volcano",
            "Pack ice",
            "Iceberg",
            "Ice floe",
            "Gear",
            "Wall",
            "Hall",
            "Fog",
            "Dense fog",
            "Wall of fire",
            "Maelstrom"
        ];

        public static readonly string[] NAMES =
        [
            "Unbekannt",        // UNKNOWN
            "Ozean",            // Ocean
            "Sumpf",
            "Ebene",
            "W\u00fcste",
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

        protected static readonly Dictionary<BlockType, string> BLOCKNAMES = new Dictionary<BlockType, string>
        {
            { BlockType.VERSION, "VERSION" },
            { BlockType.OPTIONS, "OPTIONEN" },
            { BlockType.FACTION, "PARTEI" },
            { BlockType.GROUP, "GRUPPE" },
            { BlockType.ALLIANCE, "ALLIANZ" },
            { BlockType.REGION, "REGION" },
            { BlockType.ISLAND, "ISLAND" },
            { BlockType.SCHEMEN, "SCHEMEN" },
            { BlockType.RESOURCE, "RESOURCE" },
            { BlockType.PRICES, "PREISE" },
            { BlockType.DURCHREISE, "DURCHREISE" },
            { BlockType.DURCHSCHIFFUNG, "DURCHSCHIFFUNG" },
            // LATER: check why BORDER is defined two times
            // GRENZE is in global_map.cr. Check if it is the case in recent reports.
            { BlockType.BORDER, "GRENZE" },
            //{ BlockType.BORDER, "BORDER" },
            { BlockType.SHIP, "SCHIFF" },
            { BlockType.BUILDING, "BURG" },
            { BlockType.UNIT, "EINHEIT" },
            { BlockType.UNITMESSAGES, "EINHEITSBOTSCHAFTEN" },
            { BlockType.TALENTS, "TALENTE" },
            { BlockType.SPELLS, "SPRUECHE" },
            { BlockType.COMBATSPELL, "KAMPFZAUBER" },
            { BlockType.ZAUBER, "ZAUBER" },
            { BlockType.KOMPONENTEN, "KOMPONENTEN" },
            { BlockType.TRANK, "TRANK" },
            { BlockType.ZUTATEN, "ZUTATEN" },
            { BlockType.ITEMS, "GEGENSTAENDE" },
            { BlockType.COMMANDS, "COMMANDS" },
            { BlockType.EFFECTS, "EFFECTS" },
            { BlockType.MESSAGE, "MESSAGE" },
            { BlockType.BATTLE, "BATTLE" },
            { BlockType.MESSAGETYPE, "MESSAGETYPE" },
            { BlockType.TRANSLATION, "TRANSLATION" },
            { BlockType.UNKNOWN, string.Empty },
        };

        // At this moment this method is unused.
        public string GetTranslatedValue(KeyType key)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyType() == key);
            // LATER: if default key (no translation) then return value()
            if (dataKey != null) {
                return dataKey.GetTranslatedKey();
            }
            return string.Empty;
        }

        public DataBlock()
        {
            Data = [];
        }
        public void AddKey(DataKey key) => Data.Add(key);
        public void SetKey(KeyType keyType, string value)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyType() == keyType);
            if (dataKey == null)
            {
                AddKey(new DataKey(keyType, value));
            }
        }
        public void SetKey(KeyType keyType, int value)
        {
            SetKey(keyType, Utils.Converters.ToStringVal(value));
        }

        public bool HasKey(KeyType keyType)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyType() == keyType);
            return dataKey != null;
        }

        public bool RemoveKey(KeyType keyType)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyType() == keyType);
            if (dataKey != null)
            { 
                Data.Remove(dataKey);
                return true;
            }
            return false;
        }

        public string Value(string key)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyFromType() == key);
            return dataKey != null ? dataKey.GetValue() : string.Empty;
        }

        public string Value(KeyType keyType)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyType() == keyType);
            return dataKey != null ? dataKey.GetValue() : string.Empty;
        }

        public int ValueInt(KeyType keyType, int defaultValue = 0)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyType() == keyType);
            return dataKey != null ? dataKey.GetInt() : defaultValue;
        }

        public int ValueInt(string key, int defaultValue = 0)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyFromType() == key);
            return dataKey != null ? dataKey.GetInt() : defaultValue;
        }

        public int ValueSkill(string skill)
        {
            DataKey? dataKey = Data.Find(k => k.GetKeyFromType() == skill);
            return dataKey != null ? dataKey.GetValueInt(1) : 0;
        }

        public DataKey? ValueKey(KeyType keyType) => Data.Find(k => k.GetKeyType() == keyType);

        public string IdToString()
        {
            int nn = Id;
            return nn > 0 ? Utils.Converters.IdToString(nn) : "0";
        }

        public int GetReference(BlockType type)
        {
            if (type == BlockType.SHIP)
            {
                int uid = ValueInt("ship");
                if (uid > 0)
                {
                    return uid;
                }
            }
            else if (type == BlockType.BUILDING)
            {
                int uid = ValueInt("building");
                if (uid > 0)
                {
                    return uid;
                }
            }
            else if (type == BlockType.UNIT)
            {
                foreach (var key in UNITKEYS)
                {
                    int uid = ValueInt(key);
                    if (uid > 0)
                    {
                        return uid;
                    }
                }
            }
            return 0;
        }
        public bool GetAllReferences(ref int unitId, ref int shipId, ref int buildingId, ref string location)
        {
            unitId = 0;
            shipId = 0;
            buildingId = 0;
            location = string.Empty;
            foreach (DataKey key in Data)
            {
                string kt = key.GetKeyFromType();
                if (kt == "ship")
                {
                    shipId = key.GetInt();
                }
                else if (kt == "building")
                {
                    buildingId = key.GetInt();
                }
                else if (kt == Strings.EN_MESSAGE_REGION)
                {
                    location = key.GetValue();
                }
                else if (unitId == 0)
                {
                    foreach (var unitKt in UNITKEYS)
                    {
                        if (kt == unitKt)
                        {
                            unitId = key.GetInt();
                            break;
                        }
                    }
                }
            }
            return unitId > 0 || shipId > 0 || buildingId > 0 || !string.IsNullOrEmpty(location);
        }

        /// <summary>
        /// Returns true if the specified target has a reference.
        /// A reference means :
        /// - a "X Y Id" value in some of its data for a region
        /// - an Id ("ship" value in its data) for a ship
        /// - an Id ("building" value in its data) for a building
        /// - an Id ("unit", "mage", "spy", "teacher", "student" value in its data) for a unit
        /// </summary>
        /// <param name="target">the DataBlock to check</param>
        /// <returns>true if the specified target has a reference; false otherwise</returns>
        public bool HasReference(in DataBlock target)
        {
            BlockType btype = target.GetBlockType();
            if (btype == BlockType.REGION)
            {
                string match = $"{target.GetX()} {target.GetY()} {target.GetId()}";
                foreach (DataKey key in Data)
                {
                    if (key.GetValue() == match)
                    {
                        return true;
                    }
                }
            }
            else if (btype == BlockType.SHIP)
            {
                int id = target.GetId();
                int uid = ValueInt("ship");
                if (uid == id)
                {
                    return true;
                }
            }
            else if (btype == BlockType.BUILDING)
            {
                int id = target.GetId();
                int uid = ValueInt("building");
                if (uid == id)
                {
                    return true;
                }
            }
            else if (btype == BlockType.UNIT)
            {
                int id = target.GetId();
                foreach (var key in UNITKEYS)
                {
                    int uid = ValueInt(key);
                    if (uid == id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // return true if region is unseen
        public bool IsUnseenRegion() => (Flags & (int)Flag.REGION_SEEN) == 0;

        public void SetFlags(int flags) => Flags = flags;

        public void AddFlags(int flags) =>
            // set <flags> and don't modify the other flags
            Flags |= flags;

        public static int ParseSpecialTerrain(string terrain) =>
            // (terrain that uses image of another terrain)

            // this terrain types should be kept as text,
            // so if you resave the file, it's exact terrain type (_Aktiver_ Vulkan) 
            // could be saved.

            Terrains.UNKNOWN;

        private IAttachment CreateAttachment()
        {
            if (Type == BlockType.COMMANDS)
            {
                return new OrdersAttachment();
            }

            // Type is suppoed to be BlockType.REGION
            return new RegionAttachment();
        }

        private static readonly Dictionary<string, int> TerrainMap = new Dictionary<string, int>
        {
            // LATER: compare Dictionary usage vs string equality (like in CsMapFx)

            // thesz terrainq do not need textual representation
	        { "Ozean", Terrains.OCEAN },
            { "Sumpf", Terrains.SWAMP },
            { "Ebene", Terrains.PLAINS },
            { "Wueste", Terrains.DESERT },
            { "W\u00fcste", Terrains.DESERT }, // LATER: better handle special german characters
            { "Wald", Terrains.FOREST },
            { "Hochland", Terrains.HIGHLAND },
            { "Berge", Terrains.MOUNTAIN },
            { "Gletscher", Terrains.GLACIER },
            { "Vulkan", Terrains.VOLCANO },
            { "Eisberg", Terrains.ICEBERG },
            { "Feuerwand", Terrains.FIREWALL },
            { "Mahlstrom", Terrains.MAHLSTROM },
            { "Aktiver Vulkan", Terrains.VOLCANO_ACTIVE },
            { "Packeis", Terrains.PACKICE },
            { "Eisscholle", Terrains.ICEFLOE },
            { "Wand", Terrains.WALL },
            { "Halle", Terrains.HALL },
            { "Gang", Terrains.CORRIDOR },
            { "Rauchender Vulkan", Terrains.VOLCANO_ACTIVE },
            { "Nebel", Terrains.FOG },
            { "Dichter Nebel", Terrains.THICKFOG },
        };

        public static int ParseTerrain(string terrain)
        {
            if (TerrainMap.TryGetValue(terrain, out int terrainType))
            {
                return terrainType;
            }
            return 0; // Unknown terrain type
        }

        // Try to parse str as a datablock header
        public bool Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            // Skip potential indentation
            string str = value.TrimStart();

            // must start with an uppercase letter
            if (string.IsNullOrEmpty(str) || !char.IsLetter(str[0]) || !char.IsUpper(str[0]))
            {
                return false;
            }

            // block headers cannot contain '\"' or ';'
            if (str.Contains('\"') || str.Contains(';'))
                return false;

            // unset flags
            SetFlags((int)Flag.None);

            int space = str.IndexOf(' ');
            SetTypeLabel(space == -1 ? str : str[..space]);
            SetId(space == -1 ? string.Empty : str.Remove(0, space + 1));

            // unset all other states
            SetTerrain(Terrains.UNKNOWN);
            //SetAttachment(null);
            return true;
        }

        public /*static*/ BlockType ParseType(string typeStr)
        {
            // region moved to top (performance)
            if (typeStr == "REGION")
                return BlockType.REGION;

            // if BLOCKNAMES is defined as static, null exception here
            foreach (var b in BLOCKNAMES)
            {
                if (b.Value == typeStr)
                {
                    return b.Key;
                }
            }

            return BlockType.UNKNOWN;
        }

        // string() function in cpp code
        public void SetTypeLabel(string s)
        {
            Type = ParseType(s);
            TypeLabel = Type == BlockType.UNKNOWN ? s : "";
        }

        public string GetTypeLabel()
        {
            if (Type == BlockType.UNKNOWN)
            {
                return TypeLabel;
            }

            foreach (var b in BLOCKNAMES)
            {
                if (b.Key == Type)
                {
                    return b.Value;
                }
            }
            return "";
        }

        public string GetTerrainName()
        {
            string type = Value(KeyType.TERRAIN);
            if (!string.IsNullOrEmpty(type))
            {
                return type;
            }
            int t = Terrain;
            if (t >= Terrains.UNKNOWN && t < Terrains.LAST)
            {
                return NAMES[t];
            }
            return NAMES[Terrains.UNKNOWN];
        }

        private string GetUITerrainTranslatedName()
        {
            string type = Value(KeyType.TERRAIN);
            if (!string.IsNullOrEmpty(type))
                return type;

            int t = Terrain;
            if (t >= Terrains.UNKNOWN && t < Terrains.LAST)
            {
                return TRANSLATED_NAMES[t];
            }
            return TRANSLATED_NAMES[Terrains.UNKNOWN];
        }

        // Plane name, is part of name for regions
        string GetUIPlaneName(int plane)
        {
            switch((PlaneType)plane)
                {
                case PlaneType.WORLD:
                    return "Standardebene"; // Standard level
                case PlaneType.ASTRAL:
                    return "Astralraum"; // Astral space
                case PlaneType.ARENA:
                    return "Arena";
                case PlaneType.ETERNATH:
                    return "Eternath";
                case PlaneType.CHRISTMAS_ISLAND:
                    return "Weihnachtsinsel"; // Christmas Island
                default:
                    return $"Ebene {plane}"; // level {plane}
            }
        }

        public string GetUIName()
        {
            string name = "";
            if (Type == BlockType.FACTION || Type == BlockType.ALLIANCE)
            {
                name = Value(KeyType.FACTIONNAME);
                if (string.IsNullOrEmpty(name))
                {
                   name = $"Faction {IdToString()}";
                }
            } else {
                name = Value(KeyType.NAME);
                if (string.IsNullOrEmpty(name))
                {
                    switch (Type)
                    {
                        case BlockType.UNIT:
                            return $"Unit {IdToString()}";
                        case BlockType.REGION:
                            name = GetUITerrainTranslatedName();
                            if (string.IsNullOrEmpty(name))
                            {
                                name = "Unknown";
                            }
                            break;
                    }
                }
            }
            return name;
        }

        public string GetUILabel()
        {
            string name = GetUIName();
            if (Type == BlockType.REGION || Type == BlockType.BATTLE)
            {
                int id = GetId();
                if (id > 0)
                {
                    return $"{name} ({X}, {Y}, {GetUIPlaneName(id)})";
                }
                return $"{name} ({X}, {Y})";
            }
            return $"{name} ({IdToString()})";
        }

        public string GetStringId()
        {
            // TODO: handle ISLAND Id
            return Type == BlockType.REGION ? $"{X}, {Y}" : IdToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public void SetId(string s)
        {
            SetFlags(Flags & ~((int)Flag.BLOCKID_BIT0 | (int)Flag.BLOCKID_BIT1));
            if (!string.IsNullOrEmpty(s))
            {
                int id;
                if (!s.Contains(' '))
                {
                    SetFlags((int)Flag.BLOCKID_BIT0);
                    id = Utils.Converters.StringToInt(s);
                }
                else
                {
                    // Note: only REGION and BATTLE blocks have X Y Id values
                    SetFlags((int)Flag.BLOCKID_BIT1);
                    Utils.Converters.ExtractCoordinates(s, out int x, out int y, out id);
                    if (id > 0)
                        SetFlags((int)Flag.BLOCKID_BIT0);
                    X = x;
                    Y = y;
                }

                Id = id;
            }
            else
            {
                X = 0;
                Y = 0;
                Id = 0;
            }
        }

        /// <summary>
        /// Extracts the "36 base" Id ("(g5te)") from the specified string, and convert it in "10 base".
        /// if several Ids are present in the string, the last one is extracted.
        /// In "Wagon driver to Xorlosch (g5te)", g5te will be extracted and converted to 123456.
        /// </summary>
        /// <param name="s">the string having the "36 base" Id.</param>
        /// <returns>the converted value; or -1 if extraction or conversion failed.</returns>
        public static int ExtractId(string s)
        {
            int left = s.LastIndexOf('(');
            int right = s.LastIndexOf(')');
            if (left >= 0 && right > left)
            {
                string valueInParenthesis = s.Substring(left + 1, right - left - 1);
                return Utils.Converters.DecodeBase36(valueInParenthesis);
            }
            return -1; // or throw an exception if appropriate
        }

#if DEBUG
        public override string ToString()
        {
            return $"{GetUILabel()}";
        }
#endif
    }
}
