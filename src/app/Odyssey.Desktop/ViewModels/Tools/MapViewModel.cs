using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Extensions;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Odyssey.ViewModels.Tools;

public partial class MapViewModel : DocumentToolViewModelBase/*, IRecipient<CenterMapMessage>*/
{
    [ObservableProperty]
    double _zoom = 100.0;

    [ObservableProperty]
    bool _isDragging;

    [ObservableProperty]
    RegionModel? _selectedRegion;

    [ObservableProperty]
    ShipModel? _selectedShip;

    [ObservableProperty]
    Seasons _season = Seasons.UNKNOWN;

    [ObservableProperty]
    bool _isStandardPlane;

    [ObservableProperty]
    bool _hasSeveralPlanes;

    [ObservableProperty]
    WorldPlane? _selectedPlane;

    //[ObservableCollection]
    public ObservableCollection<WorldPlane> Planes { get; } = [];

    // Set to true while the selected region is updated from an incoming selection,
    // so that OnSelectedRegionChanged does not send the selection back.
    private bool _isUpdatingSelection;

    partial void OnSelectedRegionChanged(RegionModel? oldValue, RegionModel? newValue)
    {
        // Do not send the selection back when the region is only being synchronized with
        // the current selection: it would override the selected item (unit/ship/building)
        // with a region-only selection.
        if (_isUpdatingSelection)
        {
            return;
        }

        if (oldValue != newValue && newValue != null)
        {
            IEntitySelection selection = new EntitySelection(item: newValue, region: newValue);
            // Selection changed event will be sent only when a region has been selected directly in the map (mouse click...)
            SendSelectionChangedEvent(selection);
        }
    }

    public IEnumerable<KeyValuePair<int, RegionModel>> OccupiedRegions
    {
        get => _populatedRegions;
        set
        {
            if (!EqualityComparer<IEnumerable<KeyValuePair<int, RegionModel>>>.Default.Equals(_populatedRegions, value))
            {
                _populatedRegions = value;
                OnPropertyChanged(nameof(OccupiedRegions));
            }
        }
    }

    public IEnumerable<KeyValuePair<int, RegionModel>> UnoccupiedRegions
    {
        get => _emptyRegions;
        set
        {
            if (!EqualityComparer<IEnumerable<KeyValuePair<int, RegionModel>>>.Default.Equals(_emptyRegions, value))
            {
                _emptyRegions = value;
                OnPropertyChanged(nameof(UnoccupiedRegions));
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

    public IEnumerable<KeyValuePair<int, RegionModel>> RegionsWithStructure
    {
        get => _regionsWithStructure;
        set
        {
            if (!EqualityComparer<IEnumerable<KeyValuePair<int, RegionModel>>>.Default.Equals(_regionsWithStructure, value))
            {
                _regionsWithStructure = value;
                OnPropertyChanged(nameof(RegionsWithStructure));
            }
        }
    }

    private IEnumerable<KeyValuePair<int, RegionModel>> _populatedRegions = [];
    private IEnumerable<KeyValuePair<int, RegionModel>> _emptyRegions = [];
    private IEnumerable<KeyValuePair<int, ShipModel>> _ships = [];
    private IEnumerable<KeyValuePair<int, BuildingModel>> _buildings = [];
    private IEnumerable<KeyValuePair<int, RegionModel>> _regionsWithStructure = [];

    // Ids of the factions the player can play (allies may have several).
    // Used by the map to render castles owned by the player differently.
    public IReadOnlyCollection<int> ActiveFactionIds
    {
        get => _activeFactionIds;
        set
        {
            if (!EqualityComparer<IReadOnlyCollection<int>>.Default.Equals(_activeFactionIds, value))
            {
                _activeFactionIds = value;
                OnPropertyChanged(nameof(ActiveFactionIds));
            }
        }
    }

    private IReadOnlyCollection<int> _activeFactionIds = [];

    /// <summary>Ids of the factions allied to one of the active factions.</summary>
    public IReadOnlyCollection<int> AlliedFactionIds
    {
        get => _alliedFactionIds;
        set
        {
            if (!EqualityComparer<IReadOnlyCollection<int>>.Default.Equals(_alliedFactionIds, value))
            {
                _alliedFactionIds = value;
                OnPropertyChanged(nameof(AlliedFactionIds));
            }
        }
    }

    private IReadOnlyCollection<int> _alliedFactionIds = [];

    public MapViewModel() : base()
    {
    }

    partial void OnSelectedPlaneChanged(WorldPlane? oldValue, WorldPlane? newValue)
    {
        if (oldValue != newValue && newValue != null)
        {
            //Clear();
            SelectedPlane = newValue;
            RebuildMap();
        }
    }

    private ShipModel? ResolveSelectedShip(IEntitySelection selection)
    {
        if (selection.HasShip())
        {
            return selection.Item as ShipModel;
        }

        if (selection.HasUnit() && selection.Item is UnitModel unit)
        {
            if (unit.CommandedShip is not null)
            {
                return unit.CommandedShip;
            }

            if (Report.TryFindShipForUnit(unit, out ShipModel? ship))
            {
                return ship;
            }
        }

        return null;
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this))
            return;

        // Keep track of the selected ship so its route can be highlighted on the map.
        SelectedShip = ResolveSelectedShip(selection);

        // TODO
        if (selection.IsRegionSelected() /*&& !IsSelectedRegion(sel)*/)
        {
            // Update the selected region before setting SelectedRegion, in order to not send back the selection changed event
            _isUpdatingSelection = true;
            try
            {
                SelectedRegion = selection.Region/*!.DataBlock as RegionDataBlock*/;
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }
    }

    /// <summary>
    /// Handle active document changed event.
    /// Rebuilds the tree from the new active document data.
    /// </summary>
    /// <param name="cr"></param>
    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

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
        if (SelectedPlane is null)
        {
            Clear();
            return;
        }
        IsStandardPlane = ((int)SelectedPlane.Type).IsStandardPlane();
        OccupiedRegions = SelectedPlane.OccupiedRegions;
        UnoccupiedRegions = SelectedPlane.UnoccupiedRegions;
        ActiveFactionIds = Report.ActiveFactionIds;
        AlliedFactionIds = Report.AlliedFactionIds;
        Season = DateUtils.GetGameSeason(Report.Turn);
        Debug.WriteLine($"[MAPVIEWMODEL] RebuildMap::regions - with-people: {OccupiedRegions.Count()} without-people: {UnoccupiedRegions.Count()} Plane: {SelectedPlane.Name}");
    }

    private void Clear()
    {
        // LATER: check about Selection that should be cleared also
        OccupiedRegions = [];
        UnoccupiedRegions = [];
        SelectedShip = null;
        Season = 0;
    }
}
