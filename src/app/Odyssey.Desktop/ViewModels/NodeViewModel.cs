using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Tools;
using System.Collections.ObjectModel;

namespace Odyssey.ViewModels;

public partial class NodeViewModel : ObservableObject
{
    private ObservableCollection<NodeViewModel>? _children;

    [ObservableProperty]
    private DataProperty? _data;

    private string? _description;

    public NodeViewModel()
    {
        _children = null;
        _description = null;
        Data = null;
        Parent = null;
        Header = string.Empty;
    }
    public NodeViewModel(NodeViewModel parent, DataProperty? data, int index = -1)
    {
        // TODO: what about data value ? better handling
        Data = data;
        Parent = parent;
        Header = data != null ? data.Label : string.Empty;
        string? desc = data?.Description;
        // In case the description is used as a tooltip value, empty string as to be converted to null,
        // otherwise the tooltip popup is rendered as an empty popup.
        _description = (desc == string.Empty || desc == "") ? null : desc;
        parent.Children.Insert(index != -1 ? index : parent.Children.Count, this);
    }
    public NodeViewModel? Parent { get; }
    public string Header { get; protected set; }
    /// <summary>
    /// The description of the property linked the node.
    /// Has to be a null value when not used, because a tooltip popup is rendred as an empty popup when its Tip value is string.Empty or "".
    /// </summary>
    public string? Description { get { return string.IsNullOrEmpty(_description) ? null : _description; } }
    public ObservableCollection<NodeViewModel> Children => _children ??= [];
    public bool HasChildren() => Children.Count > 0;

#if DEBUG
    public override string ToString() => $"[{Header}]";
#endif
}

