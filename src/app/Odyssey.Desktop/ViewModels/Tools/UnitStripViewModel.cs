using CommunityToolkit.Mvvm.Input;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

    /// <summary>
    /// Key of the region the strip is currently built from, or -1 when no region is loaded
    /// (used to not rebuild the strip when the selection changes inside the same region).
    /// </summary>
    private int _loadedRegionKey = -1;

    public UnitStripViewModel() : base()
    {
        // Initialize command so XAML can bind to SelectUnitCommand
        SelectUnitCommand = new RelayCommand<UnitViewModel?>(SelectUnit);
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

        // The units of the new document have to be collected again.
        _loadedRegionKey = -1;
        Units.Clear();
    }

    /// <summary>
    /// Selection change from the global selection system.
    /// We load units for the region of the new selection (region or unit's parent region).
    /// </summary>
    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this)) return;

        bool regionChanged = selType.HasFlag(SelectionChangeTypes.Region);
        bool regionSelected = selection.IsRegionSelected();
        bool unitSelected = selection.HasUnit();

        Debug.WriteLine($"[VM-UNIT-STRIP] OnSelectionChanged: region changed={regionChanged}, region selected={regionSelected}");

        // If a region is selected -> load units in that region
        if (regionSelected)
        {
            var region = selection.Region.DataBlock as RegionDataBlock;
            int regionKey = Coordinates.GetId(region.GetX(), region.GetY(), (PlaneType)region.GetId());
            LoadUnitsForRegion(regionKey);
            return;
        }

        // If a unit is selected -> resolve its seen parent region, then load units of that region
        if (unitSelected)
        {
            var unit = selection.Item!.DataBlock;
            if (unit != null)
            {
                RegionDataBlock? region = null;
                if (CRDocument.FindRegionFrom(ref region, unit))
                {
                    if (region is not null)
                    {
                        RegionDataBlock r = region as RegionDataBlock;
                        if (r is not null)
                        {
                            int regionKey = Coordinates.GetId(r.GetX(), r.GetY(), (PlaneType)region.GetId());
                            LoadUnitsForRegion(regionKey);
                            return;
                        }
                    }
                }
            }
        }

        // default: clear list
        _loadedRegionKey = -1;
        Units.Clear();

    }

    private void LoadUnitsForRegion(int regionKey)
    {
        // This method is called for any selection change occurring in a region (the region itself or
        // one of its units/buildings/... being selected): rebuilding the strip is useless when the
        // units of that region are already displayed.
        if (regionKey == _loadedRegionKey)
        {
            return;
        }

        _loadedRegionKey = regionKey;
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

        // TODO: send event
    }
}