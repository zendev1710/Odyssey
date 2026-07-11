using Avalonia.Controls;
using AvaloniaEdit.Editing;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System.Collections.Generic;
using System.Linq;

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

        public ExplorerNodeSelection(ExplorerNodeViewModel? nodeViewModel)
        {
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
            NodeViewModelSubtree = ExplorerNodeViewModel.GetNodeSubTree(nodeViewModel, true, true, false).ToList();
            RegionNodeViewModel = NodeViewModelSubtree.LastOrDefault();
            DataBlock? item = nodeViewModel.Block;
            bool isUnit = item?.GetBlockType() == BlockType.UNIT;
            DataBlock? faction = isUnit && NodeViewModelSubtree.Count >= 2 ? NodeViewModelSubtree.ElementAtOrDefault(1)?.Block : null;

            NodeViewModel = nodeViewModel;
            SetItem(item, RegionNodeViewModel?.Block, faction);
        }
    }
}
