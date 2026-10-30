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
        AvaloniaXamlLoader.Load(this);
    }
}