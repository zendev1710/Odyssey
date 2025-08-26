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
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using DryIoc;

namespace Odyssey.ViewModels.Tools;

public partial class BookmarksViewModel : DocumentToolViewModelBase
{
    public int BookmarksNumber { get { return Bookmarks.Count; } }

    public ObservableCollection<BookmarkModel> Bookmarks { get; }

    public BookmarksViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    public BookmarksViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
        _hasBookmark = false;
        Bookmarks = [];
    }

    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            CollectData();
        }
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Does not handle any selection changes.
    }

    protected void CollectData()
    {
        // TODO: load bookmarks for the current user and application
    }

    [RelayCommand(CanExecute = nameof(CanRevealBookmarkInExplorer))]
    private void RevealBookmarkInExplorer()
    {
        // TODO: publish event to select the bookmark in the explorer
    }

    private bool CanRevealBookmarkInExplorer()
    {
        return HasBookmark;
    }

    [ObservableProperty]
    private int _selectedBookmarkIndex;

    [ObservableProperty]
    private string? _selectedBookmarkText;

    [ObservableProperty]
    private bool _hasBookmark;
}
