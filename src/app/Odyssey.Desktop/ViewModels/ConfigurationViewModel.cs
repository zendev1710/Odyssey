using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Models;
using Odyssey.Services;
using Odyssey.Settings;
using Prism.Events;

namespace Odyssey.ViewModels
{
    public partial class ConfigurationViewModel : ClientAreaDockBase
    {
        [ObservableProperty]
        private ConfigPageModel _configPageModel;

        public ConfigurationViewModel(IActiveDocumentService? documentService = null) : base(documentService)
        {
            _configPageModel = ConfigFactory.ConfigFactory.Build<GlobalSettings>();
            _configPageModel.PropertyChanged += _configPageModelPropertyChanged;
        }

        private void _configPageModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO
        }

        public void GoHome()
        {
            base.GoHomeCommand.Execute(null);
        }
    }
}
