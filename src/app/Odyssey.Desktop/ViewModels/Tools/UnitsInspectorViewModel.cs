using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Core.Services;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Odyssey.ViewModels.Tools
{
    /// <summary>
    /// ViewModel for the units overview, which presents each unit with its summary.
    /// </summary>
    public partial class UnitsInspectorViewModel : DocumentToolViewModelBase
    {
        public UnitStripViewModel StripViewModel { get; }

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
            StripViewModel = new UnitStripViewModel();
            ServiceLocator.UnitNavigationOrderService.OrderChanged += OnUnitNavigationOrderChanged;
        }

        private void OnUnitNavigationOrderChanged(object? sender, EventArgs e)
        {
            if (!HasDocument || !ReferenceEquals(ServiceLocator.UnitNavigationOrderService.Report, Report))
            {
                return;
            }

            if (LocatedFaction is not null)
            {
                StripViewModel.SetUnits(GetUnitIdsForFaction(LocatedFaction));
            }
            else if (CurrentSelection.Region is RegionModel region)
            {
                StripViewModel.SetUnits(GetUnitIdsForRegion(region));
            }

            StripViewModel.RefreshConfirmedStatuses();
        }

        protected override void OnActiveDocumentChanged(CRDocument report)
        {
            base.OnActiveDocumentChanged(report);
            LocatedFaction = null;
            StripViewModel.SetUnits([]);
            StripViewModel.SetSelectionContext(null, null);
            StripViewModel.SetSelectedUnit(null);
            StripViewModel.SetOrdersColumnVisible(true);
            ResetLocatedFactionNavigation();
        }

        protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
        {
            if (ReferenceEquals(sender, this)) return;
            if (!HasDocument)
            {
                LocatedFaction = null;
                StripViewModel.SetUnits([]);
                StripViewModel.SetSelectionContext(null, null);
                StripViewModel.SetSelectedUnit(null);
                StripViewModel.SetOrdersColumnVisible(true);
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
            StripViewModel.SetOrdersColumnVisible(showOrdersColumn);
            StripViewModel.SetSelectionContext(selectedFaction?.Region ?? selection.Region, selectedFaction);

            bool factionChanged = !IsSameLocatedFaction(LocatedFaction, selectedFaction);
            if (factionChanged)
            {
                LocatedFaction = selectedFaction;
            }

            if (selectedFaction is not null)
            {
                if (factionChanged)
                {
                    StripViewModel.SetUnits(GetUnitIdsForFaction(selectedFaction));
                }
            }
            else if (region is not null)
            {
                StripViewModel.SetUnits(GetUnitIdsForRegion(region));
            }
            else
            {
                StripViewModel.SetUnits([]);
            }

            StripViewModel.SetSelectedUnit(selection.Item is UnitModel selectedUnit ? selectedUnit.Id : null);
            StripViewModel.RefreshConfirmedStatuses();
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
            StripViewModel.SetOrdersColumnVisible(Report.GetFactionStatus(faction.FactionId) is FactionStatus.ACTIVE or FactionStatus.ALLIED);
            StripViewModel.SetSelectionContext(faction.Region, faction);
            StripViewModel.SetUnits(GetUnitIdsForFaction(faction));
            StripViewModel.SetSelectedUnit(null);
            StripViewModel.RefreshConfirmedStatuses();
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