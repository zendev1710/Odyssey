using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Odyssey.ViewModels;

namespace Odyssey.Views;

public partial class BuildingsDetailsView : UserControl
{
    public BuildingsDetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TreeView_DoubleTapped(object? sender, TappedEventArgs e)
    {
        // Double-click was on a TreeView item (NodeViewModel): select the corresponding building
        // so that the details view displays the building details.
        if (DataContext is BuildingsDetailsViewModel vm && e.Source is IDataContextProvider s && s.DataContext is NodeViewModel node)
        {
            vm.DispatchSelectedNode(node);
        }
    }
}
