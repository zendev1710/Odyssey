using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Odyssey.ViewModels;

namespace Odyssey.Views;

public partial class UnitDetailsView : UserControl
{
    public UnitDetailsView()
    {
        InitializeComponent();

        var treeView = this.FindControl<TreeView>("DetailsTreeView");
        treeView?.AddHandler(PointerPressedEvent, TreeView_PointerPressed, RoutingStrategies.Tunnel);
        treeView?.AddHandler(KeyDownEvent, TreeView_KeyDown, RoutingStrategies.Tunnel);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TreeView_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            sender is TreeView treeView &&
            treeView.SelectedItem is NodeViewModel node &&
            DataContext is UnitDetailsViewModel vm &&
            node.Data?.Reference is not null)
        {
            vm.DispatchSelectedNode(node);
            e.Handled = true;
        }
    }

    private void TreeView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter &&
            e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
            sender is TreeView treeView &&
            treeView.SelectedItem is NodeViewModel node &&
            DataContext is UnitDetailsViewModel vm &&
            node.Data?.Reference is not null)
        {
            vm.DispatchSelectedNode(node);
            e.Handled = true;
        }
    }
}
