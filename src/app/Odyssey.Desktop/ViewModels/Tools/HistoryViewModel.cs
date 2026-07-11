using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Odyssey.ViewModels.Tools;

/// <summary>
/// This class is used to store the history of selections made in the Explorer view.
/// </summary>
public partial class HistoryViewModel : DocumentToolViewModelBase
{
    /// <summary>
    /// Represents the maximum number of entries that can be stored in the history.
    /// </summary>
    private const int HistoryMaxSize = 42;

    [ObservableProperty]
    private SelectionEntry? _selectedItem;

    public ObservableCollection<SelectionEntry> Items { get; } = [];

    public HistoryViewModel(IEventAggregator? eventAggregator = null) : base(eventAggregator)
    {
    }

    /// <summary>
    /// Update selection state with the selected item.
    /// Called automatically via [ObservableProperty] attribute when the property has been modified.
    /// </summary>
    partial void OnSelectedItemChanged(SelectionEntry? oldValue, SelectionEntry? newValue)
    {
        if (newValue == null)
        {
            return;
        }

        if (oldValue == newValue || (oldValue != null && oldValue.Label == newValue.Label))
        {
            // If the selected item has not changed, do nothing
            return;
        }

        // if newValue matches the current selection in Explorer, do nothing
        if (Selection != null && Selection.Item == newValue.Selection?.Item)
        {
            // If the selected item is already the current selection in Explorer, do nothing
            return;
        }

        // called when selection changing comes from key or mouse gesture,
        // or when new Explorer or Map item is selected
        RevealSelection(newValue);
    }

    protected override void OnActiveDocumentClosed(CRDocument cr)
    {
        ClearItems();
        // reset to an empty report document
        SetMapFile(new CRDocument());
    }

    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            ClearItems();
        }
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        List<string> selectorIdsExcludeFilter = [];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        // Note: an unseen region can also been selected and then added to the history
        ISelection sel = selectionChange.Selection;
        if (!IsSelected(sel))
        {
            SetSelection(sel);
            if (!InnerSelectorIs(selectionChange, Id))
            {
                AddSelectionToHistory(Selection!);
            }
        }
    }

    private void AddSelectionToHistory(ISelection sel)
    {
        string label = sel.Item?.GetUILabel() ?? string.Empty;
        // Exclude Buildings and Ships nodes from history.
        // label is empty for these nodes
        if (!string.IsNullOrEmpty(label))
        {
            var newEntry = new SelectionEntry(sel);
            // Check if an entry with the same label already exists
            var result = Items
                .Select((entry, idx) => (entry, idx))
                .FirstOrDefault(x => x.entry.Label == newEntry.Label);

            int existingIndex = result.entry != null ? result.idx : -1;
            if (existingIndex == 0)
            {
                // Already at first position, do nothing
                return;
            }

            if (existingIndex > 0)
            {
                // Remove from current position
                Items.RemoveAt(existingIndex);
            }

            // Insert at first position
            Items.Insert(0, newEntry);

            // Enforce max size
            if (Items.Count > HistoryMaxSize)
            {
                Items.RemoveAt(Items.Count - 1);
            }
        }
    }

    private void RevealSelection(SelectionEntry selectionEntry)
    {
        SendSelectionChangedEvent(selectionEntry.Selection!);
        SelectedItem = null;
    }

    private void ClearItems()
    {
        Items.Clear();
    }
}
