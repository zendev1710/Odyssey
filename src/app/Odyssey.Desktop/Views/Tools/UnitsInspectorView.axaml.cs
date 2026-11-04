using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Odyssey.Views.Tools
{
    public partial class UnitsInspectorView : UserControl
    {
        public UnitsInspectorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // ensure XAML is loaded even if codegen is disabled
            AvaloniaXamlLoader.Load(this);
        }
    }
}