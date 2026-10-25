using Odyssey.Models;
using Odyssey.Models.Documents;
using System.Collections.Generic;
using System.Linq;
using static Odyssey.Models.Documents.EntitySelection;
using static Odyssey.ViewModels.ExplorerNodeViewModel;

namespace Odyssey.ViewModels
{
    public class ExplorerNodeSelection : ExplorerChildNodeSelection
    {
        private List<ExplorerNodeViewModel> NodeViewModelSubtree = [];

        public ExplorerNodeViewModel? NodeViewModel { get; private set; }

        public ExplorerNodeViewModel? GetNodeViewModelAt(int range)
        {
            return range >= 0 && range < NodeViewModelSubtree.Count ? NodeViewModelSubtree.ElementAtOrDefault(range) : null;
        }

        public ExplorerNodeSelection(ExplorerNodeViewModel? nodeViewModel, ILocationResolver? locationResolver = null)
        {
            LocationResolver = locationResolver;
            if (nodeViewModel is not null)
            {
                Init(nodeViewModel);
            }
            else
            {
                // Layout deserialization: ExplorerDockSerializer does not restore
                // the ViewModel reference directly.  The explorer will rebuild its
                // tree when OnActiveDocumentChanged fires on a fresh CRDocument, at
                // which point the selection will be rebuilt correctly.
                SetItem(null, null, null);
            }
        }

        private void Init(ExplorerNodeViewModel nodeViewModel)
        {
            NodeViewModelSubtree = [.. ExplorerNodeViewModel.GetNodeSubTree(nodeViewModel, true, true, false)];
            RegionNodeViewModel = NodeViewModelSubtree.LastOrDefault();
            NodeViewModel = nodeViewModel;
            SetItemFromId();
        }

        /// <summary>
        /// Resolves the selected region and item models from the node ids using the location resolver,
        /// then updates the selection. The selected <see cref="ExplorerNodeViewModel"/> is stored in
        /// <see cref="EntitySelection.Data"/>.
        /// </summary>
        private void SetItemFromId()
        {
            EntityModel? item = null;
            RegionModel? region = null;

            if (LocationResolver is not null)
            {
                // The region node id is the region's id (and location id), so it can be resolved directly.
                if (RegionNodeViewModel is not null &&
                    LocationResolver.ResolveItem(RegionNodeViewModel.Id, out EntityModel? regionItem))
                {
                    region = regionItem as RegionModel;
                }

                if (NodeViewModel is not null)
                {
                    switch (NodeViewModel.NodeType)
                    {
                        case NodeTypes.REGION:
                            item = region;
                            break;

                        // Container nodes (buildings/ships lists) have no matching entity:
                        // select them as a container of the region (the Details view displays the list).
                        case NodeTypes.BUILDINGS:
                            SetContainer(SelectionTypes.Buildings, region, NodeViewModel);
                            return;
                        case NodeTypes.SHIPS:
                            SetContainer(SelectionTypes.Ships, region, NodeViewModel);
                            return;

                        default:
                            LocationResolver.ResolveItem(NodeViewModel.Id, out item);
                            break;
                    }

                    // Faction nodes have no matching entity: fall back to a region-only selection.
                    item ??= region;
                }
            }

            SetItem(item, region, NodeViewModel);
        }
    }
}
