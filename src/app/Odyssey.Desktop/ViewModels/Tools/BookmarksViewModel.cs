using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
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

    private BookmarkModel? _lastSelectedBookmark;

    private string? _currentBookmarksFile;

    private String _bookmarkFilesPrefix = string.Empty;

    private bool LoadingBookmarksInProgress { get; set; }

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
        if (newValue is not null)
        {
            _lastSelectedBookmark = newValue;
            RevealBookmark(newValue);
        }

        // To enable the commands if necessary
        JumpToPreviousBookmarkCommand.NotifyCanExecuteChanged();
        JumpToNextBookmarkCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectionIsBookmarkedChanged(bool value)
    {
        if (SelectedObject is null)
        {
            return;
        }

        if (value)
        {
            AddBookmark(SelectedObject);
        }
        else
        {
            RemoveBookmark(SelectedObject);
        }
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
        ClearAll();
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
            _lastSelectedBookmark = null;
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
        if (!LoadingBookmarksInProgress && bookmarkModel?.Target is not null)
        {
            // Publish event only if the bookmark target is not the current global selection (from Explorer/Map view)
            if (SelectedObject != bookmarkModel.Target)
            {
                ISelection sel = new SimpleItemSelection(bookmarkModel.Target, null, null);
                SendSelectionChangedEvent(sel);
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
        // if selection in event is an existing bookmark, select it in orde the user to see that it's a bookmark
        ISelection sel = selectionChange.Selection;
        if (!IsSelected(sel))
        {
            SetSelection(sel);
        }
    }

    protected override void SetSelection(ISelection? sel)
    {
        base.SetSelection(sel);
        BookmarkModel? bookmarkModel = null;
        SelectedObject = sel?.Item;
        SelectionIsBookmarkable = IsBookmarkable(SelectedObject);
        SelectionIsBookmarked = SelectionIsBookmarkable && IsInBookmarks(SelectedObject, out bookmarkModel);
        SelectionName = SelectedObject?.GetUILabel() ?? string.Empty;
        SelectedBookmark = SelectionIsBookmarked ? bookmarkModel : null;
    }

    private void UpdateSelectedBookmark()
    {
        // TODO: update bookmark status according to the if current selection (Selection) bookmarks list belonging
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
                var model = BookmarkModel.FromXml(type, id, name, GetDocument());
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
        Bookmarks.AddRange(LoadBookmarksFromFile(filePath));
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
            SelectedBookmark = null;
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
            SelectedBookmark = null;
            _lastSelectedBookmark = null;
            LoadingBookmarksInProgress = false;
            return;
        }

        // Prefer default, else first
        var defaultFile = BookmarksFiles.FirstOrDefault(f => f.Label.Equals("default", StringComparison.OrdinalIgnoreCase))
                       ?? BookmarksFiles.First();
        SelectedBookmarksFile = defaultFile;
        LoadingBookmarksInProgress = false;
        SelectedBookmark = null;
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
        SetSelection(null);

        _lastSelectedBookmark = null;
        _currentBookmarksFile = null;
        _bookmarkFilesPrefix = string.Empty;
        LoadingBookmarksInProgress = false;
    }

    private bool IsBookmarkable(DataBlock? dt)
    {
        if (dt is null)
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
        return IsBookmarkable(SelectedObject);
    }

    private bool CanRemoveAllBookmarks()
    {
        return Bookmarks.Count > 0;
    }

    private bool CanLoadBookmarksFrom()
    {
        return true;
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
