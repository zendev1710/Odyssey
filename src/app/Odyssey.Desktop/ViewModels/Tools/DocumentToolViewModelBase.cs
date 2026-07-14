using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using Odyssey.Core.Services;
using Odyssey.Events;
using Odyssey.Models.Documents;
using Odyssey.Services;
using Prism.Events;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.ViewModels.Tools
{
    /// <summary>
    /// A generic tool view model used to share some data related to the linked CR document.
    /// </summary>
    public abstract partial class DocumentToolViewModelBase : Tool, ISelector
    {
        [ObservableProperty]
        private bool hasDocument;

        private readonly IActiveDocumentService _documentService;
        protected CRDocument Report => _documentService.ActiveDocument;

        private readonly SimpleItemSelection _selection = new();

        private readonly IEventAggregator? _eventAggregator;

        protected IEventAggregator? EventAggregator { get { return _eventAggregator; } }

        /// <summary>
        /// Selection state.
        /// Each document tool view model has its own selection state; 
        /// </summary>
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
        /// Constructor.
        /// </summary>
        protected DocumentToolViewModelBase(IEventAggregator? eventAggregator = null, IActiveDocumentService? documentService = null) {
            _eventAggregator = eventAggregator ?? Prism.Events.EventAggregator.Current;
            _documentService = documentService ?? ServiceLocator.ActiveDocumentService;
            _documentService.ActiveDocumentChanged += OnActiveDocumentChanged;

            Selection = new SimpleItemSelection();

            SubscribeToSelectionChangedEvent();
        }

        /// <summary>
        /// Set Active Document.
        /// </summary>
        /// <param name="cr"></param>
        protected virtual void OnActiveDocumentChanged(CRDocument cr)
        {
            HasDocument = Report.HasData();
        }

        protected abstract void OnSelectionChanged(ISelectionChange selectionChange);

        protected virtual void SetSelection(ISelection? sel)
        {
            Selection = sel;
        }

        protected virtual int OnReportChange(ISelection? selection)
        {
            // TODO : should be handled by event aggregator
            SetSelection(selection);
            return 1;
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

        /*
        /// <summary>
        /// Get the current report document.
        /// </summary>
        /// <returns>the report document attached to the view model</returns>
        protected CRDocument GetDocument()
        {
            return Report;
        }
        */
        protected void SendSelectionChangedEvent(ISelection sel, ISelector? innerSelector = null)
        {
            var ViewModelId = this.Id;
            ISelector selector = this;
            SelectionChange selectionEvent = new SelectionChange(sel, selector, innerSelector);
            Debug.WriteLine($"[DOCTOOLVIEWMODELBASE] SendSelectionChangedEvent from {ViewModelId} for item {sel.Item} (innerSel={innerSelector?.Id}");
            PublishSelectionChangedEvent(selectionEvent);
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
