using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Core.Services;
using Odyssey.Models.Documents;
using Odyssey.Services;
using System.Diagnostics;

namespace Odyssey.ViewModels
{
    /// <summary>
    /// A generic tool view model used to share some data of the linked CR document.
    /// </summary>
    public partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private bool hasDocument;

        private readonly IDocumentService _documentService;
        private readonly IEntitySelectionService _selectionService;

        protected CRDocument Report => _documentService.ActiveDocument;
        protected IEntitySelection CurrentSelection => _selectionService.Selection;

        public string Id { get; }

        /// <summary>
        /// Selection state.
        /// Each document tool view model has its own selection state;
        /// </summary>
        //public SimpleSelection Selection { get { return _selection; } }

        protected ViewModelBase(string id)
        {
            Id = id;

            _documentService = ServiceLocator.DocumentService;
            _selectionService = ServiceLocator.EntitySelectionService;

            _documentService.ActiveDocumentChanged += OnActiveDocumentChanged;
            _selectionService.SelectionChanged += OnSelectionChanged;

            //Selection = new SimpleSelection();
        }

        protected virtual void OnActiveDocumentChanged(CRDocument report)
        {
            HasDocument = report.HasData();
        }

        protected virtual void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
        {
        }

        protected void SendSelectionChangedEvent(IEntitySelection sel)
        {
            _selectionService.SelectItem(sel.Item, sel.Region, this, sel.Data, EntitySelection.GetContainerTypes(sel), sel.LocatedFaction);
        }
    }
}
