using Avalonia.Controls;
using Odyssey.Models.Data;
using Odyssey.Models.Tools;
using System;
using System.Diagnostics;
using static Odyssey.Models.EntityModel;

namespace Odyssey.Models.Documents;

public class EntitySelection : IEntitySelection
{
    public RegionModel? Region { get; set; }
    public LocatedFactionModel? LocatedFaction { get; private set; }

    public EntityModel? Item { get; set; }

    public object? Data { get; set; }

    public SelectionTypes SelectionType { get; protected set; }

    //protected readonly IDocumentService _documentService;

    // selected flags
    [Flags]
    public enum SelectionTypes
    {
        None = 0,
        Region = (1 << 0),
        // "Buildings"/"Ships" are the container nodes of a region (the list of the region's
        // structures). Unlike the other types, they have no matching Item entity.
        Buildings = (1 << 1),
        Ships = (1 << 2),
        Building = (1 << 3),
        Ship = (1 << 4),
        Unit = (1 << 5),
        Faction = (1 << 6),
    };

    // TODO
    /*
    public EntitySelection(ILocation location)
    {
        Clear();
        // TODO: check if an ILocation can be a BattleDataBlock
        RegionDataBlock r = (RegionDataBlock)location;
        SetItem(r, r, null, null);
    }
    */
    
    public EntitySelection(EntityModel? item, RegionModel? region, object? data = null, ILocationResolver? regionResolver = null, LocatedFactionModel? locatedFaction = null)
    {
        Clear();
        SetItem(item, region, data, regionResolver, locatedFaction);
    }

    public EntitySelection(CRDocument report, int x, int y, int z)
    {
        Clear();

        // TODO: what about if 0,0 is not populated ?
        if (report.FindOccupiedRegion(x, y, (PlaneType)z, out RegionModel? regionModel)) 
        {
            SetItem(regionModel, regionModel);
        }
    }

    public EntitySelection(ILocation location)
    {
        Clear();
        /*
        if (report.FindPopulatedRegion(location.X, location.Y, location.Plane, out RegionModel? regionModel))
        {
            SetItem(regionModel, regionModel);
        }
        */
    }

    public EntitySelection()
    {
        Clear();
    }

    /*

    public EntitySelection(ILocationResolver locationResolver, int positionX, int positionY, int plane)
    {
        Clear();
        if (locationResolver.ResolveLocation(positionX, positionY, (PlaneType)plane, out ILocation? location))
        {
            SetItem(location as RegionModel, null, null);
        }
    }
    */

    public bool SetItem(EntityModel? item, RegionModel? region, object? data = null, ILocationResolver? regionResolver = null, LocatedFactionModel? locatedFaction = null)
    {
        EntityModel? previousItem = Item;
        LocatedFaction = locatedFaction;
        if (locatedFaction is not null)
        {
            SelectionType |= SelectionTypes.Faction;
        }

        // TODO: it should return false when it does not change the selection compared with the previously one

        // Note: the instance is expected to be reset before applying a new selection
        // (see EntitySelectionService which owns and reuses its selection instance).
        RegionModel? r = region;
        Data = data;
        EntityType newType = item is not null ? item.Type : EntityType.Unknown;
        switch (newType)
        {
            case EntityType.Region:
                SelectionType |= SelectionTypes.Region;
                Item = item;
                r = item as RegionModel;
                break;
            case EntityType.Unit:
                SelectionType |= SelectionTypes.Unit;
                Item = item;
                break;
            case EntityType.Building:
                SelectionType |= SelectionTypes.Building;
                Item = item;
                break;
            case EntityType.Ship:
                SelectionType |= SelectionTypes.Ship;
                Item = item;
                break;
            case EntityType.Unknown:
                break;
            default:
                Debug.WriteLine($"[ENTITY-SEL] !!! new selected item block has an unexpected type {newType} !!!");
                break;
        }

        bool regionChanged = SetRegion(r, regionResolver);
        bool selectionChanged = previousItem != Item /*|| regionChanged || factionChanged*/;

        Debug.WriteLine($"[ENTITY-SEL] selectionChanged = {selectionChanged} regionChanged = {regionChanged}");
        
        return selectionChanged;
    }

    /// <summary>
    /// Selects a container node (the "buildings" or "ships" list of a region), which has no matching
    /// entity (see <see cref="SelectionTypes.Buildings"/> / <see cref="SelectionTypes.Ships"/>).
    /// </summary>
    /// <param name="containerType">Buildings or Ships.</param>
    /// <param name="region">The region owning the container.</param>
    /// <param name="data">Optional data associated with the selection.</param>
    public void SetContainer(SelectionTypes containerType, RegionModel? region, object? data = null)
    {
        Data = data;
        LocatedFaction = null;
        SelectionType |= containerType & (SelectionTypes.Buildings | SelectionTypes.Ships);
        SetRegion(region);
    }

    private bool SetRegion(RegionModel? region, ILocationResolver? regionResolver = null)
    {
        RegionModel? previousRegion = Region;
        EntityType newType = region is not null ? region.Type : EntityType.Unknown;
        if (newType == EntityType.Region)
        {
            SelectionType |= SelectionTypes.Region;
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

    /*
    private bool SetFaction(FactionModel? faction)
    {
        bool updated = false;
        BlockType newBlockType = faction is not null ? faction.Data.GetBlockType() : BlockType.UNKNOWN;
        if (newBlockType == BlockType.FACTION)
        {
            SelectionType |= SelectionTypes.REGION;
            Faction = faction;
            // TODO: why does cr is needed here ?
            // DataBlock? faction = null;
            // if (cr.GetFaction(ref faction, faction!.GetId()))
            // {
            //    Faction = faction;
            //    Selected |= Mask.FACTION;
            //}
            
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
    */
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
    public bool Has(SelectionTypes mask)
    {
        return (SelectionType & mask) != SelectionTypes.None;
    }

    public bool IsRegionSelected()
    {
        return (SelectionType & SelectionTypes.Region) != SelectionTypes.None;
    }

    public bool HasFaction()
    {
        return (SelectionType & SelectionTypes.Faction) != SelectionTypes.None && LocatedFaction is not null;
    }

    public bool HasUnit()
    {
        return (SelectionType & SelectionTypes.Unit) != SelectionTypes.None;
    }

    public bool HasBuilding()
    {
        return (SelectionType & SelectionTypes.Building) != SelectionTypes.None;
    }

    public bool HasShip()
    {
        return (SelectionType & SelectionTypes.Ship) != SelectionTypes.None;
    }

    /// <summary>
    /// Returns true when the "buildings" container node of a region is selected
    /// (i.e. the list of the region's buildings is being displayed, not a single building).
    /// </summary>
    public bool HasBuildings()
    {
        return (SelectionType & SelectionTypes.Buildings) != SelectionTypes.None;
    }

    /// <summary>
    /// Returns true when the "ships" container node of a region is selected
    /// (i.e. the list of the region's ships is being displayed, not a single ship).
    /// </summary>
    public bool HasShips()
    {
        return (SelectionType & SelectionTypes.Ships) != SelectionTypes.None;
    }

    /// <summary>
    /// Returns the container selection types (Buildings and/or Ships) of the specified selection, if any.
    /// </summary>
    public static SelectionTypes GetContainerTypes(IEntitySelection selection)
    {
        SelectionTypes containerTypes = SelectionTypes.None;
        if (selection.HasBuildings()) containerTypes |= SelectionTypes.Buildings;
        if (selection.HasShips()) containerTypes |= SelectionTypes.Ships;
        return containerTypes;
    }

    public bool HasOnlyRegion()
    {
        // In that case Item should be equal to Region:
        // a region is selected and no unit, ship, building or container (buildings/ships list) is selected.
        return (SelectionType & (SelectionTypes.Region | SelectionTypes.Faction | SelectionTypes.Buildings | SelectionTypes.Ships | SelectionTypes.Building | SelectionTypes.Ship | SelectionTypes.Unit)) == SelectionTypes.Region;
    }

    /*
    public bool IsFactionSelected()
    {
        return (SelectionType & SelectionTypes.FACTION) != SelectionTypes.None;
    }
    */
    public bool HasRegion()
    {
        return (SelectionType & SelectionTypes.Region) != SelectionTypes.None && Region != null;
    }

    /*
    public bool HasFaction()
    {
        return (SelectionType & SelectionTypes.FACTION) != SelectionTypes.None && Faction != null;
    }
    */
    /// <summary>
    /// Determines whether the current selection is empty.
    /// </summary>
    /// <returns><see langword="true"/> if no items are selected and all related properties are null; otherwise, <see
    /// langword="false"/>.</returns>
    public bool IsEmpty()
    {
        return SelectionType == SelectionTypes.None;
    }

    public bool Clear()
    {
        bool changed = Item is not null;
        SelectionType = SelectionTypes.None;
        Region = null;
        LocatedFaction = null;
        Item = null;
        Data = null;
        return changed;
    }

    public override string ToString()
    {
        return SelectionType != SelectionTypes.None ? $"sel[{Item!}]" : string.Empty;
    }
}
