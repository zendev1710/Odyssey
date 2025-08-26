using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Odyssey.Views.Tools;

public partial class DetailsView : UserControl
{
    public DetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
