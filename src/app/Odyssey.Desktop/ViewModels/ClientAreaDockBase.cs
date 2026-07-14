using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;
using Odyssey.Core.Services;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Services;
using Prism.Events;
using System;

namespace Odyssey.ViewModels
{
    /// <summary>
    /// Dock base view model for a view displayed in the whole app client area.
    /// </summary>
    public abstract partial class ClientAreaDockBase: DockBase
    {
        [ObservableProperty]
        private bool hasDocument;

        private readonly IActiveDocumentService _documentService;

        protected CRDocument Report => _documentService.ActiveDocument;

        /// <summary>
        /// Constructor for Avalonia XAML Designer.
        /// </summary>
        protected ClientAreaDockBase(IActiveDocumentService? documentService = null) {
            _documentService = documentService ?? ServiceLocator.ActiveDocumentService;

            HasDocument = false;

            _documentService.ActiveDocumentChanged += OnActiveDocumentChanged;
        }

        /// <summary>
        /// Set Active Document.
        /// </summary>
        /// <param name="cr"></param>
        protected virtual void OnActiveDocumentChanged(CRDocument cr)
        {
            HasDocument = cr.HasData();
        }

        /// <summary>
        /// Display the home view. the SearchView is hidden
        /// </summary>
        [RelayCommand]
        private void GoHome()
        {
            // TODO: move code into NavigationDockBase
            var ctx = this.Context as IRootDock;
            if (ctx is { } root)
            {
                // Display the home view, which embeds all the layout docked windows
                root.Navigate.Execute(Ids.Home);
            }
        }
    }
}
