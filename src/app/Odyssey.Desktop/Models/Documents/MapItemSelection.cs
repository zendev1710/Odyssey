using Odyssey.Models.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.Models.Documents;

public class MapItemSelection : SimpleItemSelection
{
    public DataBlock? Region { get; set; }
    public DataBlock? Ship { get; set; }
    public DataBlock? Building { get; set; }
    public DataBlock? Faction { get; set; }
    public DataBlock? Unit { get; set; }
    public int SelX { get; set; } = 0;
    public int SelY { get; set; } = 0;
    public int SelPlane { get; set; } = 0;
    public Mask Selected { get; set; } = 0;
    public int SelChange { get; set; } = 0;
    public int FileChange { get; set; }
    public HashSet<DataBlock> RegionsSelected { get; set; } = [];

    // selected flags
    [Flags]
    public enum Mask
    {
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

    public MapItemSelection(DataBlock block) : base(block, null, null)
    {
    }

    public MapItemSelection()
    {
    }

    public MapItemSelection(in MapItemSelection state)
    {
        CopyFrom(state);
    }

    public void CopyFrom(in MapItemSelection itemSelection)
    {
        SelX = itemSelection.SelX;
        SelY = itemSelection.SelY;
        SelPlane = itemSelection.SelPlane;
        SelChange = itemSelection.SelChange;
        FileChange = itemSelection.FileChange;
        Selected = itemSelection.Selected;
        Region = itemSelection.Region;
        Faction = itemSelection.Faction;
        Building = itemSelection.Building;
        Ship = itemSelection.Ship;
        Unit = itemSelection.Unit;
        RegionsSelected = itemSelection.RegionsSelected;
    }

    /// <summary>
    /// Update selection state from the specified report.
    /// </summary>
    /// <param name="report"></param>
    public void UpdateFromReport(CRDocument report)
    {
        DataBlock? region = null;
        DataBlock? unit = null;
        DataBlock? ship = null;
        DataBlock? building = null;
        if (Selected != 0)
        {
            if (IsSelected(Mask.REGION))
            {
                // if flag is set, we consider the region block is valid and not null
                SelX = Region!.GetX();
                SelY = Region.GetY();
                SelPlane = Region.GetId();
                if (IsSelected(Mask.UNKNOWN_REGION) && !report.FindRegionFromPosition(ref region, SelX, SelY, SelPlane))
                {
                    Selected &= ~Mask.REGION;
                }
            }
            if (IsSelected(Mask.UNIT) && !report.GetUnit(ref unit, Unit!.GetId()))
            {
                Selected &= ~Mask.UNIT;
            }
            if (IsSelected(Mask.SHIP) && !report.GetShip(ref ship, Ship!.GetId()))
            {
                Selected &= ~Mask.SHIP;
            }
            if (IsSelected(Mask.BUILDING) && !report.GetShip(ref building, Building!.GetId()))
            {
                Selected &= ~Mask.BUILDING;
            }
        }
        Region = region;
        Unit = unit;
        Ship = ship;
        Building = building;
    }

    public bool SetItem(DataBlock? newBlock)
    {
        bool updated = true;
        BlockType newBlockType = newBlock != null ? newBlock.GetBlockType() : BlockType.UNKNOWN;
        switch (newBlockType)
        {
            case BlockType.REGION:
                Selected |= Mask.REGION;
                Region = newBlock;
                break;

            case BlockType.FACTION:
                Faction = newBlock;
                Selected |= Mask.FACTION;
                // TODO: why does cr is needed here ?
                /*
                DataBlock? factionBlock = null;
                if (cr.GetFaction(ref factionBlock, newBlock!.GetId()))
                {
                    Faction = factionBlock;
                    Selected |= Mask.FACTION;
                }
                */
                break;
            case BlockType.UNIT:
                Selected |= Mask.UNIT;
                Unit = newBlock;
                break;
            case BlockType.BUILDING:
                Selected |= Mask.BUILDING;
                Building = newBlock;
                break;
            case BlockType.SHIP:
                Selected |= Mask.SHIP;
                Ship = newBlock;
                break;
            case BlockType.UNKNOWN:
                updated = false;
                break;
            default:
                updated = false;
                Debug.WriteLine($"[VM-DOCTOOL-] !!! new selecteditem block has an unexpected type {newBlockType} !!!");
                break;
        }

        return updated;
    }

    public bool SetItem(DataBlock? newBlock, CRDocument cr, out bool isRegion)
    {
        isRegion = false;
        bool updated = true;
        BlockType newBlockType = newBlock != null ? newBlock.GetBlockType() : BlockType.UNKNOWN;
        switch (newBlockType)
        {
            case BlockType.REGION:
                isRegion = true;
                Selected |= Mask.REGION;
                Region = newBlock;
                break;
            case BlockType.FACTION:
                // TODO: why does cr is needed here ?
                DataBlock? factionBlock = null;
                if (cr.GetFaction(ref factionBlock, newBlock!.GetId()))
                {
                    Faction = factionBlock;
                    Selected |= Mask.FACTION;
                }
                break;
            case BlockType.UNIT:
                Selected |= Mask.UNIT;
                Unit = newBlock;
                break;
            case BlockType.BUILDING:
                Selected |= Mask.BUILDING;
                Building = newBlock;
                break;
            case BlockType.SHIP:
                Selected |= Mask.SHIP;
                Ship = newBlock;
                break;
            case BlockType.UNKNOWN:
                updated = false;
                break;
            default:
                updated = false;
                Debug.WriteLine($"[VM-DOCTOOL-] !!! new selecteditem block has an unexpected type {newBlockType} !!!");
                break;
        }

        return updated;
    }

    public bool UpdateFrom(DataBlock? oldBlock, DataBlock? newBlock, CRDocument cr, out bool isRegion)
    {
        isRegion = false;
        BlockType oldBlockType = oldBlock != null ? oldBlock.GetBlockType() : BlockType.UNKNOWN;
        BlockType newBlockType = newBlock != null ? newBlock.GetBlockType() : BlockType.UNKNOWN;
        // Remove oldBlock from selection, except if it's a region
        if (oldBlock != newBlock && oldBlockType != BlockType.UNKNOWN && oldBlockType != BlockType.REGION)
        {
            ClearSelected();
        }
        return SetItem(newBlock, cr, out isRegion);
    }

    public bool Enable(Mask mask, DataBlock block)
    {
        if (mask == Mask.REGION)
        {
            Region = block;
        }
        else if (mask == Mask.FACTION)
        {
            Faction = block;
        }
        else if (mask == Mask.UNIT)
        {
            Unit = block;
        }
        else if (mask == Mask.SHIP)
        {
            Ship = block;
        }
        else if (mask == Mask.BUILDING)
        {
            Building = block;
        }
        else if (mask == Mask.CONFIRMATION)
        {
            //Building = block;
        }
        else
        {
            return false;
        }
        return true;
    }
    public void Disable(Mask mask)
    {
        Selected &= mask;
    }
    public void SetUnknownRegion()
    {
        Selected |= Mask.UNKNOWN_REGION;
        SelX = 0;
        SelY = 0;
        SelPlane = 0;
    }

    /// <summary>
    /// Indicates that the report file has changed.
    /// To be called to force some selected item view refresh, especially when async selection events are involved.
    /// </summary>
    public void SetFileChanged()
    {
        FileChange = FileChange + 1;
    }

    /*
    /// <summary>
    /// Indicates that the selection has changed.
    /// To be called to force some selected item view refresh, especially when async selection events are involved.
    /// </summary>
    public void SetSelectionChanged()
    {
        SelChange = SelChange + 1;
    }
    */

    /*
    /// <summary>
    /// Update the selection state based on the specified block and its type.
    /// If the block has parent blocks (in a hierarchical tree), the selection state is updated accordingly.
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    public bool Update(DataBlock block) {
        Mask previousSelected = Selected;
        if (block != null)
        {
            switch (block.GetType())
            {
                case BlockType.REGION:
                    Region = block;
                    Selected |= Mask.REGION;
                    break;
                case BlockType.FACTION:
                    Faction = block;
                    Selected |= Mask.FACTION;
                    break;
                case BlockType.UNIT:
                    Unit = block;
                    Selected |= Mask.UNIT;
                    break;
                case BlockType.SHIP:
                    Ship = block;
                    Selected |= Mask.SHIP;
                    break;
                case BlockType.BUILDING:
                    Building = block;
                    Selected |= Mask.BUILDING;
                    break;
                default:
                    break;
            }
        }
        if (previousSelected != Selected)
        {
            ++SelChange;
            return true;
        }
        return false;
    }
    */
    /// <summary>
    /// Determines whether the specified mask is part of the current selection.
    /// </summary>
    /// <param name="mask">The mask to check against the current selection.</param>
    /// <returns><see langword="true"/> if the specified mask is included in the current selection; otherwise, <see
    /// langword="false"/>.</returns>
    public bool IsSelected(Mask mask)
    {
        return (Selected & mask) != 0;
    }

    public DataBlock? GetSelection()
    {
        // Check the children first in case multiple items are selected
        if (IsSelected(Mask.UNIT))
        {
            return Unit;
        }
        if (IsSelected(Mask.SHIP))
        {
            return Ship;
        }
        if (IsSelected(Mask.BUILDING))
        {
            return Building;
        }
        if (IsSelected(Mask.FACTION))
        {
            return Faction;
        }
        if (IsSelected(Mask.REGION))
        {
            return Region;
        }
        return null;
    }

    /*
    public void Transfer(CRDocument oldCr, CRDocument newCr, int xOffset, int yOffset)
    {
        if (Selected != 0)
        {
            if (IsSelected(Mask.UNKNOWN_REGION) && SelPlane == 0)
            {
                SelX += xOffset;
                SelY += yOffset;
            }
            DataBlock? block = null;
            if (IsSelected(Mask.REGION) && !newCr.GetRegion(ref block, Region!))
            {
                Selected -= Mask.REGION;
            }
            if (IsSelected(Mask.FACTION) && !newCr.GetFaction(ref block, Faction!.GetId()))
            {
                Selected -= Mask.FACTION;
            }
            if (IsSelected(Mask.UNIT) && !newCr.GetUnit(ref block, Unit!.GetId()))
            {
                Selected -= Mask.UNIT;
            }
            if (IsSelected(Mask.SHIP) && !newCr.GetShip(ref block, Ship!.GetId()))
            {
                Selected -= Mask.SHIP;
            }
            if (IsSelected(Mask.BUILDING) && !newCr.GetBuilding(ref block, Building!.GetId()))
            {
                Selected -= Mask.BUILDING;
            }
            if (RegionsSelected.Count > 0)
            {
                HashSet<DataBlock> sel = [];
                foreach (DataBlock r in RegionsSelected)
                {
                    DataBlock? match = null;
                    if (newCr.GetRegion(ref match, r))
                    {
                        sel.Add(match!);
                    }
                }
                RegionsSelected = sel;
            }
            ++SelChange;
        }
    }
    */

    /// <summary>
    /// Determines whether the current selection is empty.
    /// </summary>
    /// <returns><see langword="true"/> if no items are selected and all related properties are null; otherwise, <see
    /// langword="false"/>.</returns>
    public bool IsEmpty()
    {
        return Selected == 0 && Region == null && Faction == null && Building == null && Ship == null && Unit == null;
    }

    public void Clear()
    {
        ClearSelected();

        // Statuses
        SelChange = 0;
        FileChange = 0;
    }
    private void ClearSelected()
    {
        RegionsSelected.Clear();
        Selected = 0;
        Region = null;
        Faction = null;
        Building = null;
        Ship = null;
        Unit = null;
        SelX = 0;
        SelY = 0;
        SelPlane = 0;
    }

#if DEBUG
    public override string ToString()
    {
        string str = "";
        if (IsSelected(Mask.REGION))
        {
            str += $"r[{Region!}] ";
        }
        if (IsSelected(Mask.UNIT))
        {
            str += $"u[{Unit!}] ";
        }
        if (IsSelected(Mask.FACTION))
        {
            str += $"f[{Faction!}] ";
        }
        if (IsSelected(Mask.SHIP))
        {
            str += $"s[{Ship!}] ";
        }
        if (IsSelected(Mask.BUILDING))
        {
            str += $"b[{Building!}] ";
        }
        return str.Trim();
    }
#endif
}
