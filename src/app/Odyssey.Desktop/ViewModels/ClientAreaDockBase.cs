using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;
using Odyssey.Core.Services;
using Odyssey.Models.Documents;
using Odyssey.Services;

namespace Odyssey.ViewModels
{
    /// <summary>
    /// Dock base view model for a view displayed in the whole app client area.
    /// </summary>
    public abstract partial class ClientAreaDockBase: DockBase
    {
        [ObservableProperty]
        private bool hasDocument;

        private readonly IDocumentService _documentService;

        protected CRDocument Report => _documentService.ActiveDocument;

        /// <summary>
        /// Constructor for Avalonia XAML Designer.
        /// </summary>
        protected ClientAreaDockBase(IDocumentService? documentService = null) {
            _documentService = documentService ?? ServiceLocator.DocumentService;
            _documentService.ActiveDocumentChanged += OnActiveDocumentChanged;
        }

        /// <summary>
        /// Set Active Document.
        /// </summary>
        /// <param name="cr"></param>
        protected virtual void OnActiveDocumentChanged(CRDocument report)
        {
            HasDocument = report.HasData();
        }

        /// <summary>
        /// Display the workspace view (main window. the SearchView is hidden
        /// </summary>
        [RelayCommand]
        private void GoBackToWorkspace()
        {
            // TODO: move code into NavigationDockBase
            var ctx = this.Context as IRootDock;
            if (ctx is { } root)
            {
                // Display the workspace view (main view), which embeds all the layout docked windows
                root.Navigate.Execute(Ids.Workspace);
            }
        }
    }
}
