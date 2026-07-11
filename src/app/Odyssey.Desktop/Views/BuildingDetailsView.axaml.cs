using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Odyssey.ViewModels;
using Odyssey.ViewModels.Tools;

namespace Odyssey.Views;

public partial class BuildingDetailsView : UserControl
{
    public BuildingDetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TreeView_DoubleTapped(object? sender, TappedEventArgs e)
    {
        OnDoubleTapped(e);
    }

    private void OnDoubleTapped(/*object? sender,*/ TappedEventArgs e)
    {
        // Handle the double-tap event
        if (DataContext is BuildingDetailsViewModel vm && e.Source is IDataContextProvider s && s.DataContext is NodeViewModel node)
        {
            // Double-click was on a TreeView item (NodeViewModel)
            vm.DispatchSelectedNode(node);
        }
    }
}
