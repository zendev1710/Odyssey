using Avalonia.Controls;
using AvaloniaEdit.Editing;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System;
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

        public ExplorerNodeSelection(ExplorerNodeViewModel nodeViewModel)
        {
            Init(nodeViewModel);
        }

        private void Init(ExplorerNodeViewModel nodeViewModel)
        {
            // FIXME: nodeViewModel is null when loading from OpenLayout() deserialization
            if (nodeViewModel is null)
            {
                return;
            }

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
