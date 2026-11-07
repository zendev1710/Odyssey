using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
}
