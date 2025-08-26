using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Odyssey.ViewModels;
using HarfBuzzSharp;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Odyssey.Views.Tools;

public partial class ExplorerView : UserControl
{
    public ExplorerView()
    {
        InitializeComponent();

        TreeView = this.FindControl<TreeView>("TreeView");
        TreeView?.AddHandler(KeyDownEvent, TreeView_KeyDown, RoutingStrategies.Tunnel);
        TreeView?.AddHandler(PointerPressedEvent, TreeView_PointerPressed, RoutingStrategies.Tunnel);
        TreeView?.AddHandler(ContextRequestedEvent, TreeView_ContextRequested, RoutingStrategies.Tunnel);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private static void ExpandAll(TreeView tree)
    {
        foreach (var i in tree.GetRealizedContainers())
        {
            tree.ExpandSubTree((TreeViewItem)i);
        }
    }

    private static bool FindSelectedItem(TreeView treeview, TreeViewItem? item, List<TreeViewItem?> tviSubTree, int depth, char letter, out TreeViewItem? matchingItem)
    {
        matchingItem = null;
        if (item == null)
        {
            return false;
        }
        if (item != tviSubTree[depth])
        {
            return false;
        }

        // Ancestor found at the current depth
        depth++;
        if (depth >= tviSubTree.Count)
        {
            /*
            // item is selectedItem, so next item starting with letter can be searched now
            if (FindMatchingItem(treeview, item, letter, out matchingItem))
            {
                return true;
            }
            */
        }
        /*
        foreach (var treeviewItem in item.GetRealizedContainers())
        {
            if (item == selectedItem)
            {
                selectedItem = item;
                return true;
            }
        }
        */

        /*
        if (item == selectedItem)
        {
            selectedItem = item;
            return true;
        }

        foreach (var treeviewItem in item.GetRealizedContainers())
        {
            if (item == selectedItem)
            {
                selectedItem = item;
                return true;
            }
        }
        */
        // TODO:
        TreeViewItem? tvi = null;
        var node = treeview.TreeItemFromContainer(tvi) as ExplorerNodeViewModel;
        if (node != null && node.Header.ToLower().StartsWith(letter))
        {
            // this item has to be focused, as its label starts with the pressed key
            matchingItem = tvi;
            //break;
        }

        return false;
    }

    private static bool ItemLabelStartsWith(TreeView treeview, TreeViewItem? tvi, char letter)
    {
        return tvi != null && treeview.TreeItemFromContainer(tvi) is ExplorerNodeViewModel node && node.Header.ToLower().StartsWith(letter);
    }

    private static bool FindMatchingItemInChildren(TreeView treeview, TreeViewItem? tvi, TreeViewItem? childTvi, char letter, out TreeViewItem? matchingItem)
    {
        matchingItem = null;
        if (tvi == null)
        {
            return false;
        }
        bool childFound = childTvi == null;
        foreach (var c in tvi.GetRealizedContainers())
        {
            if (c is TreeViewItem child)
            {
                if (!childFound)
                {
                    // we are looking for the childTvi, so skip all other children
                    if (childTvi == child)
                    {
                        childFound = true;
                    }
                    continue;
                }
                if (ItemLabelStartsWith(treeview, child, letter))
                {
                    // this item has to be focused, as its label starts with the pressed key
                    matchingItem = child;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool FindMatchingItem(TreeView treeview, TreeViewItem? tvi, TreeViewItem? childTvi, char letter, out TreeViewItem? matchingItem)
    {
        matchingItem = null;
        if (tvi == null)
        {
            return false;
        }
        var node = treeview.TreeItemFromContainer(tvi) as ExplorerNodeViewModel;
        if (node != null && node.Header.ToLower().StartsWith(letter))
        {
            // this item has to be focused, as its label starts with the pressed key
            matchingItem = tvi;
            return true;
        }

        return false;
    }

    private void TreeView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key >= Key.A && e.Key <= Key.Z && TreeView != null)
        {
            List<TreeViewItem?> tviSubTree = [];
            IEnumerable<ExplorerNodeViewModel> subTree;
            char letter = e.Key.ToString().ToLower()[0];
            var selectedItem = TreeView.SelectedItem as ExplorerNodeViewModel;
            if (selectedItem != null) {
                subTree = ExplorerNodeViewModel.GetNodeSubTree(selectedItem, true, true, true);
                foreach (var a in subTree)
                {
                    tviSubTree.Add(TreeView.TreeContainerFromItem(a) as TreeViewItem);
                }
            }
            else
            {
                tviSubTree.Add(TreeView.ContainerFromIndex(0) as TreeViewItem);
            }

            TreeViewItem? matchingItem = null;
            TreeViewItem? childTvi = null;
            for (int depth= 0; depth < tviSubTree.Count; depth++)
            {
                TreeViewItem? current = tviSubTree[depth];
                if (FindMatchingItem(TreeView, current, childTvi, letter, out matchingItem))
                {
                    break;
                }
                childTvi = current;
            }

            // TODO
            /*
            IEnumerable<Control>? rootChildren = TreeView.GetRealizedContainers();
            foreach (var item in rootChildren)
            {
                if (item is TreeViewItem treeviewItem)
                {
                    if (FindSelectedItem(TreeView, treeviewItem, tviSubTree, 0, letter, out matchingItem))
                    {
                        break;
                    }
                    continue;
                }
                break;

                if (treeviewItem is TreeViewItem tvi)
                {
                    if (fromFound)
                    {
                        if (tvi != fromTreeViewItem)
                        {
                            var node = TreeView.TreeItemFromContainer(tvi) as ExplorerNodeViewModel;
                            if (node != null && node.Header.ToLower().StartsWith(letter))
                            {
                                // this item has to be focused, as its label starts with the pressed key
                                matchingElement = tvi;
                                break;
                            }
                        }
                        foreach (var child in tvi.GetRealizedContainers())
                        {
                            if (child is TreeViewItem childtvi)
                            {
                                var childnode = TreeView.TreeItemFromContainer(childtvi) as ExplorerNodeViewModel;
                                if (childnode != null && childnode.Header.ToLower().StartsWith(letter))
                                {
                                    // this item has to be focused, as its label starts with the pressed key
                                    matchingElement = childtvi;
                                    break;
                                }
                            }
                        }
                        if (matchingElement != null)
                        {
                            break;
                        }
                    }
                    // Traverse the visible tree items
                    continue;
                }
                break;
            }
            */

            if (matchingItem != null)
            {
                var item = TreeView!.TreeItemFromContainer(matchingItem);
                TreeView!.SelectedItem = item;
                e.Handled = true;
            }
        }

        /*
        // NOTE: Avalonia issue to be fixed: https://github.com/AvaloniaUI/Avalonia/issues/11787
        if (e.Key == Key.Enter && TreeView?.SelectedItem != null)
        {
            var item = TreeView?.SelectedItem;
            // if item is a leaf set it's already handled
            //if (item is ExpectedType)
            //{
            //    e.Handled = true;
            //}
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                // TODO: find next item starting with the key
            }
        }
        */
    }

    private void TreeView_ContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        /*
        if (e.Pointer.Type != PointerType.Mouse)
            return;

        var tree = this.FindControl<TreeView>("TreeView");
        var point = e.GetCurrentPoint(tree);

        if (point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            TreeViewItem? tvi = ((Control?)e.Source)?.GetVisualAncestors().OfType<TreeViewItem>().First();
            // Do work
            e.Handled = true;
        }
        */
    }

    private void TreeView_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type != PointerType.Mouse)
            return;

        var tree = this.FindControl<TreeView>("TreeView");
        var point = e.GetCurrentPoint(tree);

        if (point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
        {
            TreeViewItem? tvi = ((Control?)e.Source)?.GetVisualAncestors().OfType<TreeViewItem>().First();
            // TODO: customize contextual menu
            e.Handled = true;
        }
    }

}
