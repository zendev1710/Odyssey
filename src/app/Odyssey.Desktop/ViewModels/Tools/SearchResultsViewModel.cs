using Avalonia.Controls;
using Odyssey.Events;
using Odyssey.Models.Documents;
using Dock.Model.Mvvm.Controls;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.ViewModels.Tools;

public class SearchResultsViewModel : DocumentToolViewModelBase
{

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // This view does not handle any selection changes.
    }

    public SearchResultsViewModel(IEventAggregator? eventAggregator = null): base(eventAggregator)
    {
        // TODO: use filter on filePath to update the list only if different
        //EventAggregator?.GetEvent<ActiveDocumentChangedEvent>().Subscribe(OnActiveDocumentChanged, ThreadOption.UIThread);
        // TODO: use filter on SelectionState to update data only when needed
        //EventAggregator?.GetEvent<SelectionStateChangedEvent>().Subscribe(OnSelectionChanged, ThreadOption.UIThread);
    }

    /*
    private void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            //RebuildItems();
        }
    }
    */
    /*
    private void OnSelectionChanged(SelectionState sel)
    {
        DataBlock? regionBlock = sel.region;
        if (regionBlock != null)
        {
            SetSelection(regionBlock);
        }
    }
    */
}
