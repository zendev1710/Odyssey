using Odyssey.Models.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Odyssey.Models.Documents.MapItemSelection;

namespace Odyssey.Models.Documents;

public class SimpleItemSelection : ISelection
{
    public DataBlock? Region { get; set; }

    public DataBlock? Faction { get; set; }

    public DataBlock? Item { get; set; }

    public ItemTypes ItemType { get; protected set; }

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

    public SimpleItemSelection(DataBlock? item, DataBlock? region, DataBlock? faction)
    {
        SetItem(item, region, faction);
    }

    public SimpleItemSelection()
    {
        Clear();
    }

    public SimpleItemSelection(CRDocument report, int positionX, int positionY, int plane = 0)
    {
        Clear();
        DataBlock? region = null;
        if (report.FindSeenRegionFromPosition(ref region, positionX, positionY, plane))
        {
            SetItem(region, null, null);
        }
    }

    public bool SetItem(DataBlock? item, DataBlock? region, DataBlock? faction)
    {
        Clear();
        DataBlock? r = region;
        DataBlock? f = faction;
        bool updated = true;
        BlockType newBlockType = item != null ? item.GetBlockType() : BlockType.UNKNOWN;
        switch (newBlockType)
        {
            case BlockType.REGION:
                ItemType |= ItemTypes.REGION;
                Item = item;
                r = item;
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
                updated = false;
                break;
            default:
                updated = false;
                Debug.WriteLine($"[VM-DOCTOOL-] !!! new selecteditem block has an unexpected type {newBlockType} !!!");
                break;
        }
        updated = SetRegion(r) || updated;
        updated = SetFaction(f) || updated;
        return updated;
    }

    public bool SetRegion(DataBlock? region)
    {
        bool updated = false;
        BlockType newBlockType = region != null ? region.GetBlockType() : BlockType.UNKNOWN;
        if (newBlockType == BlockType.REGION)
        {
            ItemType |= ItemTypes.REGION;
            Region = region;
            updated = true;
        }
        return updated;
    }

    public bool SetFaction(DataBlock? faction)
    {
        bool updated = false;
        BlockType newBlockType = faction != null ? faction.GetBlockType() : BlockType.UNKNOWN;
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
        return updated;
    }

    /// <summary>
    /// Determines whether the specified mask is part of the current selection.
    /// </summary>
    /// <param name="mask">The mask to check against the current selection.</param>
    /// <returns><see langword="true"/> if the specified mask is included in the current selection; otherwise, <see
    /// langword="false"/>.</returns>
    public bool Is(ItemTypes mask)
    {
        return (ItemType & mask) != ItemTypes.None;
    }

    public bool IsRegionSelected()
    {
        return (ItemType & ItemTypes.REGION) != ItemTypes.None;
    }

    public bool IsOnlyRegionSelected()
    {
        return (ItemType & ItemTypes.REGION) == ItemTypes.REGION;
    }

    public bool IsUnitSelected()
    {
        return (ItemType & ItemTypes.UNIT) != ItemTypes.None;
    }

    public bool IsBuildingSelected()
    {
        return (ItemType & ItemTypes.BUILDING) != ItemTypes.None;
    }

    public bool IsShipSelected()
    {
        return (ItemType & ItemTypes.SHIP) != ItemTypes.None;
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
    public bool NothingSelected()
    {
        return ItemType == ItemTypes.None;
    }

    public void Clear()
    {
        ClearSelected();
    }

    private void ClearSelected()
    {
        ItemType = ItemTypes.None;
        Region = null;
        Faction = null;
        Item = null;
    }

    public override string ToString()
    {
        return ItemType != ItemTypes.None ? $"sel[{Item!}]" : string.Empty;
    }
}
