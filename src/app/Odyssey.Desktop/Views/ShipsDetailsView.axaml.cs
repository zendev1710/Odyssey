using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Odyssey.ViewModels;

namespace Odyssey.Views;

public partial class ShipsDetailsView : UserControl
{
    public ShipsDetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TreeView_DoubleTapped(object? sender, TappedEventArgs e)
    {
        // Double-click was on a TreeView item (NodeViewModel): select the corresponding ship
        // so that the details view displays the ship details.
        if (DataContext is ShipsDetailsViewModel vm && e.Source is IDataContextProvider s && s.DataContext is NodeViewModel node)
        {
            vm.DispatchSelectedNode(node);
        }
    }
}
