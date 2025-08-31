using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Prism.Events;
using System;
using Dock.Model.Mvvm.Core;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;

namespace Odyssey.ViewModels
{
    /// <summary>
    /// Dock base view model for a view displayed in the whole app client area.
    /// </summary>
    public abstract partial class ClientAreaDockBase: DockBase
    {
        [ObservableProperty]
        private bool hasDocument;

        private readonly IEventAggregator? _eventAggregator;

        protected IEventAggregator? EventAggregator { get { return _eventAggregator; } }

        protected CRDocument Report { get; private set; }

        protected ClientAreaDockBase() : this(null)
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException("This constructor should only be used in design mode.");
            }
        }
        /// <summary>
        /// Constructor for Avalonia XAML Designer.
        /// </summary>
        protected ClientAreaDockBase(IEventAggregator? eventAggregator) {
            _eventAggregator = eventAggregator;
            HasDocument = false;
            // links to an empty report document
            Report = new CRDocument();

            EventAggregator?.GetEvent<ReportDocumentChangedEvent>().Subscribe(OnActiveDocumentChanged, ThreadOption.UIThread);
        }

        /// <summary>
        /// Set Active Document.
        /// </summary>
        /// <param name="cr"></param>
        protected virtual void OnActiveDocumentChanged(CRDocument cr)
        {
            SetMapFile(cr);
        }

        /// <summary>
        /// Links the report document to the view model.
        /// </summary>
        /// <param name="cr">the report document to link</param>
        /// <returns>true if a new report document has been linked to the view model; otherwise false</returns>
        protected virtual bool SetMapFile(CRDocument cr)
        {
            bool result = false;
            if (!IsSameDocument(cr))
            {
                Report = cr;
                result = true;
            }
            HasDocument = Report.HasData();
            return result;
        }

        /// <summary>
        /// Returns true if the specified document is the same as the current document.
        /// </summary>
        /// <param name="cr"></param>
        /// <returns></returns>
        protected bool IsSameDocument(CRDocument cr)
        {
            // IMPROVE: use rather a unique Id for the document
            bool result = cr.Name == Report.Name;
            return result;
        }
        /// <summary>
        /// Get the current report document.
        /// </summary>
        /// <returns>the report document attached to the view model</returns>
        protected CRDocument GetDocument()
        {
            return Report;
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
