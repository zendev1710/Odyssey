using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Odyssey.Core.Services;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using Odyssey.ViewModels.Docks;
using Odyssey.ViewModels.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.ViewModels;

public class DockFactory : Factory
{
    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    private ITool? _mapViewModel;
    private ITool? _miniMapViewModel;
    private ITool? _explorerViewModel;
    private ITool? _bookmarksViewModel;
    private ITool? _historyViewModel;
    private ITool? _regionPropertiesViewModel;
    private ITool? _regionStatisticsViewModel;
    private ITool? _detailsViewModel;
    private ITool? _unitOrdersViewModel;
    private ITool? _regionInfoViewModel;
    private ITool? _battlesViewModel;
    private ITool? _searchResultsViewModel;
    private ITool? _reportInfoViewModel;
    private ITool? _errorListViewModel;

    public const string RootDockId = "Root";
    public const string DocumentDockId = "Files";

    public DockFactory()
    {
        // Close action just hide dock
        HideToolsOnClose = true;
    }

    public override IDocumentDock CreateDocumentDock() => new FilesDocumentDock();

    public override IRootDock CreateLayout()
    {
        Debug.WriteLine("[DOCK-FACTO] CreateLayout...");
        IDocument? doc = this.GetCurrentDocument();

        // As HideToolsOnClose was set to true, close action just hide a tool, that can be restored
        bool canCloseTool = true;

        var helpViewModel = new HelpViewModel(ServiceLocator.LanguageService);

        // all these models are ITool
        var mapViewModel = new MapViewModel() { Id = Ids.Map, Title = LocalizedTitle(Ids.Map), CanClose = canCloseTool /*false*/ };
        var miniMapViewModel = new MiniMapViewModel() { Id = Ids.MiniMap, Title = LocalizedTitle(Ids.MiniMap), CanClose = canCloseTool };
        var explorerViewModel = new ExplorerViewModel() {Id = Ids.Explorer, Title = LocalizedTitle(Ids.Explorer), CanClose = canCloseTool };
        var historyViewModel = new HistoryViewModel() { Id = Ids.History, Title = LocalizedTitle(Ids.History), CanClose = canCloseTool };
        var bookmarksViewModel = new BookmarksViewModel() {Id = Ids.Bookmarks, Title = LocalizedTitle(Ids.Bookmarks), CanClose = canCloseTool };
        var regionPropertiesViewModel = new RegionPropertiesViewModel() {Id = Ids.RegionProperties, Title = LocalizedTitle(Ids.RegionProperties), CanClose = canCloseTool };
        var regionStatisticsViewModel = new RegionStatsViewModel() {Id = Ids.RegionStatistics, Title = LocalizedTitle(Ids.RegionStatistics), CanClose = canCloseTool };
        var detailsViewModel = new DetailsViewModel() {Id = Ids.Details, Title = LocalizedTitle(Ids.Details), CanClose = canCloseTool };
        var unitsInspectorViewModel = new UnitsInspectorViewModel() { Id = Ids.UnitsInspector, Title = LocalizedTitle(Ids.UnitsInspector), CanClose = canCloseTool };
        //var unittripViewModel = new UnitStripViewModel() { Id = Ids.UnitStrip, Title = LocalizedTitle(Ids.UnitStrip), CanClose = canCloseTool };
        //var unitInspectorViewModel = new UnitInspectorViewModel() { Id = Ids.UnitInspector, Title = LocalizedTitle(Ids.UnitInspector), CanClose = canCloseTool };
        var unitOrdersViewModel = new UnitOrdersViewModel() { Id = Ids.UnitOrders, Title = LocalizedTitle(Ids.UnitOrders), CanClose = canCloseTool };
        var regionInfoViewModel = new RegionInfoViewModel() {Id = Ids.RegionInfo, Title = LocalizedTitle(Ids.RegionInfo), CanClose = canCloseTool };
        var battlesViewModel = new BattlesViewModel() {Id = Ids.Battles, Title = LocalizedTitle(Ids.Battles), CanClose = canCloseTool };
        var searchResultsViewModel = new SearchResultsViewModel() {Id = Ids.SearchResults, Title = LocalizedTitle(Ids.SearchResults), CanClose = canCloseTool };
        var reportInfoViewModel = new ReportInfoViewModel() {Id = Ids.ReportInfo, Title = LocalizedTitle(Ids.ReportInfo), CanClose = canCloseTool };
        var errorListViewModel = new ErrorListViewModel() {Id = Ids.ErrorList, Title = LocalizedTitle(Ids.ErrorList), CanClose = canCloseTool };

        // leftDock contains the regions explorer only (left, on the whole height)
        var leftDock = new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>
            (
                new ToolDock
                {
                    Proportion = 0.75,
                    ActiveDockable = explorerViewModel,
                    VisibleDockables = CreateList<IDockable>(explorerViewModel),
                    Alignment = Alignment.Left
                },
                new ProportionalDockSplitter(),
                new ToolDock
                {
                    Proportion = 0.1,
                    ActiveDockable = bookmarksViewModel,
                    VisibleDockables = CreateList<IDockable>(bookmarksViewModel),
                },
                new ProportionalDockSplitter(),
                new ToolDock
                {
                    Proportion = 0.1,
                    ActiveDockable = historyViewModel,
                    VisibleDockables = CreateList<IDockable>(historyViewModel),
                },
                new ProportionalDockSplitter(),
                new ToolDock
                {
                    ActiveDockable = miniMapViewModel,
                    VisibleDockables = CreateList<IDockable>(miniMapViewModel),
                }
            )
        };

        /////////////////////////////

        var documentDock = new FilesDocumentDock
        {
            IsCollapsable = false,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = false
        };

        var mapDock = new ProportionalDock
        {
            Proportion = 0.50,
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                new ToolDock
                {
                    ActiveDockable = mapViewModel,
                    VisibleDockables = CreateList<IDockable>(mapViewModel),
                    Alignment = Alignment.Bottom,
                }
            )
        };

        var regionPropertiesDock = new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                new ToolDock
                {
                    ActiveDockable = regionPropertiesViewModel,
                    VisibleDockables = CreateList<IDockable>(regionPropertiesViewModel),
                    Alignment = Alignment.Bottom,
                }
            )
        };

        var messagesDock = new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Horizontal,
            ActiveDockable = reportInfoViewModel,
            VisibleDockables = CreateList<IDockable>
            (
                new ToolDock
                {
                    ActiveDockable = regionInfoViewModel,
                    VisibleDockables = CreateList<IDockable>(regionInfoViewModel, battlesViewModel, searchResultsViewModel, reportInfoViewModel, errorListViewModel),
                    Alignment = Alignment.Bottom,
                }
            )
        };

        // Contains the documentDock and the messagesDock under it
        var centralDock = new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Vertical,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>
            (
                documentDock,
                new ProportionalDockSplitter(),
                mapDock,
                new ProportionalDockSplitter(),
                regionPropertiesDock,
                new ProportionalDockSplitter(),
                messagesDock
            )
        };

        var detailsDock = new ToolDock
        {
            Proportion = 0.5,
            ActiveDockable = detailsViewModel,
            VisibleDockables = CreateList<IDockable>(detailsViewModel, regionStatisticsViewModel),
        };

        var ordersDock = new ToolDock
        {
            Proportion = 0.5,
            ActiveDockable = unitOrdersViewModel,
            VisibleDockables = CreateList<IDockable>(unitOrdersViewModel),
        };

        var upperToolsDock = new ProportionalDock
        {
            Proportion = 0.5,
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                ordersDock,
                new ProportionalDockSplitter(),
                detailsDock
            )
        };

        var rightDock = new ProportionalDock
        {
            Proportion = 0.5,
            Orientation = Orientation.Vertical,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>
            (
                upperToolsDock,
                new ProportionalDockSplitter(),
                new ToolDock
                {
                    Proportion = 0.5,
                    ActiveDockable = unitsInspectorViewModel,
                    VisibleDockables = CreateList<IDockable>(unitsInspectorViewModel),
                }
            )
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>
            (
                leftDock,
                new ProportionalDockSplitter(),
                rightDock,
                new ProportionalDockSplitter(),
                centralDock
            )
        };

        var searchView = new SearchViewModel() { Id = Ids.Search, Title = "Find" };
        var configurationView = new ConfigurationViewModel() { Id = Ids.Configuration, Title = "Configuration" };

        // The workspace view is the default root view (main window), embedding all docking windows
        var workspaceView = new WorkspaceViewModel
        {
            Id = Ids.Workspace,
            Title = "Workspace",
            ActiveDockable = mainLayout,
            VisibleDockables = CreateList<IDockable>(mainLayout)
        };

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = searchView;
        rootDock.DefaultDockable = workspaceView;
        rootDock.VisibleDockables = CreateList<IDockable>(searchView, configurationView, workspaceView);

        rootDock.LeftPinnedDockables = CreateList<IDockable>();
        rootDock.RightPinnedDockables = CreateList<IDockable>();
        rootDock.TopPinnedDockables = CreateList<IDockable>();
        rootDock.BottomPinnedDockables = CreateList<IDockable>();
        rootDock.PinnedDock = null;

        _documentDock = documentDock;
        _rootDock = rootDock;

        _mapViewModel = mapViewModel;
        _miniMapViewModel = miniMapViewModel;
        _explorerViewModel = explorerViewModel;
        _bookmarksViewModel = bookmarksViewModel;
        _historyViewModel = historyViewModel;
        _regionPropertiesViewModel = regionPropertiesViewModel;
        _regionStatisticsViewModel = regionStatisticsViewModel;
        _detailsViewModel = detailsViewModel;
        _unitOrdersViewModel = unitOrdersViewModel;
        _regionInfoViewModel = regionInfoViewModel;
        _battlesViewModel = battlesViewModel;
        _searchResultsViewModel = searchResultsViewModel;
        _reportInfoViewModel = reportInfoViewModel;
        _errorListViewModel = errorListViewModel;

        Debug.WriteLine("[DOCK-FACTO] CreateLayout done");
        return rootDock;
    }

    public override IDockWindow? CreateWindowFrom(IDockable dockable)
    {
        var window = base.CreateWindowFrom(dockable);
        if (window != null)
        {
            window.Title = "Eressea Odyssey";
        }
        return window;
    }

    public override void InitLayout(IDockable layout)
    {
        Debug.WriteLine("[DOCK-FACTO] InitLayout...");

        // ContextLocator is used by FactoryBase::GetContext(<context-id>) 
        ContextLocator = new Dictionary<string, Func<object?>>
        {
            // TODO: fill all entries
            //["Document1"] = () => new CRDocument(),
            //[Explorer] = () => , //layout,
            [Ids.RegionProperties] = () => new RegionPropertiesContent(),
            //[Ids.RegionStatistics] = () => new (),
            //[Ids.Details] = () => new (),
            ["Report"] = () => new ReportContent(),
            ["RegionInfo"] = () => new RegionContent(),
            ["ErrorList"] = () => new ErrorsContent(),
            ["SearchResults"] = () => new SearchResultsContent(),
            // Navigation can be achieved on below views
            [Ids.Search] = () => layout,
            [Ids.Configuration] = () => layout,
            [Ids.Workspace] = () => ServiceLocator.DocumentService.ActiveDocument
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>()
        {
            [RootDockId] = () => _rootDock,
            [DocumentDockId] = () => _documentDock,
            [Ids.Map] = () => _mapViewModel,
            [Ids.MiniMap] = () => _miniMapViewModel,
            [Ids.Explorer] = () => _explorerViewModel,
            [Ids.Bookmarks] = () => _bookmarksViewModel,
            [Ids.History] = () => _historyViewModel,
            [Ids.RegionProperties] = () => _regionPropertiesViewModel,
            [Ids.RegionStatistics] = () => _regionStatisticsViewModel,
            [Ids.Details] = () => _detailsViewModel,
            [Ids.UnitOrders] = () => _unitOrdersViewModel,
            [Ids.RegionInfo] = () => _regionInfoViewModel,
            [Ids.Battles] = () => _battlesViewModel,
            [Ids.SearchResults] = () => _searchResultsViewModel,
            [Ids.ReportInfo] = () => _reportInfoViewModel,
            [Ids.ErrorList] = () => _errorListViewModel,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);

        Debug.WriteLine("[DOCK-FACTO] InitLayout done");
    }

    private static string LocalizedTitle(string id)
    {
        return Labels.Localize($"win_{id.ToLower()}");
    }
}

