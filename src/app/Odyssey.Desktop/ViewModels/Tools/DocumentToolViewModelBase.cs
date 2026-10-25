using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Mvvm.Core;
using Odyssey.Core.Services;
using Odyssey.Models.Documents;
using Odyssey.Services;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.ViewModels.Tools
{
    /// <summary>
    /// A generic tool view model used to share some data related to the linked CR document.
    /// </summary>
    public abstract partial class DocumentToolViewModelBase : Tool
    {
        [ObservableProperty]
        private bool hasDocument;

        private readonly IDocumentService _documentService;
        protected CRDocument Report => _documentService.ActiveDocument;

        protected IEntitySelection CurrentSelection => _selectionService.Selection;

        protected readonly IEntitySelectionService _selectionService;

        protected DocumentToolViewModelBase() {
            _documentService = ServiceLocator.DocumentService;
            _selectionService = ServiceLocator.EntitySelectionService;

            _documentService.ActiveDocumentChanged += OnActiveDocumentChanged;
            _selectionService.SelectionChanged += OnSelectionChanged;
        }

        protected virtual void OnActiveDocumentChanged(CRDocument report)
        {
            HasDocument = report.HasData();
        }

        protected virtual void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
        {
        }

        /*
        protected virtual void SetSelection(ISelection sel)
        {
            _selectionService.SelectItem(sel.Item, sel.Region, sel.Faction, this);
        }
        */

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

        protected void SendSelectionChangedEvent(IEntitySelection sel)
        {
            Debug.WriteLine($"[DOCTOOLVIEWMODELBASE] SendSelectionChangedEvent from {Id} for item={sel.Item} region={sel.Region}");
            _selectionService.SelectItem(sel.Item, sel.Region, this, sel.Data, EntitySelection.GetContainerTypes(sel));
        }
    }
}
