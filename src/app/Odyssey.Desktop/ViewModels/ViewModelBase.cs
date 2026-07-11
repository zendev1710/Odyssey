using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.ViewModels.Tools;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.ViewModels
{
    /// <summary>
    /// A generic tool view model used to share some data of the linked CR document.
    /// </summary>
    public partial class ViewModelBase : ObservableObject, ISelector
    {
        [ObservableProperty]
        private bool hasDocument;

        private readonly IEventAggregator? _eventAggregator;

        protected IEventAggregator? EventAggregator { get { return _eventAggregator; } }

        protected CRDocument Report { get; private set; }

        public string Id { get; }

        public ISelection? Selection { get; set; }

        public bool IsSelected(ISelection? selection)
        {
            if (selection == null || Selection == null)
            {
                return false;
            }
            return selection.Item == Selection.Item;
        }

        /// <summary>
        /// Selection state.
        /// Each document tool view model has its own selection state; 
        /// </summary>
        //public SimpleItemSelection Selection { get { return _selection; } }

        protected ViewModelBase(string id, IEventAggregator? eventAggregator = null)
        {
            Id = id;
            _eventAggregator = eventAggregator ?? Prism.Events.EventAggregator.Current;
            HasDocument = false;
            Report = new CRDocument();
            Selection = new SimpleItemSelection();
            
            EventAggregator?.GetEvent<ReportDocumentChangedEvent>().Subscribe(OnActiveDocumentChanged, ThreadOption.UIThread, false, null);
            EventAggregator?.GetEvent<ActiveDocumentClosedEvent>().Subscribe(OnActiveDocumentClosed, ThreadOption.UIThread, false, null);
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

        /// <summary>
        /// Called when ActiveDocumentClosedEvent is triggered.
        /// </summary>
        /// <param name="cr">Report Document</param>
        protected virtual void OnActiveDocumentClosed(CRDocument cr)
        {
            // reset to an empty report document
            SetMapFile(new CRDocument());
        }

        protected virtual void OnSelectionChanged(ISelectionChange selectionChange)
        {
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

        protected ISelection? SetSelection(ISelection? selection)
        {
            Selection = selection;
            return Selection;
        }

        protected virtual int OnReportChange(ISelection? selection)
        {
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

        protected void SendSelectionChangedEvent(ISelection sel)
        {
            var ViewModelId = this.Id;
            ISelector selector = this;
            ISelector? innerSelector = null;
            SelectionChange selectionEvent = new SelectionChange(sel, selector, innerSelector);
            Debug.WriteLine($"[VIEWMODELBASE] SendSelectionChangedEvent from {ViewModelId} for item {sel.Item}");
            PublishSelectionChangedEvent(selectionEvent);
        }

        public void PublishSelectionChangedEvent(ISelectionChange selectionChange)
        {                
            EventAggregator?.GetEvent<SelectionChangeEvent>().Publish(selectionChange);
        }

        public void SubscribeToSelectionChangedEvent()
        {
            // A viewModel base will only receive selection changed events from ExplorerViewModel.
            var selEventFilter = new Predicate<ISelectionChange>(pub => pub is ExplorerViewModel);
            EventAggregator?.GetEvent<SelectionChangeEvent>().Subscribe(OnSelectionChanged, ThreadOption.UIThread, false, null/*selEventFilter*/);
        }
    }
}
