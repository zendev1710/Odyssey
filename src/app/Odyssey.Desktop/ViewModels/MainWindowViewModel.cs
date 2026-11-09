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
using Odyssey.Assets;
using Odyssey.Extensions;
using Odyssey.Messages;
using Odyssey.Models.Dal;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Search;
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Odyssey.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDropTarget
{
    public Func<FileViewModel, Task<SaveChangesChoice>>? SaveChangesPrompt { get; set; }
    public Func<FileViewModel, Task>? SaveFailurePrompt { get; set; }

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
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _searchMatchCase = true;

    [ObservableProperty]
    private bool _searchWholeWords;

    [ObservableProperty]
    private bool _searchUseRegex;

    [ObservableProperty]
    private bool _hasDocument;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnconfirmedOrdersTooltip))]
    private int _unconfirmedOrdersCount;

    public string UnconfirmedOrdersTooltip =>
        string.Format(Lang.statusbar_orders_left_tip, UnconfirmedOrdersCount);

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
    private List<SavedDocumentState>? _documentsToRestore;
    private string? _mapPlaneSelectionsToRestore;

    private const int SavedLayoutVersion = 1;
    private const string AutoSessionFileName = "last-session.json";
    private const string LegacyAutoSessionFileName = "layout.json";
    private static readonly StringComparer DocumentPathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private static readonly JsonSerializerOptions SavedLayoutJsonOptions = new() { WriteIndented = true };

    private sealed class SavedSessionState
    {
        public SavedSessionState()
        {
        }

        public int Version { get; set; } = SavedLayoutVersion;
        public string LayoutJson { get; set; } = string.Empty;
        public string? CurrentLayoutProfilePath { get; set; }
        public string SelectionBookmarks { get; set; } = string.Empty;
        public string MapPlaneSelections { get; set; } = string.Empty;
        public List<SavedDocumentState>? Documents { get; set; } = [];
    }

    private sealed class SavedDocumentState
    {
        public SavedDocumentState()
        {
        }

        public string Path { get; set; } = string.Empty;
        public DocumentType DocumentType { get; set; }
        public bool IsActive { get; set; }
        public string? Text { get; set; }
        public string? Encoding { get; set; }
        public bool IsModified { get; set; }
        public CRDocument.EditedOrderChanges? EditedOrderChanges { get; set; }
    }

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

    private readonly DockFactory _factory;
    private readonly IDocumentService _activeDocumentService;
    private readonly GlobalSearchEngine _globalSearchEngine = new();
    private const int GlobalSearchMinimumQueryLength = 3;
    private SearchResultsViewModel? _searchResultsViewModel;
    private CancellationTokenSource? _globalSearchCancellation;
    private bool _isGlobalSearchPending;
    private bool _searchResultsAreCurrent;
    private CRDocument? _modifiedDocument;

    private readonly IEntitySelectionService _selectionService;
    private IRootDock? _layout;
    private string _globalStatus = "Global: (none)";

    private readonly bool _singleReportMode = true;

    private readonly bool _readonlyMode = true;

    // Last route mode that was not "all", so the "show all routes" toggle can restore it.
    // Default to the selected region: unchecking "all routes" must still show the routes
    // related to the current selection instead of hiding every route.
    private int _lastNonAllShipRouteMode = ShipRouteModes.SelectedRegion;

    partial void OnSearchQueryChanged(string value) => ScheduleGlobalSearch();

    partial void OnSearchMatchCaseChanged(bool value) => ScheduleGlobalSearch();

    partial void OnSearchWholeWordsChanged(bool value) => ScheduleGlobalSearch();

    partial void OnSearchUseRegexChanged(bool value) => ScheduleGlobalSearch();

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

    public string CurrentLayoutProfileLabel
    {
        get
        {
            string profilePath = GlobalSettings.Shared.CurrentLayoutProfilePath;
            string profileName = string.IsNullOrWhiteSpace(profilePath)
                ? Lang.mainmenu_window_layout_default
                : Path.GetFileNameWithoutExtension(profilePath);
            return string.Format(Lang.mainmenu_window_layout_current, profileName);
        }
    }

    public ICommand LayoutSaveAs { get; }

    public ICommand LayoutOpen { get; }

    public ICommand LayoutSave { get; }

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

        // TODO: check when it should be called exactly
        InitializeDockState();

        if (Layout is null)
        {
            Layout = _factory?.CreateLayout();
            InitLayout();
            if (Layout is not null)
            {
                _dockState?.Save(Layout);
            }
        }

        if (!Design.IsDesignMode)
        {
            bool restoredSession = GlobalSettings.Get<bool>(GlobalSettings.AUTO_SAVE_SESSION)
                && TryRestoreAutoSession();
            if (!restoredSession)
            {
                TryRestoreCurrentLayoutProfile();
            }
        }

        GlobalStatus = Layout is null
            ? "Global: (none)"
            : FormatGlobalStatus(_factory?.GlobalDockTrackingState ?? GlobalDockTrackingState.Empty);

        if (Layout is { } root)
        {
            // Display the workspace view (main view), which embeds all the layout docked windows
            root.Navigate.Execute(Ids.Workspace);
        }

        _bookmarksViewModel = _factory.GetDockable<IDockable>(Ids.Bookmarks) as BookmarksViewModel;
        _searchResultsViewModel = _factory.GetDockable<IDockable>(Ids.SearchResults) as SearchResultsViewModel;
        if (_searchResultsViewModel is not null)
        {
            _searchResultsViewModel.PropertyChanged += SearchResultsViewModel_PropertyChanged;
        }

        LayoutOpen = new AsyncRelayCommand(OpenLayout);
        LayoutSave = new AsyncRelayCommand(SaveLayout);
        LayoutSaveAs = new AsyncRelayCommand(SaveLayoutAs);

        _factory.ActiveDockableChanged += (_, _) =>
        {
            SaveFileCommand.NotifyCanExecuteChanged();
            SaveFileAsCommand.NotifyCanExecuteChanged();
            CloseFileCommand.NotifyCanExecuteChanged();
        };

        FindSetFocus = new RelayCommand(ActivateFindArea);

        UpdateViewVisibilityFromLayout();

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
        _serializer = new LayoutDockSerializer();
        _dockState = new DockState();
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
            e.SettingName != GlobalSettings.MAP_REGION_NAME_BANNER &&
            e.SettingName != nameof(GlobalSettings.CurrentLayoutProfilePath))
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
        else if (e.SettingName == nameof(GlobalSettings.CurrentLayoutProfilePath))
        {
            OnPropertyChanged(nameof(CurrentLayoutProfileLabel));
        }
    }

    private bool CanExecuteIfReportExists()
    {
        return HasDocument;
    }

    private bool CanNavigateSearchResults()
    {
        return !_isGlobalSearchPending && _searchResultsAreCurrent && (_searchResultsViewModel?.FilteredResults.Count ?? 0) > 0;
    }

    private void SearchResultsViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchResultsViewModel.FilteredResults))
        {
            NotifySearchNavigationCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanNavigateSearchResults))]
    private void FindPreviousSearchResult()
    {
        _searchResultsViewModel?.MoveSelection(-1);
    }

    [RelayCommand(CanExecute = nameof(CanNavigateSearchResults))]
    private void FindNextSearchResult()
    {
        _searchResultsViewModel?.MoveSelection(1);
    }

    [RelayCommand]
    private async Task SelectCurrentSearchResult()
    {
        CRDocument report = _activeDocumentService.ActiveDocument;
        if (!report.HasData() || string.IsNullOrWhiteSpace(SearchQuery))
        {
            return;
        }

        if (!_searchResultsAreCurrent)
        {
            _globalSearchCancellation?.Cancel();
            _globalSearchCancellation = null;
            CancellationTokenSource cancellation = new();
            _globalSearchCancellation = cancellation;
            _isGlobalSearchPending = true;
            NotifySearchNavigationCanExecuteChanged();

            bool searchCompleted = await ExecuteGlobalSearchAsync(
                cancellation,
                report,
                SearchQuery,
                SearchMatchCase,
                SearchWholeWords,
                SearchUseRegex,
                waitForDebounce: false);
            if (!searchCompleted)
            {
                return;
            }
        }

        _searchResultsViewModel?.SelectFirstResultAndApply();
    }

    private void ScheduleGlobalSearch()
    {
        _globalSearchCancellation?.Cancel();
        _globalSearchCancellation = null;
        _searchResultsAreCurrent = false;

        CRDocument report = _activeDocumentService.ActiveDocument;
        if (string.IsNullOrWhiteSpace(SearchQuery) || !report.HasData())
        {
            _isGlobalSearchPending = false;
            _searchResultsViewModel?.ReplaceResults([], string.Empty);
            NotifySearchNavigationCanExecuteChanged();
            return;
        }

        if (SearchQuery.Trim().Length < GlobalSearchMinimumQueryLength)
        {
            _isGlobalSearchPending = false;
            _searchResultsViewModel?.ReplaceResults([], Lang.find_minimum_characters);
            NotifySearchNavigationCanExecuteChanged();
            return;
        }

        _isGlobalSearchPending = true;
        _searchResultsViewModel?.ReplaceResults([], Lang.find_searching);
        CancellationTokenSource cancellation = new();
        _globalSearchCancellation = cancellation;
        NotifySearchNavigationCanExecuteChanged();
        _ = ExecuteGlobalSearchAsync(
            cancellation,
            report,
            SearchQuery,
            SearchMatchCase,
            SearchWholeWords,
            SearchUseRegex,
            waitForDebounce: true);
    }

    private async Task<bool> ExecuteGlobalSearchAsync(
        CancellationTokenSource cancellation,
        CRDocument report,
        string query,
        bool matchCase,
        bool wholeWords,
        bool useRegex,
        bool waitForDebounce)
    {
        CancellationToken cancellationToken = cancellation.Token;
        try
        {
            if (waitForDebounce)
            {
                await Task.Delay(180);
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
            }

            GlobalSearchResponse response = await Task.Run(
                () => _globalSearchEngine.Search(report, query, matchCase, wholeWords, useRegex, cancellationToken));

            if (response.Error == GlobalSearchError.Cancelled ||
                cancellationToken.IsCancellationRequested ||
                !ReferenceEquals(_activeDocumentService.ActiveDocument, report))
            {
                return false;
            }

            string status = response.Error switch
            {
                GlobalSearchError.InvalidPattern => Lang.find_invalid_regex,
                GlobalSearchError.TimedOut => Lang.find_regex_timeout,
                _ => string.Format(Lang.find_result_count, response.Results.Count)
            };
            _searchResultsViewModel?.ReplaceResults(response.Results, status);
            if (waitForDebounce && response.Results.Count == 1 && _searchResultsViewModel?.FilteredResults.Count == 1)
            {
                _searchResultsViewModel?.ApplySelectedResult();
            }

            _searchResultsAreCurrent = true;
            _isGlobalSearchPending = false;
            if (!IsSearchResultsViewVisible)
            {
                ToggleView(Ids.SearchResults);
            }
            NotifySearchNavigationCanExecuteChanged();
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        finally
        {
            if (ReferenceEquals(_globalSearchCancellation, cancellation))
            {
                _globalSearchCancellation = null;
                _isGlobalSearchPending = false;
                NotifySearchNavigationCanExecuteChanged();
            }
            cancellation.Dispose();
        }
    }

    private void NotifySearchNavigationCanExecuteChanged()
    {
        FindPreviousSearchResultCommand.NotifyCanExecuteChanged();
        FindNextSearchResultCommand.NotifyCanExecuteChanged();
    }

    public void OpenInitialDocumentIfNeeded()
    {
        RestorePendingDocuments();

        if (!string.IsNullOrEmpty(InitialDocumentPathname))
        {
            if (File.Exists(InitialDocumentPathname))
            {
                if (!OpenDocument(InitialDocumentPathname) && FindFileViewModelByPath(InitialDocumentPathname) is { } existingFile)
                {
                    _factory?.SetActiveDockable(existingFile);
                }
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

    private void UpdateViewVisibilityFromLayout()
    {
        foreach ((string id, Action<MainWindowViewModel, bool> setter) in _viewVisibilitySetters)
        {
            IDockable? dockable = _factory.GetDockable<IDockable>(id);
            IRootDock? rootDock = dockable is null ? null : _factory.FindRoot(dockable, _ => true);
            bool isHidden = rootDock?.HiddenDockables?.Any(hiddenDockable =>
                string.Equals(hiddenDockable.Id, id, StringComparison.Ordinal)) == true;
            setter(this, !isHidden);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
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

    private static bool SaveFileViewModel(FileViewModel fileViewModel)
    {
        DocumentType docType = fileViewModel.DocumentType;
        string path = fileViewModel.Path;
        CRDocument? doc = fileViewModel.Document as CRDocument;
        if (doc is null)
        {
            return false;
        }

        bool saved;
        switch (docType)
        {
            case DocumentType.ERESSEA_REPORT:
                saved = ReportFileService.SaveWithOrderEdits(path, doc, MapType.FULL, []);
                if (saved)
                {
                    doc.CommitEditedOrderChanges();
                    doc.SetModified(false);
                    fileViewModel.IsModified = false;
                }
                break;
            case DocumentType.ERESSEA_ORDERS:
                //OrdersFileService.Save(path, doc, MapType.FULL, []);
                saved = false;
                break;
            case DocumentType.TXT:
                //TxtFileService.Save(path, doc, MapType.FULL, []);
                saved = false;
                break;
            default:
                Debug.WriteLine($"[MAINWINDOW] WARNING | SaveFileViewModel - Unable to save file of type {fileViewModel.DocumentType}!");
                saved = false;
                break;
        }
        return saved;
    }

    public async Task<bool> ConfirmCloseAllDocumentsAsync()
    {
        IDocumentDock filesDock = _factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId)
            ?? throw new InvalidOperationException("The document dock is not available.");
        List<FileViewModel> files = filesDock.VisibleDockables?.OfType<FileViewModel>().ToList() ?? [];

        foreach (FileViewModel file in files)
        {
            if (file.IsDocumentModified() && !await ConfirmCloseFileAsync(file))
            {
                return false;
            }
        }

        foreach (FileViewModel file in files)
        {
            file.AllowCloseOnce();
        }

        return true;
    }

    private async Task<bool> ConfirmCloseFileAsync(FileViewModel file)
    {
        if (!file.IsDocumentModified())
        {
            return true;
        }

        SaveChangesChoice choice = SaveChangesPrompt is null
            ? SaveChangesChoice.Cancel
            : await SaveChangesPrompt(file);

        switch (choice)
        {
            case SaveChangesChoice.Save:
                if (SaveFileViewModel(file))
                {
                    return true;
                }

                if (SaveFailurePrompt is not null)
                {
                    await SaveFailurePrompt(file);
                }
                return false;
            case SaveChangesChoice.Discard:
                file.DiscardChanges();
                return true;
            default:
                return false;
        }
    }

    private void CollectPlanes()
    {
        Planes.Clear();
        Planes.Add(new WorldPlane(PlaneType.WORLD));
    }

    private void OnActiveDocumentChanged(CRDocument report)
    {
        if (_modifiedDocument is not null)
        {
            _modifiedDocument.ModifiedChanged -= OnDocumentModifiedChanged;
            _modifiedDocument.OrderConfirmationChanged -= OnOrderConfirmationChanged;
            _modifiedDocument.EditedOrderDataChanged -= OnEditedOrderDataChanged;
        }
        _modifiedDocument = report;
        _modifiedDocument.ModifiedChanged += OnDocumentModifiedChanged;
        _modifiedDocument.OrderConfirmationChanged += OnOrderConfirmationChanged;
        _modifiedDocument.EditedOrderDataChanged += OnEditedOrderDataChanged;
        SaveFileCommand.NotifyCanExecuteChanged();
        SaveFileAsCommand.NotifyCanExecuteChanged();
        CloseFileCommand.NotifyCanExecuteChanged();
        OnActiveReportDocumentChanged(report);
        _globalSearchCancellation?.Cancel();
        _globalSearchEngine.ClearIndex();
        ScheduleGlobalSearch();
    }

    private void OnDocumentModifiedChanged(object? sender, EventArgs args)
    {
        SaveFileCommand.NotifyCanExecuteChanged();
    }

    private void OnEditedOrderDataChanged(object? sender, EventArgs args)
    {
        SaveFileCommand.NotifyCanExecuteChanged();
    }

    private void OnOrderConfirmationChanged(object? sender, EventArgs args)
    {
        if (sender is CRDocument report)
        {
            UnconfirmedOrdersCount = report.UnconfirmedOrdersCount;
        }
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
        return FindFileViewModelByPath(pathname) is not null;
    }

    private void AddFileViewModel(FileViewModel fileViewModel)
    {
        IRootDock layout = Layout ?? throw new InvalidOperationException("The layout is not available.");
        IDocumentDock files = _factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId)
            ?? throw new InvalidOperationException("The document dock is not available.");
        fileViewModel.ConfirmCloseAsync = ConfirmCloseFileAsync;
        fileViewModel.CloseApproved = file => _factory.CloseDockable(file);
        fileViewModel.PropertyChanged += OnFileViewModelPropertyChanged;
        _factory.AddDockable(files, fileViewModel);
        _factory.SetActiveDockable(fileViewModel);
        _factory.SetFocusedDockable(layout, fileViewModel);
        SaveFileAsCommand.NotifyCanExecuteChanged();
        CloseFileCommand.NotifyCanExecuteChanged();
    }

    private void OnFileViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileViewModel.IsModified))
        {
            SaveFileCommand.NotifyCanExecuteChanged();
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
        isModified = hasActive ? fileViewModel!.IsDocumentModified() : false;
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

        string layoutsDirectory = GetLayoutsDirectory();
        Directory.CreateDirectory(layoutsDirectory);
        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang.mainmenu_window_layout_open,
            FileTypeFilter = GetOpenLayoutFileTypes(),
            AllowMultiple = false,
            SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(layoutsDirectory)
        });

        var file = result.FirstOrDefault();

        if (file is not null)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                string json = await reader.ReadToEndAsync();
                SavedSessionState savedLayout = ParseLayoutState(json, []);
                savedLayout.SelectionBookmarks = string.Empty;
                RestoreLayoutState(savedLayout, preserveCurrentContext: true);
                GlobalSettings.Shared.CurrentLayoutProfilePath = Path.GetFullPath(file.Path.LocalPath);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[MAINWINDOW] ERROR | Failed to open layout: {e}");
                Console.Error.WriteLine(e);
            }
        }
    }

    private async Task SaveLayout()
    {
        if (_serializer is null || _dockState is null || Layout is null)
        {
            return;
        }

        string profilePath = GlobalSettings.Shared.CurrentLayoutProfilePath;
        if (string.IsNullOrWhiteSpace(profilePath))
        {
            await SaveLayoutAs();
            return;
        }

        await SaveLayoutToPath(profilePath);
    }

    private async Task SaveLayoutAs()
    {
        if (_serializer is null || _dockState is null || Layout is null)
        {
            return;
        }

        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        string layoutsDirectory = GetLayoutsDirectory();
        Directory.CreateDirectory(layoutsDirectory);
        string activePath = GlobalSettings.Shared.CurrentLayoutProfilePath;
        string suggestedName = string.IsNullOrWhiteSpace(activePath)
            ? "layout"
            : Path.GetFileNameWithoutExtension(activePath);
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Lang.mainmenu_window_layout_save_as,
            FileTypeChoices = GetSaveLayoutFileTypes(),
            SuggestedFileName = suggestedName,
            DefaultExtension = "json",
            ShowOverwritePrompt = true,
            SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(layoutsDirectory)
        });

        if (file is not null)
        {
            await SaveLayoutToPath(file.Path.LocalPath);
        }
    }

    private async Task SaveLayoutToPath(string profilePath)
    {
        if (_serializer is null || _dockState is null || Layout is null)
        {
            return;
        }

        try
        {
            string? directory = Path.GetDirectoryName(profilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(profilePath, SerializeLayoutProfile(), Encoding.UTF8);
            GlobalSettings.Shared.CurrentLayoutProfilePath = profilePath;
            Debug.WriteLine("[MAINWINDOW] Saved layout profile to " + profilePath);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"[MAINWINDOW] ERROR | Failed to save layout profile: {e}");
            Console.Error.WriteLine(e);
        }
    }

    private string SerializeLayoutProfile()
    {
        if (_serializer is null || Layout is null)
        {
            throw new InvalidOperationException("The Dock layout is not initialized.");
        }

        _dockState?.Reset();
        _dockState?.Save(Layout);
        return _serializer.Serialize(Layout);
    }

    public void SaveSessionOnApplicationClosing()
    {
        if (!GlobalSettings.Get<bool>(GlobalSettings.AUTO_SAVE_SESSION) || Layout is null)
        {
            return;
        }

        try
        {
            string path = GetAutoSessionPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, SerializeSessionState(), Encoding.UTF8);
            Debug.WriteLine($"[MAINWINDOW] Auto-saved session to '{path}'.");
        }
        catch (Exception e)
        {
            Debug.WriteLine($"[MAINWINDOW] ERROR | Failed to auto-save session: {e}");
            Console.Error.WriteLine(e);
        }
    }

    private string SerializeSessionState()
    {
        if (_serializer is null || Layout is null)
        {
            throw new InvalidOperationException("The Dock layout is not initialized.");
        }

        _dockState?.Reset();
        _dockState?.Save(Layout);
        return JsonSerializer.Serialize(new SavedSessionState
        {
            LayoutJson = _serializer.Serialize(Layout),
            CurrentLayoutProfilePath = GlobalSettings.Shared.CurrentLayoutProfilePath,
            SelectionBookmarks = GlobalSettings.Shared.ReportSelectionBookmarksJson,
            MapPlaneSelections = (_factory.GetDockable<IDockable>(Ids.Map) as MapViewModel)?.CaptureSessionSelectionState() ?? string.Empty,
            Documents = CaptureOpenDocuments()
        }, SavedLayoutJsonOptions);
    }

    private List<SavedDocumentState> CaptureOpenDocuments()
    {
        List<SavedDocumentState> documents = [];
        IDocumentDock? filesDock = _factory?.GetDockable<IDocumentDock>(DockFactory.DocumentDockId);
        if (filesDock?.VisibleDockables is not { } dockables)
        {
            return documents;
        }

        foreach (FileViewModel fileViewModel in dockables.OfType<FileViewModel>())
        {
            if (string.IsNullOrWhiteSpace(fileViewModel.Path))
            {
                continue;
            }

            documents.Add(new SavedDocumentState
            {
                Path = Path.GetFullPath(fileViewModel.Path),
                DocumentType = fileViewModel.DocumentType,
                IsActive = ReferenceEquals(filesDock.ActiveDockable, fileViewModel),
                Text = !fileViewModel.DocumentType.IsReportType() && fileViewModel.IsModified
                    ? fileViewModel.Text
                    : null,
                Encoding = fileViewModel.Encoding,
                IsModified = fileViewModel.IsModified,
                EditedOrderChanges = (fileViewModel.Document as CRDocument)?.CaptureEditedOrderChanges()
            });
        }

        return documents;
    }

    private SavedSessionState ParseLayoutState(string json, List<SavedDocumentState> fallbackDocuments)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.EnumerateObject().Any(property => string.Equals(property.Name, nameof(SavedSessionState.LayoutJson), StringComparison.OrdinalIgnoreCase)))
        {
            SavedSessionState? savedLayout = JsonSerializer.Deserialize<SavedSessionState>(json, SavedLayoutJsonOptions);
            if (savedLayout is null || savedLayout.Version != SavedLayoutVersion || string.IsNullOrWhiteSpace(savedLayout.LayoutJson))
            {
                throw new InvalidDataException("The saved layout file is invalid or uses an unsupported version.");
            }

            savedLayout.Documents ??= fallbackDocuments;
            return savedLayout;
        }

        return new SavedSessionState
        {
            LayoutJson = json,
            Documents = fallbackDocuments
        };
    }

    private bool TryRestoreAutoSession()
    {
        string path = GetAutoSessionPath();
        if (!File.Exists(path))
        {
            path = GetLegacyAutoSessionPath();
        }
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            SavedSessionState savedLayout = ParseLayoutState(File.ReadAllText(path, Encoding.UTF8), []);
            RestoreLayoutState(savedLayout);
            Debug.WriteLine($"[MAINWINDOW] Restored auto-saved session from '{path}'.");
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"[MAINWINDOW] ERROR | Failed to restore auto-saved session: {e}");
            Console.Error.WriteLine(e);
            return false;
        }
    }

    private bool TryRestoreCurrentLayoutProfile()
    {
        string profilePath = GlobalSettings.Shared.CurrentLayoutProfilePath;
        if (string.IsNullOrWhiteSpace(profilePath) || !File.Exists(profilePath))
        {
            return false;
        }

        try
        {
            SavedSessionState savedLayout = ParseLayoutState(File.ReadAllText(profilePath, Encoding.UTF8), []);
            savedLayout.Documents = [];
            savedLayout.SelectionBookmarks = string.Empty;
            RestoreLayoutState(savedLayout);
            GlobalSettings.Shared.CurrentLayoutProfilePath = Path.GetFullPath(profilePath);
            Debug.WriteLine($"[MAINWINDOW] Restored current layout profile from '{profilePath}'.");
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"[MAINWINDOW] ERROR | Failed to restore current layout profile '{profilePath}': {e}");
            Console.Error.WriteLine(e);
            return false;
        }
    }

    private void RestoreLayoutState(SavedSessionState savedLayout, bool preserveCurrentContext = false)
    {
        if (_serializer is null)
        {
            throw new InvalidOperationException("The Dock serializer is not initialized.");
        }

        List<SavedDocumentState> documentsToRestore;
        if (preserveCurrentContext)
        {
            documentsToRestore = [];
        }
        else if (savedLayout.Documents is { Count: > 0 } savedDocuments)
        {
            documentsToRestore = savedDocuments;
        }
        else
        {
            documentsToRestore = CaptureOpenDocuments();
        }

        IRootDock? layout = _serializer.Deserialize<IRootDock?>(savedLayout.LayoutJson);
        if (layout is null)
        {
            throw new InvalidDataException("The saved layout does not contain a Dock root.");
        }

        _dockState?.Reset();
        if (Layout is not null)
        {
            _dockState?.Save(Layout);
        }
        _dockState?.Restore(layout);

        Layout = layout;
        _factory?.InitLayout(layout);
        UpdateViewVisibilityFromLayout();
        if (!preserveCurrentContext)
        {
            RemoveDeserializedDocuments();
        }
        _dockState?.Save(layout);
        _documentsToRestore = preserveCurrentContext ? null : documentsToRestore;
        _mapPlaneSelectionsToRestore = preserveCurrentContext ? null : savedLayout.MapPlaneSelections;

        if (!preserveCurrentContext && !string.IsNullOrEmpty(savedLayout.SelectionBookmarks))
        {
            GlobalSettings.Shared.ReportSelectionBookmarksJson = savedLayout.SelectionBookmarks;
        }

        if (!preserveCurrentContext && savedLayout.CurrentLayoutProfilePath is not null)
        {
            GlobalSettings.Shared.CurrentLayoutProfilePath = savedLayout.CurrentLayoutProfilePath;
        }
    }

    private void RemoveDeserializedDocuments()
    {
        IDocumentDock? filesDock = _factory?.GetDockable<IDocumentDock>(DockFactory.DocumentDockId);
        if (filesDock?.VisibleDockables is not { } dockables)
        {
            return;
        }

        foreach (FileViewModel fileViewModel in dockables.OfType<FileViewModel>().ToList())
        {
            dockables.Remove(fileViewModel);
        }

        if (filesDock.ActiveDockable is FileViewModel)
        {
            filesDock.ActiveDockable = null;
        }
    }

    private void RestorePendingDocuments()
    {
        if (_documentsToRestore is not { } documents)
        {
            RestorePendingMapPlaneSelections();
            return;
        }

        _documentsToRestore = null;
        FileViewModel? activeFile = null;
        foreach (SavedDocumentState savedDocument in documents)
        {
            if (string.IsNullOrWhiteSpace(savedDocument.Path) || !File.Exists(savedDocument.Path))
            {
                Debug.WriteLine($"[MAINWINDOW] Skipping saved document that no longer exists: '{savedDocument.Path}'.");
                continue;
            }

            FileViewModel? fileViewModel = FindFileViewModelByPath(savedDocument.Path);
            if (fileViewModel is null && !OpenDocument(savedDocument.Path, savedDocument.DocumentType, out fileViewModel))
            {
                Debug.WriteLine($"[MAINWINDOW] WARNING | Could not restore document '{savedDocument.Path}'.");
                continue;
            }

            if (fileViewModel is null)
            {
                continue;
            }

            RestoreDocumentState(fileViewModel, savedDocument);
            if (savedDocument.IsActive)
            {
                activeFile = fileViewModel;
            }
        }

        if (activeFile is not null)
        {
            _factory?.SetActiveDockable(activeFile);
        }

        RestorePendingMapPlaneSelections();
    }

    private void RestorePendingMapPlaneSelections()
    {
        if (string.IsNullOrWhiteSpace(_mapPlaneSelectionsToRestore))
        {
            return;
        }

        string savedSelections = _mapPlaneSelectionsToRestore;
        _mapPlaneSelectionsToRestore = null;
        if (_factory.GetDockable<IDockable>(Ids.Map) is MapViewModel mapViewModel)
        {
            mapViewModel.RestoreSessionSelectionState(savedSelections);
        }
    }

    private static void RestoreDocumentState(FileViewModel fileViewModel, SavedDocumentState savedDocument)
    {
        if (savedDocument.Text is not null)
        {
            fileViewModel.Text = savedDocument.Text;
        }
        if (savedDocument.Encoding is not null)
        {
            fileViewModel.Encoding = savedDocument.Encoding;
        }
        fileViewModel.IsModified = savedDocument.IsModified;

        if (fileViewModel.Document is CRDocument report && savedDocument.EditedOrderChanges is not null)
        {
            report.RestoreEditedOrderChanges(savedDocument.EditedOrderChanges);
        }
    }

    private FileViewModel? FindFileViewModelByPath(string pathname)
    {
        string fullPath = Path.GetFullPath(pathname);
        IDocumentDock? filesDock = _factory?.GetDockable<IDocumentDock>(DockFactory.DocumentDockId);
        return filesDock?.VisibleDockables?.OfType<FileViewModel>()
            .FirstOrDefault(fileViewModel => !string.IsNullOrWhiteSpace(fileViewModel.Path)
                && DocumentPathComparer.Equals(Path.GetFullPath(fileViewModel.Path), fullPath));
    }

    private static string GetAutoSessionPath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "Odyssey", AutoSessionFileName);
    }

    private static string GetLegacyAutoSessionPath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "Odyssey", LegacyAutoSessionFileName);
    }

    private static string GetLayoutsDirectory()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "Odyssey", "Layouts");
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
            if (eresseaDocument is CRDocument reportDocument)
            {
                reportDocument.SourcePath = pathname;
            }

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
        if (_factory is { } factory && Layout is { } rootDock)
        {
            IDockable? viewModel = factory.GetDockable<IDockable>(viewModelId);
            if (viewModel is not null)
            {
                rootDock = factory.FindRoot(viewModel, _ => true) ?? rootDock;
                IList<IDockable>? hiddenDockables = rootDock.HiddenDockables;
                IDockable? hiddenDockable = hiddenDockables?.FirstOrDefault(dockable =>
                    string.Equals(dockable.Id, viewModelId, StringComparison.Ordinal));

                if (hiddenDockable is null)
                {
                    factory.HideDockable(viewModel);
                }
                else
                {
                    if (hiddenDockable.OriginalOwner is null)
                    {
                        List<IToolDock> emptyToolDocks = factory.Find(rootDock, dockable => dockable is IToolDock { IsEmpty: true })
                            .OfType<IToolDock>()
                            .ToList();
                        IToolDock? restoreOwner = emptyToolDocks
                            .OrderByDescending(dock => !double.IsNaN(dock.CollapsedProportion) && dock.CollapsedProportion > 0)
                            .FirstOrDefault()
                            ?? factory.Find(rootDock, dockable => dockable is IToolDock).OfType<IToolDock>().FirstOrDefault();

                        if (restoreOwner is null)
                        {
                            Debug.WriteLine($"[MAINWINDOW] Cannot restore hidden view '{viewModelId}': no tool dock is available.");
                            return;
                        }

                        hiddenDockable.OriginalOwner = restoreOwner;
                    }

                    // Restore the item from the active root; the ID overload searches DockControls.
                    factory.RestoreDockable(hiddenDockable);
                    if (hiddenDockable.Owner is IDock && hiddenDockables?.Contains(hiddenDockable) != true)
                    {
                        factory.SetActiveDockable(hiddenDockable);
                        factory.SetFocusedDockable(rootDock, hiddenDockable);
                    }
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
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            if (desktopLifetime.MainWindow is { } mainWindow)
            {
                mainWindow.Close();
            }
            else
            {
                desktopLifetime.Shutdown();
            }
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
        //IDocument? currentDocument = _factory.GetCurrentDocument();
        if (GetActiveFileViewModel(out fileViewModel, out _))
        {
            _factory!.CloseDockable(fileViewModel);
        }
    }

    /// <summary>
    /// Indicates if SaveFile command can be executed.
    /// </summary>
    /// <returns>true if a document is opened and has been modified; otherwise false.</returns>
    private bool CanSaveFile()
    {
        if (!GetActiveFileViewModel(out FileViewModel? fileViewModel, out _))
        {
            return false;
        }

        return fileViewModel!.DocumentType != DocumentType.ERESSEA_REPORT_FROM_ZIP
            && fileViewModel.IsDocumentModified();
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
        if (fileViewModel.Document is CRDocument reportDocument)
        {
            reportDocument.SourcePath = path;
        }
    }

    private void OnDockableClosed(object? sender, DockableClosedEventArgs args)
    {
        if (args.Dockable is FileViewModel fileViewModel)
        {
            fileViewModel.PropertyChanged -= OnFileViewModelPropertyChanged;
            if (fileViewModel.DocumentType.IsReportType())
            {
                _activeDocumentService.ClearDocument();
            }
        }

        SaveFileCommand.NotifyCanExecuteChanged();
        SaveFileAsCommand.NotifyCanExecuteChanged();
        CloseFileCommand.NotifyCanExecuteChanged();
    }

    private void CollectGlobalReportInformation(CRDocument report)
    {
        HasDocument = report.HasData();
        UnconfirmedOrdersCount = report.UnconfirmedOrdersCount;
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
