using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Shows the units selected by the composite units inspector.
/// Inherits from <see cref="DocumentToolViewModelBase"/> so the Report is provided later via SetMapFile.
/// </summary>
public partial class UnitStripViewModel : DocumentToolViewModelBase
{
    public ObservableCollection<UnitViewModel> Units { get; } = new();

    private readonly List<int> _loadedUnitIds = [];
    private RegionModel? _regionContext;
    private LocatedFactionModel? _locatedFactionContext;
    private int? _selectedUnitId;

    [ObservableProperty]
    private GridLength _ordersColumnWidth = new(220);

    public UnitStripViewModel() : base()
    {
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);
        _loadedUnitIds.Clear();
        Units.Clear();
        _regionContext = null;
        _locatedFactionContext = null;
        _selectedUnitId = null;
    }

    public void SetUnits(IEnumerable<int> unitIds)
    {
        List<int> requestedIds = [.. unitIds];
        if (requestedIds.SequenceEqual(_loadedUnitIds))
        {
            return;
        }

        _loadedUnitIds.Clear();
        _loadedUnitIds.AddRange(requestedIds);
        Units.Clear();
        if (!HasDocument)
        {
            return;
        }

        foreach (int unitId in requestedIds)
        {
            Units.Add(UnitViewModel.FromDocument(Report, unitId, includeSummary: true));
        }
        UpdateSelectedBadge();
    }

    public void SetSelectedUnit(int? unitId)
    {
        _selectedUnitId = unitId;
        UpdateSelectedBadge();
    }

    private void UpdateSelectedBadge()
    {
        foreach (UnitViewModel unit in Units)
        {
            unit.IsSelected = unit.Model?.Id == _selectedUnitId;
        }
    }

    public void SetSelectionContext(RegionModel? region, LocatedFactionModel? locatedFaction)
    {
        _regionContext = region;
        _locatedFactionContext = locatedFaction;
    }

    public void SetOrdersColumnVisible(bool isVisible)
    {
        OrdersColumnWidth = new GridLength(isVisible ? 220 : 0);
    }

    [RelayCommand]
    private void SelectUnit(UnitViewModel? unit)
    {
        if (unit?.Model is not UnitModel selectedUnit)
        {
            return;
        }

        SetSelectedUnit(selectedUnit.Id);
        SendSelectionChangedEvent(new EntitySelection(
            selectedUnit,
            _regionContext,
            locatedFaction: _locatedFactionContext));
    }
}