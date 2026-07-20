using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DryIoc;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Services;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;

namespace Odyssey.ViewModels.Tools;

public partial class MiniMapViewModel : DocumentToolViewModelBase
{
    public int MiniMapNumber { get { return 0; } } // MiniMap.Count; } }

    //public ObservableCollection<BookmarkModel> MiniMap { get; }

    public MiniMapViewModel(IEventAggregator? eventAggregator = null, IActiveDocumentService? documentService = null) : base(eventAggregator, documentService)
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
    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

        CollectData();
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
