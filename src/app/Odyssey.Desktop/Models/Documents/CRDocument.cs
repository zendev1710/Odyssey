using Odyssey.Extensions;
using Odyssey.Models.Data;
using Odyssey.Models.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Odyssey.Models.Documents;

public enum OwnerType
{
    ENEMY,
    OWN,
    ALLY,
} 

public class CRDocument : EresseaDocument
{
    private static readonly int HELP_GUARD = 16;

    // "got taxes" message (E3 only)
    private const int MESSAGE_TYPE_GOT_TAXES            = 1264208711;

    private const int MESSAGE_TYPE_PASSWORD             = 1784377885;
    private const int MESSAGE_TYPE_INCOME               = 771334452;
    private const int MESSAGE_TYPE_COST                 = 443066738;

    private const int MESSAGE_TYPE_UNIT_NOT_FOUND       = 987417476;
    private const int MESSAGE_TYPE_RECEIVE_SOMETHING    = 1235024123;
    private const int MESSAGE_TYPE_GIVE_SOMETHING       = 5281483;
    private const int MESSAGE_TYPE_TRANSFER_PERSONS     = 815345085;
    private const int MESSAGE_TYPE_MAKE_SOMETHING       = 2087428775;
    private const int MESSAGE_TYPE_SELL_SOMETHING       = 1478912224;
    // LATER: Check if find herb or also some other things
    private const int MESSAGE_TYPE_FIND_SOMETHING       = 1511758069;
    // LATER: Check if pay only for luxury items or for some other things
    private const int MESSAGE_TYPE_PAY_FOR_ITEMS        = 170076;
    // LATER: Check if buy only for luxury items or for some other things
    private const int MESSAGE_TYPE_BUY_LUXURY_ITEMS     = 1549031288;
    // LATER: Check if it's also for some other things
    // 1412: This region is guarded by Vuk Karadžić (apg7), a non-allied unit.
    // global_map:  'MOVE nw' - Begleiter (kjuo) is guarding the region.
    private const int MESSAGE_TYPE_ROUTE_GUARD_ISSUE    = 428515567;
    // WALK / RIDE / SAIL (ROUTE COMMAND)
    private const int MESSAGE_TYPE_MOVE                 = 1242100855;
    private const int MESSAGE_TYPE_SAIL                 = 2026874001;
    private const int MESSAGE_TYPE_REGENERATE_AUTA      = 442874678;

    // active factions (could be more than one in case of alliances).
    // An active faction is a faction for which the player has units (and can set commands for them) in the current report.
    private Dictionary<int, FactionModel> ActiveFactions = [];

    public bool IsActiveFaction(int id)
    {
        return ActiveFactions.ContainsKey(id);
    }

    // The first active faction encountered in the report.
    public FactionModel ActiveFaction { get; private set; }

    public Dictionary<int, Rectangle> ContentSizes { get; private set; } = [];

    /// <summary>
    /// Returns the short name of the document.
    /// The short name is the trimmed right part of the name after the first dash ('-').
    /// i.e. for '1200 - Fullmap', it returns 'Fullmap'.
    /// </summary>
    public string ShortName
    {
        get
        {
            if (string.IsNullOrEmpty(Name))
            {
                return string.Empty;
            }
            int idx = Name.IndexOf('-');
            if (idx >= 0 && idx < Name.Length - 1)
            {
                return Name[(idx + 1)..].Trim();
            }
            return Name.Trim();
        }
    }

    public string Name { get; private set; }
    public string Version { get; private set; }
    public bool HasData() => Blocks.Count > 0;
    public bool IsEmpty() => !HasData();
    private LinkedList<DataBlock> Blocks { get; set; }

    private bool OptimizeDataConsuming { get; set; }

    public LinkedListNode<DataBlock>? FirstBlock { get { return Blocks?.First; } }
    public LinkedListNode<DataBlock>? LastBlock { get { return Blocks?.Last; } }
    public Dictionary<int, DataBlock> Units { get; private set; } = [];
    public Dictionary<int, DataBlock> Regions { get; private set; } = [];
    public Dictionary<int, DataBlock> Ships { get; private set; } = [];
    public Dictionary<int, DataBlock> Buildings { get; private set; } = [];
    public Dictionary<int, DataBlock> Islands { get; private set; } = [];
    public Dictionary<int, DataBlock> Battles { get; private set; } = [];
    public Dictionary<int, DataBlock> Groups { get; private set; } = [];
    public List<WorldPlane> Planes { get; private set; } = [];
    private Dictionary<int, FactionModel> Factions { get; set; } = [];

    public int RegionsNumber { get { return Regions.Count; } }
    public int SeenRegionsNumber { get; private set; } = 0;
    public int Turn { get; private set; } = -1;
    //public int GetActiveFactionId() { return ActiveFactionId; }
    //public DataBlock? GetActiveFaction() { return ActiveFaction; }
    public OrdersDocument OrdersDocument { get; private set; }

    public CRDocument(string name, LinkedList<DataBlock> blocks)
    {
        // TODO: define it as a global setting
        OptimizeDataConsuming = true;
        Version = string.Empty;
        // LATER : use default locale settings
        Locale = GameLanguage.UNKNOWN;
        Name = name;
        Blocks = blocks;
        OrdersDocument = new OrdersDocument();
        if (HasData())
        {
            CreateHashTables();
            ParseMessages();
        }
    }

    public CRDocument() : this("", []) 
    { 
    }

    // data of selection state (what region, what unit is actually selected?)

    public class Stack : List<BlockType> { }
    public class TSet : HashSet<BlockType> { }
    public class TypesToTSet : Dictionary<BlockType, TSet> { }

    /// <summary>
    /// Set depth for each block, based on the hierachy defined according to the block types.
    /// </summary>
    private void CreateHierarchy()
    {
        // map of PARENT-BLOCK . set of CHILD_BLOCKs
        TypesToTSet enclosed = [];

        // fill map
        foreach (BlockType bt in Enum.GetValues(typeof(BlockType)))
        {
            if (bt == BlockType.UNKNOWN || bt == BlockType.LAST)
            {
                continue;
            }
            enclosed[bt] = [BlockType.UNKNOWN];
        }

        // define hierarchy configuration.
        // for each block type, define the possible children block types
        enclosed[BlockType.VERSION].Add(BlockType.BATTLE);
        enclosed[BlockType.BATTLE].Add(BlockType.MESSAGE);
        enclosed[BlockType.VERSION].Add(BlockType.MESSAGETYPE);
        enclosed[BlockType.VERSION].Add(BlockType.ISLAND);
        enclosed[BlockType.VERSION].Add(BlockType.TRANSLATION);
        enclosed[BlockType.VERSION].Add(BlockType.REGION);
        enclosed[BlockType.REGION].Add(BlockType.SCHEMEN);
        enclosed[BlockType.REGION].Add(BlockType.BORDER);
        enclosed[BlockType.REGION].Add(BlockType.RESOURCE);
        enclosed[BlockType.REGION].Add(BlockType.ITEMS);
        enclosed[BlockType.REGION].Add(BlockType.UNIT);
        enclosed[BlockType.UNIT].Add(BlockType.EFFECTS);
        enclosed[BlockType.UNIT].Add(BlockType.COMMANDS);
        enclosed[BlockType.UNIT].Add(BlockType.ITEMS);
        enclosed[BlockType.UNIT].Add(BlockType.SPELLS);
        enclosed[BlockType.UNIT].Add(BlockType.COMBATSPELL);
        enclosed[BlockType.UNIT].Add(BlockType.UNITMESSAGES);
        enclosed[BlockType.UNIT].Add(BlockType.TALENTS);
        enclosed[BlockType.REGION].Add(BlockType.BUILDING);
        enclosed[BlockType.BUILDING].Add(BlockType.EFFECTS);
        enclosed[BlockType.REGION].Add(BlockType.DURCHREISE);
        enclosed[BlockType.REGION].Add(BlockType.DURCHSCHIFFUNG);
        enclosed[BlockType.REGION].Add(BlockType.SHIP);
        enclosed[BlockType.SHIP].Add(BlockType.EFFECTS);
        enclosed[BlockType.REGION].Add(BlockType.PRICES);
        enclosed[BlockType.REGION].Add(BlockType.EFFECTS);
        enclosed[BlockType.REGION].Add(BlockType.MESSAGE);
        enclosed[BlockType.FACTION].Add(BlockType.ZAUBER);
        enclosed[BlockType.ZAUBER].Add(BlockType.KOMPONENTEN);
        enclosed[BlockType.VERSION].Add(BlockType.TRANK);
        enclosed[BlockType.TRANK].Add(BlockType.ZUTATEN);
        enclosed[BlockType.VERSION].Add(BlockType.FACTION);
        enclosed[BlockType.FACTION].Add(BlockType.GROUP);
        enclosed[BlockType.GROUP].Add(BlockType.ALLIANCE);
        enclosed[BlockType.FACTION].Add(BlockType.ALLIANCE);
        enclosed[BlockType.FACTION].Add(BlockType.BATTLE);
        enclosed[BlockType.FACTION].Add(BlockType.MESSAGE);
        enclosed[BlockType.FACTION].Add(BlockType.OPTIONS);
        enclosed[BlockType.FACTION].Add(BlockType.ITEMS);

        // build hierarchy from hierarchy configuration.
        // Parent block types size == depth in hierarchy.
        Stack parents = [];                      
        foreach (var block in Blocks)
        {
            BlockType blockType = block.GetBlockType();
            while (parents.Count > 0)
            {
                // get the last parent block type
                var t = parents[^1];
                // enclosed does not contain BlockType.UNKNOWN
                if (t != BlockType.UNKNOWN)
                {
                    TSet types = enclosed[t];
                    // found in hierarchy config?
                    if (types.Contains(blockType))
                    {
                        break;
                    }
                }

                // parent-block cannot have this block as child
                // remove the last parent and try again
                parents.RemoveAt(parents.Count - 1);
            }
            parents.Add(blockType);
            block.SetDepth(parents.Count);
        }
    }

    public bool HasActiveFaction() { return ActiveFactions.Count > 0; }

    public FactionModel? GetFaction(int id)
    {
        return Factions.TryGetValue(id, out FactionModel? faction) ? faction : null;
    }

    public bool GetFaction(ref FactionModel? faction, int id)
    {
        faction = GetFaction(id);
        return faction != null;
    }
    /*
    public void CreateIslands()
    {
        Islands.Clear();
        foreach (var block in Blocks)
        {

            if (block.GetBlockType() == BlockType.ISLAND)
            {
                Islands[block.GetId()] = block;
            }
        }
        FloodIslandNames();
    }
    */

    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit">Unit </param>
    /// <returns></returns>
    public static int GetFactionIdForUnit(DataBlock unit)
    {
        if (unit.ValueInt(Strings.EN_UNIT_TRAITOR, 0) != 0)
        {
            return (int)SpecialFaction.TRAITOR;
        }
        return unit.ValueInt(KeyType.FACTION, (int)SpecialFaction.ANONYMOUS);
    }

    public string GetActiveFactionName()
    {
        if (ActiveFactions.Count == 0)
        {
            return string.Empty;
        }
        if (ActiveFactions.Count > 1)
        {
            // TODO: localize
            return "Alliance";
        }
        return GetFactionName(ActiveFaction);
    }

    public static string GetFactionName(FactionModel? faction)
    {
        if (faction is null)
        {
            return string.Empty;
        }
        return faction.Name;
    }

    public string GetFactionName(int factionId)
    {
        FactionModel? faction = null;
        if (factionId >= 0 && GetFaction(ref faction, factionId))
        {
            return GetFactionName(faction);
        }
        if (factionId == (int)SpecialFaction.ANONYMOUS)
        {
            return Labels.Localize(Labels.DISGUISED);
        }
        return Labels.Localize(Labels.TRAITOR);
    }

    /*
    public static bool IsEphemeral(BlockType type)
    {
        return type switch
        {
            BlockType.UNIT or BlockType.DURCHREISE or BlockType.DURCHSCHIFFUNG or BlockType.SHIP or BlockType.MESSAGE => true,
            _ => false,
        };
    }
    */

    public DataBlock? FindUnit(int id)
    {
        return Units.TryGetValue(id, out var unit) ? unit : null;
    }

    public DataBlock? FindShip(int id)
    {
        return Ships.TryGetValue(id, out var ship) ? ship : null;
    }

    public DataBlock? FindBuilding(int id)
    {
        return Buildings.TryGetValue(id, out var building) ? building : null;
    }

    public DataBlock? FindGroup(int id)
    {
        return Groups.TryGetValue(id, out var group) ? group : null;
    }

    public DataBlock? FindIsland(int id)
    {
        return Islands.TryGetValue(id, out var island) ? island : null;
    }

    public DataBlock? FindBattleFromPosition(int x, int y, int plane)
    {
        var coordinates = new Coordinates(x, y, plane);
        return Battles.TryGetValue((int)coordinates, out var battle) ? battle : null;
    }

    public bool FindBattleFromPosition(ref DataBlock? battle, int x, int y, int plane)
    {
        battle = FindBattleFromPosition(x, y, plane);
        return battle != null;
    }

    /*
    public bool HasBattleAtPosition(int x, int y, int plane)
    {
        var coordinates = new Coordinates(x, y, plane);
        return Battles.ContainsKey((int)coordinates);
    }
    */
    public DataBlock? FindRegionFromPosition(int x, int y, int plane)
    {
        var coordinates = new Coordinates(x, y, plane);
        return Regions.TryGetValue((int)coordinates, out var region) ? region : null;
    }

    public bool FindRegionFromPosition(ref DataBlock? region, int x, int y, int plane)
    {
        region = FindRegionFromPosition(x, y, plane);
        return region != null;
    }

    public DataBlock? Island(int id)
    {
        if (Islands.TryGetValue(id, out var island))
        {
            return island;
        }
        return null;
    }

    public bool IsConfirmed(DataBlock block)
    {
        if (block == null)
        {
            return false;
        }
        if (block.GetBlockType() == BlockType.REGION) {
            RegionAttachment? stats = block.GetAttachment() as RegionAttachment;
            if (stats != null) 
            { 
                return stats.Unconfirmed == 0;
            }
        }
        else if (block.GetBlockType() == BlockType.UNIT && IsActiveFaction(block.ValueInt(KeyType.FACTION)))
        {
            return block.ValueInt(KeyType.ORDERS_CONFIRMED) != 0;
        }
        return true;
    }

    /// <summary>
    /// Change the confirmation status of the specified unit.
    /// confirmation status means orders are confirmed.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="confirmed"></param>
    public void SetConfirmed(ref DataBlock unit, bool confirmed = true)
    {
        if (confirmed != IsConfirmed(unit))
        {
            RegionAttachment? stats = null;
            DataBlock? region = null;
            if (GetSeenParent(ref region, unit))
            {
                stats = region!.GetAttachment() as RegionAttachment;
            }
            if (confirmed)
            {
                unit.SetKey(KeyType.ORDERS_CONFIRMED, 1);
                if (stats != null)
                {
                    --stats.Unconfirmed;
                }
            }
            else
            {
                unit.RemoveKey(KeyType.ORDERS_CONFIRMED);
                if (stats != null)
                {
                    ++stats.Unconfirmed;
                }
            }
        }
    }

    /// <summary>
    /// Attempts to find the parent <see cref="DataBlock"/> of the specified child <see cref="DataBlock"/>.
    /// </summary>
    /// <remarks>A parent <see cref="DataBlock"/> is determined by finding the first ancestor in the linked
    /// list whose depth is less than the depth of the specified child <see cref="DataBlock"/>.</remarks>
    /// <param name="parent">When this method returns, contains the parent <see cref="DataBlock"/> of the specified child, if a parent is
    /// found; otherwise, <see langword="null"/>.</param>
    /// <param name="child">The child <see cref="DataBlock"/> for which to find the parent. This parameter is read-only.</param>
    /// <returns><see langword="true"/> if a parent <see cref="DataBlock"/> is found; otherwise, <see langword="false"/>.</returns>
    public static bool GetSeenParent(ref DataBlock? parent, in DataBlock child)
    {
        int iterationsNumber = 0;
        //DataBlock child = child.Node;
        for (parent = child; parent != null; parent = parent.GetPreviousBlock())
        {
            iterationsNumber++;
            if (parent.GetDepth() < child.GetDepth())
            {
                //parent = parentNode.Value;
                Debug.WriteLine($"[DOCUMENT] parent found after {iterationsNumber} iterations");
                return true;
            }
        }
        Debug.WriteLine("[DOCUMENT] WARNING | GetSeenParent: not found ]");
        return false;
    }

    /// <summary>
    /// Attempts to find the parent <see cref="DataBlock"/> of the specified child <see cref="DataBlock"/>.
    /// </summary>
    /// <remarks>A parent <see cref="DataBlock"/> is determined by finding the first ancestor in the linked
    /// list whose depth is less than the depth of the specified child <see cref="DataBlock"/>.</remarks>
    /// <param name="parent">When this method returns, contains the parent <see cref="DataBlock"/> of the specified child, if a parent is
    /// found; otherwise, <see langword="null"/>.</param>
    /// <param name="child">The child <see cref="DataBlock"/> for which to find the parent. This parameter is read-only.</param>
    /// <returns><see langword="true"/> if a parent <see cref="DataBlock"/> is found; otherwise, <see langword="false"/>.</returns>
    public static bool GetParent(ref DataBlock? parent, in DataBlock child)
    {
        int iterationsNumber = 0;
        LinkedListNode<DataBlock>? childNode = child.Node;
        for (var parentNode = childNode; parentNode != null; parentNode = parentNode.Previous)
        {
            iterationsNumber++;
            if (parentNode.Value.GetDepth() < child.GetDepth())
            {
                parent = parentNode.Value;
                Debug.WriteLine($"[DOCUMENT] parent found after {iterationsNumber} iterations");
                return true;
            }
        }
        Debug.WriteLine("[DOCUMENT] !!! GetParent: not found ]");
        return false;
    }

    public static bool HasSeenChild(in DataBlock parent, in DataBlock child)
    {
        int depth = parent.GetDepth();
        var firstChild = parent?.GetNextBlock();
        for (var b = firstChild; b != null; b = b.GetNextBlock())
        {
            //DataBlock b = node.Value;
            if (b.GetDepth() <= depth)
            {
                break;
            }
            if (b == child)
            {
                return true;
            }
        }
        return false;
    }

    /*
     * Check if 'child' is a child block of 'parent'.
     * Note: this function does not check if 'parent' is really a parent block (it may be any ancestor).
     * Note: this function does not check if 'child' is really a child block (it may be any descendant).
     * Note: this function does not check if 'parent' and 'child' belong to the same document.
     */
    /*
    public static bool HasChild(in DataBlock parent, in DataBlock child)
    {
        int depth = parent.GetDepth();
        LinkedListNode<DataBlock>? parentNode = parent.Node;
        var firstChildNode = parentNode?.Next;
        for (var node = firstChildNode; node != null; node = node.Next)
        {
            DataBlock b = node.Value;
            if (b.GetDepth() <= depth)
            {
                break;
            }
            if (b == child)
            {
                return true;
            }
        }
        return false;
    }
    */

    public static bool GetSeenChild(ref DataBlock? child, DataBlock? parent, BlockType type)
    {
        child = null;
        if (parent == null)
        {
            return false;
        }

        int depth = parent.GetDepth();
        var firstChild = parent?.GetNextBlock();
        for (var b = firstChild; b != null; b = b.GetNextBlock())
        {
            if (b.GetDepth() <= depth)
            {
                break;
            }
            if (b.GetBlockType() == type)
            {
                child = b;
                return true;
            }
        }
        return false;
    }

    public static bool GetChild(ref DataBlock? child, LinkedListNode<DataBlock>? parentNode, BlockType type)
    {
        child = null;
        DataBlock? parent = parentNode?.Value;
        if (parent == null)
        {
            return false;
        }

        int depth = parent.GetDepth();
        var firstChildNode = parentNode?.Next;
        for (var node = firstChildNode; node != null; node = node.Next)
        {
            DataBlock b = node.Value;
            if (b.GetDepth() <= depth)
            {
                break;
            }   
            if (b.GetBlockType() == type) {
                child = b;
                return true;
            }
        }
        return false;
    }
    public static bool GetNext(ref LinkedListNode<DataBlock>? node, BlockType type)
    {
        if (node == null)
        {
            return false;
        }
        int depth = node.Value.GetDepth();
        LinkedListNode<DataBlock>? currentNode = node.Next;
        while (currentNode != null)
        {
            if (type == currentNode.Value.GetBlockType())
            {
                node = currentNode;
                return true;
            }
            if (currentNode.Value.GetDepth() < depth)
            {
                break;
            }
            currentNode = currentNode.Next;
        }
        return false;
    }

    public static bool GetSeenCommands(ref DataBlock? commands, DataBlock? unit)
    {
        return GetSeenChild(ref commands, unit, BlockType.COMMANDS);
    }

    public static bool GetCommands(ref DataBlock? commands, LinkedListNode<DataBlock> unitNode)
    {
        //DataBlock? unit = unitNode?.Value;
        //FXASSERT(unit.Type == BlockType.UNIT);
        return GetChild(ref commands, unitNode, BlockType.COMMANDS);
    }

    public bool GetUnit(ref DataBlock? unit, int id)
    {
        unit = FindUnit(id);
        return unit != null;
    }
    public bool GetShip(ref DataBlock? ship, int id)
    {
        ship = FindShip(id);
        return ship != null;
    }
    public bool GetBuilding(ref DataBlock? building, int id)
    {
        building = FindBuilding(id);
        return building != null;
    }
    public bool GetGroup(ref DataBlock? group, int id)
    {
        group = FindGroup(id);
        return group != null;
    }

    public bool GetSeenRegion(ref DataBlock? region, in DataBlock block)
    {
        return FindRegionFromPosition(ref region, block.GetX(), block.GetY(), block.GetId());
    }

    public bool GetRegion(ref DataBlock @out, int x, int y, int plane)
    {
        return FindRegionFromPosition(ref @out, x, y, plane);
    }

    private DataBlock AddBlock(DataBlock block, LinkedListNode<DataBlock>? insertionNode = null)
    {
        if (block == null) 
        {
            return null;
        }

        block.SetNode(insertionNode != null ? Blocks.AddAfter(insertionNode, block) : Blocks.AddLast(block));
        return block;
    }

    protected void MergeBlock(DataBlock block, in DataBlock parent, ref DataBlock end)
    {
        // TODO: implement this
        /*
        int parentDepth = parent.GetDepth();
        BlockType blockType = block.GetBlockType();
        int info = block.GetId();

        DataBlock insert = std::next(parent);
        bool found = false;
        for (DataBlock child = insert; child != Globals.end; ++child)
        {
            if (child.GetDepth() == parentDepth)
            {
                Globals.end = child;
                break;
            }
            if (child.GetBlockType() == blockType)
            {
                if (insert.GetBlockType() != blockType)
                {
                    // TODO
                    //insert.CopyFrom(child);
                }
                if (child.GetId() == info)
                {
                    // we already have a newer report for this same block
                    found = true;
                    break;
                }
            }
        }
        if (!found)
        {
            // we do not have this kind of block
            // TODO
            //Blocks.insert(insert, *block);
        }
        */
    }

    /// <summary>
    /// Removes the visibility status from all blocks of type <see cref="BlockType.REGION"/>.
    /// </summary>
    /// <remarks>This method iterates through all blocks in the collection and removes the visibility key 
    /// from blocks identified as regions. Blocks of other types are unaffected.</remarks>
    public void RemoveTemporary()
    {
        foreach (var block in Blocks)
        {
            // Remove visibility status from older of the two reports
            if (block.GetBlockType() == BlockType.REGION)
            {
                block.RemoveKey(KeyType.VISIBILITY);
            }
        }
    }

    public void Merge(CRDocument old_cr, int x_offset = 0, int y_offset = 0)
    {
        /*
        // then: append file to the current CR (only map information)
        DataBlock old_end = old_cr.m_blocks.end();
        bool copy_children = false;
        for (DataBlock old_r = old_cr.m_blocks.begin(); old_r != old_end;)
        {
            // handle only regions
            if (old_r.type() == block_type.TYPE_REGION)
            {
                int x = old_r.x();
                int y = old_r.y();
                int plane = old_r.GetId();
                if (plane == 0)
                {
                    x += x_offset;
                    y += y_offset;
                }

                DataBlock new_r = new DataBlock();
                if (getRegion(ref new_r, x, y, old_r.GetId())) // add some info to old cr (island names)
                {
                    bool is_seen = !new_r.hasKey(KeyType.VISIBILITY);
                    copy_children = false;
                    if (const DataKey* islandkey = old_r.valueKey(KeyType.ISLAND))
					{
            if (!new_r.valueKey(KeyType.ISLAND))
            {
                if (islandkey && !islandkey.isInt()) // add only Vorlage-style islands (easier)
                {
                    new_r.addKey(*islandkey);
                }
            }
        }
        if (!is_seen)
        {
            // old region contained good data that we may want to keep
            foreach (DataKey key in old_r.data())
            {
                if (key.GetKeyType() == KeyType.NAME || key.GetKeyType() == KeyType.TERRAIN)
                {
                    continue;
                }
                if (!new_r.hasKey(key.GetKeyType()))
                {
                    new_r.addKey(key);
                }
            }
            // copy child blocks if we don't have them
            int depth = old_r.depth();
            DataBlock new_end = m_blocks.end();
            for (old_r++; old_r != old_end && old_r.type() != block_type.TYPE_REGION; ++old_r)
            {
                if (old_r.depth() == depth + 1)
                {
                    block_type type = old_r.type();
                    if (!CRDocument.isEphemeral(type))
                    {
                        mergeBlock(old_r, new_r, ref new_end);
                    }
                }
            }
            if (old_r == old_end)
            {
                break;
            }
            if (old_r.type() != block_type.TYPE_REGION)
            {
                ++old_r;
            }
        }
        else
        {
            ++old_r;
        }
        continue;
    }
				else // append region to this cr
				{
					if (x_offset != 0 || y_offset != 0)
					{
						if (old_r.GetId() == 0)
						{
							old_r.move(x_offset, y_offset);
						}
					}
					copy_children = true;
old_r.attachment(null);
m_blocks.push_back(*old_r);
				}
			}

            else if (copy_children)
{
    // part of a new region that is appended to the report, copy detail blocks
    switch (old_r.type())
    {
        case block_type.TYPE_BUILDING:
        case block_type.TYPE_PRICES:
        case block_type.TYPE_BORDER:
        case block_type.TYPE_RESOURCE:
            m_blocks.push_back(*old_r);
            break;
        default:
            break;
    }
}
++old_r;
		}
		createHashTables();
        if (old_cr.getActiveFactionId() != getActiveFactionId())
        {
            if (m_password.empty())
            {
                m_password.CopyFrom(old_cr.m_password);
            }
        }
        */
	}

    public string GetUnitName(in DataBlock unit, bool verbose = false)
    {
        if (verbose)
        {
            string factionName;
            int uid = unit.GetId();
            int fid = unit.ValueInt(KeyType.FACTION);
            FactionModel? ownerFaction = null;
            string unitName = unit.Value(KeyType.NAME);
            string id = Utils.Converters.IdToString(uid);
            string fidx = Utils.Converters.IdToString(fid);
            if (GetFaction(ref ownerFaction, fid))
            {
                factionName = ownerFaction.Name;
                //factionName = factionOwner!.Value(KeyType.FACTIONNAME);
            }
            else
            {
                factionName = "unknown";
            }
            return $"{unitName} ({id}), {factionName} ({fidx})";
        }
        return unit.GetUIName();
    }

    public DataBlock? GetMessageTarget(DataBlock msg)
    {
        DataBlock? unitTarget = null;
        DataBlock? shipTarget = null;
        DataBlock? buildingTarget = null;
        DataBlock? regionTarget = null;
        int unitId = 0;
        int shipId = 0;
        int buildingId = 0;
        string location = string.Empty;

        if (msg.GetAllReferences(ref unitId, ref shipId, ref buildingId, ref location))
        {
            if (unitId > 0 && GetUnit(ref unitTarget, unitId))
            {
                return unitTarget;
            }
            if (shipId > 0 && GetShip(ref shipTarget, shipId))
            {
                return shipTarget;
            }
            if (buildingId > 0 && GetBuilding(ref buildingTarget, buildingId))
            {
                return buildingTarget;
            }

            //return unitTarget ?? shipTarget ?? buildingTarget /*?? regionTarget*/;
        }

        if (!string.IsNullOrEmpty(location))
        {
            // LATER: Utils.Converters::ExtractCoordinates should be used
            var parts = location.Split(' ');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            int plane = int.Parse(parts[2]);
            if (FindRegionFromPosition(ref regionTarget, x, y, plane))
            {
                return regionTarget;
            }
        }

        return null;
    }

    public List<DataBlock> GetMessageTargets(DataBlock msg)
    {
        DataBlock? unitTarget = null;
        DataBlock? shipTarget = null;
        DataBlock? buildingTarget = null;
        DataBlock? regionTarget = null;
        int unitId = 0;
        int shipId = 0;
        int buildingId = 0;
        string location = string.Empty;

        List<DataBlock> targets = [];

        if (msg.GetAllReferences(ref unitId, ref shipId, ref buildingId, ref location))
        {
            if (unitId > 0 && GetUnit(ref unitTarget, unitId))
            {
                targets.Add(unitTarget!);
            }
            if (shipId > 0 && GetShip(ref shipTarget, shipId))
            {
                targets.Add(shipTarget!);
            }
            if (buildingId > 0 && GetBuilding(ref buildingTarget, buildingId))
            {
                targets.Add(buildingTarget!);
            }
        }

        if (!string.IsNullOrEmpty(location))
        {
            // LATER: Utils.Converters::ExtractCoordinates should be used
            var parts = location.Split(' ');
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            int plane = int.Parse(parts[2]);
            if (FindRegionFromPosition(ref regionTarget, x, y, plane))
            {
                targets.Add(regionTarget!);
            }
        }

        return targets;
    }

    /// <summary>
    /// Parse all messages for the active faction to compute for each region :
    /// - Incomes
    /// - Costs
    /// - Got taxes (old CR versions)
    /// </summary>
    private void ParseMessages()
    {
        int messagesWithoutRegionNumber = 0;
        int messagesNumber = 0;
        int incomeMessagesNumber = 0;
        int costMessagesNumber = 0;

        if (!HasActiveFaction())
        {
            return;
        }

        foreach (var faction in ActiveFactions.Values)
        {
            DataBlock? startBlock = null;
            // TODO: parse messages for each active faction
            if (!GetSeenChild(ref startBlock, faction.Data, BlockType.MESSAGE))
            {
                // No MESSAGE block (optimize mode)
                continue;
            }

            // Iterate through all messages in the active faction
            for (DataBlock? block = startBlock; block != null; block = block?.GetNextBlock())
            {
                if (block == null || block.GetBlockType() != BlockType.MESSAGE)
                {
                    break;
                }

                messagesNumber++;
                int type = block!.ValueInt(KeyType.MSG_TYPE);
                if (type != MESSAGE_TYPE_INCOME && type != MESSAGE_TYPE_COST)
                {
                    // Skip messages that are not related to income or costs
                    if (type != MESSAGE_TYPE_PASSWORD)
                    {
#if DEBUG
                        switch (type)
                        {
                            case MESSAGE_TYPE_UNIT_NOT_FOUND:
                            case MESSAGE_TYPE_RECEIVE_SOMETHING:
                            case MESSAGE_TYPE_GIVE_SOMETHING:
                            case MESSAGE_TYPE_TRANSFER_PERSONS:
                            case MESSAGE_TYPE_MAKE_SOMETHING:
                            case MESSAGE_TYPE_SELL_SOMETHING:
                            case MESSAGE_TYPE_FIND_SOMETHING:
                            case MESSAGE_TYPE_PAY_FOR_ITEMS:
                            case MESSAGE_TYPE_BUY_LUXURY_ITEMS:
                            case MESSAGE_TYPE_ROUTE_GUARD_ISSUE:
                            case MESSAGE_TYPE_MOVE:
                            case MESSAGE_TYPE_SAIL:
                            case MESSAGE_TYPE_REGENERATE_AUTA:
                                break;
                            default:
                                Debug.WriteLine($"[DOCUMENT] message type {type} unknown and ignored.");
                                break;
                        }
#endif
                        if (type == MESSAGE_TYPE_SAIL)
                        {
                            // LATER: parse sail messages to add ship movement info
                        }
                        continue;
                    }

                    // faction password message
                    // LATER: get faction password value
                    //m_password = block.Value(Strings.EN_MESSAGE_PASSWORD);
                }

                List<DataKey> list = block.GetData();
                int amount = 0;
                int mode = 0;
                DataBlock? messageRegion = null;

                // Parse the message data keys to find the region (by its coordinates), amount and type of cost
                foreach (var dataKey in list)
                {
                    KeyType key = dataKey.GetKeyType();
                    if (key == KeyType.MSG_REGION)
                    {
                        if (Utils.Converters.ExtractCoordinates(dataKey.GetValue(), out int x, out int y, out int plane))
                        {
                            if (!FindRegionFromPosition(ref messageRegion, x, y, plane))
                            {
                                // if no region, not needed continuing iteration
                                break;
                            }
                        }
                    }
                    else if (key == KeyType.MSG_AMOUNT)
                    {
                        amount = dataKey.GetInt();
                    }
                    else if (key == KeyType.MSG_COST)
                    {
                        amount = dataKey.GetInt();
                    }
                    else if (key == KeyType.MSG_MODE)
                    {
                        mode = dataKey.GetInt();
                    }
                }

                if (messageRegion != null)
                {
                    if (mode < (int)Income.Kind.MISC || mode >= (int)Income.Kind.MAX)
                    {
                        mode = (int)Income.Kind.MISC;
                    }

                    RegionAttachment attachment = messageRegion.GetAttachment() as RegionAttachment;
                    if (type == MESSAGE_TYPE_COST)
                    {
                        costMessagesNumber++;
                        // Ausgaben fuer teure Talente (+Akademie)
                        if (attachment != null)
                        {
                            attachment.LearnCost += amount;
                        }
                        else
                        {
                            //Debug.WriteLine($"[DOCUMENT] No attachment for region {messageRegion.GetId()} to add learn cost.");
                        }
                    }
                    else // Einnahmen
                    {
                        incomeMessagesNumber++;
                        attachment?.AddIncome((Income.Kind)mode, amount);
                    }
                }
                else
                {
                    messagesWithoutRegionNumber++;
                }
            }
        }
        if (messagesWithoutRegionNumber > 0)
        {
            Debug.WriteLine($"[DOCUMENT] {messagesWithoutRegionNumber} messages are not related to a region.");
        }

        Debug.WriteLine($"[DOCUMENT] {incomeMessagesNumber} income messages. {costMessagesNumber} cost messages.");
        Debug.WriteLine($"[DOCUMENT] {messagesNumber} total messages. {messagesWithoutRegionNumber} 'without region' messages.");
    }

    private void CreateHashTables()
    {
        ContentSizes.Clear();
        CreateHierarchy();
        UpdateHashTables();
        FloodIslandNames();
    }

    /// <summary>
    /// Iterate from first data block until active faction is found.
    /// While iterating, store global report settings like turn number, program build version, cr locale.
    /// </summary>
    /// <param name="currentNode">start node to iterate from. returns as the first active faction node</param>
    /// <returns>true if the active faction has been found; otherwise false.</returns>
    private bool CollectGeneralGameData(out LinkedListNode<DataBlock>? firstActiveFactionNode)
    {
        // LATER: set Locale and Version with their initial default values
        Turn = 0;
        firstActiveFactionNode = null;
        LinkedListNode<DataBlock>?  currentNode = Blocks.First;
        while (currentNode != null)
        {
            DataBlock b = currentNode.Value;
            BlockType t = b.GetBlockType();
            // set turn number to that found in version block
            if (t == BlockType.VERSION)
            {
                Turn = b.ValueInt(KeyType.TURN, Turn);
                Version = b.Value(Strings.EN_VERSION_BUILD);
                Locale = b.Value(Strings.EN_VERSION_LOCALE).ToLocaleType();
            }
            else if (t == BlockType.FACTION)
            {
                if (AddFaction(b, b.GetId(), out var factionModel))
                {
                    if (factionModel!.IsActive)
                    {
                        // get turn from faction block if VERSION block has not
                        if (Turn == 0)
                        {
                            Turn = b.ValueInt(KeyType.TURN, Turn);
                        }
                        // currentNode is the active faction node
                        firstActiveFactionNode = currentNode;
                        ActiveFaction = factionModel!;
                        return true;
                    }
                }
            }
            currentNode = currentNode.Next;
        }
        return false;
    }

    /// <summary>
    /// Indicates if the specified faction DataBlock is an active faction.
    /// A faction is considered as active when it has options set and has some people.
    /// </summary>
    public static bool FactionIsActive(DataBlock factionBlock)
    {
        string option = factionBlock.Value(KeyType.OPTIONS);
        int factionPeople = factionBlock.ValueInt(Strings.DE_FACTION_PEOPLE_NUMBER, -1);
        // An active faction must have some people (to be fixed in CxMapFx EN and DE)
        return !string.IsNullOrEmpty(option) && factionPeople != -1;
    }

    /// <summary>
    /// What factions do we have HELP status set to?
    /// </summary>
    /// <param name="currentNode">node to iterate from. modified as the active faction node</param>
    /// <returns></returns>
    private Dictionary<int, int> CollectAlliedStatus(ref LinkedListNode<DataBlock>? currentNode)
    {
        Dictionary<int, int> alliedStatus = [];
        if (currentNode != null)
        {
            int factionDepth = currentNode.Value.GetDepth();
            currentNode = currentNode.Next;
            while (currentNode != null)
            {
                var b = currentNode.Value;
                if (b.GetDepth() <= factionDepth)
                {
                    break;
                }
                if (b.GetBlockType() == BlockType.ALLIANCE)
                {
                    alliedStatus[b.GetId()] = b.ValueInt(Strings.EN_ALLIANCE_STATUS, 0);
                }
                currentNode = currentNode.Next;
            }
        }
        return alliedStatus;
    }

    private static bool IsknownRegion(DataBlock region) {
        if (region.GetAttachment() is RegionAttachment ra)
        {
            return ra.People.Count > 0;
        }
        return false;
    }

    private void UpdateHashTables()
    {
        if (!CollectGeneralGameData(out var firstActiveFactionNode))
        {
            Debug.WriteLine("[DOCUMENT] ERROR ! There is no active faction.");
            return;
        }

        Dictionary<int, DataBlock> ships = [];
        Dictionary<int, List<DataBlock>> shipsCrossedRegions = [];

        FactionModel currentActiveFaction = ActiveFaction!;

        LinkedListNode<DataBlock>? currentNode = firstActiveFactionNode;
        // Continue to evaluate ALLIANCE blocks for active faction
        Dictionary<int, int> alliedStatus = CollectAlliedStatus(ref currentNode);
        // TODO: maybe insertFactionNode should be after the ALLIIANZ blocks
        LinkedListNode<DataBlock>? insertFactionNode = currentNode;
        DataBlock? region = null;
        int unconfirmed = 0;
        int regionOwn = 0;
        int regionAlly = 0;
        int regionEnemy = 0;
        // Units that got taxes (MSG id 1264208711); the regions will get a coins icon
        HashSet<int> unitGotTaxes = [];

        int nbSeenRegions = 0;
        int nbVisibilityLighthouse = 0;
        int nbVisibilityTraveler = 0;
        int nbVisibilityNeighbours = 0;
        int nbVisibilityEmpty = 0;
        int nbVisibilityOther = 0;
        LinkedListNode<DataBlock>? blockNodeToChange = null;
        LinkedListNode<DataBlock>? previousNode = null;
        DataBlock? firstSeenRegion = null;
        DataBlock? lastSeenRegion = null;

        // Iterate through all blocks from current node
        for (var node = currentNode; node != null; node = node.Next)
        {
            DataBlock b = node.Value;
            BlockType btype = b.GetBlockType();
            int blockId = b.GetId();
            switch (btype)
            {
                case BlockType.BATTLE:
                    // Add battle to list
                    Battles[(int)new Coordinates(b.GetX(), b.GetY(), blockId)] = b;
                    break;
                case BlockType.REGION:
                    // Add region to region list
                    if (region != null)
                    {
                        if (SetRegionStats(region, regionOwn, regionAlly, regionEnemy, unconfirmed))
                        {
                            AddSeenRegion(region, OptimizeDataConsuming ? blockNodeToChange : null);
                            nbSeenRegions++;
                            if (firstSeenRegion == null)
                            {
                                firstSeenRegion = region;
                            }
                            lastSeenRegion = region;
                            blockNodeToChange = null;
                        }
                        else
                        {
                            if (blockNodeToChange == null)
                            {
                                blockNodeToChange = region.Node?.Previous;
                            }
                        }
                    }
                    region = b;
                    // unset all flags except BLOCKID flags
                    region.SetFlags(region.GetFlags() & ((int)Flag.BLOCKID_BIT0 | (int)Flag.BLOCKID_BIT1));
                    unconfirmed = regionOwn = regionAlly = regionEnemy = 0;

                    String visibility = b.Value(KeyType.VISIBILITY);
                    if (visibility == Strings.EN_REGION_VISIBILITY_VALUE_LIGHTHOUSE)
                    {
                        // region is seen by lighthouse
                        region.AddFlags((int)Flag.LIGHTHOUSE);
                        nbVisibilityLighthouse++;
                    }
                    else if (visibility == Strings.EN_REGION_VISIBILITY_VALUE_TRAVEL)
                    {
                        // region is seen by traveling through
                        region.AddFlags((int)Flag.TRAVEL);
                        nbVisibilityTraveler++;
                    }
                    else
                    {
                        if (visibility == Strings.EN_REGION_VISIBILITY_VALUE_NEIGHBOUR)
                        {
                            nbVisibilityNeighbours++;
                        }
                        else if (visibility == "")
                        {
                            nbVisibilityEmpty++;
                        }
                        else
                        {
                            nbVisibilityOther++;
                            Debug.WriteLine($"[DOCUMENT] Unknown visibility value '{visibility}' for region {region} ({region.GetId()})");
                        }
                    }

                    Regions[(int)new Coordinates(b.GetX(), b.GetY(), blockId)] = b;
                    // get region owner (E3 only)
                    int ownerFactionId = b.ValueInt(Strings.EN_REGION_OWNER, -1);
                    // TODO: check if -1 can be a region owner (monster...), because in that case it could be a faction with -1 id
                    if (ownerFactionId != -1)
                    {
                        Debug.WriteLine($"[DOCUMENT] region {region} ({region.GetId()}) owner faction id: {ownerFactionId}");

                        // All factions were loaded before any region, so we can check owner now
                        FactionModel? ownerFactionModel = Factions.GetValueOrDefault(ownerFactionId);
                        if (ownerFactionModel is not null)
                        {
                            if (ownerFactionModel.IsActive)
                            {
                                region.AddFlags((int)Flag.REGION_OWN);
                            }
                            else
                            {
                                if (ownerFactionId != -1)
                                {
                                    // TODO: change alliedStatus management for multi active factions
                                    if ((alliedStatus[ownerFactionId] & HELP_GUARD) != 0)
                                    {
                                        region.AddFlags((int)Flag.REGION_ALLY);
                                    }
                                    else
                                    {
                                        region.AddFlags((int)Flag.REGION_ENEMY);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // No trace, because it's a usual case (no real owner, but a guard unit is defined)
                            //Debug.WriteLine($"[DOCUMENT] no owner faction  for region {region} ({region.GetId()})");
                        }
                    }
                    break;

                case BlockType.SHIP:
                    // add ships to their list
                    // TODO
                    //ships[blockId] = b;
                    Ships[blockId] = b;
                    /*
                    // record ship final region position (region is the parent region variable)
                    if (region != null)
                    {
                        if (!shipsFinalPosition.TryGetValue(blockId, out var finalList))
                        {
                            finalList = new List<DataBlock>();
                            shipsFinalPosition[blockId] = finalList;
                        }
                        finalList.Add(region);
                    }
                    */
                    break;

                case BlockType.BUILDING:
                    // add buildings to their list
                    Buildings[blockId] = b;
                    break;

                case BlockType.MESSAGE:
                    // generate list of units that got a "got taxes" message (E3 only)
                    int type = b.ValueInt(KeyType.MSG_TYPE);
                    if (type == MESSAGE_TYPE_GOT_TAXES)
                    {
                        int unitId = b.ValueInt(Strings.EN_MESSAGE_UNIT, -1);
                        unitGotTaxes.Add(unitId);
                    }
                    break;

                case BlockType.GROUP:
                    // add units to unit list
                    Groups[blockId] = b;
                    break;

                case BlockType.UNIT:
                    Units[blockId] = b;
                    int factionId = b.ValueInt(KeyType.FACTION, (int)SpecialFaction.ANONYMOUS);
                    if (!factionId.IsKnownFaction())
                    {
                        // Unknown faction is a monster or a disguised unit; a mountain guard (Bergwächter)...
                        Debug.WriteLine($"[DOCUMENT] {b} [{blockId}] does not belong to a known faction {factionId}");
                    }
                    if (!HasFactionWithId(factionId))
                    {
                        // TODO: check where it's optimal to insert a new faction block
                        string factionIdStr = Utils.Converters.IntToString(factionId);
                        DataBlock faction = new();
                        faction.SetTypeLabel("PARTEI");
                        faction.SetId(factionIdStr);
                        string factionName = factionId.IsMonster() ? "Monster" : $"Partei {factionIdStr}";
                        faction.AddKey(new DataKey(DataKey.GetTypeValue(KeyType.FACTIONNAME, false), btype, factionName));
                        AddBlock(faction, insertFactionNode);
                        Factions[factionId] = new FactionModel(faction);
                    }
                    else
                    {
                        if (ActiveFactions.ContainsKey(factionId))
                        {
                            // set attachment for unit of active factions
                            DataBlock? orders = null;
                            // at that moment optimization is partial so not useful to use GetSeenCommands 
                            if (GetCommands(ref orders, node))
                            {
                                // add orders to command block
                                OrdersAttachment? att = orders!.GetAttachment() as OrdersAttachment;
                                att?.Add(orders.GetData());
                            }
                        }
                        else
                        {

                        }
                    }
                    break;

                case BlockType.FACTION:
                    if (AddFaction(b, blockId, out var factionModel))
                    {
                        Debug.WriteLine($"[DOCUMENT] Added faction {factionModel}.");
                        if (factionModel!.IsActive)
                        {
                            currentActiveFaction = factionModel;
                            // reset allied status for new active faction
                            alliedStatus = [];
                        }
                    }
                    break;

                case BlockType.ALLIANCE:
                    // alliance as placeholder-faction
                    currentActiveFaction.AddAlliance(b, blockId);
                    // TODO: add status to Alliance model in AddAlliance method
                    alliedStatus[b.GetId()] = b.ValueInt(Strings.EN_ALLIANCE_STATUS, 0);
                    break;

                case BlockType.ISLAND:
                    // add islands to island list
                    Islands[blockId] = b;
                    break;

                default: break;
            }

            if (region is not null)
            {
                if (btype == BlockType.UNIT)
                {
                    DataBlock unitPtr = b;
                    // region has units
                    region.AddFlags((int)Flag.TROOPS);

                    // count people
                    int peopleNumber = b.ValueInt(KeyType.NUMBER, 0);
                    OwnerType owner = OwnerType.ENEMY;

                    int factionId = GetFactionIdForUnit(unitPtr);
                    if (factionId > 0)
                    {
                        if (ActiveFactions.ContainsKey(factionId))
                        {
                            regionOwn += peopleNumber;
                            peopleNumber = 0;
                            owner = OwnerType.OWN;
                            if (!IsConfirmed(b))
                            {
                                ++unconfirmed;
                            }
                        }
                        else if (alliedStatus.ContainsKey(factionId))
                        {
                            regionAlly += peopleNumber;
                            owner = OwnerType.ALLY;
                            peopleNumber = 0;
                        }
                        else
                        {
                            // enemy unit
                        }
                    }

                    regionEnemy += peopleNumber;

                    if (b.ValueInt(Strings.DE_UNIT_IS_GUARDING) == 1)
                    {
                        if (owner == OwnerType.OWN)
                        {
                            region.AddFlags((int)Flag.GUARD_OWN);
                        }
                        else if (owner == OwnerType.ALLY)
                        {
                            region.AddFlags((int)Flag.GUARD_ALLY);
                        }
                        else
                        {
                            region.AddFlags((int)Flag.GUARD_ENEMY);
                        }
                    }

                    if (unitGotTaxes.Contains(blockId))
                        region.SetFlags((int)Flag.REGION_TAXES);

                    int unitFactionId = b.ValueInt(KeyType.FACTION, -1);
                    if (unitFactionId.IsMonster())
                    {
                        region.AddFlags((int)Flag.MONSTER);
                        string typeName = b.Value(KeyType.TYPE);
                        Debug.WriteLine($"[DOCUMENT] Monster {b} of type {typeName} on {region}");
                    }
                    else if (unitFactionId == -1)
                    {
                        // disguised units, monsters, sea snakes or dragons
                        string typeName = b.Value(KeyType.TYPE);
                        // LATER: check if other monsters names should be added
                        if (typeName == Strings.DE_MONSTER_TYPE_VALUE_SKELETTE ||
                            typeName == Strings.DE_MONSTER_TYPE_VALUE_SKELETTHERREN ||
                            typeName == Strings.DE_MONSTER_TYPE_VALUE_ZOMBIES ||
                            typeName == Strings.DE_MONSTER_TYPE_VALUE_JUJU_ZOMBIES ||
                            typeName == Strings.DE_MONSTER_TYPE_VALUE_GHOULE ||
                            typeName == Strings.DE_MONSTER_TYPE_VALUE_GHASTE ||
                            typeName == Strings.DE_MONSTER_TYPE_VALUE_ENTS ||
                            typeName == Strings.DE_MONSTER_TYPE_VALUE_BAUERN ||
                            typeName == Strings.DE_MONSTER_TYPE_VALUE_HIRNTOTER)
                        {
                            // monsters in the region
                            region.AddFlags((int)Flag.MONSTER);
                        }
                        else if (typeName == Strings.DE_UNIT_TYPE_NAME_VALUE_SEA_SNAKE)
                        {
                            // sea snake in region
                            region.AddFlags((int)Flag.SEASNAKE);
                        }
                        else
                        {
                            if (typeName == Strings.DE_UNIT_TYPE_NAME_VALUE_YOUNG_DRAGON ||
                                typeName == Strings.DE_UNIT_TYPE_NAME_VALUE_DRAGON ||
                                typeName == Strings.DE_UNIT_TYPE_NAME_VALUE_DRAGON_WYRM)
                            {
                                // a dragon is in the region
                                region.AddFlags((int)Flag.DRAGON);
                            }
                            else
                            {
                                Debug.WriteLine($"[DOCUMENT] Disguised unit {b} with no faction and unknown type '{typeName}' on {region}");
                            }
                        }
                    }
                }
                else if (btype == BlockType.DURCHSCHIFFUNG)
                {
                    // a boat traveled through this region (ocean)
                    region.AddFlags((int)Flag.SHIPTRAVEL);
                    // get ship id from first data key information, which is "<name> (<id>)"
                    var dk = b.GetData().FirstOrDefault();
                    if (dk != null)
                    {
                        int shipId = DataBlock.ExtractId(dk.GetValue());
                        if (shipId > 0)
                        {
                            // record the region (where DURCHSCHIFFUNG occurred) in ShipTravels
                            if (!shipsCrossedRegions.TryGetValue(shipId, out var travelList))
                            {
                                travelList = new List<DataBlock>();
                                shipsCrossedRegions[shipId] = travelList;
                            }
                            // store the region (not the DURCHSCHIFFUNG child) so sorting by coordinates is straightforward
                            if (region != null)
                            {
                                travelList.Add(region);
                            }
                        }
                    }
                    int shipÎd = DataBlock.ExtractId(b.GetData().First().GetValue());
                }
                else if (btype == BlockType.BUILDING)
                {
                    // region has a building
                    // TODO: why CASTLE and not specific building flag ?
                    region.AddFlags((int)Flag.CASTLE);
                    if (b.Value(KeyType.TYPE) == Strings.DE_REGION_BUILDING_VALUE_WORMHOLE)
                    {
                        // a wormhole is in the region
                        region.AddFlags((int)Flag.WORMHOLE);     
                    }
                }
                else if (btype == BlockType.SHIP)
                {
                    // region has ships inside
                    region.AddFlags((int)Flag.SHIP);
                }
                else if (btype == BlockType.BORDER)
                {
                    string type = b.Value(Strings.DE_BORDER_TYPE);
                    if (type == Strings.DE_REGION_BORDER_TYPE_VALUE_ROAD_DIGRAPH || type == Strings.DE_REGION_BORDER_TYPE_VALUE_ROAD)
                    {
                        // region has street in some direction
                        int direction = b.ValueInt(Strings.DE_REGION_ROAD_DIRECTION);
                        if (direction >= 0 && direction <= 5)
                        {
                            if (b.ValueInt(Strings.DE_ROAD_PERCENT) == 100)
                            {
                                // region has a complete street
                                region.AddFlags((int)Flag.STREET << direction);
                            }
                            else
                            {
                                // region has an incomplete street
                                region.AddFlags((int)Flag.STREET_UNDONE << direction);
                            }
                        }
                    }
                }
            }
            previousNode = node;
        }

        if (region != null) {
            if (SetRegionStats(region, regionOwn, regionAlly, regionEnemy, unconfirmed))
            {
                AddSeenRegion(region, OptimizeDataConsuming ? blockNodeToChange : null);
                nbSeenRegions++;
            }
            else
            {
                if (OptimizeDataConsuming && blockNodeToChange != null)
                {
                    blockNodeToChange.Value.SetNextBlock(previousNode?.Value);
                    previousNode?.Value.SetPreviousBlock(blockNodeToChange.Value);
                }
            }
        }

        // Just to check data consuming optimization is ok
        if (OptimizeDataConsuming) {
            DataBlock? previousBlock = null;
            for (var block = firstSeenRegion; block != null; block = block.GetNextBlock())
            {
                if (block.GetBlockType() == BlockType.REGION)
                { 
                    if (!IsknownRegion(block))
                    {
                        Debug.WriteLine($"[DOCUMENT] WARNING | {block} unknown region found. It means there is a bug in data consuming optimization.");
                    }
                }
                previousBlock = block;
            }
        }

        SeenRegionsNumber = nbSeenRegions;

        // TODO: fill Ships dictionary from ships and shipsCrossedRegions
        /*
        foreach (var shipId in shipsCrossedRegions.Keys.ToList())
        {
            var intermediates = shipsCrossedRegions[shipId];
            if (intermediates == null || intermediates.Count == 0)
                continue;

            // Determine start region: prefer final position if available, otherwise pick an arbitrary start
            DataBlock? startRegion = null;
            if (shipsFinalPosition.TryGetValue(shipId, out var finals) && finals != null && finals.Count > 0)
            {
                startRegion = finals[0];
            }
            else
            {
                startRegion = intermediates[0];
            }

            var ordered = OrderRegionsChain(startRegion, intermediates);
            shipsCrossedRegions[shipId] = ordered;
        }
        */

        Debug.WriteLine($"[DOCUMENT] AllRegions number : {Regions.Count} ");
        Debug.WriteLine($"[DOCUMENT] Seen regions number : {SeenRegionsNumber} ");
        Debug.WriteLine($"[DOCUMENT] Factions number : {Factions.Count} ");
        Debug.WriteLine($"[DOCUMENT] Active factions number : {ActiveFactions.Count} ");
        Debug.WriteLine($"[DOCUMENT] Units number : {Units.Count} ");
        Debug.WriteLine($"[DOCUMENT] Ships number : {Ships.Count} ");
        Debug.WriteLine($"[DOCUMENT] Buildings number : {Buildings.Count} ");
        Debug.WriteLine($"[DOCUMENT] Battles number : {Battles.Count} ");
        Debug.WriteLine($"[DOCUMENT] Islands number : {Islands.Count} ");
        Debug.WriteLine($"[DOCUMENT] Groups number : {Groups.Count} ");
    }

    /// <summary>
    /// Convert axial (q, r) to cube coordinates (x, y, z).
    /// </summary>
    private static (int x, int y, int z) AxialToCube(int q, int r)
    {
        int x = q;
        int z = r;
        int y = -x - z;
        return (x, y, z);
    }

    /// <summary>
    /// Hex distance between two axial coordinates (pointy-top axial system).
    /// </summary>
    private static int HexDistance((int q, int r) a, (int q, int r) b)
    {
        var ca = AxialToCube(a.q, a.r);
        var cb = AxialToCube(b.q, b.r);
        return Math.Max(Math.Abs(ca.x - cb.x), Math.Max(Math.Abs(ca.y - cb.y), Math.Abs(ca.z - cb.z)));
    }

    /// <summary>
    /// Order regions by chaining neighbours starting from startRegion. If no direct neighbour is found,
    /// chooses the nearest remaining region (greedy fallback).
    /// </summary>
    private static List<DataBlock> OrderRegionsChain(DataBlock? startRegion, List<DataBlock> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return new List<DataBlock>();

        // copy candidates to mutable list
        var remaining = new List<DataBlock>(candidates);
        var ordered = new List<DataBlock>();

        // neighbor offsets for pointy-topped axial coords (q,r)
        int[,] offsets = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 }, { 1, -1 }, { -1, 1 } };

        // start with provided startRegion if not null
        if (startRegion != null)
        {
            ordered.Add(startRegion);
            // remove any candidate that matches startRegion by coordinates and plane
            remaining.RemoveAll(r => r.GetX() == startRegion.GetX() && r.GetY() == startRegion.GetY() && r.GetId() == startRegion.GetId());
        }

        while (remaining.Count > 0)
        {
            DataBlock? last = ordered.Count > 0 ? ordered[^1] : null;
            DataBlock? next = null;

            if (last != null)
            {
                int lx = last.GetX(), ly = last.GetY(), lplane = last.GetId();
                // find a direct neighbour in same plane
                foreach (var candidate in remaining)
                {
                    if (candidate.GetId() != lplane) continue;
                    int cx = candidate.GetX(), cy = candidate.GetY();
                    for (int i = 0; i < 6; i++)
                    {
                        if (cx == lx + offsets[i, 0] && cy == ly + offsets[i, 1])
                        {
                            next = candidate;
                            break;
                        }
                    }
                    if (next != null) break;
                }
            }

            if (next == null)
            {
                // fallback: choose nearest by hex distance to last (or arbitrary if last null)
                if (last == null)
                {
                    next = remaining[0];
                }
                else
                {
                    int bestDist = int.MaxValue;
                    foreach (var candidate in remaining)
                    {
                        if (candidate.GetId() != last.GetId()) continue;
                        int d = HexDistance((last.GetX(), last.GetY()), (candidate.GetX(), candidate.GetY()));
                        if (d < bestDist)
                        {
                            bestDist = d;
                            next = candidate;
                        }
                    }
                    // if still null (e.g., different planes), pick any remaining
                    next ??= remaining[0];
                }
            }

            ordered.Add(next);
            remaining.Remove(next);
        }

        return ordered;
    }

    private void AddSeenRegion(DataBlock region, LinkedListNode<DataBlock>? previousNode)
    {
        if (previousNode?.Value != null)
        {
            region.SetPreviousBlock(previousNode.Value);
            previousNode.Value.SetNextBlock(region);
        }
    }

    private void FloodIslandNames()
    {
        // LATER: check if it's useful (where is islands usage?)

        // regions whose island names flood the island
        List<DataBlock> floodislands = new List<DataBlock> { };
        foreach (var r in Regions)
        {
            DataBlock b = r.Value;
            if (b.GetBlockType() != BlockType.REGION)
            {
                continue;
            }

            // get regions name
            DataKey? islandKey = b.ValueKey(KeyType.ISLAND);
            if (islandKey != null)
            {
                string name = string.Empty;
                int islandId = Utils.Converters.StringToInt(islandKey.GetValue());

                if (islandId > 0)
                {
                    // Magellan-style: integer-Island-tags and ISLAND blocks with names
                    DataBlock? island = Island(islandId);
                    if (island != null)
                    {
                        name = island.Value(KeyType.NAME);
                    }
                }
                else
                {
                    // Vorlage-style: string-Island-tags that flood the island
                    name = islandKey.GetValue();
                    // Vorlage-style floods the islands
                    floodislands.Add(b);
                }

                if (!string.IsNullOrEmpty(name))
                {
                    RegionAttachment? stats = b.GetAttachment() as RegionAttachment;
                    stats!.Island = name;
                }
            }

        }

        // Flood island names. add regions that get a name to list so that they also flood.
        int[,] offsets = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 }, { 1, -1 }, { -1, 1 } };

        foreach (var b in floodislands)
        {
            string name = "";
            RegionAttachment? stats = b.GetAttachment() as RegionAttachment;
            if (stats != null)
            {
                name = stats.Island;
            }
            int x = b.GetX();
            int y = b.GetY();
            int z = b.GetId();

            // get neighbours
            for (int i = 0; i < 6; i++)
            {
                int nx = x + offsets[i, 0];
                int ny = y + offsets[i, 1];
                DataBlock? neighbour = FindRegionFromPosition(nx, ny, z);
                if (neighbour != null)
                {
                    // Only flood to "Festland"
                    if (neighbour.GetTerrain() == Terrains.OCEAN ||
                        neighbour.GetTerrain() == Terrains.MAHLSTROM ||
                        neighbour.GetTerrain() == Terrains.FIREWALL)
                        continue;

                    RegionAttachment? neighbourStats = neighbour.GetAttachment() as RegionAttachment;
                    if (string.IsNullOrEmpty(neighbourStats!.Island)) {
                        neighbourStats.Island = name;
                        floodislands.Add(neighbour);
                    }
                }
            }
        }
    }

    private bool AddFaction(DataBlock data, int id, out FactionModel? factionModel)
    {
        if (Factions.ContainsKey(id))
        {
            factionModel = Factions[id];
        }
        else
        {
            factionModel = new FactionModel(data);
            Factions[id] = factionModel;
        }
        if (factionModel.IsActive)
        {
            ActiveFactions[id] = factionModel;
        }
        return true;
    }

    private bool HasFactionWithId(int id)
    {
        return Factions.ContainsKey(id);
    }

    private static int BarHeight2(int people)
    {
        return (int)(Math.Log2(people * 4 + 1));
    }

    private static bool SetRegionStats(DataBlock region, int ownNumber, int allyNumber, int enemyNumber, int unconfirmed)
    {
        int ownNumberLogarithm = BarHeight2(ownNumber);
        int allyNumberLogarithm = BarHeight2(allyNumber);
        int enemyNumberLogarithm = BarHeight2(enemyNumber);
        bool hasPeople = false;

        // if everything is zero, it means this is an unknown region (no people)
        if (ownNumberLogarithm > 0 || allyNumberLogarithm > 0 || enemyNumberLogarithm > 0 || unconfirmed > 0)
        {
            // GetAttachment creates a RegionAttachment if not existing
            if (region.GetAttachment() is RegionAttachment stats)
            {
                // generate new style flag information. log_2(people) = 1 to 13
                stats.Unconfirmed = unconfirmed;
                // Increase the size of the People list as wanted
                stats.People.Capacity = Math.Max(stats.People.Capacity, enemyNumberLogarithm > 0 ? 3 : 2);
                stats.People.Add(ownNumberLogarithm / 13.0f);
                stats.People.Add(allyNumberLogarithm / 13.0f);
                if (enemyNumberLogarithm > 0)
                {
                    stats.People.Add(enemyNumberLogarithm / 13.0f);
                }
            }
            hasPeople = true;
        }
        region.AddFlags(hasPeople ? (int)Flag.REGION_SEEN : 0);
        return hasPeople;
    }

    public override string ToString()
    {
        return Name;
    }
}
