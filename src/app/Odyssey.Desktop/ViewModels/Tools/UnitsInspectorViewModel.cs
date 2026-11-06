using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Core.Services;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Settings;
using Odyssey.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Odyssey.ViewModels.Tools
{
    /// <summary>
    /// ViewModel for the units overview, which presents each unit with its summary.
    /// </summary>
    public partial class UnitsInspectorViewModel : DocumentToolViewModelBase
    {
        public event Action<int>? SelectedUnitNeedsReveal;

        public ObservableCollection<UnitViewModel> Units { get; } = new();

        private readonly List<int> _loadedUnitIds = [];
        private RegionModel? _regionContext;
        private LocatedFactionModel? _locatedFactionContext;
        private int? _badgeSelectionInProgress;
        private CRDocument? _confirmationReport;

        [ObservableProperty]
        private int? _selectedUnitId;

        [ObservableProperty]
        private GridLength _ordersColumnWidth = new(2, GridUnitType.Star);

        [ObservableProperty]
        private bool _isOrdersColumnVisible = true;

        private GridLength _lastVisibleOrdersColumnWidth = new(2, GridUnitType.Star);

        [ObservableProperty]
        private LocatedFactionModel? locatedFaction;

        private readonly List<LocatedFactionModel> _locatedFactionsInRegion = [];

        [ObservableProperty]
        private int _locatedFactionIndex = -1;

        public bool CanMoveToPreviousFaction => LocatedFactionIndex > 0;
        public bool CanMoveToNextFaction => LocatedFactionIndex >= 0 && LocatedFactionIndex < _locatedFactionsInRegion.Count - 1;
        public bool ShowFirstLastLocatedFactionButtons => _locatedFactionsInRegion.Count > 2;

        public UnitsInspectorViewModel() : base()
        {
            GlobalSettings.Shared.SettingChanged += OnGlobalSettingChanged;
            ServiceLocator.UnitNavigationOrderService.OrderChanged += OnUnitNavigationOrderChanged;
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
                unit.IsSelected = unit.Model?.Id == SelectedUnitId;
            }
        }

        public void SetSelectionContext(RegionModel? region, LocatedFactionModel? locatedFaction)
        {
            _regionContext = region;
            _locatedFactionContext = locatedFaction;
        }

        public void SetOrdersColumnVisible(bool isVisible)
        {
            IsOrdersColumnVisible = isVisible;
            if (isVisible)
            {
                if (OrdersColumnWidth.Value <= 0)
                {
                    OrdersColumnWidth = _lastVisibleOrdersColumnWidth;
                }
            }
            else
            {
                if (OrdersColumnWidth.Value > 0)
                {
                    _lastVisibleOrdersColumnWidth = OrdersColumnWidth;
                }
                OrdersColumnWidth = new GridLength(0);
            }
        }

        partial void OnOrdersColumnWidthChanged(GridLength value)
        {
            if (value.Value > 0)
            {
                _lastVisibleOrdersColumnWidth = value;
            }
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

        private void OnUnitNavigationOrderChanged(object? sender, EventArgs e)
        {
            if (!HasDocument || !ReferenceEquals(ServiceLocator.UnitNavigationOrderService.Report, Report))
            {
                return;
            }

            if (LocatedFaction is not null)
            {
                SetUnits(GetUnitIdsForFaction(LocatedFaction));
            }
            else if (CurrentSelection.Region is RegionModel region)
            {
                SetUnits(GetUnitIdsForRegion(region));
            }

            RefreshConfirmedStatuses();
        }

        protected override void OnActiveDocumentChanged(CRDocument report)
        {
            base.OnActiveDocumentChanged(report);
            if (_confirmationReport is not null)
            {
                _confirmationReport.OrderConfirmationChanged -= OnOrderConfirmationChanged;
            }
            _confirmationReport = report;
            _confirmationReport.OrderConfirmationChanged += OnOrderConfirmationChanged;

            LocatedFaction = null;
            SetUnits([]);
            SetSelectionContext(null, null);
            SetSelectedUnit(null);
            SetOrdersColumnVisible(true);
            ResetLocatedFactionNavigation();
        }

        private void OnOrderConfirmationChanged(object? sender, EventArgs e)
        {
            if (ReferenceEquals(sender, _confirmationReport))
            {
                RefreshConfirmedStatuses();
            }
        }

        protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
        {
            if (ReferenceEquals(sender, this)) return;
            if (!HasDocument)
            {
                LocatedFaction = null;
                SetUnits([]);
                SetSelectionContext(null, null);
                SetSelectedUnit(null);
                SetOrdersColumnVisible(true);
                ResetLocatedFactionNavigation();
                return;
            }

            ExplorerNodeViewModel? selectedNode = selection.Data as ExplorerNodeViewModel;
            LocatedFactionModel? selectedFaction = selection.LocatedFaction;
            if (selection.HasUnit() && selectedFaction is null &&
                selection.Item is UnitModel unit && selection.Region is RegionModel unitRegion)
            {
                int factionId = unit.EffectiveFactionId;
                selectedFaction = new LocatedFactionModel(
                    Report.GetFaction(factionId), unitRegion, factionId, Report.GetFactionName(factionId));
            }
            else if (selectedFaction is null && selectedNode is not null)
            {
                selectedFaction = FindFirstLocatedFactionForSelection(selectedNode);
            }

            if (selection.HasFaction() && !selection.HasUnit() &&
                IsSameLocatedFaction(LocatedFaction, selectedFaction))
            {
                return;
            }

            RegionModel? region = selectedFaction?.Region ?? selection.Region;
            RefreshLocatedFactions(region, selectedFaction);
            if (selectedFaction is null && _locatedFactionsInRegion.Count > 0)
            {
                selectedFaction = _locatedFactionsInRegion[0];
            }

            bool showOrdersColumn = selectedFaction is null || Report.GetFactionStatus(selectedFaction.FactionId) is FactionStatus.ACTIVE or FactionStatus.ALLIED;
            SetOrdersColumnVisible(showOrdersColumn);
            SetSelectionContext(selectedFaction?.Region ?? selection.Region, selectedFaction);

            bool factionChanged = !IsSameLocatedFaction(LocatedFaction, selectedFaction);
            if (factionChanged)
            {
                LocatedFaction = selectedFaction;
            }

            if (selectedFaction is not null)
            {
                if (factionChanged)
                {
                    SetUnits(GetUnitIdsForFaction(selectedFaction));
                }
            }
            else if (region is not null)
            {
                SetUnits(GetUnitIdsForRegion(region));
            }
            else
            {
                SetUnits([]);
            }

            SetSelectedUnit(selection.Item is UnitModel selectedUnit ? selectedUnit.Id : null);
            RefreshConfirmedStatuses();
            LocatedFactionIndex = _locatedFactionsInRegion.FindIndex(faction => IsSameLocatedFaction(faction, selectedFaction));
        }

        private static bool IsSameLocatedFaction(LocatedFactionModel? left, LocatedFactionModel? right)
        {
            return left is null
                ? right is null
                : right is not null && left.FactionId == right.FactionId && left.Region.Id == right.Region.Id;
        }

        private static LocatedFactionModel? FindFirstLocatedFactionForSelection(ExplorerNodeViewModel selectedNode)
        {
            ExplorerNodeViewModel? regionNode = selectedNode;
            while (regionNode is not null && regionNode.NodeType != ExplorerNodeViewModel.NodeTypes.REGION)
            {
                regionNode = regionNode.Parent;
            }

            return regionNode is null ? null : FindFirstLocatedFaction(regionNode);
        }

        private static LocatedFactionModel? FindFirstLocatedFaction(ExplorerNodeViewModel node)
        {
            if (node.LocatedFaction is not null)
            {
                return node.LocatedFaction;
            }

            foreach (ExplorerNodeViewModel child in node.Children)
            {
                LocatedFactionModel? locatedFaction = FindFirstLocatedFaction(child);
                if (locatedFaction is not null)
                {
                    return locatedFaction;
                }
            }

            return null;
        }

        private List<int> GetUnitIdsForFaction(LocatedFactionModel faction)
        {
            IUnitNavigationOrderService order = ServiceLocator.UnitNavigationOrderService;
            if (ReferenceEquals(order.Report, Report))
            {
                RegionUnitOrder? regionOrder = order.OrderedRegions
                    .FirstOrDefault(region => region.Region.Id == faction.Region.Id);
                if (regionOrder is not null)
                {
                    return regionOrder.Units
                        .Where(unit => unit.EffectiveFactionId == faction.FactionId)
                        .Select(unit => unit.Id)
                        .ToList();
                }
            }

            return Report.GetUnitsInRegion(faction.Region)
                .Where(unit => unit.EffectiveFactionId == faction.FactionId)
                .Select(unit => unit.Id)
                .ToList();
        }

        private List<int> GetUnitIdsForRegion(RegionModel region)
        {
            IUnitNavigationOrderService order = ServiceLocator.UnitNavigationOrderService;
            if (ReferenceEquals(order.Report, Report))
            {
                RegionUnitOrder? regionOrder = order.OrderedRegions
                    .FirstOrDefault(item => item.Region.Id == region.Id);
                if (regionOrder is not null)
                {
                    return regionOrder.Units.Select(unit => unit.Id).ToList();
                }
            }

            return Report.GetUnitsInRegion(region).Select(unit => unit.Id).ToList();
        }

        [RelayCommand(CanExecute = nameof(CanNavigateToFirstLocatedFaction))]
        private void NavigateToFirstLocatedFaction() => ShowLocatedFactionAt(0);
        private bool CanNavigateToFirstLocatedFaction() => LocatedFactionIndex > 0;

        [RelayCommand(CanExecute = nameof(CanNavigatePreviousLocatedFaction))]
        private void NavigateToPreviousLocatedFaction() => ShowLocatedFactionAt(LocatedFactionIndex - 1);
        private bool CanNavigatePreviousLocatedFaction() => CanMoveToPreviousFaction;

        [RelayCommand(CanExecute = nameof(CanNavigateNextLocatedFaction))]
        private void NavigateToNextLocatedFaction() => ShowLocatedFactionAt(LocatedFactionIndex + 1);
        private bool CanNavigateNextLocatedFaction() => CanMoveToNextFaction;

        [RelayCommand(CanExecute = nameof(CanNavigateToLastLocatedFaction))]
        private void NavigateToLastLocatedFaction() => ShowLocatedFactionAt(_locatedFactionsInRegion.Count - 1);
        private bool CanNavigateToLastLocatedFaction() => LocatedFactionIndex >= 0 && LocatedFactionIndex < _locatedFactionsInRegion.Count - 1;

        partial void OnLocatedFactionIndexChanged(int oldValue, int newValue)
        {
            OnPropertyChanged(nameof(CanMoveToPreviousFaction));
            OnPropertyChanged(nameof(CanMoveToNextFaction));
            NotifyFactionNavigationCommands();
        }

        private void NotifyFactionNavigationCommands()
        {
            NavigateToFirstLocatedFactionCommand.NotifyCanExecuteChanged();
            NavigateToPreviousLocatedFactionCommand.NotifyCanExecuteChanged();
            NavigateToNextLocatedFactionCommand.NotifyCanExecuteChanged();
            NavigateToLastLocatedFactionCommand.NotifyCanExecuteChanged();
        }

        private void RefreshLocatedFactions(RegionModel? region, LocatedFactionModel? selectedFaction)
        {
            _locatedFactionsInRegion.Clear();
            if (region is not null)
            {
                IUnitNavigationOrderService order = ServiceLocator.UnitNavigationOrderService;
                RegionUnitOrder? regionOrder = ReferenceEquals(order.Report, Report)
                    ? order.OrderedRegions.FirstOrDefault(item => item.Region.Id == region.Id)
                    : null;
                IEnumerable<UnitModel> units = regionOrder is not null
                    ? regionOrder.Units
                    : Report.GetUnitsInRegion(region);
                foreach (UnitModel unit in units)
                {
                    int factionId = unit.EffectiveFactionId;
                    AddLocatedFactionIfMissing(new LocatedFactionModel(
                        Report.GetFaction(factionId), region, factionId, Report.GetFactionName(factionId)));
                }

                if (selectedFaction is not null && selectedFaction.Region.Id == region.Id)
                {
                    AddLocatedFactionIfMissing(selectedFaction);
                }
            }

            OnPropertyChanged(nameof(ShowFirstLastLocatedFactionButtons));
            NotifyFactionNavigationCommands();
        }

        private void AddLocatedFactionIfMissing(LocatedFactionModel faction)
        {
            if (_locatedFactionsInRegion.Any(existingFaction =>
                    existingFaction.FactionId == faction.FactionId && existingFaction.Region.Id == faction.Region.Id))
            {
                return;
            }

            _locatedFactionsInRegion.Add(faction);
        }

        private void ShowLocatedFactionAt(int index)
        {
            if (index < 0 || index >= _locatedFactionsInRegion.Count)
            {
                return;
            }

            LocatedFactionModel faction = _locatedFactionsInRegion[index];
            LocatedFaction = faction;
            LocatedFactionIndex = index;
            SetOrdersColumnVisible(Report.GetFactionStatus(faction.FactionId) is FactionStatus.ACTIVE or FactionStatus.ALLIED);
            SetSelectionContext(faction.Region, faction);
            SetUnits(GetUnitIdsForFaction(faction));
            SetSelectedUnit(null);
            RefreshConfirmedStatuses();
            SendSelectionChangedEvent(new EntitySelection(null, faction.Region, locatedFaction: faction));
        }

        private void ResetLocatedFactionNavigation()
        {
            _locatedFactionsInRegion.Clear();
            LocatedFactionIndex = -1;
            OnPropertyChanged(nameof(ShowFirstLastLocatedFactionButtons));
            NotifyFactionNavigationCommands();
        }

    }
}