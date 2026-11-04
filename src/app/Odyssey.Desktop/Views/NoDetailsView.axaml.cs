using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Odyssey.Views;

public partial class NoDetailsView : UserControl
{
    public NoDetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
