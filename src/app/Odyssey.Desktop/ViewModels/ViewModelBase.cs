using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Core.Services;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Services;
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

        private readonly IActiveDocumentService _documentService;
        protected CRDocument Report => _documentService.ActiveDocument;

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

        protected ViewModelBase(string id, IEventAggregator? eventAggregator = null, IActiveDocumentService? documentService = null)
        {
            Id = id;
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
