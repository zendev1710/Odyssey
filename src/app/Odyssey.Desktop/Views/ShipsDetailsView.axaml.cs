using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
}
