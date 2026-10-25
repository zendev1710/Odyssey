using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static BookmarkModel;

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
    private EntityModel? _selectedObject;

    [ObservableProperty]
    private bool _selectionIsBookmarkable;

    [ObservableProperty]
    private bool _selectionIsBookmarked;

    [ObservableProperty]
    private BookmarkModel? _selectedBookmark;

    [ObservableProperty]
    private bool _hasBookmark;

    private BookmarkModel? _lastSelectedBookmark;

    private string? _currentBookmarksFile;

    private String _bookmarkFilesPrefix = string.Empty;

    private bool LoadingBookmarksInProgress { get; set; }

    /// <summary>
    /// True while <see cref="SelectedBookmark"/> is updated from the selection (or while a bookmark is
    /// added or removed for the selection): the bookmark is then not selected by the user, so it must
    /// not be revealed in the explorer.
    /// </summary>
    private bool _syncingSelectedBookmark;


    public BookmarksViewModel() : base()
    {
        Bookmarks.CollectionChanged += (_, _) => OnBookmarksChanged();
    }

    /// <summary>
    /// </summary>
    partial void OnSelectedBookmarkChanged(BookmarkModel? oldValue, BookmarkModel? newValue)
    {
        if (newValue is not null)
        {
            _lastSelectedBookmark = newValue;

            // Reveal the bookmark only when it has been selected by the user (from the bookmarks list
            // or the next/previous bookmark commands), not when SelectedBookmark is synchronized with
            // the current selection.
            if (!_syncingSelectedBookmark)
            {
                RevealBookmark(newValue);
            }
        }

        // To enable the commands if necessary
        JumpToPreviousBookmarkCommand.NotifyCanExecuteChanged();
        JumpToNextBookmarkCommand.NotifyCanExecuteChanged();
    }

    // The selection changed: the bookmark state of the new selection has still to be computed, but the
    // ability to toggle a bookmark on it is already known (see UpdateFromGlobalSelection)
    partial void OnSelectedObjectChanged(EntityModel? value)
    {
        ToggleBookmarkCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Sets <see cref="SelectedBookmark"/> without revealing it: the bookmark is not selected by the
    /// user but synchronized with the current selection (or added/removed for it).
    /// </summary>
    private void SetSelectedBookmark(BookmarkModel? bookmarkModel)
    {
        _syncingSelectedBookmark = true;
        try
        {
            SelectedBookmark = bookmarkModel;
        }
        finally
        {
            _syncingSelectedBookmark = false;
        }
    }

    private void OnBookmarksChanged()
    {
        HasBookmark = Bookmarks.Count > 0;

        // A bookmark matching the current selection may have been added or removed (this also happens
        // when the bookmarks are reloaded from a file or when all the bookmarks are removed)
        UpdateSelectedBookmark();

        if (SelectedBookmark is not null && !Bookmarks.Contains(SelectedBookmark))
        {
            SetSelectedBookmark(null);
        }

        // To enable the commands if necessary
        ToggleBookmarkCommand.NotifyCanExecuteChanged();
        JumpToPreviousBookmarkCommand.NotifyCanExecuteChanged();
        JumpToNextBookmarkCommand.NotifyCanExecuteChanged();
        RemoveAllBookmarksCommand.NotifyCanExecuteChanged();
        SaveBookmarksAsCommand.NotifyCanExecuteChanged();
    }

    // Called when the active document changes (CRDocument loaded)
    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

        ClearAll();
        AutoLoadBookmarksForDocument(report);
    }

    private void AddBookmark(EntityModel dt)
    {
        if (IsBookmarkable(dt, out BookmarkType type) && !IsInBookmarks(dt, out BookmarkModel? _))
        {
            var model = BookmarkModel.FromObject(dt);
            Debug.WriteLine($"[BOOKMARKS] Add bookmark {type} {model.Id} for selection {dt}");
            Bookmarks.Insert(0, model);
            AutoSave();
            SetSelectedBookmark(model);
            SelectionIsBookmarked = true;
        }
        else
        {
            Debug.WriteLine($"[BOOKMARKS] WARNING | bookmark not added for {dt}");
        }
    }

    private void RemoveBookmark(EntityModel dt)
    {
        if (IsBookmarkable(dt, out _) && IsInBookmarks(dt, out BookmarkModel? bookmarkModel))
        {
            Debug.WriteLine($"[BOOKMARKS] Remove bookmark {bookmarkModel!.Type} {bookmarkModel.Id} for selection {dt}");
            Bookmarks.Remove(bookmarkModel!);
            AutoSave();
            SetSelectedBookmark(null);
            SelectionIsBookmarked = false;
            _lastSelectedBookmark = null;
        }
        else
        {
            Debug.WriteLine($"[BOOKMARKS] WARNING | bookmark not removed for {dt}");
        }
    }

    private bool IsInBookmarks(EntityModel? dt, out BookmarkModel? bookmarkModel)
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

        Debug.WriteLine($"[BOOKMARKS] ToggleBookmark for {SelectedObject} (bookmarked={SelectionIsBookmarked})");

        // The bookmarks list is the source of truth (and not the SelectionIsBookmarked flag, which
        // could be out of sync). Adding or removing the bookmark updates SelectionIsBookmarked (see
        // AddBookmark and RemoveBookmark), which in turn updates the star displayed in the status bar.
        if (IsInBookmarks(SelectedObject, out BookmarkModel? _))
        {
            RemoveBookmark(SelectedObject);
        }
        else
        {
            AddBookmark(SelectedObject);
        }
    }

    // Reveal bookmark in explorer (selects the object in the explorer view)
    private void RevealBookmark(BookmarkModel? bookmarkModel)
    {
        if (LoadingBookmarksInProgress || bookmarkModel?.Target is not EntityModel target)
        {
            return;
        }

        Debug.WriteLine($"[BOOKMARKS] Reveal bookmark {bookmarkModel.Type} {bookmarkModel.Id} for target {target} (current selection {SelectedObject})");

        // A region is selected with itself as its region. The other entities are selected without any
        // region: the explorer resolves the region of a unit, a ship or a building from its item.
        // Note: the selection is always sent, even when the target is already the current selection
        // (as MessagesListViewModel does when revealing an item in the explorer): the explorer is then
        // asked to show the item (expand its parents) in the tree.
        SendSelectionChangedEvent(new EntitySelection(target, target as RegionModel, null));
    }

    /// <summary>
    /// Reveals the bookmark currently selected in the bookmarks list.
    /// Normally called when the user clicks (selects) a bookmark in the bookmarks list, including
    /// when it is already the selected one: the explorer must then still show the item.
    /// </summary>
    public void RevealSelectedBookmark()
    {
        RevealBookmark(SelectedBookmark);
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        // The bookmark state shown in the status bar star (and the bookmark highlighted in the
        // bookmarks list) must always be the state of the current selection, whoever raised the
        // selection change event.
        // Note: this view model also raises such events (see RevealBookmark): the sender must not be
        // used to ignore them, otherwise SelectedObject would keep the previous selection. The star
        // would then show the bookmark state of another item than the current one, and RevealBookmark
        // would consider the clicked bookmark as already selected and would not reveal it.
        UpdateFromGlobalSelection(selection.Item);
    }

    private void UpdateFromGlobalSelection(EntityModel? item)
    {
        BookmarkModel? bookmarkModel = null;
        SelectedObject = item;
        SelectionIsBookmarkable = IsBookmarkable(SelectedObject, out _);
        SelectionIsBookmarked = SelectionIsBookmarkable && IsInBookmarks(SelectedObject, out bookmarkModel);
        SelectionName = SelectedObject?.DataBlock.GetUILabel() ?? string.Empty;
        SetSelectedBookmark(SelectionIsBookmarked ? bookmarkModel : null);
    }

    /// <summary>
    /// Update the bookmark state of the current selection. Called when the bookmarks list changes:
    /// the current selection may then be a bookmark or not anymore.
    /// </summary>
    private void UpdateSelectedBookmark()
    {
        if (SelectedObject is null)
        {
            return;
        }

        SelectionIsBookmarked = IsInBookmarks(SelectedObject, out _);
    }

    // Load bookmarks from XML file
    private List<BookmarkModel> LoadBookmarksFromFile(string filePath)
    {
        List<BookmarkModel> bookmarks = [];
        if (!File.Exists(filePath))
        {
            return bookmarks;
        }

        var doc = XDocument.Load(filePath);
        var root = doc.Root;
        if (root == null)
        {
            return bookmarks;
        }

        if (root.HasElements)
        {
            foreach (var el in root.Elements("bookmark"))
            {
                var type = el.Attribute("type")?.Value;
                var id = el.Attribute("id")?.Value;
                var name = el.Attribute("name")?.Value;
                var model = BookmarkModel.FromXml(type, id, name, Report);
                if (model != null)
                {
                    bookmarks.Add(model);
                }
            }
        }
        return bookmarks;

    }

    // Load bookmarks from XML file
    private void LoadBookmarks(string filePath)
    {
        // LATER: improve empty bookmark file handling (should be removed from the bookmarks files list)
        Bookmarks.Clear();

        foreach (var item in LoadBookmarksFromFile(filePath))
        {
            Bookmarks.Add(item);
        }
        _currentBookmarksFile = filePath;
    }

    /// <summary>
    /// Save bookmarks to the specified XML file.
    /// </summary>
    /// <param name="filePath">where bookmarks have to be saved</param>
    private void SaveBookmarksInto(IEnumerable<BookmarkModel> bookmarks, string filePath)
    {
        var doc = new XDocument(
            new XElement("Items",
                bookmarks.Select(b => b.ToXmlElement())
            )
        );
        doc.Save(filePath);
    }

    /// <summary>
    /// Save bookmarks to the specified XML file.
    /// </summary>
    /// <param name="filePath">where bookmarks have to be saved</param>
    private void SaveBookmarks(string filePath)
    {
        SaveBookmarksInto(Bookmarks, filePath);
    }

    // Auto-load bookmarks file for the current CRDocument
    private void AutoLoadBookmarksForDocument(CRDocument cr)
    {
        LoadingBookmarksInProgress = true;
        var shortName = cr.ShortName;
        // LATER: in Linux/Mac, comparison should be case-sensitive
        if (string.Equals(shortName, _bookmarkFilesPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Same prefix means same bookmarks file
            // I's not useful to reload them
            LoadingBookmarksInProgress = false;
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
            SetSelectedBookmark(null);
            _lastSelectedBookmark = null;
            _bookmarkFilesPrefix = string.Empty;
            LoadingBookmarksInProgress = false;
            return;
        }

        var bookmarksDir = GetDefaultBookmarksDirectory();
        Directory.CreateDirectory(bookmarksDir);

        var pattern = $"{_bookmarkFilesPrefix}-*.xml";
        var pathNames = Directory.GetFiles(bookmarksDir, pattern);

        BookmarksFiles.Clear();
        foreach (var pathName in pathNames)
        {
            // Label is the part after <ShortName>- and before .xml
            var fileName = Path.GetFileNameWithoutExtension(pathName);
            var label = fileName.Length > _bookmarkFilesPrefix.Length + 1
                ? fileName[(_bookmarkFilesPrefix.Length + 1)..]
                : fileName;
            BookmarksFiles.Add(new BookmarksFileItem(label, pathName));
        }

        if (BookmarksFiles.Count == 0)
        {
            // Add a "default" file/item if no bookmarks file is found
            var defaultFilePath = Path.Combine(bookmarksDir, $"{_bookmarkFilesPrefix}-default.xml");
            _currentBookmarksFile = defaultFilePath;
            AutoSave();
            BookmarksFiles.Add(new BookmarksFileItem("default", defaultFilePath));
            SelectedBookmarksFile = BookmarksFiles.First();
            SetSelectedBookmark(null);
            _lastSelectedBookmark = null;
            LoadingBookmarksInProgress = false;
            return;
        }

        // Prefer default, else first
        var defaultFile = BookmarksFiles.FirstOrDefault(f => f.Label.Equals("default", StringComparison.OrdinalIgnoreCase))
                       ?? BookmarksFiles.First();
        SelectedBookmarksFile = defaultFile;
        LoadingBookmarksInProgress = false;
        SetSelectedBookmark(null);
    }

    partial void OnSelectedBookmarksFileChanged(BookmarksFileItem? oldValue, BookmarksFileItem? newValue)
    {
        if (newValue != null)
        {
            // LATER : if _lastSelectedBookmark matches an existing bookmark in the new list, should set it
            _lastSelectedBookmark = null;
            LoadBookmarks(newValue.FilePath);
            UpdateSelectedBookmark();
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

    private void ClearAll()
    {
        Bookmarks.Clear();
        BookmarksFiles.Clear();
        UpdateFromGlobalSelection(null);

        _lastSelectedBookmark = null;
        _currentBookmarksFile = null;
        _bookmarkFilesPrefix = string.Empty;
        LoadingBookmarksInProgress = false;
    }

    private bool IsBookmarkable(EntityModel? dt, out BookmarkType bmt)
    {
        // The type of bookmark an entity can have is defined by the bookmark model itself.
        bmt = BookmarkModel.GetBookmarkType(dt);
        return bmt != BookmarkType.Unknown;
    }

    [RelayCommand(CanExecute = nameof(CanJumpToPreviousBookmark))]
    private void JumpToPreviousBookmark(object? bookmark)
    {
        BookmarkModel? currentBookmark = SelectedBookmark ?? _lastSelectedBookmark;
        if (currentBookmark == null)
        {
            SelectedBookmark = Bookmarks.First();
            return;
        }
        int index = Bookmarks.IndexOf(currentBookmark);
        if (index == -1) { index = 1; }
        SelectedBookmark = index == 0 ? Bookmarks.Last() : Bookmarks[index - 1];
    }


    [RelayCommand(CanExecute = nameof(CanJumpToNextBookmark))]
    private void JumpToNextBookmark(object? bookmark)
    {
        BookmarkModel? currentBookmark = SelectedBookmark ?? _lastSelectedBookmark;
        if (currentBookmark == null)
        {
            SelectedBookmark = Bookmarks.First();
            return;
        }
        int index = Bookmarks.IndexOf(currentBookmark);
        SelectedBookmark = index == Bookmarks.Count - 1 ? Bookmarks.First() : Bookmarks[index + 1];
    }


    [RelayCommand(CanExecute = nameof(CanLoadBookmarksFrom))]
    private async Task LoadBookmarksFrom()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }
        // LATER ? should have a custom overwrite check box to define behaviour about current bookmarks
        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Labels.Localize(Labels.DIALOG_TITLE_BOOKMARKS_LOAD),
            FileTypeFilter = GetOpenBookmarksFileTypes(),
            AllowMultiple = false
        });

        var file = result[0];
        if (file is not null)
        {
            // Save a copy into default location, load then select if several bookmarks files
            var bookmarksDir = GetDefaultBookmarksDirectory();
            var sourcePath = file.Path.LocalPath;
            var sourceFolder = Path.GetDirectoryName(sourcePath);
            // if it is an existing bookmarks file from default bookmarks, do nothing
            if (!string.Equals(sourceFolder, bookmarksDir, StringComparison.OrdinalIgnoreCase))
            {
                var shortFileNameWithExt = Path.GetFileName(sourcePath);
                var destinationPathname = Path.Combine(bookmarksDir, shortFileNameWithExt);
                var shortFileName = Path.GetFileNameWithoutExtension(sourcePath);

                var i = 1;
                // rename until file does not exist
                while (File.Exists(destinationPathname))
                {
                    // rename it
                    destinationPathname = Path.Combine(bookmarksDir, $"{shortFileName}-{i}.{StorageService.BookmarksExt}");
                    i++;
                }

                // could be also a simple file copy
                var bookmarks = LoadBookmarksFromFile(sourcePath);
                SaveBookmarksInto(bookmarks, destinationPathname);
                if (!string.IsNullOrEmpty(_bookmarkFilesPrefix))
                {
                    var pattern = $"{_bookmarkFilesPrefix}-";
                    var fileName = Path.GetFileNameWithoutExtension(destinationPathname);
                    // Load and add bookmarks file only if file prefix is the same as the current prefix
                    if (fileName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        LoadingBookmarksInProgress = true;
                        var label = fileName[(_bookmarkFilesPrefix.Length + 1)..];
                        // LATER: add at the right place (based on label sorting?)
                        BookmarksFiles.Add(new BookmarksFileItem(label, destinationPathname));
                        SelectedBookmarksFile = BookmarksFiles.Last();
                        LoadBookmarks(destinationPathname);
                        LoadingBookmarksInProgress = false;
                    }
                }
            }
        }
    }

    private String GetDefaultBookmarksDirectory()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documentsPath, "Odyssey", "bookmarks");
    }

    /// <summary>
    /// Save current bookmarks list as a user-selected file.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveBookmarksAs))]
    private async Task SaveBookmarksAs()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var bookmarksDir = GetDefaultBookmarksDirectory();
        var suggestFilePathName = Path.Combine(bookmarksDir, $"{_bookmarkFilesPrefix}-bookmarks.xml");
        // LATER: based on current prefix, suggest a filename
        //string pathname = Path.ChangeExtension(fileViewModel.Path, defaultExtension);
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Labels.Localize(Labels.DIALOG_TITLE_BOOKMARKS_SAVE_AS),
            FileTypeChoices = [StorageService.Bookmarks],
            SuggestedFileName = suggestFilePathName,
            //SuggestedStartLocation = StorageFileLocation.Documents,
            DefaultExtension = StorageService.BookmarksExt,
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            var sourcePath = file.Path.LocalPath;
            SaveBookmarks(sourcePath);
            var sourceFolder = Path.GetDirectoryName(sourcePath);
            var pattern = $"{_bookmarkFilesPrefix}-";
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            if (string.Equals(sourceFolder, bookmarksDir, StringComparison.OrdinalIgnoreCase) && fileName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: check if element is added as duplicata if already exists in list
                LoadingBookmarksInProgress = true;
                var label = fileName[(_bookmarkFilesPrefix.Length + 1)..];
                // LATER: add at the right place (based on label sorting?)
                BookmarksFiles.Add(new BookmarksFileItem(label, sourcePath));
                SelectedBookmarksFile = BookmarksFiles.Last();
                LoadBookmarks(sourcePath);
                LoadingBookmarksInProgress = false;
            }
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
        return IsBookmarkable(SelectedObject, out _);
    }

    private bool CanRemoveAllBookmarks()
    {
        return Bookmarks.Count > 0;
    }

    private bool CanLoadBookmarksFrom()
    {
        return HasDocument;
    }

    private bool CanSaveBookmarksAs()
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
}
