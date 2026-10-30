using CommunityToolkit.Mvvm.ComponentModel;
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
        }

        protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
        {
            // If event come from this viewModel, ignore it
            if (ReferenceEquals(sender, this)) return;
            if (!HasDocument)
            {
                LocatedFaction = null;
                StripViewModel.SetUnits([]);
                StripViewModel.SetSelectionContext(null, null);
                StripViewModel.SetSelectedUnit(null);
                StripViewModel.SetOrdersColumnVisible(true);
                return;
            }

            LocatedFactionModel? selectedFaction = selection.LocatedFaction;
            if (selection.HasUnit() && selectedFaction is null &&
                selection.Item is UnitModel unit && selection.Region is RegionModel unitRegion)
            {
                int factionId = CRDocument.GetFactionIdForUnit(unit.DataBlock);
                selectedFaction = new LocatedFactionModel(
                    Report.GetFaction(factionId), unitRegion, factionId, Report.GetFactionName(factionId));
            }
            else if (selectedFaction is null && selection.Data is ExplorerNodeViewModel selectedNode)
            {
                selectedFaction = FindFirstLocatedFactionForSelection(selectedNode);
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
                    if (!TryGetUnitIdsFromExplorer(selection.Data as ExplorerNodeViewModel, selectedFaction, out List<int> unitIds))
                    {
                        unitIds = GetUnitIdsForFaction(selectedFaction);
                    }
                    StripViewModel.SetUnits(unitIds);
                }
            }
            else if (selection.Region is RegionModel selectedRegion)
            {
                StripViewModel.SetUnits(Report.GetUnitIdsInRegion(selectedRegion.Id));
            }
            else
            {
                StripViewModel.SetUnits([]);
            }

            StripViewModel.SetSelectedUnit(selection.Item is UnitModel selectedUnit ? selectedUnit.Id : null);
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
            ExplorerNodeViewModel? regionNode = selectedNode;
            while (regionNode is not null && regionNode.NodeType != ExplorerNodeViewModel.NodeTypes.REGION)
            {
                regionNode = regionNode.Parent;
            }

            if (regionNode is null)
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
            return true;
        }

        private List<int> GetUnitIdsForFaction(LocatedFactionModel faction)
        {
            return Report.GetUnitIdsInRegion(faction.Region.Id)
                .Where(unitId => Report.GetUnit(unitId) is UnitModel unit &&
                                 CRDocument.GetFactionIdForUnit(unit.DataBlock) == faction.FactionId)
                .ToList();
        }
    }
}