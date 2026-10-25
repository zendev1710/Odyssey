using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System.Collections.ObjectModel;


namespace Odyssey.ViewModels.Tools;

public abstract partial class MessagesListViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    protected MessageEntry? selectedItem;

    public ObservableCollection<MessageEntry> Items { get; } = [];

    protected MessagesListViewModel() : base()
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
        EntityModel? item = messageEntry?.Selection?.GetDefaultTarget();
        //DataBlock? item = messageEntry?.Selection?.GetDefaultTarget()?.DataBlock;
        RegionModel? region = messageEntry?.Selection?.Region;
        //RegionDataBlock? region = messageEntry?.Selection?.Region?.DataBlock as RegionDataBlock;
        if (item != null || region != null)
        {
            IEntitySelection sel = new EntitySelection(item, region, null);
            SendSelectionChangedEvent(sel);
        }
    }

    protected void Clear()
    {
        Items.Clear();
        SelectedItem = null;
    }
}
