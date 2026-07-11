using Avalonia;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Tools;
using System;
using static Odyssey.Models.Tools.DataProperty;
using static Odyssey.Models.ShipModel;

namespace Odyssey.ViewModels;

public partial class TreeNodeViewModel : NodeViewModel
{
    public TreeNodeViewModel()
    {
    }

    public TreeNodeViewModel(NodeViewModel parent, DataProperty? data, string header = "", int index = -1) : base(parent, data, index)
    {
        if (!string.IsNullOrEmpty(header))
        {
            Header = header;
        }

        if (data == null)
        {
            Data = new DataProperty(header);
        }
    }
}


