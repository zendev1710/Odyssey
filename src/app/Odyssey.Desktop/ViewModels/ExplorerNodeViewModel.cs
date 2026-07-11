using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Odyssey.ViewModels;

public partial class ExplorerNodeViewModel : ObservableObject
{
    private ObservableCollection<ExplorerNodeViewModel>? _children;

    [ObservableProperty]
    private bool isExpanded;

    public ExplorerNodeViewModel? Parent { get; }
    public string Header { get; }
    public object? AdditionalInfo { get; }
    public DataBlock? Block { get; private set; }
    public bool Unconfirmed { get; set; }
    public ObservableCollection<ExplorerNodeViewModel> Children => _children ??= [];
    public bool HasChildren() => Children.Count > 0;

    public override string ToString() => Header;

    public ExplorerNodeViewModel()
    {
        _children = null;
        Parent = null;
        Header = "";
        Block = null;
        AdditionalInfo = null;
        Unconfirmed = false;
    }
    public ExplorerNodeViewModel(ExplorerNodeViewModel parent, string header, DataBlock? block, bool unconfirmed = false, object? additionalInfo = null, int index = -1)
    {
        Parent = parent;
        Header = header;
        Block = block;
        AdditionalInfo = additionalInfo;
        if (unconfirmed && block?.GetBlockType() == BlockType.UNIT)
        {
            Unconfirmed = true;
        }
        parent.Children.Insert(index != -1 ? index : parent.Children.Count, this);

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
    }

    static public ExplorerNodeViewModel? GetAncestor(ExplorerNodeViewModel node)
    {
        ExplorerNodeViewModel? parent = node?.Parent;
        ExplorerNodeViewModel? ancestor = null;
        while (parent != null)
        {
            ancestor = parent;
            parent = parent.Parent;
        }

        return ancestor;
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

