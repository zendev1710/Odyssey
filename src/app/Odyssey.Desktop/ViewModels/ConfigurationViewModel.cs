using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Models;
using Odyssey.Core.Services;
using Odyssey.Settings;

namespace Odyssey.ViewModels
{
    public partial class ConfigurationViewModel : ClientAreaDockBase
    {
        [ObservableProperty]
        private ConfigPageModel _configPageModel;

        public ConfigurationViewModel(IDocumentService? documentService = null) : base(documentService)
        {
            _configPageModel = ConfigFactory.ConfigFactory.Build<GlobalSettings>();
            _configPageModel.PropertyChanged += _configPageModelPropertyChanged;
        }

        private void _configPageModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO
        }

        public void GoBackToWorkspace()
        {
            // TODO: handle Esc key to go back to workspace
            base.GoBackToWorkspaceCommand.Execute(null);
        }
    }
}
