using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.ViewModels;
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
                int factionId = CRDocument.GetFactionIdForUnit(unit.DataBlock);
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
            RefreshLocatedFactions(region, selectedNode, selectedFaction);
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
                    if (!TryGetUnitIdsFromExplorer(selectedNode, selectedFaction, out List<int> unitIds))
                    {
                        unitIds = GetUnitIdsForFaction(selectedFaction);
                    }
                    StripViewModel.SetUnits(unitIds);
                }
            }
            else if (region is not null)
            {
                StripViewModel.SetUnits(Report.GetUnitIdsInRegion(region.Id));
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

        private static bool TryGetUnitIdsFromExplorer(
            ExplorerNodeViewModel? selectedNode, LocatedFactionModel faction, out List<int> unitIds)
        {
            unitIds = [];
            ExplorerNodeViewModel? regionNode = FindRegionNode(selectedNode);
            if (regionNode is null || regionNode.Id != faction.Region.Id)
            {
                return false;
            }

            ExplorerNodeViewModel? factionNode = regionNode.Children.FirstOrDefault(node =>
                node.NodeType == ExplorerNodeViewModel.NodeTypes.FACTION &&
                node.LocatedFaction is LocatedFactionModel locatedFaction &&
                IsSameLocatedFaction(locatedFaction, faction));
            ExplorerNodeViewModel unitsParent = factionNode ?? regionNode;

            unitIds = unitsParent.Children
                .Where(node => node.NodeType == ExplorerNodeViewModel.NodeTypes.UNIT &&
                               node.LocatedFaction is LocatedFactionModel locatedFaction &&
                               IsSameLocatedFaction(locatedFaction, faction))
                .Select(node => node.Id)
                .ToList();
            return factionNode is not null || unitIds.Count > 0;
        }

        private List<int> GetUnitIdsForFaction(LocatedFactionModel faction)
        {
            return Report.GetUnitIdsInRegion(faction.Region.Id)
                .Where(unitId => Report.GetUnit(unitId) is UnitModel unit &&
                                 CRDocument.GetFactionIdForUnit(unit.DataBlock) == faction.FactionId)
                .ToList();
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

        private void RefreshLocatedFactions(RegionModel? region, ExplorerNodeViewModel? selectedNode, LocatedFactionModel? selectedFaction)
        {
            _locatedFactionsInRegion.Clear();
            if (region is not null)
            {
                ExplorerNodeViewModel? regionNode = FindRegionNode(selectedNode);
                if (regionNode is not null)
                {
                    CollectLocatedFactions(regionNode, region);
                }

                foreach (int unitId in Report.GetUnitIdsInRegion(region.Id))
                {
                    if (Report.GetUnit(unitId) is UnitModel unit)
                    {
                        int factionId = CRDocument.GetFactionIdForUnit(unit.DataBlock);
                        AddLocatedFactionIfMissing(new LocatedFactionModel(
                            Report.GetFaction(factionId), region, factionId, Report.GetFactionName(factionId)));
                    }
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
            if (!TryGetUnitIdsFromExplorer(CurrentSelection.Data as ExplorerNodeViewModel, faction, out List<int> unitIds))
            {
                unitIds = GetUnitIdsForFaction(faction);
            }
            StripViewModel.SetUnits(unitIds);
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

        private static ExplorerNodeViewModel? FindRegionNode(ExplorerNodeViewModel? node)
        {
            while (node is not null && node.NodeType != ExplorerNodeViewModel.NodeTypes.REGION)
            {
                node = node.Parent;
            }

            return node;
        }

        private void CollectLocatedFactions(ExplorerNodeViewModel node, RegionModel region)
        {
            if (node.LocatedFaction is LocatedFactionModel faction && faction.Region.Id == region.Id)
            {
                AddLocatedFactionIfMissing(faction);
            }

            foreach (ExplorerNodeViewModel child in node.Children)
            {
                CollectLocatedFactions(child, region);
            }
        }
    }
}