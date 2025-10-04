using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DryIoc;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Odyssey.ViewModels.Tools;

public class BookmarksFileItem
{
    public string Label { get; }
    public string FilePath { get; }

    public BookmarksFileItem(string label, string filePath)
    {
        Label = label;
        FilePath = filePath;
    }

    public override string ToString() => Label;
}

public partial class BookmarksViewModel : DocumentToolViewModelBase
{
    public ObservableCollection<BookmarksFileItem> BookmarksFiles { get; } = [];

    [ObservableProperty]
    private BookmarksFileItem? _selectedBookmarksFile;

    [ObservableProperty]
    private string _selectionName = string.Empty;

    public ObservableCollection<BookmarkModel> Bookmarks { get; } = [];

    // Add a property for the selected object (bind this in your view)
    [ObservableProperty]
    private DataBlock? _selectedObject;

    [ObservableProperty]
    private bool _selectionIsBookmarkable;

    [ObservableProperty]
    private bool _selectionIsBookmarked;

    [ObservableProperty]
    private BookmarkModel? _selectedBookmark;

    [ObservableProperty]
    private bool _hasBookmark;

    private string? _currentBookmarksFile;

    private String _bookmarkFilesPrefix = string.Empty;

    private bool LoadingBookMarksInProgress { get; set; }

    public BookmarksViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    public BookmarksViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
        Bookmarks.CollectionChanged += (_, _) => HasBookmark = Bookmarks.Count > 0;
    }

    /// <summary>
    /// </summary>
    partial void OnSelectedBookmarkChanged(BookmarkModel? oldValue, BookmarkModel? newValue)
    {
        RevealBookmark(newValue);
    }

    // Called when the active document changes (CRDocument loaded)
    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            AutoLoadBookmarksForDocument(cr);
        }
    }

    protected override void OnActiveDocumentClosed(CRDocument cr)
    {
        // TODO: do not call this OnActiveDocumentClosed method if recently opened another document with the same short name
        ClearItems();
        // reset to an empty report document
        SetMapFile(new CRDocument());
    }

    private void AddBookmark(DataBlock dt)
    {
        if (IsBookmarkable(dt) && !IsInBookmarks(dt, out BookmarkModel? bookmarkModel))
        {
            var model = BookmarkModel.FromObject(dt);
            if (model != null)
            {
                Bookmarks.Insert(0, model);
                AutoSave();
                SelectedBookmark = model;
                SelectionIsBookmarked = true;
            }
        }
    }

    private void RemoveBookmark(DataBlock dt)
    {
        if (IsBookmarkable(dt) && IsInBookmarks(dt, out BookmarkModel? bookmarkModel))
        {
            Bookmarks.Remove(bookmarkModel!);
            AutoSave();
            SelectedBookmark = null;
            SelectionIsBookmarked = false;
        }
    }

    private bool IsInBookmarks(DataBlock? dt, out BookmarkModel? bookmarkModel)
    {
        bookmarkModel = null;
        if (dt is null)
        { 
            return false;
        }
        bookmarkModel = Bookmarks.FirstOrDefault(b => b.IsSameObject(dt));
        return bookmarkModel != null;
    }

    // Toggle bookmark for the selected object (region, unit, ship, building)
    [RelayCommand(CanExecute = nameof(CanToggleBookmark))]
    private void ToggleBookmark()
    {
        if (SelectedObject is null)
        {
            return;
        }

        SelectionIsBookmarked = !SelectionIsBookmarked;
    }

    // Reveal bookmark in explorer (selects the object in the explorer view)
    private void RevealBookmark(BookmarkModel? bookmarkModel)
    {
        if (!LoadingBookMarksInProgress && bookmarkModel?.Target is not null)
        {
            // Publish event only if the bookmark target is not the current global selection (from Explorer/Map view)
            if (SelectedObject != bookmarkModel.Target)
            {
                ISelection sel = new SimpleItemSelection(bookmarkModel.Target, null, null);
                PublishSelectionChangedEvent(new SelectionChange(sel, this, null));
            }
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

        // TODO: if selection is an existing bookmark, select it
        // in orde the user to see that it's a bookmark
        ISelection sel = selectionChange.Selection;
        if (!IsSelected(sel))
        {
            SetSelection(sel);
        }
    }
    protected override void SetSelection(ISelection sel)
    {
        base.SetSelection(sel);
        BookmarkModel? bookmarkModel = null;
        SelectedObject = sel?.Item;
        SelectionIsBookmarkable = IsBookmarkable(SelectedObject);
        SelectionIsBookmarked = SelectionIsBookmarkable && IsInBookmarks(SelectedObject, out bookmarkModel);
        SelectionName = SelectedObject?.GetUILabel() ?? string.Empty;
        SelectedBookmark = SelectionIsBookmarked ? bookmarkModel : null;
    }

    // Load bookmarks from XML file
    private void LoadBookmarks(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        Bookmarks.Clear();
        var doc = XDocument.Load(filePath);
        var root = doc.Root;
        if (root == null)
        {
            return;
        }

        if (root.HasElements)
        {
            foreach (var el in root.Elements("bookmark"))
            {
                var type = el.Attribute("type")?.Value;
                var id = el.Attribute("id")?.Value;
                var name = el.Attribute("name")?.Value;
                var model = BookmarkModel.FromXml(type, id, name, GetDocument());
                if (model != null)
                {
                    Bookmarks.Add(model);
                }
            }
        }
        _currentBookmarksFile = filePath;
    }

    /// <summary>
    /// Save bookmarks to the specified XML file.
    /// </summary>
    /// <param name="filePath">where bookmarks have to be saved</param>
    private void SaveBookmarks(string filePath)
    {
        var doc = new XDocument(
            new XElement("Items",
                Bookmarks.Select(b => b.ToXmlElement())
            )
        );
        doc.Save(filePath);
    }

    // Auto-load bookmarks file for the current CRDocument
    private void AutoLoadBookmarksForDocument(CRDocument cr)
    {
        LoadingBookMarksInProgress = true;
        var shortName = cr.ShortName;
        // LATER: in Linux/Mac, comparison should be case-sensitive
        if (string.Equals(shortName, _bookmarkFilesPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Same prefix means same bookmarks file
            // I's not useful to reload them
            LoadingBookMarksInProgress = false;
            return;
        }
        _bookmarkFilesPrefix = shortName;
        if (string.IsNullOrEmpty(_bookmarkFilesPrefix))
        {
            // No CRDocument or unnamed document
            // Should empty/disable Bookmarks view
            // Should disable add/remove bookmark buttons and so on 
            BookmarksFiles.Clear();
            Bookmarks.Clear();
            SelectedBookmarksFile = null;
            SelectedBookmark = null;
            _bookmarkFilesPrefix = string.Empty;
            LoadingBookMarksInProgress = false;
            return;
        }

        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var bookmarksDir = Path.Combine(documentsPath, "Odyssey", "bookmarks");
        Directory.CreateDirectory(bookmarksDir);

        var pattern = $"{_bookmarkFilesPrefix}-*.xml";
        var files = Directory.GetFiles(bookmarksDir, pattern);

        BookmarksFiles.Clear();
        foreach (var file in files)
        {
            // Label is the part after <ShortName>- and before .xml
            var fileName = Path.GetFileNameWithoutExtension(file);
            var label = fileName.Length > _bookmarkFilesPrefix.Length + 1
                ? fileName[(_bookmarkFilesPrefix.Length + 1)..]
                : fileName;
            BookmarksFiles.Add(new BookmarksFileItem(label, file));
        }

        if (BookmarksFiles.Count == 0)
        {
            // Add a "default" file/item if no bookmarks file is found
            var defaultFilePath = Path.Combine(bookmarksDir, $"{_bookmarkFilesPrefix}-default.xml");
            _currentBookmarksFile = defaultFilePath;
            AutoSave();
            BookmarksFiles.Add(new BookmarksFileItem("default", defaultFilePath));
            //Bookmarks.Clear();
            SelectedBookmarksFile = BookmarksFiles.First();
            SelectedBookmark = null;
            LoadingBookMarksInProgress = false;
            return;
        }

        // Prefer default, else first
        var defaultFile = BookmarksFiles.FirstOrDefault(f => f.Label.Equals("default", StringComparison.OrdinalIgnoreCase))
                       ?? BookmarksFiles.First();
        SelectedBookmarksFile = defaultFile;
        LoadingBookMarksInProgress = false;
        // TODO: should be set if current selection is a bookmark
        SelectedBookmark = null;
    }

    partial void OnSelectedBookmarksFileChanged(BookmarksFileItem? oldValue, BookmarksFileItem? newValue)
    {
        if (newValue != null)
        {
            LoadBookmarks(newValue.FilePath);
            SelectedBookmark = Bookmarks.FirstOrDefault();
        }
    }

    // Call this after any modification
    private void AutoSave()
    {
        if (!string.IsNullOrEmpty(_currentBookmarksFile))
        {
            SaveBookmarks(_currentBookmarksFile);
        }
    }

    private void ClearItems()
    {
        Bookmarks.Clear();
        BookmarksFiles.Clear();
    }

    private bool IsBookmarkable(DataBlock? dt)
    {
        if (dt == null)
        {
            return false;
        }
        var bt = dt.GetBlockType();
        return bt == BlockType.REGION
            || bt == BlockType.UNIT
            || bt == BlockType.SHIP
            || bt == BlockType.BUILDING;
    }

    [RelayCommand(CanExecute = nameof(CanJumpToPreviousBookmark))]
    private void JumpToPreviousBookmark(object? bookmark)
    {
        if (SelectedBookmark == null)
        {
            SelectedBookmark = Bookmarks.First();
            return;
        }
        int index = Bookmarks.IndexOf(SelectedBookmark);
        SelectedBookmark = index == 0 ? Bookmarks.Last() : Bookmarks[index - 1];
    }


    [RelayCommand(CanExecute = nameof(CanJumpToNextBookmark))]
    private void JumpToNextBookmark(object? bookmark)
    {
        if (SelectedBookmark == null)
        {
            SelectedBookmark = Bookmarks.First();
            return;
        }
        int index = Bookmarks.IndexOf(SelectedBookmark);
        SelectedBookmark = index == Bookmarks.Count - 1 ? Bookmarks.First() : Bookmarks[index + 1];
    }


    [RelayCommand(CanExecute = nameof(CanImportBookmarks))]
    private async Task ImportBookmarks()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }
        // LATER ? should have a custom overwrite check box to define behaviour about current bookmarks
        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            // LATER: translate title
            Title = "Import Bookmarks file",
            FileTypeFilter = GetOpenBookmarksFileTypes(),
            // LATER: multiple bookmarks files could be opened (means merging all)
            AllowMultiple = false
        });

        // TODO: check it's not in the default bookmarks folder and not the current bookmarks file
        var file = result.FirstOrDefault();

        if (file is not null)
        {
            // TODO: save file in the default bookmarks folder
            LoadBookmarks(file.Path.LocalPath);
        }
    }

    /// <summary>
    /// Export current bookmarks list to a user-selected file.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExportBookmarks))]
    private async Task ExportBookmarks()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        // LATER: based on current prefix, suggest a filename
        //string pathname = Path.ChangeExtension(fileViewModel.Path, defaultExtension);
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            // LATER: translate title
            Title = "Export Bookmarks As",
            FileTypeChoices = [StorageService.Bookmarks],
            //SuggestedFileName = pathname,
            DefaultExtension = StorageService.BookmarksExt,
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            SaveBookmarks(file.Path.LocalPath);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRemoveAllBookmarks))]
    private void RemoveAllBookmarks()
    {
        Bookmarks.Clear();
        AutoSave();
    }

    private bool CanJumpToPreviousBookmark()
    {
        return Bookmarks.Count > 1;
    }

    private bool CanJumpToNextBookmark()
    {
        return Bookmarks.Count > 1;
    }

    private bool CanToggleBookmark()
    {
        return IsBookmarkable(SelectedObject);
    }

    private bool CanRemoveAllBookmarks()
    {
        return Bookmarks.Count > 0;
    }

    private bool CanImportBookmarks()
    {
        return true;
    }
    private bool CanExportBookmarks()
    {
        return Bookmarks.Count > 0;
    }

    private static List<FilePickerFileType> GetOpenBookmarksFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Bookmarks
        };
    }

    partial void OnSelectionIsBookmarkedChanged(bool value)
    {
        if (SelectedObject is null)
            return;

        if (value)
            AddBookmark(SelectedObject);
        else
            RemoveBookmark(SelectedObject);
    }
}
