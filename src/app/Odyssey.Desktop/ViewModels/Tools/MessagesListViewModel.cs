using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Services;
using Prism.Events;
using System;
using System.Collections.ObjectModel;


namespace Odyssey.ViewModels.Tools;

public abstract partial class MessagesListViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    protected MessageEntry? selectedItem;

    public ObservableCollection<MessageEntry> Items { get; } = [];

    protected MessagesListViewModel(IEventAggregator? eventAggregator = null, IActiveDocumentService? documentService = null) : base(eventAggregator, documentService)
    {
        SelectedItem = null;
    }

    /// <summary>
    /// Dispatch the select node Event using the event aggregator.
    /// Normally called by a double-click or a space key pressed on the currently selected item of this tree messages list in the corresponding view.
    /// Event is published only if the selected item is linked to a DataBlock (selection state has changed).
    /// </summary>
    /// <param name="node"></param>
    public void RevealSelectedItemInExplorer()
    {
        RevealInExplorer(SelectedItem);
    }

    private void RevealInExplorer(MessageEntry? messageEntry)
    {
        DataBlock? item = messageEntry?.Selection?.GetDefaultTarget();
        DataBlock? region = messageEntry?.Selection?.Region;
        if (item != null || region != null)
        {
            ISelection sel = new SimpleItemSelection(item, region, null);
            SendSelectionChangedEvent(sel);
        }
    }

    protected void Clear()
    {
        Items.Clear();
        SelectedItem = null;
    }
}
