using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using Odyssey.ViewModels.Docks;
using Odyssey.ViewModels.Documents;
using Odyssey.ViewModels.Tools;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.ViewModels;

public class DockFactory : Factory
{
    private readonly object _context;
    private readonly IEventAggregator? _eventAggregator;
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

    protected IEventAggregator? EventAggregator { get { return _eventAggregator; } }

    public const string Root = "Root";

    public DockFactory(IEventAggregator? eventAggregator, object context)
    {
        // Commenting the following line will cause all views to stay empty, due to no publishing nor receiving event from event aggregation 
        _eventAggregator = eventAggregator;
        _context = context;
        // Close action just hide dock
        HideToolsOnClose = true;
    }

    public override IDocumentDock CreateDocumentDock() => new FilesDocumentDock();

    public override IRootDock CreateLayout()
    {
        Debug.WriteLine("[DOCK-FACTO] Layout creation");

        // As HideToolsOnClose was set to true, close action just hide a tool, that can be restored
        bool canCloseTool = true;

        // all these models are ITool
        var mapViewModel = new MapViewModel(EventAggregator) { Id = Ids.Map, Title = LocalizedTitle(Ids.Map), CanClose = canCloseTool /*false*/ };
        var miniMapViewModel = new MiniMapViewModel(EventAggregator) { Id = Ids.MiniMap, Title = LocalizedTitle(Ids.MiniMap), CanClose = canCloseTool };
        var explorerViewModel = new ExplorerViewModel(EventAggregator) {Id = Ids.Explorer, Title = LocalizedTitle(Ids.Explorer), CanClose = canCloseTool };
        var historyViewModel = new HistoryViewModel(EventAggregator) { Id = Ids.History, Title = LocalizedTitle(Ids.History), CanClose = canCloseTool };
        var bookmarksViewModel = new BookmarksViewModel(EventAggregator) {Id = Ids.Bookmarks, Title = LocalizedTitle(Ids.Bookmarks), CanClose = canCloseTool };
        var regionPropertiesViewModel = new RegionPropertiesViewModel(EventAggregator) {Id = Ids.RegionProperties, Title = LocalizedTitle(Ids.RegionProperties), CanClose = canCloseTool };
        var regionStatisticsViewModel = new RegionStatsViewModel(EventAggregator) {Id = Ids.RegionStatistics, Title = LocalizedTitle(Ids.RegionStatistics), CanClose = canCloseTool };
        var detailsViewModel = new DetailsViewModel(EventAggregator) {Id = Ids.Details, Title = LocalizedTitle(Ids.Details), CanClose = canCloseTool };
        var unitOrdersViewModel = new UnitOrdersViewModel(EventAggregator) { Id = Ids.UnitOrders, Title = LocalizedTitle(Ids.UnitOrders), CanClose = canCloseTool };
        var regionInfoViewModel = new RegionInfoViewModel(EventAggregator) {Id = Ids.RegionInfo, Title = LocalizedTitle(Ids.RegionInfo), CanClose = canCloseTool };
        var battlesViewModel = new BattlesViewModel(EventAggregator) {Id = Ids.Battles, Title = LocalizedTitle(Ids.Battles), CanClose = canCloseTool };
        var searchResultsViewModel = new SearchResultsViewModel(EventAggregator) {Id = Ids.SearchResults, Title = LocalizedTitle(Ids.SearchResults), CanClose = canCloseTool };
        var reportInfoViewModel = new ReportInfoViewModel(EventAggregator) {Id = Ids.ReportInfo, Title = LocalizedTitle(Ids.ReportInfo), CanClose = canCloseTool };
        var errorListViewModel = new ErrorListViewModel(EventAggregator) {Id = Ids.ErrorList, Title = LocalizedTitle(Ids.ErrorList), CanClose = canCloseTool };

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
                    ActiveDockable = historyViewModel,
                    VisibleDockables = CreateList<IDockable>(historyViewModel),
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

        // contains the properties and so on, until orders at the bottom 
        var rightDock = new ProportionalDock
        {
            Proportion = 0.25,
            Orientation = Orientation.Vertical,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>
            (
                new ToolDock
                {
                    Proportion = 0.75,
                    ActiveDockable = detailsViewModel,
                    VisibleDockables = CreateList<IDockable>(detailsViewModel, regionStatisticsViewModel),
                },
                new ProportionalDockSplitter(),
                new ToolDock
                {
                    ActiveDockable = unitOrdersViewModel,
                    VisibleDockables = CreateList<IDockable>(unitOrdersViewModel),
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
                centralDock,
                new ProportionalDockSplitter(),
                rightDock
            )
        };

        var searchView = new SearchViewModel(EventAggregator) { Id = Ids.Search, Title = "Find" };
        var configurationView = new ConfigurationViewModel(EventAggregator) { Id = Ids.Configuration, Title = "Configuration" };

        // The home view is the default root view, embedding all docking windows
        var homeView = new HomeViewModel
        {
            Id = Ids.Home,
            Title = "Home",
            ActiveDockable = mainLayout,
            VisibleDockables = CreateList<IDockable>(mainLayout)
        };

        var rootDock = CreateRootDock();

        rootDock.IsCollapsable = false;
        rootDock.ActiveDockable = searchView;
        rootDock.DefaultDockable = homeView;
        rootDock.VisibleDockables = CreateList<IDockable>(searchView, configurationView, homeView);

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

        Debug.WriteLine("[DOCK-FACTO] Layout creation done");
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
        // ContextLocator is used by FactoryBase::GetContext("<context-id>") 
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
            [Ids.Home] = () => _context
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>()
        {
            [Root] = () => _rootDock,
            ["Documents"] = () => _documentDock,
            ["Files"] = () => _documentDock,
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
    }

    private static string LocalizedTitle(string id)
    {
        return Labels.Localize($"win_{id.ToLower()}");
    }
}

