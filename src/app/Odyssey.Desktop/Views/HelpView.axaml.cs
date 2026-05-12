// =============================================
// 4. HelpView.axaml.cs - Code-behind
// =============================================
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaWebView;
using Odyssey.Core.Help.Models;
using Odyssey.Core.Services;
using Odyssey.Help;
using Odyssey.ViewModels;

namespace Odyssey.Views
{
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            InitializeComponent();

            //DataContext = new HelpViewModel(ServiceLocator.LanguageService);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
