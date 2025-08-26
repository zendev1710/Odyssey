using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Dock.Model.Mvvm.Controls;
using Prism.Events;
using System.Collections.Generic;
using System.Diagnostics;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;

namespace Odyssey.ViewModels.Tools
{
    /// <summary>
    /// A generic tool view model used to share some data related to the linked CR document.
    /// </summary>
    public abstract partial class DocumentToolViewModelBase : Tool, ISelector
    {
        [ObservableProperty]
        private bool hasDocument;

        private readonly SimpleItemSelection _selection = new();

        private readonly IEventAggregator? _eventAggregator;

        protected IEventAggregator? EventAggregator { get { return _eventAggregator; } }

        protected CRDocument Report { get; private set; }

        /// <summary>
        /// Selection state.
        /// Each document tool view model has its own selection state; 
        /// </summary>
        //public ItemSelection Selection { get { return _selection; } }
        public ISelection? Selection { get; set; }

        public bool IsSelected(ISelection? selection)
        {
            if (selection == null || Selection == null)
            {
                return false;
            }

            // LATER: maybe compare some kind of unique ids
            return selection.Item == Selection.Item;
        }

        public bool IsSelectedRegion(ISelection? selection)
        {
            if (selection == null || Selection == null)
            {
                return false;
            }

            return selection.Region == Selection.Region;
        }

        public bool IsSelectedFaction(ISelection? selection)
        {
            if (selection == null || Selection == null)
            {
                return false;
            }

            return selection.Faction == Selection.Faction;
        }

        protected ISelector? InnerSelector { get; set; }

        /// <summary>
        /// Constructor for Avalonia XAML Designer.
        /// </summary>
        protected DocumentToolViewModelBase(IEventAggregator? eventAggregator) {
            _eventAggregator = eventAggregator;
            HasDocument = false;
            // links to an empty report document
            Report = new CRDocument();
            Selection = new SimpleItemSelection();

            EventAggregator?.GetEvent<ActiveDocumentChangedEvent>().Subscribe(OnActiveDocumentChanged, ThreadOption.UIThread);
            SubscribeToSelectionChangedEvent();
        }

        /// <summary>
        /// Set Active Document.
        /// </summary>
        /// <param name="cr"></param>
        protected virtual void OnActiveDocumentChanged(CRDocument cr)
        {
            SetMapFile(cr);
        }

        protected abstract void OnSelectionChanged(ISelectionChange selectionChange);

        protected void SetSelection(ISelection sel)
        {
            // LATER: maybe it would be better to set a copy of sel
            Selection = sel;
        }

        protected virtual int OnMapChange(ISelection selection)
        {
            // TODO : should be handled by event aggregator
            SetSelection(selection);
            return 1;
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
                Debug.WriteLine($"[DOCTOOL-] SetMapFile {cr.Name}");
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
            Debug.WriteLine($"[DOCTOOL-] IsSameDocument {result}");
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

        public bool ShouldIgnoreSelectionChangedEvent(ISelectionChange selectionChange, List<string> selectorIdsIncludeFilter, List<string> selectorIdsExcludeFilter)
        {
            return ISelector.ShouldIgnoreSelector(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter);
        }

        public bool InnerSelectorIs(ISelectionChange selectionChange, string id)
        {
            // Check if the inner selector matches the specified Id
            string innerSelectorId = selectionChange.InnerSelector?.Id ?? string.Empty;
            return innerSelectorId == Id;
        }

        /// <summary>
        /// Subscribes to the <see cref="SelectionChangeEvent"/> to handle selection changes.
        /// </summary>
        /// <remarks>This method registers the <c>OnSelectionChanged</c> handler to be invoked on the UI
        /// thread whenever the <see cref="SelectionChangeEvent"/> is published. Ensure that the
        /// <c>EventAggregator</c> is properly initialized before calling this method.</remarks>
        public void SubscribeToSelectionChangedEvent()
        {
            EventAggregator?.GetEvent<SelectionChangeEvent>().Subscribe(OnSelectionChanged, ThreadOption.UIThread, false, null);
        }

        /// <summary>
        /// Publishes a selection changed event to notify subscribers of a change in the selection state.
        /// </summary>
        public void PublishSelectionChangedEvent(ISelectionChange selectionChange)
        {
            EventAggregator?.GetEvent<SelectionChangeEvent>().Publish(selectionChange);
            InnerSelector = null;
        }
    }
}
