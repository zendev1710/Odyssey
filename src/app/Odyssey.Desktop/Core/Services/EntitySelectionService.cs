using System;
using System.Diagnostics;
using Odyssey.Models;
using Odyssey.Models.Documents;
using static Odyssey.Models.Documents.EntitySelection;

namespace Odyssey.Core.Services;

public interface IEntitySelectionService
{
    IEntitySelection Selection { get; }
    event Action<IEntitySelection, SelectionChangeTypes, object?>? SelectionChanged;

    void SelectItem(EntityModel? item, RegionModel? region, /*DataBlock? faction,*/ object? sender, object? data = null, SelectionTypes containerTypes = SelectionTypes.None);
    //void SelectRegion(RegionDataBlock? region, object? sender);
    void Clear(object? sender);
}

public class EntitySelectionService : IEntitySelectionService
{
    private readonly EntitySelection _selection;

    public IEntitySelection Selection => _selection;

    public event Action<IEntitySelection, SelectionChangeTypes, object?>? SelectionChanged;

    public EntitySelectionService()
    {
        _selection = new EntitySelection();
    }

    public void SelectItem(EntityModel? item, RegionModel? region, /*DataBlock? faction,*/ object? sender, object? data = null, SelectionTypes containerTypes = SelectionTypes.None)
    {
        Debug.WriteLine($"[ENTITY-SERVICE-SEL] Select from {sender} for item={item} region={region} container={containerTypes}");

        // Keep the previous selection state to know whether the selection actually changed.
        // The item alone is not sufficient: container nodes (buildings/ships lists) have no item,
        // so two selections on the same region would be considered unchanged.
        EntityModel? previousItem = _selection.Item;
        RegionModel? previousRegion = _selection.Region;
        SelectionTypes previousTypes = _selection.SelectionType;

        // The service owns the single shared selection instance: reset it before applying the
        // new selection so that no state (flags, item, region) leaks from the previous one.
        // Otherwise selecting a building just after a unit would still report the unit as selected.
        _selection.Clear();
        if (containerTypes != SelectionTypes.None)
        {
            _selection.SetContainer(containerTypes, region);
        }
        else
        {
            _selection.SetItem(item, region/*, faction*/);
        }

        if (previousItem != _selection.Item ||
            previousRegion != _selection.Region ||
            previousTypes != _selection.SelectionType)
        {
            _selection.Data = data;
            SelectionChanged?.Invoke(
                _selection,
                GetSelectionChangeTypes(previousItem, previousRegion, previousTypes,
                                        _selection.Item, _selection.Region, _selection.SelectionType),
                sender);
        }
    }

    /*
    public void SelectRegion(DataBlock? region, object? sender)
    {
        if (_selection.SetRegion(region))
        {
            SelectionChanged?.Invoke(_selection, ToSelectionChangeType(_selection.ItemType), sender);
        }
    }
    */

    /*
    public void SelectFaction(DataBlock? faction, object? sender)
    {
        if (_selection.SetFaction(faction))
        {
            SelectionChanged?.Invoke(_selection, ToSelectionChangeType(_selection.ItemType), sender);
        }
    }
   */

    public void Clear(object? sender)
    {
        if (_selection.Clear())
        {
            SelectionChanged?.Invoke(_selection, SelectionChangeTypes.Cleared, sender);
        }
    }

    /// <summary>
    /// Determines which parts of the selection have really changed, by comparing the new selection with
    /// the previous one.
    /// The change types cannot be derived from the new selection flags only: the region flag is set as
    /// soon as a region is known, so any selection (a unit, a building, a container, ...) would then
    /// report a region change, making every view rebuild its region related data on each selection
    /// change (and not only when the region really changed).
    /// </summary>
    /// <returns>The change types of the new selection compared with the previous one.</returns>
    static private SelectionChangeTypes GetSelectionChangeTypes(
        EntityModel? previousItem, RegionModel? previousRegion, SelectionTypes previousTypes,
        EntityModel? item, RegionModel? region, SelectionTypes types)
    {
        SelectionChangeTypes result = SelectionChangeTypes.None;

        // The region related data has to be rebuilt only when the selected region really changed.
        // Note: region is null when no region is known (the previous region is then irrelevant).
        if (region is not null && previousRegion != region)
        {
            result |= SelectionChangeTypes.Region;
        }

        // A container node (the buildings/ships list of a region) has no item:
        // its "identity" is the region it belongs to.
        if (Has(types, SelectionTypes.Buildings) &&
            (!Has(previousTypes, SelectionTypes.Buildings) || previousRegion != region))
        {
            result |= SelectionChangeTypes.Buildings;
        }

        if (Has(types, SelectionTypes.Ships) &&
            (!Has(previousTypes, SelectionTypes.Ships) || previousRegion != region))
        {
            result |= SelectionChangeTypes.Ships;
        }

        // A single item type is identified by its item: the change type is set when such an item is
        // newly selected, or when another item of the same type replaces it.
        if (Has(types, SelectionTypes.Building) &&
            (!Has(previousTypes, SelectionTypes.Building) || previousItem != item))
        {
            result |= SelectionChangeTypes.Building;
        }

        if (Has(types, SelectionTypes.Ship) &&
            (!Has(previousTypes, SelectionTypes.Ship) || previousItem != item))
        {
            result |= SelectionChangeTypes.Ship;
        }

        if (Has(types, SelectionTypes.Unit) &&
            (!Has(previousTypes, SelectionTypes.Unit) || previousItem != item))
        {
            result |= SelectionChangeTypes.Unit;
        }

        return result;
    }

    static private bool Has(SelectionTypes types, SelectionTypes mask)
    {
        return (types & mask) != SelectionTypes.None;
    }
}
