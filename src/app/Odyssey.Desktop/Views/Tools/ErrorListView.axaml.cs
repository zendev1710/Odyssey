using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Odyssey.Views.Tools;

public partial class ErrorListView : UserControl
{
    public ErrorListView()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}