using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Odyssey.ViewModels;

namespace Odyssey.Views;

public partial class UnitDetailsView : UserControl
{
    public UnitDetailsView()
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
        if (DataContext is UnitDetailsViewModel vm && e.Source is IDataContextProvider s && s.DataContext is NodeViewModel node)
        {
            // Double-click on a TreeView item
            vm.DispatchSelectedNode(node);
        }
    }
}
