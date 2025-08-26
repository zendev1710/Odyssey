using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Odyssey.Views.Documents;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
