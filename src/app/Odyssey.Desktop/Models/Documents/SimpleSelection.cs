//using Odyssey.Core.Services;
using Odyssey.Models.Data;
using System;
using System.Diagnostics;

namespace Odyssey.Models.Documents;

public class SimpleSelection : ISelection
{
    public RegionDataBlock? Region { get; set; }

    public DataBlock? Faction { get; set; }

    public DataBlock? Item { get; set; }

    public ItemTypes ItemType { get; protected set; }

    //protected readonly IDocumentService _documentService;

    // selected flags
    [Flags]
    public enum ItemTypes
    {
        None = 0,
        // Region marked, region block is valid
        REGION = (1 << 0),
        // Unknown region selected, sel_(x|y|plane) is valid
        UNKNOWN_REGION = (1 << 1),
        FACTION = (1 << 2),
        BUILDING = (1 << 3),
        SHIP = (1 << 4),
        UNIT = (1 << 5),
        // Confirmation status of the current unit has changed, update widgets that care
        CONFIRMATION = (1 << 6)
    };

    public SimpleSelection(ILocation location)
    {
        Clear();
        // TODO: check if an ILocation can be a BattleDataBlock
        RegionDataBlock r = (RegionDataBlock)location;
        SetItem(r, r, null, null);
    }

    public SimpleSelection(DataBlock? item, RegionDataBlock? region, DataBlock? faction, ILocationResolver? regionResolver = null)
    {
        Clear();
        SetItem(item, region, faction, regionResolver);
    }

    public SimpleSelection()
    {
        Clear();
    }

    /*
    public SimpleSelection(IDocumentService documentService)
    {
        _documentService = documentService;
        Clear();
    }
    */

    public SimpleSelection(ILocationResolver locationResolver, int positionX, int positionY, int plane)
    {
        Clear();
        if (locationResolver.ResolveLocation(positionX, positionY, (PlaneType)plane, out ILocation? location))
        {
            SetItem(location as RegionDataBlock, null, null);
        }
    }

    public bool SetItem(DataBlock? item, RegionDataBlock? region, DataBlock? faction, ILocationResolver? regionResolver = null)
    {
        DataBlock? previousItem = Item;
        // TODO: it should return false when it does not change the selection compared with the previously one
        RegionDataBlock? r = region;
        DataBlock? f = faction;
        BlockType newBlockType = item is not null ? item.GetBlockType() : BlockType.UNKNOWN;
        switch (newBlockType)
        {
            case BlockType.REGION:
                ItemType |= ItemTypes.REGION;
                Item = item;
                r = item as RegionDataBlock;
                break;
            case BlockType.FACTION:
                ItemType |= ItemTypes.FACTION;
                Item = item;
                f = item;
                break;
            case BlockType.UNIT:
                ItemType |= ItemTypes.UNIT;
                Item = item;
                break;
            case BlockType.BUILDING:
                ItemType |= ItemTypes.BUILDING;
                Item = item;
                break;
            case BlockType.SHIP:
                ItemType |= ItemTypes.SHIP;
                Item = item;
                break;
            case BlockType.UNKNOWN:
                break;
            default:
                Debug.WriteLine($"[SIMPLE-SEL] !!! new selected item block has an unexpected type {newBlockType} !!!");
                break;
        }

        bool regionChanged = SetRegion(r, regionResolver);
        bool factionChanged = SetFaction(f);
        bool selectionChanged = previousItem != Item /*|| regionChanged || factionChanged*/;

        Debug.WriteLine($"[SIMPLE-SEL] selectionChanged = {selectionChanged} regionChanged = {regionChanged} factionChanged = {factionChanged}");
        
        return selectionChanged;
    }

    private bool SetRegion(RegionDataBlock? region, ILocationResolver? regionResolver = null)
    {
        DataBlock? previousRegion = Region;
        BlockType newBlockType = region is not null ? region.GetBlockType() : BlockType.UNKNOWN;
        if (newBlockType == BlockType.REGION)
        {
            ItemType |= ItemTypes.REGION;
            Region = region;
        }
        else
        {
            if (regionResolver is not null /*&& regionResolver.FindFromPosition (ref region, x , y)*/)
            {
                // TODO: use regionResolver
            }
        }
        return previousRegion != Region;
    }

    private bool SetFaction(DataBlock? faction)
    {
        bool updated = false;
        BlockType newBlockType = faction is not null ? faction.GetBlockType() : BlockType.UNKNOWN;
        if (newBlockType == BlockType.FACTION)
        {
            ItemType |= ItemTypes.REGION;
            Faction = faction;
            // TODO: why does cr is needed here ?
            /*
            DataBlock? faction = null;
            if (cr.GetFaction(ref faction, faction!.GetId()))
            {
                Faction = faction;
                Selected |= Mask.FACTION;
            }
            */
            updated = true;
        }
        else
        {
            if (UpdateFactionFromItem())
            {
                updated = true;
            }
        }

        return updated;
    }

    protected virtual bool UpdateRegionFromItem()
    {
        // TODO
        return false;
    }

    protected virtual bool UpdateFactionFromItem()
    {
        // TODO
        return false;
    }

    /// <summary>
    /// Determines whether the specified mask is part of the current selection.
    /// </summary>
    /// <param name="mask">The mask to check against the current selection.</param>
    /// <returns><see langword="true"/> if the specified mask is included in the current selection; otherwise, <see
    /// langword="false"/>.</returns>
    public bool Has(ItemTypes mask)
    {
        return (ItemType & mask) != ItemTypes.None;
    }

    public bool IsRegionSelected()
    {
        return (ItemType & ItemTypes.REGION) != ItemTypes.None;
    }

    public bool HasUnit()
    {
        return (ItemType & ItemTypes.UNIT) != ItemTypes.None;
    }

    public bool HasBuilding()
    {
        return (ItemType & ItemTypes.BUILDING) != ItemTypes.None;
    }

    public bool HasShip()
    {
        return (ItemType & ItemTypes.SHIP) != ItemTypes.None;
    }

    public bool HasOnlyRegion()
    {
        // In that case Item should be equal to Region
        return (ItemType & ItemTypes.REGION) == ItemTypes.REGION;
    }

    public bool IsFactionSelected()
    {
        return (ItemType & ItemTypes.FACTION) != ItemTypes.None;
    }
    public bool HasRegion()
    {
        return (ItemType & ItemTypes.REGION) != ItemTypes.None && Region != null;
    }

    public bool HasFaction()
    {
        return (ItemType & ItemTypes.FACTION) != ItemTypes.None && Faction != null;
    }

    /// <summary>
    /// Determines whether the current selection is empty.
    /// </summary>
    /// <returns><see langword="true"/> if no items are selected and all related properties are null; otherwise, <see
    /// langword="false"/>.</returns>
    public bool IsEmpty()
    {
        return ItemType == ItemTypes.None;
    }

    public bool Clear()
    {
        bool changed = Item is not null;
        ItemType = ItemTypes.None;
        Region = null;
        Faction = null;
        Item = null;
        return changed;
    }

    public override string ToString()
    {
        return ItemType != ItemTypes.None ? $"sel[{Item!}]" : string.Empty;
    }
}
