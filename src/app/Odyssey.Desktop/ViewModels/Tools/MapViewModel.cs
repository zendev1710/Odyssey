using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Extensions;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Settings;
using Odyssey.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Odyssey.ViewModels.Tools;

public partial class MapViewModel : DocumentToolViewModelBase/*, IRecipient<CenterMapMessage>*/
{
    [ObservableProperty]
    double _zoom = 100.0;

    partial void OnZoomChanged(double value)
    {
        if (double.IsFinite(value))
        {
            GlobalSettings.Shared.MapZoomPercentage = Math.Clamp((int)Math.Round(value), 20, 400);
        }
    }

    [ObservableProperty]
    bool _isDragging;

    [ObservableProperty]
    bool _isSelectionCenterPending;

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
    private readonly Dictionary<int, SavedPlaneSelection> _selectedItemsByPlane = [];

    private sealed class SavedPlaneSelectionState
    {
        public SavedPlaneSelectionState() { }

        public string ReportPath { get; set; } = string.Empty;
        public int ActivePlane { get; set; }
        public List<SavedPlaneSelection> Selections { get; set; } = [];
    }

    private sealed class SavedPlaneSelection
    {
        public SavedPlaneSelection() { }

        public int Plane { get; set; }
        public EntityModel.EntityType ItemType { get; set; }
        public int ItemId { get; set; }
        public int RegionX { get; set; }
        public int RegionY { get; set; }
        public int ContainerTypes { get; set; }
        public int? LocatedFactionId { get; set; }
    }

    partial void OnSelectedRegionChanged(RegionModel? oldValue, RegionModel? newValue)
    {
        if (!_isUpdatingSelection)
        {
            if (newValue is not null)
            {
                _selectedItemsByPlane[newValue.GetPlane()] = CreateSavedSelection(
                    new EntitySelection(newValue, newValue))!;
            }
            else if (SelectedPlane is not null)
            {
                _selectedItemsByPlane.Remove((int)SelectedPlane.Type);
            }
        }

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

    public Dictionary<int, RegionModel> OccupiedRegions
    {
        get => _populatedRegions;
        set
        {
            if (!ReferenceEquals(_populatedRegions, value))
            {
                _populatedRegions = value;
                OnPropertyChanged(nameof(OccupiedRegions));
            }
        }
    }

    public Dictionary<int, RegionModel> UnoccupiedRegions
    {
        get => _emptyRegions;
        set
        {
            if (!ReferenceEquals(_emptyRegions, value))
            {
                _emptyRegions = value;
                OnPropertyChanged(nameof(UnoccupiedRegions));
            }
        }
    }


    public Dictionary<int, ShipModel> Ships
    {
        get => _ships;
        set
        {
            if (!ReferenceEquals(_ships, value))
            {
                _ships = value;
                OnPropertyChanged(nameof(Ships));
            }
        }
    }

    public Dictionary<int, BuildingModel> Buildings
    {
        get => _buildings;
        set
        {
            if (!ReferenceEquals(_buildings, value))
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

    private Dictionary<int, RegionModel> _populatedRegions = [];
    private Dictionary<int, RegionModel> _emptyRegions = [];
    private Dictionary<int, ShipModel> _ships = [];
    private Dictionary<int, BuildingModel> _buildings = [];
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
        Zoom = Math.Clamp(GlobalSettings.Shared.MapZoomPercentage, 20, 400);
    }

    partial void OnSelectedPlaneChanged(WorldPlane? oldValue, WorldPlane? newValue)
    {
        if (oldValue is not null
            && SelectedRegion is { } previousRegion
            && previousRegion.GetPlane() == (int)oldValue.Type
            && !_selectedItemsByPlane.ContainsKey((int)oldValue.Type))
        {
            _selectedItemsByPlane[(int)oldValue.Type] = CreateSavedSelection(
                new EntitySelection(previousRegion, previousRegion))!;
        }

        RebuildMap();

        if (!_isUpdatingSelection)
        {
            RestoreSelectionForPlane(newValue);
        }
    }

    private SavedPlaneSelection? CreateSavedSelection(IEntitySelection selection)
    {
        RegionModel? region = selection.Region ?? selection.LocatedFaction?.Region ?? selection.Item as RegionModel;
        if (region is null && selection.Item is { } item)
        {
            region = Report.FindRegionById(item.LocationId);
        }

        if (region is null)
        {
            return null;
        }

        return new SavedPlaneSelection
        {
            Plane = region.GetPlane(),
            ItemType = selection.Item?.Type ?? EntityModel.EntityType.Unknown,
            ItemId = selection.Item?.Id ?? 0,
            RegionX = region.GetX(),
            RegionY = region.GetY(),
            ContainerTypes = (int)EntitySelection.GetContainerTypes(selection),
            LocatedFactionId = selection.LocatedFaction?.FactionId
        };
    }

    private void RestoreSelectionForPlane(WorldPlane? plane)
    {
        RegionModel? previousRegion = SelectedRegion;
        EntitySelection? selection = null;
        if (plane is not null
            && _selectedItemsByPlane.TryGetValue((int)plane.Type, out SavedPlaneSelection? savedSelection))
        {
            if (TryRestoreSelection(savedSelection, out selection))
            {
                _isUpdatingSelection = true;
                try
                {
                    SelectedRegion = selection.Region;
                    SelectedShip = ResolveSelectedShip(selection);
                }
                finally
                {
                    _isUpdatingSelection = false;
                }

                SendSelectionChangedEvent(selection);
                IsSelectionCenterPending = true;
                return;
            }

            _selectedItemsByPlane.Remove((int)plane.Type);
        }

        _isUpdatingSelection = true;
        try
        {
            SelectedRegion = null;
            SelectedShip = null;
        }
        finally
        {
            _isUpdatingSelection = false;
        }

        if (previousRegion is not null)
        {
            _selectionService.Clear(this);
        }
    }

    private bool TryRestoreSelection(SavedPlaneSelection saved, out EntitySelection? selection)
    {
        selection = null;
        if (saved.Plane != (int?)SelectedPlane?.Type
            || !Report.FindRegion(saved.RegionX, saved.RegionY, (PlaneType)saved.Plane, out RegionModel? region)
            || region is null)
        {
            return false;
        }

        EntityModel? item;
        switch (saved.ItemType)
        {
            case EntityModel.EntityType.Unknown:
                item = null;
                break;
            case EntityModel.EntityType.Region:
                item = region;
                break;
            case EntityModel.EntityType.Unit:
                if (!Report.FindUnit(saved.ItemId, out UnitModel? unit) || unit is null) return false;
                item = unit;
                break;
            case EntityModel.EntityType.Ship:
                if (!Report.FindShip(saved.ItemId, out ShipModel? ship) || ship is null) return false;
                item = ship;
                break;
            case EntityModel.EntityType.Building:
                if (!Report.FindBuilding(saved.ItemId, out BuildingModel? building) || building is null) return false;
                item = building;
                break;
            default:
                return false;
        }

        LocatedFactionModel? locatedFaction = null;
        if (saved.LocatedFactionId is int factionId)
        {
            locatedFaction = new LocatedFactionModel(
                Report.GetFaction(factionId), region, factionId, Report.GetFactionName(factionId));
        }

        selection = new EntitySelection(item, region, locatedFaction: locatedFaction);
        EntitySelection.SelectionTypes containerTypes = (EntitySelection.SelectionTypes)saved.ContainerTypes
            & (EntitySelection.SelectionTypes.Buildings | EntitySelection.SelectionTypes.Ships);
        if (containerTypes != EntitySelection.SelectionTypes.None)
        {
            selection.SetContainer(containerTypes, region);
        }
        return true;
    }

    public string CaptureSessionSelectionState()
    {
        if (!HasDocument || string.IsNullOrWhiteSpace(Report.SourcePath))
        {
            return string.Empty;
        }

        if (!CurrentSelection.IsEmpty() && CreateSavedSelection(CurrentSelection) is { } currentSelection)
        {
            _selectedItemsByPlane[currentSelection.Plane] = currentSelection;
        }

        return JsonSerializer.Serialize(new SavedPlaneSelectionState
        {
            ReportPath = Path.GetFullPath(Report.SourcePath),
            ActivePlane = (int)(SelectedPlane?.Type ?? PlaneType.WORLD),
            Selections = _selectedItemsByPlane.Values.ToList()
        });
    }

    public void RestoreSessionSelectionState(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(Report.SourcePath))
        {
            return;
        }

        SavedPlaneSelectionState? state;
        try
        {
            state = JsonSerializer.Deserialize<SavedPlaneSelectionState>(json);
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[MAPVIEWMODEL] Failed to read saved plane selections: {ex.Message}");
            return;
        }

        StringComparison pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (state is null
            || !Path.GetFullPath(Report.SourcePath).Equals(state.ReportPath, pathComparison))
        {
            return;
        }

        _selectedItemsByPlane.Clear();
        foreach (SavedPlaneSelection savedSelection in state.Selections ?? [])
        {
            if (Report.Planes.ContainsKey(savedSelection.Plane))
            {
                _selectedItemsByPlane[savedSelection.Plane] = savedSelection;
            }
        }

        WorldPlane? activePlane = Report.Planes.TryGetValue(state.ActivePlane, out WorldPlane? restoredPlane)
            ? restoredPlane
            : Report.Planes.TryGetValue((int)PlaneType.WORLD, out WorldPlane? worldPlane)
                ? worldPlane
                : Planes.FirstOrDefault();

        if (activePlane is not null)
        {
            if (SelectedPlane != activePlane)
            {
                SelectedPlane = activePlane;
            }
            else
            {
                RestoreSelectionForPlane(activePlane);
            }
        }
        else
        {
            SelectedPlane = null;
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

        bool isRestoredReportSelection = sender is ExplorerViewModel { IsRestoringReportSelection: true };

        SavedPlaneSelection? savedSelection = null;
        if (selection.IsEmpty())
        {
            if (SelectedPlane is not null)
            {
                _selectedItemsByPlane.Remove((int)SelectedPlane.Type);
            }

            _isUpdatingSelection = true;
            try
            {
                SelectedRegion = null;
                SelectedShip = null;
            }
            finally
            {
                _isUpdatingSelection = false;
            }
            return;
        }
        else
        {
            savedSelection = CreateSavedSelection(selection);
            if (savedSelection is not null)
            {
                _selectedItemsByPlane[savedSelection.Plane] = savedSelection;
            }
        }

        // Keep track of the selected ship so its route can be highlighted on the map.
        SelectedShip = ResolveSelectedShip(selection);

        RegionModel? selectedRegion = selection.Region;
        if (selectedRegion is null && savedSelection is not null)
        {
            Report.FindRegion(savedSelection.RegionX, savedSelection.RegionY, (PlaneType)savedSelection.Plane, out selectedRegion);
        }

        if (selectedRegion is not null)
        {
            // Update the selected region before setting SelectedRegion, in order to not send back the selection changed event
            _isUpdatingSelection = true;
            try
            {
                if (Report.Planes.TryGetValue(selectedRegion.GetPlane(), out WorldPlane? plane))
                {
                    SelectedPlane = plane;
                }
                SelectedRegion = selectedRegion;
                if (isRestoredReportSelection)
                {
                    IsSelectionCenterPending = true;
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }
    }

    public void CompleteSelectionCenterRequest()
    {
        IsSelectionCenterPending = false;
    }

    /// <summary>
    /// Handle active document changed event.
    /// Rebuilds the tree from the new active document data.
    /// </summary>
    /// <param name="cr"></param>
    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

        _selectedItemsByPlane.Clear();
        _isUpdatingSelection = true;
        try
        {
            SelectedRegion = null;
        }
        finally
        {
            _isUpdatingSelection = false;
        }

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
        SelectedPlane = Report.Planes.TryGetValue((int)PlaneType.WORLD, out WorldPlane? worldPlane)
            ? worldPlane : Planes.FirstOrDefault();
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
        Ships = Report.Ships;
        Buildings = Report.Buildings;
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
        Ships = [];
        Buildings = [];
        SelectedShip = null;
        Season = 0;
    }
}
