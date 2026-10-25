using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using Dock.Serializer;
using Odyssey.Controls.Ships;
using Odyssey.Core.Services;
using Odyssey.Extensions;
using Odyssey.Messages;
using Odyssey.Models.Dal;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Settings;
using Odyssey.Utils;
using Odyssey.ViewModels.Documents;
using Odyssey.ViewModels.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Odyssey.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDropTarget
{
    public string AppVersion =>
    Assembly.GetEntryAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion ?? "Unknown";

    public string WindowTitle => $"Odyssey v{AppVersion}";

    private RecentFilesManager _recentFilesManager = new RecentFilesManager("Odyssey");

    public ObservableCollection<string> RecentFiles { get; } = new();

    private string? InitialDocumentPathname { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _enableSearchFeature;

    [ObservableProperty]
    private bool _enableLayoutFeature;

    [ObservableProperty]
    private bool _hasDocument;

    [ObservableProperty]
    private string? _gameTurn;

    [ObservableProperty]
    private string? _gameDate;

    [ObservableProperty]
    private string? _gameName; // Name of the game (from report); usually Eressea or E3

    [ObservableProperty]
    private string? _reportName;

    [ObservableProperty]
    private string? _factionName;

    [ObservableProperty]
    private bool _isFullscreen;

    [ObservableProperty]
    private bool _isMapViewVisible;

    [ObservableProperty]
    private bool _isMiniMapViewVisible;

    [ObservableProperty]
    private bool _isExplorerViewVisible;

    [ObservableProperty]
    private bool _isHistoryViewVisible;

    [ObservableProperty]
    private bool _isBookmarksViewVisible;

    [ObservableProperty]
    private bool _isRegionPropertiesViewVisible;

    [ObservableProperty]
    private bool _isRegionStatisticsViewVisible;

    [ObservableProperty]
    private bool _isDetailsViewVisible;

    [ObservableProperty]
    private bool _isUnitOrdersViewVisible;

    [ObservableProperty]
    private bool _isRegionInfoViewVisible;

    [ObservableProperty]
    private bool _isBattlesViewVisible;

    [ObservableProperty]
    private bool _isSearchResultsViewVisible;

    [ObservableProperty]
    private bool _isReportInfoViewVisible;

    [ObservableProperty]
    private bool _isErrorsListViewVisible;

    private IDockSerializer? _serializer;
    private IDockState? _dockState;

    private static readonly Dictionary<string, Action<MainWindowViewModel, bool>> _viewVisibilitySetters = new()
    {
        { Ids.Map,                (vm, v) => vm.IsMapViewVisible = v },
        { Ids.MiniMap,            (vm, v) => vm.IsMiniMapViewVisible = v },
        { Ids.Explorer,           (vm, v) => vm.IsExplorerViewVisible = v },
        { Ids.History,            (vm, v) => vm.IsHistoryViewVisible = v },
        { Ids.Bookmarks,          (vm, v) => vm.IsBookmarksViewVisible = v },
        { Ids.RegionProperties,   (vm, v) => vm.IsRegionPropertiesViewVisible = v },
        { Ids.RegionStatistics,   (vm, v) => vm.IsRegionStatisticsViewVisible = v },
        { Ids.Details,            (vm, v) => vm.IsDetailsViewVisible = v },
        { Ids.UnitOrders,         (vm, v) => vm.IsUnitOrdersViewVisible = v },
        { Ids.RegionInfo,         (vm, v) => vm.IsRegionInfoViewVisible = v },
        { Ids.Battles,            (vm, v) => vm.IsBattlesViewVisible = v },
        { Ids.SearchResults,      (vm, v) => vm.IsSearchResultsViewVisible = v },
        { Ids.ReportInfo,         (vm, v) => vm.IsReportInfoViewVisible = v },
        { Ids.ErrorList,          (vm, v) => vm.IsErrorsListViewVisible = v },
    };

    private static readonly List<string> _inProgressFeaturesIds =
    [
        Ids.MiniMap,
        Ids.RegionStatistics,
        Ids.SearchResults,
        Ids.ErrorList,
    ];

    private readonly IFactory _factory;
    private readonly IDocumentService _activeDocumentService;

    private readonly IEntitySelectionService _selectionService;
    private IRootDock? _layout;
    private string _globalStatus = "Global: (none)";

    private readonly bool _singleReportMode = true;

    private readonly bool _readonlyMode = true;

    // Last route mode that was not "all", so the "show all routes" toggle can restore it.
    // Default to the selected region: unchecking "all routes" must still show the routes
    // related to the current selection instead of hiding every route.
    private int _lastNonAllShipRouteMode = ShipRouteModes.SelectedRegion;

    /// <summary>True when the map currently draws every ship route.</summary>
    public bool AreAllShipRoutesVisible =>
        GlobalSettings.Get<int>(GlobalSettings.MAP_SHIP_ROUTE_MODE) == ShipRouteModes.All;

    /// <summary>True when region names are drawn on the map.</summary>
    public bool AreRegionNamesVisible =>
        GlobalSettings.Get<bool>(GlobalSettings.MAP_SHOW_REGION_NAMES);

    /// <summary>True when region names are drawn inside a gold banner.</summary>
    public bool IsRegionNameBannerEnabled =>
        GlobalSettings.Get<bool>(GlobalSettings.MAP_REGION_NAME_BANNER);

    private BookmarksViewModel? _bookmarksViewModel;
    public BookmarksViewModel? BookmarksViewModel => _bookmarksViewModel;

    // NOTE: planes management has a combobox (in the document status bar) and a menu. Should have a SelectedPlane property.
    // SelectedPlaneName should be the name of the selected plane in the combobox.
    public ObservableCollection<WorldPlane> Planes { get; private set; } = [];

    public string GlobalStatus
    {
        get => _globalStatus;
        set => SetProperty(ref _globalStatus, value);
    }

    public IRootDock? Layout
    {
        get => _layout;
        set => SetProperty(ref _layout, value);
    }

    public ICommand LayoutNew { get; }

    public ICommand LayoutOpen { get; }

    public ICommand LayoutSave { get; }

    public ICommand LayoutClose { get; }

    public ICommand FindSetFocus  { get; }

    public string Id { get; } = Ids.MainWindowVmId;

    public MainWindowViewModel()
    {
        // TODO: check with "if (!Design.IsDesignMode)" if we do not want to do some actions in design mode


        _activeDocumentService = ServiceLocator.DocumentService;
        _selectionService = ServiceLocator.EntitySelectionService;

        _activeDocumentService.ActiveDocumentChanged += OnActiveDocumentChanged;
        _selectionService.SelectionChanged += OnSelectionChanged;

        int initialRouteMode = GlobalSettings.Get<int>(GlobalSettings.MAP_SHIP_ROUTE_MODE);
        _lastNonAllShipRouteMode = initialRouteMode == ShipRouteModes.All
            ? ShipRouteModes.SelectedRegion
            : initialRouteMode;
        GlobalSettings.Shared.SettingChanged += OnGlobalSettingChanged;

        _factory = new DockFactory();
        _isFullscreen = false;
        bool hideInProgressFeatures = GlobalSettings.Get<bool>(GlobalSettings.HIDE_IN_PROGRESS_FEATURES);
        _enableSearchFeature = !hideInProgressFeatures;
        _enableLayoutFeature = !hideInProgressFeatures;

        _factory.DockableHidden += OnDockableHidden;
        _factory.DockableRestored += OnDockableRestored;

        DebugFactoryEvents(_factory);

        //_serializer = new DockSerializer(typeof(AvaloniaList<>));
        //_dockState = new DockState();

        Layout = _factory?.CreateLayout();
        InitLayout();
        GlobalStatus = Layout is null
            ? "Global: (none)"
            : FormatGlobalStatus(_factory?.GlobalDockTrackingState ?? GlobalDockTrackingState.Empty);

        // TODO: check when it should be called exactly
        InitializeDockState();

        if (Layout is { } root)
        {
            // Display the workspace view (main view), which embeds all the layout docked windows
            root.Navigate.Execute(Ids.Workspace);
        }

        _bookmarksViewModel = _factory.GetDockable<IDockable>(Ids.Bookmarks) as BookmarksViewModel;

        LayoutNew = new RelayCommand(ResetLayout);
        LayoutOpen = new RelayCommand(FileOpenLayout);
        LayoutSave = new RelayCommand(FileSaveLayout);
        LayoutClose = new RelayCommand(CloseLayout);

        FindSetFocus = new RelayCommand(ActivateFindArea);

        foreach (var setter in _viewVisibilitySetters.Values)
        {
            setter(this, true);
        }

        /*
        foreach (var id in _viewVisibilitySetters.Keys)
        {
            _factory.RestoreDockable(_factory.GetDockable<IDockable>(id)!);
        }
        */
        if (hideInProgressFeatures)
        {
            foreach (var id in _inProgressFeaturesIds)
            {
                _factory.HideDockable(_factory.GetDockable<IDockable>(id)!);
            }
        }

        _factory.DockableClosed += OnDockableClosed;

        if (!Design.IsDesignMode)
        {
            if (App.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                if (desktopLifetime.Args?.Length > 0)
                {
                    InitialDocumentPathname = desktopLifetime.Args[0];
                }
            }
            // After loading recent files from manager:
            foreach (var file in _recentFilesManager.RecentFiles)
            {
                RecentFiles.Add(file);
            }

            if (string.IsNullOrEmpty(InitialDocumentPathname) && GlobalSettings.Get<bool>(GlobalSettings.OPEN_LAST_REPORT_AT_STARTUP))
            {
                // Retrieve last opened report file pathname to open it, if any
                Func<string, bool> predicate = static f => IsSupportedFileType(f, out DocumentType fileType) && fileType.IsReportType();
                InitialDocumentPathname = RecentFiles.FirstOrDefault(predicate);
            }
        }

        //OpenRecentFileCommand = new RelayCommand<string>(OpenRecentFile);
        
        // DockSettings.CloseFloatingWindowsOnMainWindowClose
        // DockSettings.EnableDiagnosticsLogging
        // DockSettings.UseFloatingDockAdorner
        // DockSettings.UsePinnedDockWindow
        // DockSettings.DocumentWindowDragScope
        // DocumentLayoutMode.Tabbed (not MDI)
    }

    private void InitializeDockState()
    {
        _serializer = new DockSerializer(typeof(ObservableCollection<>));
        _dockState = new DockState();
        if (Layout is not null)
        {
            _dockState.Save(Layout);
         }
    }

    [RelayCommand]
    private async Task OpenRecentFileAsync(string pathname)
    {
        // Close the menu by forcing execution priority
        // This allows the "Recent files" popup/sub-menu to close before starting OpenDocument
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        // Perform the heavy loading
        OpenDocument(pathname);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfReportExists))]
    private void CenterMapOnSelectedRegion()
    {
        // On envoie le message globalement dans l'application
        WeakReferenceMessenger.Default.Send(new CenterMapMessage(IsGlobal: false));
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfReportExists))]
    private void CenterMapGlobally()
    {
        WeakReferenceMessenger.Default.Send(new CenterMapMessage(IsGlobal: true));
    }

    /// <summary>
    /// Shows or hides every ship route with one menu entry / keyboard shortcut. When hiding, the
    /// previous non-"all" mode (selected ship, selected region, or none) is restored.
    /// </summary>
    [RelayCommand]
    private void ToggleAllShipRoutes()
    {
        GlobalSettings settings = GlobalSettings.Shared;
        int current = GlobalSettings.Get<int>(GlobalSettings.MAP_SHIP_ROUTE_MODE);

        if (current == ShipRouteModes.All)
        {
            // The "all routes" shortcut never falls back to "none": unchecking it must leave
            // selection-based routes visible. "None" remains available in the settings dialog.
            settings.ShipRouteMode = _lastNonAllShipRouteMode is ShipRouteModes.All or ShipRouteModes.None
                ? ShipRouteModes.SelectedRegion
                : _lastNonAllShipRouteMode;
        }
        else
        {
            _lastNonAllShipRouteMode = current;
            settings.ShipRouteMode = ShipRouteModes.All;
        }

        OnPropertyChanged(nameof(AreAllShipRoutesVisible));
    }

    /// <summary>Shows or hides region names without opening the configuration dialog.</summary>
    [RelayCommand]
    private void ToggleRegionNames()
    {
        GlobalSettings settings = GlobalSettings.Shared;
        settings.ShowRegionNames = !settings.ShowRegionNames;
        OnPropertyChanged(nameof(AreRegionNamesVisible));
    }

    /// <summary>Toggles the gold banner around region names.</summary>
    [RelayCommand]
    private void ToggleRegionNameBanner()
    {
        GlobalSettings settings = GlobalSettings.Shared;
        settings.RegionNameBanner = !settings.RegionNameBanner;
        OnPropertyChanged(nameof(IsRegionNameBannerEnabled));
    }

    private void OnGlobalSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName != GlobalSettings.MAP_SHIP_ROUTE_MODE &&
            e.SettingName != GlobalSettings.MAP_SHOW_REGION_NAMES &&
            e.SettingName != GlobalSettings.MAP_REGION_NAME_BANNER)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnGlobalSettingChanged(sender, e));
            return;
        }

        if (e.SettingName == GlobalSettings.MAP_SHIP_ROUTE_MODE)
        {
            int mode = GlobalSettings.Get<int>(GlobalSettings.MAP_SHIP_ROUTE_MODE);
            if (mode != ShipRouteModes.All)
            {
                _lastNonAllShipRouteMode = mode;
            }

            OnPropertyChanged(nameof(AreAllShipRoutesVisible));
        }
        else if (e.SettingName == GlobalSettings.MAP_SHOW_REGION_NAMES)
        {
            OnPropertyChanged(nameof(AreRegionNamesVisible));
        }
        else if (e.SettingName == GlobalSettings.MAP_REGION_NAME_BANNER)
        {
            OnPropertyChanged(nameof(IsRegionNameBannerEnabled));
        }
    }

    private bool CanExecuteIfReportExists()
    {
        return HasDocument;
    }

    public void OpenInitialDocumentIfNeeded()
    {
        if (!string.IsNullOrEmpty(InitialDocumentPathname))
        {
            if (File.Exists(InitialDocumentPathname))
            {
                OpenDocument(InitialDocumentPathname);
            }
            InitialDocumentPathname = string.Empty;
        }
    }

    private void OnDockableRestored(object? sender, Dock.Model.Core.Events.DockableRestoredEventArgs e)
    {
        var dockableId = e.Dockable?.Id;
        if (dockableId is not null && _viewVisibilitySetters.TryGetValue(dockableId, out var setter))
        {
            setter(this, true);
        }
    }

    private void OnDockableHidden(object? sender, Dock.Model.Core.Events.DockableHiddenEventArgs e)
    {
        var dockableId = e.Dockable?.Id;
        if (dockableId is not null && _viewVisibilitySetters.TryGetValue(dockableId, out var setter))
        {
            setter(this, false);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
    }

    public async void FileOpenLayout()
    {
        await OpenLayout();
    }

    public async void FileSaveLayout()
    {
        await SaveLayout();
    }

    public void CloseLayout()
    {
        Debug.WriteLine("[MAINWINDOW] CloseLayout...");
        if (Layout is IDock dock)
        {
            if (dock.Close.CanExecute(null))
            {
                Debug.WriteLine("[MAINWINDOW] Closing dock...");
                dock.Close.Execute(null);
                Debug.WriteLine("[MAINWINDOW] dock closed");
            }
            else
                Debug.WriteLine("[MAINWINDOW] dock.Close.CanExecute returned false");
        }
        Layout = null;
        Debug.WriteLine("[MAINWINDOW] CloseLayout ended.");
    }

    private void ResetLayout()
    {
        Debug.WriteLine("[MAINWINDOW] ResetLayout...");
        if (Layout is not null)
        {
            if (Layout.Close.CanExecute(null))
            {
                Layout.Close.Execute(null);
            }
        }

        Layout = _factory.CreateLayout();
        InitLayout();

        Debug.WriteLine("[MAINWINDOW] ResetLayout ended.");
    }

    public void ActivateFindArea()
    {
        // TO IMPLEMENT:
        // only if at lease a document is opened :
        // Set focus to the quick find text box
        // Maybe use a bool representing the IsActiveFind and :
        // - findtextbox.Focus() called when this property changes to true
        // - IsActiveFind set to false when focus is changed/lost
        Debug.WriteLine("[MAINWINDOW] Activate find area");
    }

    public void DragOver(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.None;
            e.Handled = true;
        }
    }

    public void Drop(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            var result = e.DataTransfer.TryGetFiles();
            if (result is { })
            {
                // Drop with Ctrl key pressed => merge requested
                OpenOrMergeFiles(result, e.KeyModifiers.HasFlag(KeyModifiers.Control));
            }
            e.Handled = true;
        }
    }

    private static Encoding GetEncoding(string path)
    {
        using var reader = new StreamReader(path, Encoding.Default, true);
        if (reader.Peek() >= 0)
        {
            reader.Read();
        }
        return reader.CurrentEncoding;
    }

    private static void SaveFileViewModel(FileViewModel fileViewModel)
    {
        DocumentType docType = fileViewModel.DocumentType;
        string path = fileViewModel.Path;
        CRDocument doc = (fileViewModel.Document as CRDocument)!;

        switch (docType)
        {
            case DocumentType.ERESSEA_REPORT:
            case DocumentType.ERESSEA_REPORT_FROM_ZIP:
                ReportFileService.Save(path, doc, MapType.FULL, []);
                break;
            case DocumentType.ERESSEA_ORDERS:
                //OrdersFileService.Save(path, doc, MapType.FULL, []);
                break;
            case DocumentType.TXT:
                //TxtFileService.Save(path, doc, MapType.FULL, []);
                break;
            default:
                Debug.WriteLine($"[MAINWINDOW] WARNING | SaveFileViewModel - Unable to save file of type {fileViewModel.DocumentType}!");
                break;
        }
    }

    private void CollectPlanes()
    {
        Planes.Clear();
        Planes.Add(new WorldPlane(PlaneType.WORLD));
    }

    private void OnActiveDocumentChanged(CRDocument report)
    {
        OnActiveReportDocumentChanged(report);
    }

    private void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        // If event come from this viewModel, ignore it
        if (ReferenceEquals(sender, this))
            return;

        // TODO
    }

    private void OnActiveReportDocumentChanged(CRDocument report)
    {
        CollectGlobalReportInformation(report);
        CollectPlanes();

        if (report.IsEmpty())
        {
            _selectionService.Clear(this);
            return;
        }

        // TODO: Selection id and type should have been saved before (region Id as an automatic bookmark), then cleared
        if (GlobalSettings.Get<bool>(GlobalSettings.EXPLORER_SELECT_LAST_ACTIVE_REGION_ON_REPORT_OPENING))
        {
            // TODO: get active region from CRDocument and set as selection
        }

        OnReportChange(report);
    }

    private void OnReportDocumentLoaded(CRDocument report)
    {
        if (HasReport(out CRDocument? previousReport))
        {
            // TODO: maybe it should be done before entering here
            _activeDocumentService.ClearDocument();
        }

        OnReportChange(report);
        _activeDocumentService.SetActiveDocument(report);
    }

    private bool FileViewModelExists(string pathname)
    {
        var files = _factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId);
        if (files is { })
        {
            // TODO: Check if a FileViewModel with the same Id and pathname already exists in the document dock
            /*
            foreach (var dockable in files)
            {
                if (dockable is FileViewModel fvm && fvm.Path == pathname)
                {
                    return true;
                }
            }
            */
        }
        return false;
    }

    private void AddFileViewModel(FileViewModel fileViewModel)
    {
        var files = _factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId);
        if (Layout is { } && files is { })
        {
            _factory.AddDockable(files, fileViewModel);
            _factory.SetActiveDockable(fileViewModel);
            _factory.SetFocusedDockable(Layout, fileViewModel);
        }
    }

    /// <summary>
    /// Get the file view model for the active document.
    /// </summary>
    /// <returns> the file view model for the active document; null if no document is opened.</returns>
    private bool GetActiveFileViewModel(out FileViewModel? fileViewModel, out bool isModified)
    {
        var files = _factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId);
        fileViewModel = files?.ActiveDockable as FileViewModel;
        bool hasActive = fileViewModel is not null;
        isModified = hasActive ? fileViewModel!.IsModified : false;
        return hasActive;
    }

    /// <summary>
    /// Gets the active opened document or null if no document is opened.
    /// </summary>
    /// <returns>The active opened document; null if no document is opened.</returns>
    private EresseaDocument? GetActiveDocument(out DocumentType docFileType)
    {
        docFileType = DocumentType.UNKNOWN;
        FileViewModel? fileViewModel;
        if (GetActiveFileViewModel(out fileViewModel, out _))
        {
            docFileType = fileViewModel!.DocumentType;
            return fileViewModel.Document;
        }
        return null;
    }

    private bool HasReport(out CRDocument? report)
    {
        FileViewModel? fileViewModel;
        report = null;
        if (GetActiveFileViewModel(out fileViewModel, out _) && (fileViewModel is not null && fileViewModel.DocumentType.IsReportType()))
        {
           report = fileViewModel.Document as CRDocument;
           return report is not null;
        }
        return false;
    }

    /// <summary>
    /// Gets the active opened document or null if no document is opened.
    /// </summary>
    /// <returns>The active opened document; null if no document is opened.</returns>
    private EresseaDocument? GetActiveDocument()
    {
        FileViewModel? fileViewModel;
        if (GetActiveFileViewModel(out fileViewModel, out _))
        {
            return fileViewModel?.Document;
        }
        return null;
    }

    private bool ActiveDocumentIsModified()
    {
        EresseaDocument? d = GetActiveDocument();
        if (d != null)
        {
            return d.IsModified;
        }
        return false;
    }

    private static Window? GetWindow()
    {
        Debug.WriteLine("[MAINWINDOW] Getting window");
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return desktopLifetime.MainWindow;
        }
        return null;
    }

    private static List<FilePickerFileType> GetOpenReportFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Report,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetOpenOrdersFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Orders,
            StorageService.All
        };
    }
    private static List<FilePickerFileType> GetMergeEresseaFilesTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.ReportEresseaFiles,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetOpenEresseaFilesTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.EresseaFiles,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetOpenLayoutFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetSaveLayoutFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private async Task OpenReportOrOdersFile()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }
        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            // TODO: translate title
            Title = "Open Report or Orders file",
            FileTypeFilter = GetOpenEresseaFilesTypes(),
            // Multiple files can be opened (or merged)
            AllowMultiple = true
        });
        OpenFiles(result);
    }

    private async Task LoadThenMergeReportFiles()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }
        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            // TODO: translate title
            Title = "Open Report files to be merged",
            FileTypeFilter = GetMergeEresseaFilesTypes(),
            // Merge has to be performed on multiple files
            AllowMultiple = true
        });
        MergeFiles(result);
    }

    private bool OpenDocument(string pathname)
    {
        if (!IsSupportedFileType(pathname, out DocumentType fileType))
        {
            // LATER: log it in docked windows console
            Debug.WriteLine($"[MAINWINDOW] WARNING | OpenDocument - '{pathname}' is not an Eressea file");
            return false;
        }
        FileViewModel? fileViewModel = null;
        return OpenDocument(pathname, fileType, out fileViewModel);
    }

    private void UpdateRecentFiles(string pathname)
    {
        // Update RecentFilesManager (handles duplicates and persistence)
        _recentFilesManager.AddFile(pathname);
        // Update UI collection
        // Remove if already present
        int existingIndex = RecentFiles.IndexOf(pathname);
        if (existingIndex >= 0)
            RecentFiles.RemoveAt(existingIndex);
        // Insert at top
        RecentFiles.Insert(0, pathname);
        // Limit to 10 items
        // TODO: make this limit configurable
        while (RecentFiles.Count > 10)
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
    }

    private bool OpenDocument(string pathname, DocumentType fileType, out FileViewModel? fileViewModel)
    {
        fileViewModel = null;
        if (FileViewModelExists(pathname))
        {
            Debug.WriteLine($"[MAINWINDOW] WARNING | OpenDocument - '{pathname}' is already opened");
            return false;
        }

        // TODO: in case a CR should be opened here, remove previous CR document data if exist
        // See OnDockableClosed
        try
        {
            fileViewModel = OpenFileViewModel(pathname, fileType);
            if (fileViewModel is not null)
            {
                AddFileViewModel(fileViewModel);
                UpdateRecentFiles(pathname);
                if (fileType.IsReportType())
                {
                    OnReportDocumentLoaded((fileViewModel.Document as CRDocument)!);
                }
                return true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }

    private void OpenOrMergeFiles(IEnumerable<IStorageItem> storageItems, bool mergeReportsRequested)
    {
        FileViewModel? lastTxtFileViewModel = null;
        FileViewModel? lastOrdersFileViewModel = null;
        FileViewModel? lastReportFileViewModel = null;
        List<DocumentType> reportDocTypes = [];
        List<String> reportPathnames = [];
        List<String> ordersPathnames = [];
        List<String> txtPathnames = [];
        bool mergeReports = mergeReportsRequested;

        // store pathnames
        foreach (IStorageItem file in storageItems.Where(IsFile))
        {
            string pathname = file.Path.LocalPath;
            if (!IsSupportedFileType(pathname, out DocumentType documentType))
            {
                Debug.WriteLine($"[MAINWINDOW] WARNING | OpenOrMergeFiles - '{pathname}' is not a supported file type to be opened in Odyssey");
                continue;
            }
            switch (documentType)
            {
                case DocumentType.ERESSEA_REPORT:
                case DocumentType.ERESSEA_REPORT_FROM_ZIP:
                    reportPathnames.Add(pathname);
                    reportDocTypes.Add(documentType);
                    break;
                case DocumentType.ERESSEA_ORDERS:
                    ordersPathnames.Add(pathname);
                    break;
                case DocumentType.TXT:
                    txtPathnames.Add(pathname);
                    break;
                default:
                    Debug.WriteLine($"[MAINWINDOW] WARNING | OpenOrMergeFiles - Unable to handle file of type {documentType}!");
                    break;
            }
        }

        // LATER: sort ordersPathnames and reportPathnames according settings
        // TODO: handle any currently opened document if any (ask to save if modified, then close it)

        if (reportPathnames.Count > 1 && _singleReportMode)
        {
            if (!mergeReports)
            {
                // TODO: inform user with confirmation dialog that merge mode is forced
                // if confirmed => merge
                // if not confirmed => nothing more done
            }
            mergeReports = true;
        }

        FileViewModel? fileViewModel = null;
        int reportIndex = 0;
        // Load report .cr or .zip file(s) if any
        foreach (String pathname in reportPathnames)
        {
            if (OpenDocument(pathname, reportDocTypes[reportIndex++], out fileViewModel))
            {
                if (mergeReports)
                {
                    //TODO: merge the loaded report with the current one
                }
                lastReportFileViewModel = fileViewModel;
            }
        }

        // Load orders .txt file(s) if any
        foreach (String pathname in ordersPathnames)
        {
            if (OpenDocument(pathname, DocumentType.ERESSEA_ORDERS, out fileViewModel))
            {
                lastOrdersFileViewModel = fileViewModel;
            }
        }

        // Load other .txt file(s) if any
        foreach (String pathname in txtPathnames)
        {
            if (OpenDocument(pathname, DocumentType.TXT, out fileViewModel))
            {
                lastTxtFileViewModel = fileViewModel;
            }
        }

        // LATER: notify map change if an orders file has been loaded ?
        //FileViewModel? lastFileViewModel = lastReportFileViewModel ?? lastOrdersFileViewModel;
        // Note: OpenDocument does OnReportDocumentLoaded, so no need here
        //if (lastReportFileViewModel is not null)
        //{
        //    OnReportDocumentLoaded((lastReportFileViewModel.Document as CRDocument)!);
        //}
    }

    private void OpenFiles(IEnumerable<IStorageItem> storageItems)
    {
        OpenOrMergeFiles(storageItems, false);
    }

    private void MergeFiles(IEnumerable<IStorageItem> storageItems)
    {
        FileViewModel? resultReportFileViewModel = null;
        foreach (IStorageItem file in storageItems.Where(IsFile))
        {
            try
            {
                string pathname = file.Path.LocalPath;
                if (!IsSupportedFileType(pathname, out DocumentType fileType))
                {
                    Debug.WriteLine($"[MAINWINDOW] WARNING | '{pathname}' is not an Eressea file");
                    continue;
                }
                EresseaDocument eresseaDocument = null;
                EresseaDocument currentDocument;
                if (LoadReportFile(pathname, fileType, out currentDocument))
                {
                    // TODO: merge the loaded report with the current one
                    /*
                    lastFileViewModel = fileViewModel;
                    if (fileType == DocumentType.REPORT || fileType == DocumentType.REPORT_FROM_ZIP)
                    {
                        lastReportFileViewModel = lastFileViewModel;
                    }
                    AddFileViewModel(lastFileViewModel);
                    */
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        if (resultReportFileViewModel != null)
        {
            OnReportDocumentLoaded((resultReportFileViewModel.Document as CRDocument)!);
        }
    }

    private void InitLayout()
    {
        if (Layout is null)
        {
            return;
        }

        _factory?.InitLayout(Layout);
    }

    private async Task OpenLayout()
    {
        if (_serializer is null || _dockState is null)
        {
            return;
        }

        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            // TODO: translate title
            Title = "Open layout",
            FileTypeFilter = GetOpenLayoutFileTypes(),
            AllowMultiple = false
        });

        var file = result.FirstOrDefault();

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var layout = _serializer.Load<IRootDock?>(stream);
                if (layout is not null)
                {
                    _dockState.Restore(layout);
                     Layout = layout;
                    _factory?.InitLayout(Layout);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private async Task SaveLayout()
    {
        if (_serializer is null || _dockState is null || Layout is null)
        {
            return;
        }

        // Close all docked documents before serializing the layout state.
        // This prevents DockSerializer from persisting domain object references
        // (DataBlock, Bookmarks, Explorer selections, ...) that are lost on restore.
        var filesDock = _factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId);

        List<FileViewModel> filesToClose = [];
        if (filesDock?.VisibleDockables != null)
        {
            foreach (var dockable in filesDock.VisibleDockables)
            {
                if (dockable is FileViewModel fvm && fvm.Document is not null)
                {
                    filesToClose.Add(fvm);
                }
            }
        }

        foreach (var fvm in filesToClose)
        {
            _factory?.RemoveDockable(fvm, false);
        }

        // Save the layout to the user-selected file path.
        Debug.WriteLine($"[MAINWINDOW] SaveLayout: closing {filesToClose.Count} documents");

        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save layout",
            FileTypeChoices = GetSaveLayoutFileTypes(),
            SuggestedFileName = "layout",
            DefaultExtension = "json",
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                _serializer.Save(stream, Layout);
                Debug.WriteLine("[MAINWINDOW] SaveLayout succeeded to " + file.Path.LocalPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        Debug.WriteLine("[MAINWINDOW] SaveLayout ended.");
    }

    /*
    private CRDocument GetActiveReportDocument()
    {
        FileViewModel? fileViewModel = GetActiveFileViewModel();
        if (fileViewModel is { })
        {
            return fileViewModel.Document;
        }
        return new CRDocument();
    }
    */

    private int OnReportChange(CRDocument reportDocument)
    {
        /*
        if (pstate.FileChange != Selection.FileChange) {
            //searchResults.setActiveFactionId(report ? report.getActiveFactionId() : 0);
        }
        */
        CRDocument report = reportDocument;
        if (!report.HasData())
        {
            return 0;
        }

        //getApp().beginWaitCursor();

        //updateFileNames();

        //showProperties(unitProperties, pstate.selected & Mask.UNIT);
        //showProperties(shipProperties, pstate.selected & Mask.SHIP);
        //showProperties(buildingProperties, pstate.selected & Mask.BUILDING);

        /*
        int factionId = 0;
        if (report) {
            int turn = report.turn();
            status_turn.setText(FXStringVal(turn));
            status_turn.show();
            status_lturn.show();
            FXString date = gameDate(turn);
            status_date.setText(date);
            status_date.show();
            status_ldate.show();
            factionId = report.getFactionId();
        }
        else {
            status_turn.hide();
            status_lturn.hide();
            status_file.hide();
            status_lfile.hide();
            status_date.hide();
            status_ldate.hide();
            status.recalc();
        }

        // update faction name in status bar
        if (factionId > 0)
        {
            // update statusbar
            FXString faction;
            faction.format("%s (%s)", report.activefaction().Value(TYPE_FACTIONNAME).text(),
                FXStringValEx(factionId, 36).text());

            status_faction.setText(faction);
            status_faction.show();
            status_lfaction.show();
        }
        else
        {
            status_faction.hide();
            status_lfaction.hide();
            status.recalc();
        }
        */

        return 1;
    }

    private void CheckOrders()
    {
        // Actions to be done :
        // - get echeckw.exe folder (from registry/settings/environment variable...?) => see CSMap::create()

    }

    private static bool IsOrdersFile(string pathname)
    {
        // TODO: check content to distnguish between orders and other text files
        return true;
    }

    /// <summary>
    /// Returns true if the file is an Eressea file (report, orders, or other text file with .txt extension).
    /// An Eressea file is identified by its extension (.cr, .txt or .zip are valid).
    /// </summary>
    /// <param name="pathname">pathname to be checked</param>
    /// <param name="fileType">document type according to the file extension</param>
    /// <returns>true if file extension is a supported file extension; otherwise, false</returns>
    private static bool IsSupportedFileType(string pathname, out DocumentType fileType)
    {
        fileType = DocumentType.UNKNOWN;
        switch (FileUtils.GetFileExtensionWithoutDot(pathname))
        {
            case StorageService.CrExt:
                fileType = DocumentType.ERESSEA_REPORT;
                break;
            case StorageService.ZipExt:
                fileType = DocumentType.ERESSEA_REPORT_FROM_ZIP;
                break;
            case StorageService.TxtExt:
                fileType = IsOrdersFile(pathname) ? DocumentType.ERESSEA_ORDERS : DocumentType.TXT;
                break;
            default: return false;
        }
        return true;
    }

    private static bool IsFile(IStorageItem file)
    {
        return file != null && file.Path.IsFile && !string.IsNullOrEmpty(file.Path.LocalPath);
    }

    private static bool LoadReportFile(string pathname, DocumentType fileType, out EresseaDocument eresseaDocument)
    {
        eresseaDocument = null;
        bool loaded = false;
        string errorMessage;
        switch (fileType)
        {
            case DocumentType.ERESSEA_REPORT:
                loaded = ReportFileService.LoadFile(pathname, out eresseaDocument, out errorMessage);
                break;
            case DocumentType.ERESSEA_REPORT_FROM_ZIP:
                if (ZipFileService.LoadFile(pathname, out eresseaDocument, out errorMessage))
                {
                    // CR file content has been extracted from the zip file
                    //pathname = Path.ChangeExtension(pathname, StorageService.CrExt);
                    //fileType = DocumentType.REPORT;
                    loaded = true;
                }
                break;
            case DocumentType.ERESSEA_ORDERS:
                loaded = OrdersFileService.LoadFile(pathname, out eresseaDocument, out errorMessage);
                break;
            default:
                // TODO: handle other text files
                break;
        }

        if (loaded && eresseaDocument is not null && fileType != DocumentType.ERESSEA_ORDERS)
        {
            // set the current game language according to the report locale,
            // e.g. by using a dedicated service for report metadata or by initializing ServiceLocator.LanguageService before loading the report document
            ServiceLocator.LanguageService.CurrentGameLanguage = eresseaDocument.Locale;
        }

        return loaded;
    }

    private FileViewModel? OpenFileViewModel(string pathname, DocumentType fileType)
    {
        EresseaDocument? eresseaDocument = null;
        bool loaded = LoadReportFile(pathname, fileType, out eresseaDocument);
        var encoding = GetEncoding(pathname);
        FileViewModel? fileViewModel = null;
        if (loaded)
        {
            string title = System.IO.Path.GetFileNameWithoutExtension(pathname);
            if (fileType == DocumentType.ERESSEA_REPORT_FROM_ZIP)
            {
                title += " [zip]";
            }
            // TODO: handle ordersDocument
            fileViewModel = new()
            {
                // DocumentDock properties
                Id = $"Document {title}",
                Title = title,
                // FileViewModel properties
                Text = eresseaDocument!.Text!,
                Document = eresseaDocument,
                Path = pathname,
                Encoding = encoding.WebName,
                DocumentType = fileType,
            };
        }
        return fileViewModel;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteIfReportExists))]
    private void NavigateToAdvancedSearch()
    {

        if (Layout is { } root)
        {
            root.Navigate.Execute(Ids.Search);
        }
    }

    /// <summary>
    /// Hide the specified view if it's visible, or restore it to its original location if it's hidden.
    /// </summary>
    /// <remarks>This method retrieves the dockable view associated with the provided <paramref
    /// name="viewModelId"/>  and sets it as the active and focused dockable view within the current layout. If the view
    /// cannot be found or the factory is not initialized, no action is performed.</remarks>
    /// <param name="viewModelId">The unique identifier of the view model to activate. Cannot be null or empty.</param>
    [RelayCommand]
    private void ToggleView(string viewModelId)
    {
        // See https://github.com/wieslawsoltes/Dock/blob/master/docs/dock-restore-dockable.md
        if (_factory is { })
        {
            IDockable? viewModel = _factory.GetDockable<IDockable>(viewModelId);
            if (viewModel is not null)
            {
                IRootDock? rootDock = _factory.FindRoot(viewModel, _ => true);
                IList<IDockable>? hiddenDockables = rootDock?.HiddenDockables;
                bool isVisible = hiddenDockables is null ? true : !hiddenDockables.Contains(viewModel);

                if (isVisible)
                {
                    _factory.HideDockable(viewModelId);
                }
                else
                {
                    _factory.RestoreDockable(viewModelId);
                    _factory.SetActiveDockable(viewModel);
                    _factory.SetFocusedDockable(Layout!, viewModel);
                }
            }
        }
    }

    [RelayCommand]
    private void ShowHelp(string? topicId = null)
    {
        // TODO
        //_ = HelpSystem.Instance.ShowHelp(topicId);
    }

    /// <summary>
    /// Execute the Exit command, which quits from the application.
    /// </summary>
    [RelayCommand]
    private void Exit()
    {
        // TODO:
        // - Handle modified documents
        // - Auto-save layout configuration
        // - Auto-save settings
        // - Auto-save opened documents list (recent document handling...)
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        await OpenReportOrOdersFile();
    }

    [RelayCommand]
    private async Task MergeReportFiles()
    {
        // TODO: handled currently opened document(s) if any (e.g. if some are modified)
        await LoadThenMergeReportFiles();
    }

    [RelayCommand(CanExecute = nameof(CanSaveFile))]
    private void SaveFile()
    {
        FileViewModel? fileViewModel = null;
        bool isModified = false;
        if (GetActiveFileViewModel(out fileViewModel, out isModified) && isModified)
        {
            SaveFileViewModel(fileViewModel!);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveFileAs))]
    private async Task SaveFileAs()
    {
        if (GetActiveFileViewModel(out FileViewModel? fileViewModel, out _))
        {
            await FileSaveAsImpl(fileViewModel!);
        }
    }

    private async Task FileSaveAsImpl(FileViewModel fileViewModel)
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        bool isOrdersFile = fileViewModel.DocumentType == DocumentType.ERESSEA_ORDERS;
        string defaultExtension = isOrdersFile ? StorageService.TxtExt : StorageService.CrExt;
        string pathname = Path.ChangeExtension(fileViewModel.Path, defaultExtension);
        FilePickerFileType fileTypeChoice = isOrdersFile ? StorageService.Orders : StorageService.Report;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            // TODO: translate title
            Title = "Save As",
            FileTypeChoices = [fileTypeChoice],
            SuggestedFileName = pathname,
            DefaultExtension = defaultExtension,
            ShowOverwritePrompt = true
        });

        if (file is not null)
        {
            UpdateFileViewModel(fileViewModel, file.Path.LocalPath);
            SaveFileViewModel(fileViewModel);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveOrders))]
    private void SaveOrders()
    {
        // TODO
    }

    [RelayCommand(CanExecute = nameof(CanCloseFile))]
    private void CloseFile()
    {
        FileViewModel? fileViewModel = null;
        bool isModified = false;
        //IDocument? currentDocument = _factory.GetCurrentDocument();
        if (GetActiveFileViewModel(out fileViewModel, out isModified))
        {
            if (fileViewModel!.IsDocumentModified())
            {
                // TODO: modal dialog box with save/do not save/cancel
            }
            _factory!.CloseDockable(fileViewModel);
        }
    }

    /// <summary>
    /// Indicates if SaveFile command can be executed.
    /// </summary>
    /// <returns>true if a document is opened and has been modified; otherwise false.</returns>
    private bool CanSaveFile()
    {
        EresseaDocument? d = GetActiveDocument(out DocumentType docFileType);
        return d != null && docFileType != DocumentType.ERESSEA_REPORT_FROM_ZIP && d.IsModified;
    }

    /// <summary>
    /// Indicates if SaveOrders command can be executed.
    /// </summary>
    /// <returns>true if a report document is opened; otherwise false.</returns>
    private bool CanSaveOrders()
    {
        DocumentType docFileType;
        EresseaDocument? d = GetActiveDocument(out docFileType);
        return d != null && docFileType.IsReportType();
    }

    /// <summary>
    /// Indicates if SaveFileAs command can be executed.
    /// </summary>
    /// <returns>true if a document is opened; otherwise false.</returns>
    private bool CanSaveFileAs()
    {
        // It's possible to save as any opened document. The currently active one will be saved as.
        return HasActiveDocument();
    }

    /// <summary>
    /// Indicates if CloseFile command can be executed.
    /// </summary>
    /// <returns>true if a document is opened; otherwise false.</returns>
    private bool CanCloseFile()
    {
        // It's possible to close any opened document
        return HasActiveDocument();
    }

    /// <summary>
    /// Indicates if a document is opened.
    /// </summary>
    /// <returns>true if a document is opened; otherwise false.</returns>
    private bool HasActiveDocument()
    {
        return GetActiveFileViewModel(out _, out _);
    }

    /// <summary>
    /// Toggles the application's fullscreen mode status.
    /// </summary>
    /// <remarks>This method switches the value of the <see cref="IsFullscreen"/> property between <see
    /// langword="true"/> and <see langword="false"/>. When <see cref="IsFullscreen"/> is  <see langword="true"/>, the
    /// application is in fullscreen mode; otherwise, it is not.</remarks>
    [RelayCommand]
    private void ToggleFullscreen()
    {
        IsFullscreen = !IsFullscreen;
    }

    /*
    private bool SelectionIsBookmarkable()
    {
        // TODO: true if the selection is an island, region or a unit, ship building in a region, or a unit spell
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanSaveBookmarks))]
    private void SaveBookmarks()
    {
        // TODO: Implement logic to "save as..." existing bookmarks
    }

    private bool CanSaveBookmarks()
    {
        // Enable only if there are some existing bookmark
        // TODO
        return HasActiveDocument(); //&& HasBoookmarks()
    }
    */

    private static void UpdateFileViewModel(FileViewModel fileViewModel, string path)
    {
        fileViewModel.Path = path;
        fileViewModel.Title = Path.GetFileNameWithoutExtension(path);
    }

    private void OnDockableClosed(object? sender, DockableClosedEventArgs args)
    {
        if (args.Dockable is FileViewModel fileViewModel && fileViewModel.DocumentType.IsReportType())
        {
            _activeDocumentService.ClearDocument();
        }
    }

    private void CollectGlobalReportInformation(CRDocument report)
    {
        HasDocument = report.HasData();
        FactionName = report.GetActiveFactionName();
        ReportName = HasDocument ? report.Name : string.Empty;

        GameTurn = HasDocument ? $"[{report.Turn}]" : string.Empty;
        GameDate = HasDocument ? DateUtils.GetGameDateLabel(report.Turn) : string.Empty;
        GameName = HasDocument ? report.GetGameName() : string.Empty;
    }

    private static void DebugFactoryEvents(IFactory factory)
    {
        factory.ActiveDockableChanged += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] ActiveDockableChanged Title='{args.Dockable?.Title}'");
        };

        factory.FocusedDockableChanged += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] FocusedDockableChanged Title='{args.Dockable?.Title}'");
        };

        factory.DockableAdded += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] DockableAdded Title='{args.Dockable?.Title}'");
        };

        factory.DockableRemoved += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] DockableRemoved Title='{args.Dockable?.Title}'");
        };

        factory.DockableClosed += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] DockableClosed Title='{args.Dockable?.Title}'");
        };

        factory.DockableMoved += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] DockableMoved Title='{args.Dockable?.Title}'");
        };

        factory.DockableSwapped += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] DockableSwapped Title='{args.Dockable?.Title}'");
        };

        factory.DockablePinned += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] DockablePinned Title='{args.Dockable?.Title}'");
        };

        factory.DockableUnpinned += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] DockableUnpinned Title='{args.Dockable?.Title}'");
        };

        factory.WindowOpened += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] WindowOpened Title='{args.Window?.Title}'");
        };

        factory.WindowClosed += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] WindowClosed Title='{args.Window?.Title}'");
        };

        factory.WindowClosing += (_, args) =>
        {
            // NOTE: Set to True to cancel window closing.
#if false
                args.Cancel = true;
#endif
            Debug.WriteLine($"[MAINWINDOW] WindowClosing Title='{args.Window?.Title}', Cancel={args.Cancel}");
        };

        factory.WindowAdded += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] WindowAdded Title='{args.Window?.Title}'");
        };

        factory.WindowRemoved += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] WindowRemoved Title='{args.Window?.Title}'");
        };

        factory.WindowMoveDragBegin += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] WindowMoveDragBegin Title='{args.Window?.Title}', Cancel={args.Cancel}, X='{args.Window?.X}', Y='{args.Window?.Y}'");
        };

        factory.WindowMoveDrag += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] WindowMoveDrag Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
        };

        factory.WindowMoveDragEnd += (_, args) =>
        {
            Debug.WriteLine($"[MAINWINDOW] WindowMoveDragEnd Title='{args.Window?.Title}', X='{args.Window?.X}', Y='{args.Window?.Y}");
        };
    }

    private static string FormatGlobalStatus(GlobalDockTrackingState state)
    {
        var dockableTitle = state.Dockable?.Title ?? "(none)";
        var rootId = state.RootDock?.Id ?? "(none)";
        var windowTitle = state.Window?.Title ?? "(main)";
        var host = state.HostWindow?.GetType().Name ?? "(main)";
        return $"Dockable: {dockableTitle} | Root: {rootId} | Window: {windowTitle} | Host: {host}";
    }
}
