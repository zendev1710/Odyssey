using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Core.Help.Models;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Services;
using Odyssey.Utils;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Odyssey.ViewModels.Tools;

public partial class MapViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    DataBlock? _selectedRegion;

    [ObservableProperty]
    Seasons _season = Seasons.UNKNOWN;

    [ObservableProperty]
    bool _hasSeveralPlanes;

    [ObservableProperty]
    WorldPlane? _selectedPlane;

    //[ObservableCollection]
    public ObservableCollection<WorldPlane> Planes { get; } = [];

    partial void OnSelectedRegionChanged(DataBlock? oldValue, DataBlock? newValue)
    {
        if (oldValue != newValue && newValue != null)
        {
            ISelection selection = new SimpleItemSelection(item: newValue, region: newValue, faction: null);
            if (IsSelectedRegion(selection))
            {
                return;
            }
            SetSelection(selection);
            // Selection changed event will be sent only when a region has been selected directly in the map (mouse click...)
            SendSelectionChangedEvent(selection);
        }
    }

    public IEnumerable<KeyValuePair<int, DataBlock>> Regions
    {
        get => _regions;
        set
        {
            if (!EqualityComparer<IEnumerable<KeyValuePair<int, DataBlock>>>.Default.Equals(_regions, value))
            {
                _regions = value;
                OnPropertyChanged(nameof(Regions));
            }
        }
    }

    public IEnumerable<KeyValuePair<int, ShipModel>> Ships
    {
        get => _ships;
        set
        {
            if (!EqualityComparer<IEnumerable<KeyValuePair<int, ShipModel>>>.Default.Equals(_ships, value))
            {
                _ships = value;
                OnPropertyChanged(nameof(Ships));
            }
        }
    }

    public IEnumerable<KeyValuePair<int, BuildingModel>> Buildings
    {
        get => _buildings;
        set
        {
            if (!EqualityComparer<IEnumerable<KeyValuePair<int, BuildingModel>>>.Default.Equals(_buildings, value))
            {
                _buildings = value;
                OnPropertyChanged(nameof(Buildings));
            }
        }
    }

    public IEnumerable<KeyValuePair<int, RegionModel>> RegionsWithContainer
    {
        get => _regionsWithContainer;
        set
        {
            if (!EqualityComparer<IEnumerable<KeyValuePair<int, RegionModel>>>.Default.Equals(_regionsWithContainer, value))
            {
                _regionsWithContainer = value;
                OnPropertyChanged(nameof(RegionsWithContainer));
            }
        }
    }

    private IEnumerable<KeyValuePair<int, DataBlock>> _regions = [];
    private IEnumerable<KeyValuePair<int, ShipModel>> _ships = [];
    private IEnumerable<KeyValuePair<int, BuildingModel>> _buildings = [];
    private IEnumerable<KeyValuePair<int, RegionModel>> _regionsWithContainer = [];

    public MapViewModel(IEventAggregator? eventAggregator = null, IActiveDocumentService? documentService = null) : base(eventAggregator, documentService)
    {
    }

    partial void OnSelectedPlaneChanged(WorldPlane? value)
    {
        if (value != null)
        {
            //Clear();
            //RebuildMap();
        }
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        ISelection sel = selectionChange.Selection;
        if (sel.IsRegionSelected() && !IsSelectedRegion(sel))
        {
            // Update the selected region before setting SelectedRegion, in order to not send back the selection changed event
            SetSelection(sel);
            SelectedRegion = sel?.Region!;
        }
    }

    /// <summary>
    /// Handle active document changed event.
    /// Rebuilds the tree from the new active document data.
    /// </summary>
    /// <param name="cr"></param>
    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        base.OnActiveDocumentChanged(cr);
        RebuildPlanes();
        RebuildMap();
    }

    /// <summary>
    /// Rebuilds the map from the data in the report document.
    /// </summary>
    private void RebuildPlanes()
    {
        Planes.Clear();
        // Sort the planes by their planeType value (standard, astral...)
        var sortedPlanes = Report.Planes.OrderBy(p => p.Key).ToList();
        foreach (var planeElt in sortedPlanes)
        {
            Planes.Add(planeElt.Value);
        }

        HasSeveralPlanes = Report.Planes.Count > 1;
        SelectedPlane = Report.Planes.FirstOrDefault().Value;
    }

    /// <summary>
    /// Rebuilds the map from the data in the report document.
    /// </summary>
    private void RebuildMap()
    {
        // TODO: Reports.Regions is for the standard plane only, we should have a Regions collection for each plane
        Regions = Report.Regions;
        Season = DateUtils.GetGameSeason(Report.Turn);
    }

    private void Clear()
    {
        // LATER: check about Selection that should be cleared also
        Regions = [];
        Season = 0;
    }
}
