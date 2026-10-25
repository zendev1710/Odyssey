using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

    public HistoryViewModel() : base()
    {
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this))
            return;

        // Note: an unseen region can also been selected and then added to the history
        AddSelectionToHistory(selection);
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
        IEntitySelection selection = CurrentSelection;
        if (selection.Item == newValue.Selection?.Item)
        {
            // If the selected item is already the current selection in Explorer, do nothing
            return;
        }

        // called when selection changing comes from key or mouse gesture,
        // or when new Explorer or Map item is selected
        RevealSelection(newValue);
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);
        ClearItems();
    }

    private void AddSelectionToHistory(IEntitySelection sel)
    {
        // TODO: optimize it(s called too frequently
        string label = sel.Item?.DataBlock.GetUILabel() ?? string.Empty;
        // Exclude Buildings and Ships nodes from history.
        // label is empty for these nodes
        if (!string.IsNullOrEmpty(label))
        {
            // Check if an entry with the same label already exists
            var result = Items
                .Select((entry, idx) => (entry, idx))
                .FirstOrDefault(x => x.entry.Label == label);

            int existingIndex = result.entry is not null ? result.idx : -1;
            if (existingIndex == 0)
            {
                // Already at first position, just select it in list
                SelectedItem = result.entry;
                return;
            }

            if (existingIndex > 0)
            {
                // Remove from current position
                Items.RemoveAt(existingIndex);
            }

            // Insert at first position
            var newEntry = new SelectionEntry(sel);
            Items.Insert(0, newEntry);

            // Enforce max size
            if (Items.Count > HistoryMaxSize)
            {
                Items.RemoveAt(Items.Count - 1);
            }

            SelectedItem = newEntry;
        }
    }

    private void RevealSelection(SelectionEntry selectionEntry)
    {
        IEntitySelection? sel = selectionEntry.Selection;
        Debug.WriteLine($"[HISTORY-VM] SendSelectionChangedEvent from {Id} for item={sel.Item} region={sel.Region}");
        _selectionService.SelectItem(sel.Item, sel.Region, /*sel.Faction,*/ this, sel.Data, EntitySelection.GetContainerTypes(sel));
        //SelectedItem = null;
    }

    private void ClearItems()
    {
        Items.Clear();
        SelectedItem = null;
    }
}
