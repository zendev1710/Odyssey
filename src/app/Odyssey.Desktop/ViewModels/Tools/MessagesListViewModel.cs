using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Events;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Prism.Events;
using System;
using System.Collections.ObjectModel;


namespace Odyssey.ViewModels.Tools;

public abstract partial class MessagesListViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    protected MessageEntry? selectedItem;

    public ObservableCollection<MessageEntry> Items { get; }

    public MessagesListViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    protected MessagesListViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
        // LATER: use filter on filePath to update the list only if different
        EventAggregator?.GetEvent<ActiveDocumentChangedEvent>().Subscribe(OnEventActiveDocumentChanged, ThreadOption.UIThread);
        Items = new ObservableCollection<MessageEntry>();
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

    private void OnEventActiveDocumentChanged(CRDocument cr)
    {
        OnActiveDocumentChanged(cr);
    }

    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        SetMapFile(cr);
    }

    private void RevealInExplorer(MessageEntry? messageEntry)
    {
        DataBlock? item = messageEntry?.Selection?.GetDefaultTarget();
        DataBlock? region = messageEntry?.Selection?.Region;
        if (item != null || region != null)
        {
            ISelection sel = new SimpleItemSelection(item, region, null);
            PublishSelectionChangedEvent(new SelectionChange(sel, this, null));
        }
    }

}
