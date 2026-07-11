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


/// <summary>
/// Error List View Model.
/// Contains errors detected by ECheck See Csmapp::checkCommands() in original code.
/// Should be updated after map change (after a report file is loaded).
/// </summary>
public class ErrorListViewModel : DocumentToolViewModelBase
{
    public ErrorListViewModel(IEventAggregator? eventAggregator = null): base(eventAggregator)
    {
    }

    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        Debug.WriteLine($"[VM-ERRORS--] DOCUMENT CHANGED {cr} -> BEGIN");
        if (SetMapFile(Report)) {
            // TODO
            //RebuildItems();
        }
        Debug.WriteLine($"[VM-ERRORS--] DOCUMENT CHANGED {cr} -> ENDED");
    }
    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // This view does not hadnle any selection changes.
    }
}
