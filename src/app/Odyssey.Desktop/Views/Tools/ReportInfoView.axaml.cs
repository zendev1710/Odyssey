using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Odyssey.ViewModels.Tools;

namespace Odyssey.Views.Tools;

public partial class ReportInfoView : UserControl
{
    public ReportInfoView()
    {
        InitializeComponent();

        // LATER: why Name XAML property, auto-generated in partial code-behin, is null here (Messages) 
        var messages = this.Find<ListBox>("Messages");
        messages!.KeyDown += ListBox_KeyDown;
        messages!.DoubleTapped += ListBox_DoubleTapped;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ListBox_KeyDown(object? sender, KeyEventArgs e)
    {
        // TODO: SHIFT+F10 => should display the dynamic contextual menu (as mouse right click)
        if (e.Key == Key.Enter || e.Key == Key.Space && DataContext is MessagesListViewModel vm)
        {
            // SPACE key pressed => select related item in the Explorer view
            RevealSelectedItemInExplorer();
            e.Handled = true;
        }
    }

    private void ListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        RevealSelectedItemInExplorer();
    }

    private void RevealSelectedItemInExplorer()
    {
        if (DataContext is MessagesListViewModel vm)
        {
            // LATER: better to send a command
            vm.RevealSelectedItemInExplorer();
        }
    }
}
