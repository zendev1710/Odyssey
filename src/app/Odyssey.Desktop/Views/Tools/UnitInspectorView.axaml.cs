using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Odyssey.Views.Tools
{
    public partial class UnitInspectorView : UserControl
    {
        public UnitInspectorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
