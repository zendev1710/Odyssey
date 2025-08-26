using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Odyssey.ViewModels.Tools;

namespace Odyssey.Views.Tools;

public partial class RegionInfoView : UserControl
{
    public RegionInfoView()
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
        if (DataContext is MessagesViewModel vm && e.Source is IDataContextProvider s && s.DataContext is MessagesViewModel.Node node)
        {
            // Double-click was on a TreeView item (MessagesViewModel.Node)
            vm.DispatchSelectedNode(node);
        }
    }
}
