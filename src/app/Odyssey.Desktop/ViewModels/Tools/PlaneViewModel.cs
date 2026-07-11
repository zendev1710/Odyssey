using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Documents;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// View model for a Plane.
/// </summary>
public partial class PlaneViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    public string name;

    public PlaneViewModel(IEventAggregator? eventAggregator = null) : base(eventAggregator)
    {
        // Astral or world
        Name = string.Empty;
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        // TODO
        /*
        if (!HandleSelection(selectionChange))
        {
            Debug.WriteLine("[MAP] selected item did not change");
        }
        */
    }

    /// <summary>
    /// Handle active document changed event.
    /// Rebuilds the tree from the new active document data.
    /// </summary>
    /// <param name="cr"></param>
    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            RebuildMap();
        }
    }

    protected override void OnActiveDocumentClosed(CRDocument cr)
    {
        //Clear();
        // reset to an empty report document
        SetMapFile(new CRDocument());
        RebuildMap();
    }

    /// <summary>
    /// Rebuilds the map from the data in the report document.
    /// </summary>
    private void RebuildMap()
    {
        Debug.WriteLine("[PLANE] Data collection...");

        // Clear tree and build a new one from newly opened document data
        //Items.Clear();
        //var report = GetDocument();
    }
}
