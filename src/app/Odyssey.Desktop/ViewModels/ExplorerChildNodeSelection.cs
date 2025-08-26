using Avalonia.Controls;
using AvaloniaEdit.Editing;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Odyssey.ViewModels
{
    public class ExplorerChildNodeSelection : SimpleItemSelection
    {
        public ExplorerNodeViewModel? RegionNodeViewModel { get; protected set; }

        public ExplorerChildNodeSelection()
        {
            // Default constructor
        }

        public ExplorerChildNodeSelection(ExplorerNodeViewModel regionNodeViewModel, DataBlock? item)
        {
            RegionNodeViewModel = regionNodeViewModel;
            SetItem(item, RegionNodeViewModel?.Block, null);
        }
    }
}
