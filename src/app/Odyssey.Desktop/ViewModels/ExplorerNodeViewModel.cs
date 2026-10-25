using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Odyssey.ViewModels;


// selected flags

public partial class ExplorerNodeViewModel : ObservableObject
{
    public enum NodeTypes
    {
        NONE = 0,
        REGION = (1 << 0),
        FACTION = (1 << 2),
        BUILDING = (1 << 3),
        SHIP = (1 << 4),
        UNIT = (1 << 5),
        BUILDINGS = (1 << 6),
        SHIPS = (1 << 7),
    };

    private ObservableCollection<ExplorerNodeViewModel>? _children;

    [ObservableProperty]
    private bool isExpanded;

    private readonly int _id;

    public int Id { get { return _id; } }
    public NodeTypes NodeType { get; }
    public ExplorerNodeViewModel? Parent { get; }
    public string Header { get; }
    public object? AdditionalInfo { get; }
    public string? IconUri { get; }
    public string? RaceIconUri { get; }
    public string? StatusIconUri { get; }
    public object? IconSource => IconUri ?? AdditionalInfo;
    public bool Hightlighted { get; private set; }
    public ObservableCollection<ExplorerNodeViewModel> Children => _children ??= [];
    public bool HasChildren() => Children.Count > 0;

    public override string ToString() => Header;

    public ExplorerNodeViewModel()
    {
        _children = null;
        Parent = null;
        Header = "";
        _id = -1;
        NodeType = NodeTypes.NONE;
        AdditionalInfo = null;
        IconUri = null;
        StatusIconUri = null;
        Hightlighted = false;
    }
    public ExplorerNodeViewModel(ExplorerNodeViewModel parent, string header, int id, NodeTypes nodeType, bool hightlighted, object? additionalInfo = null, int index = -1, string? iconUri = null, string? raceIconUri = null, string? statusIconUri = null)
    {
        Parent = parent;
        Header = header;
        _id = id;
        NodeType = nodeType;
        AdditionalInfo = additionalInfo;
        IconUri = iconUri;
        RaceIconUri = raceIconUri;
        StatusIconUri = statusIconUri;
        Hightlighted = hightlighted;

        /*
        if (unconfirmed && block?.GetBlockType() == BlockType.UNIT)
        {
            Unconfirmed = true;
        }*/

        parent.Children.Insert(index != -1 ? index : parent.Children.Count, this);

        HighlightParentNodes(hightlighted);
        /*
        if (unconfirmed) 
        { 
            ExplorerNodeViewModel? item = parent;
            while (item != null)
            {
                if (item.Block?.GetBlockType() == BlockType.REGION)
                {
                    item.Unconfirmed = true;
                }
                item = item?.Parent;
            }
        }
        */
    }

    private void HighlightParentNodes(bool hightlight)
    {
        if (hightlight)
        {
            ExplorerNodeViewModel? item = Parent;
            while (item != null)
            {
                //if (item.Block?.GetBlockType() == BlockType.REGION)
                {
                    item.Hightlighted = hightlight;
                }
                item = item?.Parent;
            }
        }
    }

    /// <summary>
    /// Gets the subtree from the higher parent to the specified node.
    /// </summary>
    /// <param name="node">the node for which the subtree is to be retrieved</param>
    /// <param name="sortFromChildToParent">Whether the subtree elements are sorted from child to parent</param>
    /// <param name="includeNode">Whether the specified node is included to the returned subtree</param>
    /// <param name="includeRoot">Whether the root node is included in the returned subtree</param>
    /// <returns>Subtree as an <see cref="IEnumerable"/> sorted according to tje <paramref name="olderFirst"/></returns>
    static public IEnumerable<ExplorerNodeViewModel> GetNodeSubTree(ExplorerNodeViewModel node, bool sortFromChildToParent, bool includeNode, bool includeRoot)
    {
        List<ExplorerNodeViewModel> ancestors = includeNode ? [node] : [];
        ExplorerNodeViewModel? parent = node?.Parent;
        while (parent != null && (includeRoot || parent.Parent != null))
        {
            ancestors.Add(parent);
            parent = parent.Parent;
        }
        if (!sortFromChildToParent)
        {
            ancestors.Reverse();
        }
        return ancestors;
    }
}

