using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Settings;
using System;
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
    public event Action<int>? SelectedUnitNeedsReveal;

    public ObservableCollection<UnitViewModel> Units { get; } = new();

    private readonly List<int> _loadedUnitIds = [];
    private RegionModel? _regionContext;
    private LocatedFactionModel? _locatedFactionContext;
    private int? _badgeSelectionInProgress;

    [ObservableProperty]
    private int? _selectedUnitId;

    [ObservableProperty]
    private GridLength _ordersColumnWidth = new(220);

    public UnitStripViewModel() : base()
    {
        GlobalSettings.Shared.SettingChanged += OnGlobalSettingChanged;
    }

    private void OnGlobalSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName is GlobalSettings.UNIT_SKILL_SORT_MODE
            or GlobalSettings.UNIT_SKILL_CUSTOM_ORDER
            or GlobalSettings.UNIT_SKILL_GROUP_BY_CATEGORY
            or GlobalSettings.UNIT_SKILL_CATEGORY_ORDER)
        {
            foreach (UnitViewModel unit in Units)
            {
                unit.SortSkillItems();
            }
        }
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);
        _loadedUnitIds.Clear();
        Units.Clear();
        _regionContext = null;
        _locatedFactionContext = null;
        SelectedUnitId = null;
    }

    public void SetUnits(IEnumerable<int> unitIds, bool revealSelectedUnit = true)
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
        if (revealSelectedUnit && SelectedUnitId is int selectedUnitId &&
            _badgeSelectionInProgress != selectedUnitId && requestedIds.Contains(selectedUnitId))
        {
            SelectedUnitNeedsReveal?.Invoke(selectedUnitId);
        }
    }

    public void SetSelectedUnit(int? unitId)
    {
        SelectedUnitId = unitId;
        UpdateSelectedBadge();
        if (unitId is int selectedUnitId && _badgeSelectionInProgress != selectedUnitId)
        {
            SelectedUnitNeedsReveal?.Invoke(selectedUnitId);
        }
    }

    public void RefreshConfirmedStatuses()
    {
        foreach (UnitViewModel unit in Units)
        {
            if (unit.Model is UnitModel model)
            {
                unit.IsConfirmed = model.OrdersConfirmed;
            }
        }
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

        SelectedUnitId = selectedUnit.Id;
        UpdateSelectedBadge();
        _badgeSelectionInProgress = selectedUnit.Id;
        try
        {
            SendSelectionChangedEvent(new EntitySelection(
                selectedUnit,
                _regionContext,
                locatedFaction: _locatedFactionContext));
        }
        finally
        {
            _badgeSelectionInProgress = null;
        }
    }
}