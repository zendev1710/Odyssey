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
using System.Linq;

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
            Proportion = 0.6,
            ActiveDockable = detailsViewModel,
            VisibleDockables = CreateList<IDockable>(detailsViewModel, regionStatisticsViewModel),
        };

        var ordersDock = new ToolDock
        {
            Proportion = 0.4,
            ActiveDockable = unitOrdersViewModel,
            VisibleDockables = CreateList<IDockable>(unitOrdersViewModel),
        };

        var upperToolsDock = new ProportionalDock
        {
            Proportion = 0.5,
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>
            (
                detailsDock,
                new ProportionalDockSplitter(),
                ordersDock
            )
        };

        var rightDock = new ProportionalDock
        {
            Proportion = 0.5,
            Orientation = Orientation.Vertical,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>
            (
                new ToolDock
                {
                    Proportion = 0.5,
                    ActiveDockable = unitsInspectorViewModel,
                    VisibleDockables = CreateList<IDockable>(unitsInspectorViewModel),
                },
                new ProportionalDockSplitter(),
                upperToolsDock
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

    public IRootDock CreateLayoutPreservingContext()
    {
        Dictionary<string, ITool> currentTools = CaptureToolDockables(_rootDock);
        IDocumentDock? currentDocumentDock = _documentDock;
        IRootDock layout = CreateLayout();

        ReplaceToolDockables(layout, currentTools);
        TransferDocumentDockContents(currentDocumentDock, layout);
        BindDockables(layout);

        return layout;
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

        if (_rootDock is not null && !ReferenceEquals(_rootDock, layout))
        {
            ReplaceToolDockables(layout, CaptureToolDockables(_rootDock));
            TransferDocumentDockContents(_documentDock, layout);
        }

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

        BindDockables(layout);
        base.InitLayout(layout);

        BindDockables(layout);

        Debug.WriteLine("[DOCK-FACTO] InitLayout done");
    }

    private static Dictionary<string, ITool> CaptureToolDockables(IDockable? layout)
    {
        Dictionary<string, ITool> tools = new(StringComparer.Ordinal);
        if (layout is null)
        {
            return tools;
        }

        Dictionary<string, IDockable> dockables = new(StringComparer.Ordinal);
        CollectDockables(layout, dockables, new HashSet<IDockable>());
        foreach (ITool tool in dockables.Values.OfType<ITool>())
        {
            if (!string.IsNullOrEmpty(tool.Id))
            {
                tools[tool.Id] = tool;
            }
        }

        return tools;
    }

    private static void ReplaceToolDockables(IDockable layout, IReadOnlyDictionary<string, ITool> tools)
    {
        ReplaceDockable(layout, tools, new HashSet<IDockable>());
    }

    private static void ReplaceDockable(IDockable dockable, IReadOnlyDictionary<string, ITool> tools, ISet<IDockable> visited)
    {
        if (!visited.Add(dockable))
        {
            return;
        }

        if (dockable is IDock dock)
        {
            ReplaceToolDockablesInList(dock.VisibleDockables, tools, visited, dock);
            dock.ActiveDockable = GetCurrentTool(dock.ActiveDockable, tools);
        }

        if (dockable is IRootDock rootDock)
        {
            ReplaceToolDockablesInList(rootDock.HiddenDockables, tools, visited, rootDock);
            ReplaceToolDockablesInList(rootDock.LeftPinnedDockables, tools, visited, rootDock);
            ReplaceToolDockablesInList(rootDock.RightPinnedDockables, tools, visited, rootDock);
            ReplaceToolDockablesInList(rootDock.TopPinnedDockables, tools, visited, rootDock);
            ReplaceToolDockablesInList(rootDock.BottomPinnedDockables, tools, visited, rootDock);
            rootDock.DefaultDockable = GetCurrentTool(rootDock.DefaultDockable, tools);

            if (rootDock.Windows is { } windows)
            {
                foreach (IDockWindow window in windows)
                {
                    if (window.Layout is { } windowLayout)
                    {
                        ReplaceDockable(windowLayout, tools, visited);
                    }
                }
            }
        }
    }

    private static void ReplaceToolDockablesInList(
        IList<IDockable>? dockables,
        IReadOnlyDictionary<string, ITool> tools,
        ISet<IDockable> visited,
        IDockable owner)
    {
        if (dockables is null)
        {
            return;
        }

        for (int i = 0; i < dockables.Count; i++)
        {
            IDockable serializedDockable = dockables[i];
            IDockable? dockable = GetCurrentTool(serializedDockable, tools);
            if (dockable is null)
            {
                continue;
            }

            if (!ReferenceEquals(dockable, serializedDockable))
            {
                dockable.Owner = owner;
                dockable.OriginalOwner = serializedDockable.OriginalOwner;
                dockable.Factory = serializedDockable.Factory ?? dockable.Factory;
            }

            dockables[i] = dockable;
            ReplaceDockable(dockable, tools, visited);
        }
    }

    private static IDockable? GetCurrentTool(IDockable? dockable, IReadOnlyDictionary<string, ITool> tools)
    {
        if (dockable is ITool tool && !string.IsNullOrEmpty(tool.Id) && tools.TryGetValue(tool.Id, out ITool? currentTool))
        {
            return currentTool;
        }

        return dockable;
    }

    private void TransferDocumentDockContents(IDocumentDock? source, IDockable targetLayout)
    {
        if (source is null)
        {
            return;
        }

        Dictionary<string, IDockable> dockables = new(StringComparer.Ordinal);
        CollectDockables(targetLayout, dockables, new HashSet<IDockable>());
        if (!dockables.TryGetValue(DocumentDockId, out IDockable? targetDockable) || targetDockable is not IDocumentDock target)
        {
            return;
        }

        List<IDockable> documents = source.VisibleDockables?.ToList() ?? [];
        IDockable? activeDocument = source.ActiveDockable;
        target.VisibleDockables = CreateList<IDockable>(documents.ToArray());
        target.ActiveDockable = activeDocument is not null && documents.Contains(activeDocument) ? activeDocument : null;

        if (!ReferenceEquals(source, target))
        {
            source.VisibleDockables?.Clear();
            source.ActiveDockable = null;
        }
    }

    private void BindDockables(IDockable layout)
    {
        Dictionary<string, IDockable> dockables = new(StringComparer.Ordinal);
        CollectDockables(layout, dockables, new HashSet<IDockable>());

        _rootDock = layout as IRootDock;
        _documentDock = FindDockable<IDocumentDock>(DocumentDockId);
        _mapViewModel = FindTool(Ids.Map);
        _miniMapViewModel = FindTool(Ids.MiniMap);
        _explorerViewModel = FindTool(Ids.Explorer);
        _bookmarksViewModel = FindTool(Ids.Bookmarks);
        _historyViewModel = FindTool(Ids.History);
        _regionPropertiesViewModel = FindTool(Ids.RegionProperties);
        _regionStatisticsViewModel = FindTool(Ids.RegionStatistics);
        _detailsViewModel = FindTool(Ids.Details);
        _unitOrdersViewModel = FindTool(Ids.UnitOrders);
        _regionInfoViewModel = FindTool(Ids.RegionInfo);
        _battlesViewModel = FindTool(Ids.Battles);
        _searchResultsViewModel = FindTool(Ids.SearchResults);
        _reportInfoViewModel = FindTool(Ids.ReportInfo);
        _errorListViewModel = FindTool(Ids.ErrorList);

        ITool? FindTool(string id) => dockables.TryGetValue(id, out IDockable? dockable) ? dockable as ITool : null;
        T? FindDockable<T>(string id) where T : class, IDockable => dockables.TryGetValue(id, out IDockable? dockable) ? dockable as T : null;
    }

    private static void CollectDockables(IDockable dockable, IDictionary<string, IDockable> dockables, ISet<IDockable> visited)
    {
        if (!visited.Add(dockable))
        {
            return;
        }

        if (!string.IsNullOrEmpty(dockable.Id))
        {
            dockables[dockable.Id] = dockable;
        }

        if (dockable is IDock dock && dock.VisibleDockables is { } visibleDockables)
        {
            foreach (IDockable child in visibleDockables)
            {
                CollectDockables(child, dockables, visited);
            }
        }

        if (dockable is IRootDock rootDock)
        {
            CollectDockablesFromList(rootDock.HiddenDockables, dockables, visited);
            CollectDockablesFromList(rootDock.LeftPinnedDockables, dockables, visited);
            CollectDockablesFromList(rootDock.RightPinnedDockables, dockables, visited);
            CollectDockablesFromList(rootDock.TopPinnedDockables, dockables, visited);
            CollectDockablesFromList(rootDock.BottomPinnedDockables, dockables, visited);

            if (rootDock.Windows is { } windows)
            {
                foreach (IDockWindow window in windows)
                {
                    if (window.Layout is { } windowLayout)
                    {
                        CollectDockables(windowLayout, dockables, visited);
                    }
                }
            }
        }
    }

    private static void CollectDockablesFromList(IList<IDockable>? children, IDictionary<string, IDockable> dockables, ISet<IDockable> visited)
    {
        if (children is null)
        {
            return;
        }

        foreach (IDockable child in children)
        {
            CollectDockables(child, dockables, visited);
        }
    }

    private static string LocalizedTitle(string id)
    {
        return Labels.Localize($"win_{id.ToLower()}");
    }
}

