using AvaloniaEdit.Editing;
using Odyssey.Models.Documents;
using Dock.Model.Mvvm.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.ViewModels;

public interface ISelector
{
    string Id { get; }

    ISelection? Selection { get; set; }

    bool IsSelected(ISelection? selection);

    bool ShouldIgnoreSelectionChangedEvent(ISelectionChange selectionChange, List<string> selectorIdsIncludeFilter, List<string> selectorIdsExcludeFilter);

    bool InnerSelectorIs(ISelectionChange selectionChange, string id);

    /// <summary>
    /// Subscribes to the selection changed event of a UI component.
    /// </summary>
    /// <remarks>This method sets up an event handler to respond to changes in selection, allowing the
    /// application to react when the user selects a different item. Ensure that the UI component supports selection
    /// change events before calling this method.</remarks>
    void SubscribeToSelectionChangedEvent();

    /// <summary>
    /// Publish a SelectionChangedEvent.
    /// This event will be received by the ViewModels having subscribed to it.
    /// </summary>
    void PublishSelectionChangedEvent(ISelectionChange selectionChange);

    static bool ShouldIgnoreSelector(ISelectionChange selectionChange, List<string> selectorIdsIncludeFilter, List<string> selectorIdsExcludeFilter)
    {
        string selectorId = selectionChange.Selector?.Id ?? string.Empty;
        string innerSelectorId = selectionChange.InnerSelector?.Id ?? string.Empty;
        if (string.IsNullOrEmpty(selectorId))
        {
            return true;
        }
        if (selectorIdsIncludeFilter.Count > 0 && !selectorIdsIncludeFilter.Contains(selectorId))
        {
            return true;
        }
        if (selectorIdsExcludeFilter.Count > 0 && selectorIdsExcludeFilter.Contains(selectorId))
        {
            return true;
        }
        if (string.IsNullOrEmpty(innerSelectorId))
        {
            return false;
        }

        // If the inner selector is not empty, ensure it's not in the exlude filter
        if (selectorIdsExcludeFilter.Count > 0 && selectorIdsExcludeFilter.Contains(innerSelectorId))
        {
            return true;
        }

        return false;
    }
}
