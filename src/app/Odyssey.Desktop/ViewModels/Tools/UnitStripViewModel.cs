using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Prism.Events;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Shows a horizontal strip of units for the currently selected region.
/// Inherits from <see cref="DocumentToolViewModelBase"/> so the Report is provided later via SetMapFile.
/// </summary>
public partial class UnitStripViewModel : DocumentToolViewModelBase
{
    public ObservableCollection<UnitViewModel> Units { get; } = new();

    // Public command property exposed to the view
    public IRelayCommand<UnitViewModel?> SelectUnitCommand { get; }

    public UnitStripViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
        // Initialize command so XAML can bind to SelectUnitCommand
        SelectUnitCommand = new RelayCommand<UnitViewModel?>(SelectUnit);
    }

    /// <summary>
    /// Selection change from the global selection system.
    /// We load units for the region of the new selection (region or unit's parent region).
    /// </summary>
    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // ignore selection changes that come from this selector itself
        var include = new System.Collections.Generic.List<string> { Ids.Explorer };
        var exclude = new System.Collections.Generic.List<string> { Id };
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, include, exclude))
            return;

        var sel = selectionChange.Selection;
        if (sel == null)
        {
            Units.Clear();
            return;
        }

        // If a region is selected -> load units in that region
        if (sel.IsRegionSelected())
        {
            var region = sel.Region;
            if (region != null)
            {
                int regionKey = Coordinates.GetId(region.GetX(), region.GetY(), (PlaneType)region.GetId());
                LoadUnitsForRegion(regionKey);
                return;
            }
        }

        // If a unit is selected -> resolve its seen parent region, then load units of that region
        if (sel.IsUnitSelected())
        {
            var unit = sel.Item;
            if (unit != null)
            {
                DataBlock? region = null;
                if (CRDocument.GetSeenParent(ref region, unit))
                {
                    if (region != null)
                    {
                        int regionKey = Coordinates.GetId(region.GetX(), region.GetY(), (PlaneType)region.GetId());
                        LoadUnitsForRegion(regionKey);
                        return;
                    }
                }
            }
        }

        // default: clear list
        Units.Clear();
    }

    private void LoadUnitsForRegion(int regionKey)
    {
        Units.Clear();
        if (!HasDocument)
            return;

        var ids = Report.GetUnitIdsInRegion(regionKey);
        foreach (var id in ids)
        {
            var uvm = UnitViewModel.FromDocument(Report, id);
            Units.Add(uvm);
        }
    }

    /// <summary>
    /// Called from the UnitStrip view when the user clicks a unit tile.
    /// Publishes the selected unit id so other tools (Inspector, Orders) can react.
    /// </summary>
    private void SelectUnit(UnitViewModel? unit)
    {
        if (unit == null)
            return;

        int id = 0;
        if (unit.DataBlock != null)
        {
            id = unit.DataBlock.GetId();
        }
        else
        {
            int.TryParse(unit.Id, out id);
        }

        if (id <= 0)
            return;

        EventAggregator?.GetEvent<UnitSelectedEvent>().Publish(id);
    }
}