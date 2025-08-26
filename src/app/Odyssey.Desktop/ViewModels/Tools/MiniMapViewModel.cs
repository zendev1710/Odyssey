using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System.Collections.Generic;
using System.Diagnostics;
using Prism.Events;
using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using Odyssey.Models.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using DryIoc;

namespace Odyssey.ViewModels.Tools;

public partial class MiniMapViewModel : DocumentToolViewModelBase
{
    public int MiniMapNumber { get { return 0; } } // MiniMap.Count; } }

    //public ObservableCollection<BookmarkModel> MiniMap { get; }
    public MiniMapViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    public MiniMapViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
        _selectedBookmarkIndex = -1;
        _selectedBookmarkText = string.Empty;
        _hasBookmark = false;
        //MiniMap = [];
    }
    /*
    partial void OnSelectedBookmarkIndexChanged(int oldValue, int newValue)
    {
        SelectedBookmarkText = newValue >= 0 ? MiniMap[newValue].Content : string.Empty;
    }
    */
    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            CollectData();
        }
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Exclude this selector from selection change events to avoid reentrancy issues
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        ISelection sel = selectionChange.Selection;
        if (sel.HasRegion() && !IsSelectedRegion(sel))
        {
            SetSelection(sel);
            string label = Selection.Region!.GetUILabel();
            /*
            if (MiniMap.FirstOrDefault(b => b.Label!.StartsWith(label)) is BookmarkModel Bookmark)
            {
                SelectedBookmarkIndex = MiniMap.IndexOf(Bookmark);
            }
            */
        }
    }

    protected void CollectData()
    {
        /*
        List<BookmarkModel> MiniMap = [];
        LinkedList<DataBlock> blocks = Report.Blocks;
        LinkedListNode<DataBlock>? firstBlockNode = blocks.First;
        string currentFactionName = string.Empty;
        for (var node = firstBlockNode; node != null; node = node.Next)
        {
            DataBlock block = node.Value;
            BlockType type = block.GetBlockType();
            if (type == BlockType.REGION)
            {
                break;
            }
            else if (type == BlockType.FACTION)
            {
                currentFactionName = GetFactionName(block);
            }
            else if (type == BlockType.Bookmark)
            {
                BookmarkModel BookmarkModel = new(Report, currentFactionName);
                if (BookmarkModel.CollectData(block))
                {
                    MiniMap.Add(BookmarkModel);
                }
            }
        }
        if (MiniMap.Count > 0)
        {
            MiniMap.AddRange(MiniMap.OrderBy(b => b.Label));
            SelectedBookmarkIndex = 0;
        }

        HasBookmark = MiniMapNumber > 0;
        Debug.WriteLine($"nb MiniMap = {MiniMapNumber}");
        */
    }

    [RelayCommand(CanExecute = nameof(CanRevealBookmarkInExplorer))]
    private void RevealBookmarkInExplorer()
    {
        // TO BE IMPLEMENTED LATER
    }

    private bool CanRevealBookmarkInExplorer()
    {
        // TO BE IMPLEMENTED LATER
        return false;
    }

    [ObservableProperty]
    private int _selectedBookmarkIndex;

    [ObservableProperty]
    private string? _selectedBookmarkText;

    [ObservableProperty]
    private bool _hasBookmark;
}
