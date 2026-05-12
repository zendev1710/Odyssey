using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Odyssey.Views.Tools;

public partial class UnitStripView : UserControl
{
    public UnitStripView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Ensure XAML is loaded even if XAML code-gen is not present
        AvaloniaXamlLoader.Load(this);
    }
}