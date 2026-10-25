using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting; // added for TextLayout
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Odyssey.Controls.Buildings;
using Odyssey.Controls.Ships;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Localization;
using Odyssey.Settings;
using Odyssey.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Odyssey.Controls;


/***
 * Represents a hexagonal map control for displaying regions with different terrains.
 * 
 * In the context of hexagonal grids, Q and R are standard names for the two coordinates in the axial coordinate system.
 * This system is commonly used to represent positions on a hex grid because it simplifies calculations compared to using offset or cube coordinates.
 * Why use Q and R ?
 * - Q: represents the column(or "x-like" axis) in the hex grid.
 * - R: represents the row(or "y-like" axis) in the hex grid.
 * This naming convention is widely used in hex grid algorithms and libraries(see Red Blob Games: Hexagonal Grids), 
 * making the code easier to understand for developers familiar with hex maps.
*/
public class HexMapControl : Control
{
    // TODO: mouse wheel + ctrl for zoom-in zoom-out
    // TODO: display zoom percentage while zooming with slider
    // TODO: scrollbars should stay when map view is in float mode
    // TODO: do not darken oceans where someone traveled, where a lighthouse can watch...
    // TODO: zoom slider as a map setting "dispaly zoom slider at the top of the map"
    // TODO: 'show fog of war' setting and handle
    // TODO: show see monsters, storms...
    // TODO: 'image background' setting and handle
    // MG: BorderCellRenderer : streats, coasts...

    private class BoatWake
    { 
        public int Q;
        public int R;
        public double AngleDegrees;
        public DateTime Created;
        public double DurationSeconds;
    }

    private const int defaultZoom = 100; // 100% zoom

    private readonly object _wakesLock = new();
    private readonly List<BoatWake> _boatWakes = new();
    private Bitmap? _boatWakeBitmap;

    // currently hovered region (for tooltip)
    private RegionModel? _hoveredRegion;

    // Selected region coordinates (Q, R) for internal tracking
    private bool _isPointerCapturedForPanning = false;
    private bool _isPanning = false;

    // pointer start in viewport (ScrollViewer) coordinates to avoid double-count when content scrolls
    private Point _pointerStartViewport = new Point();

    // pointer start fallback (control local coords) - kept for fallback if no ScrollViewer present
    private Point _pointerStartLocal = new Point();

    private Vector _scrollStartOffset = new Vector();
    private const double PanMinPixels = 6.0;
    private double ComputePanThreshold() => Math.Max(PanMinPixels, (HexSize * Zoom / 100.0) * 0.20);

    //private const double PanStartThreshold = 4.0; // pixels

    // Default size of hexagon as the width of the bounding square in pixels
    private static readonly double defaultHexSideSize = 64; //64 or 80

    // Default size of hexagon as from radius to corner
    private static readonly double defaultHexSize = defaultHexSideSize / Math.Sqrt(3);

    public static readonly StyledProperty<Seasons> SeasonProperty =
        AvaloniaProperty.Register<HexMapControl, Seasons>(nameof(Season), Seasons.UNKNOWN);

    public static readonly StyledProperty<Dictionary<int, RegionModel>> OccupiedRegionsProperty =
        AvaloniaProperty.Register<HexMapControl, Dictionary<int, RegionModel>>(nameof(OccupiedRegions));

    public static readonly StyledProperty<Dictionary<int, RegionModel>> UnoccupiedRegionsProperty =
        AvaloniaProperty.Register<HexMapControl, Dictionary<int, RegionModel>>(nameof(UnoccupiedRegions));

    public static readonly StyledProperty<Dictionary<int, ShipModel>> ShipsProperty =
        AvaloniaProperty.Register<HexMapControl, Dictionary<int, ShipModel>>(nameof(Ships));

    public static readonly StyledProperty<Dictionary<int, BuildingModel>> BuildingsProperty =
       AvaloniaProperty.Register<HexMapControl, Dictionary<int, BuildingModel>>(nameof(Buildings));

    public static readonly StyledProperty<Dictionary<int, RegionModel>> RegionsWithStructureProperty =
       AvaloniaProperty.Register<HexMapControl, Dictionary<int, RegionModel>>(nameof(RegionsWithStructure));

    public static readonly StyledProperty<IReadOnlyCollection<int>> ActiveFactionIdsProperty =
       AvaloniaProperty.Register<HexMapControl, IReadOnlyCollection<int>>(nameof(ActiveFactionIds));

    public static readonly StyledProperty<IReadOnlyCollection<int>> AlliedFactionIdsProperty =
       AvaloniaProperty.Register<HexMapControl, IReadOnlyCollection<int>>(nameof(AlliedFactionIds));

    public static readonly StyledProperty<ShipModel?> SelectedShipProperty =
       AvaloniaProperty.Register<HexMapControl, ShipModel?>(nameof(SelectedShip));

    public static readonly StyledProperty<double> HexSizeProperty =
        AvaloniaProperty.Register<HexMapControl, double>(nameof(HexSize), defaultHexSize);

    public static readonly StyledProperty<int> ZoomProperty =
        AvaloniaProperty.Register<HexMapControl, int>(nameof(Zoom), defaultZoom);

    public static readonly StyledProperty<RegionModel?> SelectedRegionProperty =
        AvaloniaProperty.Register<HexMapControl, RegionModel?>(nameof(SelectedRegion));

    public static readonly StyledProperty<bool> IsStandardPlaneProperty =
        AvaloniaProperty.Register<HexMapControl, bool>(nameof(IsStandardPlane), false);

    public Dictionary<int, RegionModel> OccupiedRegions
    {
        get => GetValue(OccupiedRegionsProperty) ?? new Dictionary<int, RegionModel>();
        set => SetValue(OccupiedRegionsProperty, value ?? new Dictionary<int, RegionModel>());
    }
    public Dictionary<int, RegionModel> UnoccupiedRegions
    {
        get => GetValue(UnoccupiedRegionsProperty) ?? new Dictionary<int, RegionModel>();
        set => SetValue(UnoccupiedRegionsProperty, value ?? new Dictionary<int, RegionModel>());
    }

    public Dictionary<int, ShipModel> Ships
    {
        get => GetValue(ShipsProperty) ?? new Dictionary<int, ShipModel>();
        set => SetValue(ShipsProperty, value ?? new Dictionary<int, ShipModel>());
    }

    public Dictionary<int, BuildingModel> Buildings
    {
        get => GetValue(BuildingsProperty) ?? new Dictionary<int, BuildingModel>();
        set => SetValue(BuildingsProperty, value ?? new Dictionary<int, BuildingModel>());
    }

    public Dictionary<int, RegionModel> RegionsWithStructure
    {
        get => GetValue(RegionsWithStructureProperty) ?? new Dictionary<int, RegionModel>();
        set => SetValue(RegionsWithStructureProperty, value ?? new Dictionary<int, RegionModel>());
    }

    // Ids of the factions the player can play (alliances may have several active factions).
    // Used to render castles/fortifications owned by the player differently.
    public IReadOnlyCollection<int> ActiveFactionIds
    {
        get => GetValue(ActiveFactionIdsProperty) ?? Array.Empty<int>();
        set => SetValue(ActiveFactionIdsProperty, value ?? Array.Empty<int>());
    }

    /// <summary>Ids of the factions allied to one of the active factions.</summary>
    public IReadOnlyCollection<int> AlliedFactionIds
    {
        get => GetValue(AlliedFactionIdsProperty) ?? Array.Empty<int>();
        set => SetValue(AlliedFactionIdsProperty, value ?? Array.Empty<int>());
    }

    /// <summary>Ship whose route must be emphasized, when one is selected in the explorer/details.</summary>
    public ShipModel? SelectedShip
    {
        get => GetValue(SelectedShipProperty);
        set => SetValue(SelectedShipProperty, value);
    }

    public Seasons Season
    {
        get => GetValue(SeasonProperty);
        set => SetValue(SeasonProperty, value);
    }

    public double HexSize
    {
        get => GetValue(HexSizeProperty);
        set => SetValue(HexSizeProperty, value);
    }

    public int Zoom
    {
        get => GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public bool IsStandardPlane
    {
        get => GetValue(IsStandardPlaneProperty);
        set => SetValue(IsStandardPlaneProperty, value);
    }

    public RegionModel? SelectedRegion
    {
        get => GetValue(SelectedRegionProperty);
        set => SetValue(SelectedRegionProperty, value);
    }

    static HexMapControl()
    {
        PreloadStandardTerrainImages();
        OccupiedRegionsProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnOccupiedRegionsChanged());
        UnoccupiedRegionsProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnUnoccupiedRegionsChanged());
        HexSizeProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnHexSizeChanged());
        ZoomProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnZoomChanged());
        SeasonProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnSeasonChanged());
        SelectedRegionProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnSelectedRegionChanged(e));
        AlliedFactionIdsProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.InvalidateVisual());
        SelectedShipProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.InvalidateVisual());
    }

    // Helper to find ancestor ScrollViewer
    private ScrollViewer? GetParentScrollViewer()
    {
        return this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
    }

    private bool _useSeasonImages;

    // Map building renderer settings (read from GlobalSettings).
    private bool _showBuildings = true;
    private int _buildingStyle = BuildingStyles.Auto;
    private int _buildingContrast = BuildingContrastModes.Plate;

    // Map ship renderer settings (read from GlobalSettings).
    private bool _showShips = true;
    private int _shipStyle = ShipStyles.Image;
    private int _shipRouteMode = ShipRouteModes.All;
    private int _shipRouteOpacity = 35;
    private int _selectedShipRouteOpacity = 70;

    // Map region-name settings (read from GlobalSettings).
    private bool _showRegionNames = true;
    private bool _regionNameBanner = false;

    public HexMapControl()
    {
        Season = Seasons.UNKNOWN;
        LoadMapSettings();
        OccupiedRegions = new Dictionary<int, RegionModel>();
        UnoccupiedRegions = new Dictionary<int, RegionModel>();
        UpdateTerrainImagesFromSeason();
        this.PointerPressed += OnPointerPressed;
        this.PointerReleased += OnPointerReleased;
        this.PointerMoved += OnPointerMoved;
        this.PointerExited += OnPointerExited;

        // Set a short default tooltip show delay (optional)
        ToolTip.SetShowDelay(this, 500);
        
        // Load directional wake image (add a PNG at Assets/map/effects/boat_wake.png)
        //_boatWakeBitmap = LoadBitmap("/Assets/map/effects/boat_wake.png", -1, false);
    }

    // True while the control listens to GlobalSettings.SettingChanged.
    private bool _listeningToSettings;

    /// <summary>
    /// Reads the map-related settings (building renderer and season images).
    /// Called at construction, when the control is attached to the visual tree and whenever one
    /// of these settings changes.
    /// </summary>
    private void LoadMapSettings()
    {
        _useSeasonImages = GlobalSettings.Get<bool>(GlobalSettings.MAP_USE_SEASON_IMAGES);
        _showBuildings = GlobalSettings.Get<bool>(GlobalSettings.MAP_SHOW_BUILDINGS);
        _buildingStyle = GlobalSettings.Get<int>(GlobalSettings.MAP_BUILDING_STYLE);
        _buildingContrast = GlobalSettings.Get<int>(GlobalSettings.MAP_BUILDING_CONTRAST);
        _showShips = GlobalSettings.Get<bool>(GlobalSettings.MAP_SHOW_SHIPS);
        _shipStyle = GlobalSettings.Get<int>(GlobalSettings.MAP_SHIP_STYLE);
        _shipRouteMode = GlobalSettings.Get<int>(GlobalSettings.MAP_SHIP_ROUTE_MODE);
        _shipRouteOpacity = GlobalSettings.Get<int>(GlobalSettings.MAP_SHIP_ROUTE_OPACITY);
        _selectedShipRouteOpacity = GlobalSettings.Get<int>(GlobalSettings.MAP_SELECTED_SHIP_ROUTE_OPACITY);
        _showRegionNames = GlobalSettings.Get<bool>(GlobalSettings.MAP_SHOW_REGION_NAMES);
        _regionNameBanner = GlobalSettings.Get<bool>(GlobalSettings.MAP_REGION_NAME_BANNER);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        LoadMapSettings();

        if (!_listeningToSettings)
        {
            GlobalSettings.Shared.SettingChanged += OnGlobalSettingChanged;
            _listeningToSettings = true;
        }

        InvalidateVisual();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_listeningToSettings)
        {
            GlobalSettings.Shared.SettingChanged -= OnGlobalSettingChanged;
            _listeningToSettings = false;
        }

        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Applies map settings immediately when they are changed in the configuration dialog.
    /// </summary>
    private void OnGlobalSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnGlobalSettingChanged(sender, e));
            return;
        }

        switch (e.SettingName)
        {
            case GlobalSettings.MAP_SHOW_BUILDINGS:
            case GlobalSettings.MAP_BUILDING_STYLE:
            case GlobalSettings.MAP_BUILDING_CONTRAST:
            case GlobalSettings.MAP_SHOW_SHIPS:
            case GlobalSettings.MAP_SHIP_STYLE:
            case GlobalSettings.MAP_SHIP_ROUTE_MODE:
            case GlobalSettings.MAP_SHIP_ROUTE_OPACITY:
            case GlobalSettings.MAP_SELECTED_SHIP_ROUTE_OPACITY:
            case GlobalSettings.MAP_SHOW_REGION_NAMES:
            case GlobalSettings.MAP_REGION_NAME_BANNER:
                LoadMapSettings();
                InvalidateVisual();
                break;

            case GlobalSettings.MAP_USE_SEASON_IMAGES:
                LoadMapSettings();
                UpdateTerrainImagesFromSeason();
                InvalidateVisual();
                break;
        }
    }

    public Vector? CenterToRegionCoordinates(int q, int r, Vector viewportOffset, Size viewport, Size viewportExtent, bool evenIfAlreadyVisible)
    {
        // Calculate the pixel position of the region coordinates (q, r)
        var hexaCenter = HexToPixel(q, r, HexSize * Zoom / 100.0);

        // The visible area in content coordinates
        var viewLeft = viewportOffset.X;
        var viewTop = viewportOffset.Y;
        var viewRight = viewLeft + viewport.Width;
        var viewBottom = viewTop + viewport.Height;

        if (!evenIfAlreadyVisible)
        {
            // Optionally, add a margin so the region isn't right at the edge
            double margin = HexSize * Zoom / 100.0 * 0.5;

            // Check if the center is already visible (with margin)
            if (hexaCenter.X >= viewLeft + margin && hexaCenter.X <= viewRight - margin &&
                hexaCenter.Y >= viewTop + margin && hexaCenter.Y <= viewBottom - margin)
            {
                // Already visible, do nothing
                return null;
            }
        }

        // Center the region in the ScrollViewer
        double targetX = hexaCenter.X - viewport.Width / 2;
        double targetY = hexaCenter.Y - viewport.Height / 2;

        // Clamp to valid scroll range
        targetX = Math.Max(0, Math.Min(targetX, viewportExtent.Width - viewport.Width));
        targetY = Math.Max(0, Math.Min(targetY, viewportExtent.Height - viewport.Height));

        return new Vector(targetX, targetY);
    }

    private void OnSeasonChanged()
    {
        UpdateTerrainImagesFromSeason();
    }

    private void UpdateMap()
    {
        ComputeRegionBounds();
        InvalidateMeasure();
        InvalidateVisual();
    }

    private void OnHexSizeChanged()
    {
        UpdateMap();
    }

    private void OnZoomChanged()
    {
        UpdateMap();
    }

    private void OnOccupiedRegionsChanged()
    {
        UpdateMap();
    }

    private void OnUnoccupiedRegionsChanged()
    {
        UpdateMap();
    }

    //private Rect regionsBound;

    // Bound values in the CR coordinates system
    private double _minX, _maxX, _minY, _maxY;
    private bool _hasBounds = false;
    private bool _shouldCenterNewSelection = true;

    private void ComputeRegionBounds()
    {

        _hasBounds = false;
        /**/
        // TODO: compute that before in CRDocument and store in a property (in CR coordinates system) to avoid computing it every time
        /*
        int minX = 0;
        int minY = 0;
        int maxX = 0;
        int maxY = 0;
        */

        if (OccupiedRegions.Count == 0 && UnoccupiedRegions.Count == 0)
        {
            _minX = _maxX = _minY = _maxY = 0;
            return;
        }

        double hexSize = HexSize * Zoom / 100.0;

        foreach (var kvp in OccupiedRegions)
        {
            var region = kvp.Value;
            int q = region.GetX();
            int r = region.GetY();
            int invertedR = -r;
            // TODO: remove after checked it's same result
            var pt = HexToPixelUncentered(q, r, hexSize);

            if (!_hasBounds)
            {
                /*
                minX = maxX = q;
                minY = maxY = r;
                */

                _minX = _maxX = pt.X;
                _minY = _maxY = pt.Y;

                _hasBounds = true;
            }
            else
            {
                _minX = Math.Min(_minX, pt.X);
                _maxX = Math.Max(_maxX, pt.X);
                _minY = Math.Min(_minY, pt.Y);
                _maxY = Math.Max(_maxY, pt.Y);

                /*
                minX = Math.Min(minX, q);
                maxX = Math.Max(maxX, invertedR);
                minY = Math.Min(minY, q);
                maxY = Math.Max(maxY, invertedR);
                */
            }
        }


        foreach (var kvp in UnoccupiedRegions)
        {
            var region = kvp.Value;
            int q = region.GetX();
            int r = region.GetY();
            int invertedR = -r;
            // TODO: remove after checked it's same result
            var pt = HexToPixelUncentered(q, r, hexSize);

            if (!_hasBounds)
            {
                /*
                minX = maxX = q;
                minY = maxY = r;
                */

                _minX = _maxX = pt.X;
                _minY = _maxY = pt.Y;

                _hasBounds = true;
            }
            else
            {
                _minX = Math.Min(_minX, pt.X);
                _maxX = Math.Max(_maxX, pt.X);
                _minY = Math.Min(_minY, pt.Y);
                _maxY = Math.Max(_maxY, pt.Y);

                /*
                minX = Math.Min(minX, q);
                maxX = Math.Max(maxX, invertedR);
                minY = Math.Min(minY, q);
                maxY = Math.Max(maxY, invertedR);
                */
            }
        }

        /*
        var ptMin = HexToPixelUncentered(minX, minY, hexSize);
        var ptMax = HexToPixelUncentered(maxX, maxY, hexSize);
        _minX = ptMin.X;
        _maxX = ptMax.X;
        _minY = ptMin.Y;
        _maxY = ptMax.Y;*
        */

    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property.Name != "IsPointerOver")
        {
            Debug.WriteLine($"[HEXMAP-CTRL] OnPropertyChanged {change.Property.Name} > {change.NewValue}");
        }
        base.OnPropertyChanged(change);
    }

    public override void Render(DrawingContext context)
    {
        // TODO:
        // - show island names according to option
        Debug.WriteLine("[HEXMAP-CTRL] Rendering...");
        Debug.WriteLine($"[HEXMAP-CTRL] HexSize={HexSize} Zoom={Zoom} regions: populated={OccupiedRegions.Count} empty={UnoccupiedRegions.Count}");

        base.Render(context);

        if (IsStandardPlane)
        {
            RenderStandardPlane(context);
        }
        else
        {
            RenderSpecialPlane(context);
        }

        // Region names are a dedicated final pass so they always stay above ships, routes and
        // every other overlay, regardless of the render order inside the plane renderers.
        RenderRegionNames(context);
    }

    /// <summary>
    /// Draws region names after every other map layer. This guarantees that a name (and its
    /// optional banner) is never hidden by a ship, a route or a selection highlight.
    /// </summary>
    private void RenderRegionNames(DrawingContext context)
    {
        if (!_showRegionNames)
        {
            return;
        }

        double hexSize = HexSize * Zoom / 100.0;
        if (hexSize <= 0)
        {
            return;
        }

        var drawn = new HashSet<int>();
        DrawRegionNamesFrom(context, OccupiedRegions, hexSize, drawn);
        DrawRegionNamesFrom(context, UnoccupiedRegions, hexSize, drawn);
    }

    private void DrawRegionNamesFrom(
        DrawingContext context,
        Dictionary<int, RegionModel> regions,
        double hexSize,
        HashSet<int> drawn)
    {
        foreach (var kvp in regions)
        {
            RegionModel regionModel = kvp.Value;
            if (!drawn.Add(kvp.Key) || !Terrains.CanBeNamed(regionModel.Terrain))
            {
                continue;
            }

            string name = GetRegionDisplayName(regionModel);
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            Point center = HexToPixel(regionModel.GetX(), regionModel.GetY(), hexSize);
            DrawRegionName(context, regionModel.Terrain, name, center, hexSize);
        }
    }

    private static string GetRegionDisplayName(RegionModel regionModel)
    {
        string name = regionModel.Name;
        if (!string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (regionModel.Region is { } region)
        {
            name = region.Value(KeyType.NAME);
            if (string.IsNullOrEmpty(name))
            {
                name = region.Value(KeyType.LOWERCASE_NAME);
            }
        }

        return name ?? string.Empty;
    }

    private void DrawRegionName(
        DrawingContext context,
        int terrain,
        string name,
        Point center,
        double hexSize)
    {
        try
        {
            // Font size and width are relative to the hex size, so names scale with the map zoom.
            double fontSize = Math.Max(8, hexSize * 0.33);
            double maxTextWidth = Math.Max(10, hexSize * Math.Sqrt(3) * 0.95);
            TextDecorationCollection? textDecorations = null;
            var typeface = new Typeface("Segoe UI", FontStyle.Normal, FontWeight.SemiBold);

            // On a gold banner, dark text is far more readable than the terrain-dependent colour.
            var labelBrush = _regionNameBanner
                ? new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A))
                : GetLabelBrushForTerrain(terrain);
            var shadowBrush = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0));

            using (var layout = new TextLayout(
                name,
                typeface,
                fontSize,
                labelBrush,
                TextAlignment.Left,
                TextWrapping.NoWrap,
                TextTrimming.CharacterEllipsis,
                textDecorations,
                FlowDirection.LeftToRight,
                maxTextWidth))
            using (var shadowLayout = new TextLayout(
                name,
                typeface,
                fontSize,
                shadowBrush,
                TextAlignment.Left,
                TextWrapping.NoWrap,
                TextTrimming.CharacterEllipsis,
                textDecorations,
                FlowDirection.LeftToRight,
                maxTextWidth))
            {
                double measuredWidth = layout.WidthIncludingTrailingWhitespace > 0
                    ? layout.WidthIncludingTrailingWhitespace
                    : layout.Width;
                measuredWidth = Math.Min(measuredWidth, maxTextWidth);
                double measuredHeight = layout.Height;

                // Keep the existing vertical placement so enabling the banner does not move names.
                double drop = fontSize * 0.9;
                double raise = Math.Max(0.0, fontSize * 0.3);
                var origin = new Point(
                    center.X - measuredWidth / 2.0,
                    center.Y - measuredHeight / 2.0 + drop - raise);

                if (_regionNameBanner)
                {
                    double padX = Math.Max(4.0, fontSize * 0.50);
                    double padY = Math.Max(2.0, fontSize * 0.28);
                    var bannerRect = new Rect(
                        origin.X - padX,
                        origin.Y - padY,
                        measuredWidth + padX * 2.0,
                        measuredHeight + padY * 2.0);

                    var bannerBrush = new SolidColorBrush(Color.FromRgb(0xF2, 0xC2, 0x3A));
                    var borderPen = new Pen(
                        new SolidColorBrush(Color.FromArgb(255, 0x24, 0x24, 0x24)),
                        Math.Max(1.5, fontSize * 0.12));

                    context.DrawRectangle(bannerBrush, borderPen, bannerRect, 3.0, 3.0);
                    layout.Draw(context, origin);
                }
                else
                {
                    double shadowOffset = Math.Max(1.0, fontSize * 0.07);
                    shadowLayout.Draw(context, new Point(origin.X + shadowOffset, origin.Y + shadowOffset));
                    layout.Draw(context, origin);
                }
            }
        }
        catch
        {
            // Defensive: don't crash rendering on text layout errors.
        }
    }

    private void RenderUnoccupiedRegions(DrawingContext context)
    {
        // TODO:
        // - show island names according to option
        Debug.WriteLine("[HEXMAP-CTRL] Rendering empty regions...");

        double hexSize = HexSize * Zoom / 100.0;
        foreach (var kvp in UnoccupiedRegions)
        {
            var regionModel = kvp.Value;
            int x = regionModel.GetX();
            int y = regionModel.GetY();
            var flags = regionModel.Flags;
            var center = HexToPixel(x, y, hexSize);
            var hexPoints = GetHexPoints(center, hexSize);

            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(hexPoints[0], true);
                for (int i = 1; i < hexPoints.Length; i++)
                    gc.LineTo(hexPoints[i]);
                gc.LineTo(hexPoints[0]);
                gc.EndFigure(true);
            }

            // --- Fill with terrain image ---
            var terrain = regionModel.Terrain;
            var image = GetTerrainImage(terrain);

            if (image != null)
            {
                var bounds = geometry.Bounds;
                // Draw the image stretched to the hex's bounding box
                context.DrawImage(image, new Rect(0, 0, image.Size.Width, image.Size.Height), bounds);

                // If region is unseen, overlay a semi-transparent black to darken
                if (regionModel.IsVisible(out Flag visibilityFlag))
                {
                    // Lighten oceans where someone traveled
                    //if (terrain == Terrains.OCEAN && region.IsWithPeople())
                    if (visibilityFlag == Flag.LIGHTHOUSE)
                    {
                        if (terrain != Terrains.OCEAN)
                        {
                            // Should not happen, but just in case
                        }
                        // Overlay a semi-transparent blue to lighten the ocean
                        context.DrawGeometry(
                            // something from 50 to 80 seems good for first argb parameter
                            new SolidColorBrush(Color.FromArgb(50, 100, 255, 100)), // light blue, ~31% opacity
                            null,
                            geometry
                        );
                    }
                    /*else if (visibilityFlag == Flag.TRAVEL)
                    {
                        if (terrain != Terrains.OCEAN)
                        {
                            // Should not happen, but just in case
                            continue;
                        }
                        // Overlay a semi-transparent blue to lighten the ocean
                        context.DrawGeometry(
                            new SolidColorBrush(Color.FromArgb(100, 100, 180, 255)), // light blue, ~39% opacity
                            null,
                            geometry
                        );
                    }*/
                    // TODO: lighten oceans where a lighthouse can watch
                }
                else
                {
                    // 120 as first argb parameter is a ~47% opacity black
                    context.DrawGeometry(
                        new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                        null,
                        geometry
                    );
                }
                // Optionally, overlay a semi-transparent fill for effect
                // context.DrawGeometry(new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)), null, geometry);
            }
            else
            {
                // LATER: add a warning in console. Should not happen
                //context.DrawGeometry(Brushes.LightGray, null, geometry);
                Debug.WriteLine($"[HEXMAP-CTRL] unknown terrain {terrain} for {regionModel}...");
            }

            RenderBuildingsOnRegion(context, regionModel, center, hexSize);

            // Persisting boat wake: draw for ocean regions flagged with SHIPTRAVEL
            // Keep this simple (no animation) � uses the existing _boatWakeBitmap asset.
            // If you later add direction data you can rotate/draw per-boat similarly to DrawBoatWakes.
            try
            {
                if (terrain == Terrains.OCEAN)
                {
                    if (((flags) & (int)Flag.SHIPTRAVEL) != 0 && _boatWakeBitmap != null)
                    {
                        // Draw boat wake
                        /*
                        // size roughly half the hex bounding width (tweak if needed)
                        double wakeSize = Math.Max(4, hexSize * 0.9);
                        var dest = new Rect(center.X - wakeSize / 2, center.Y - wakeSize / 2, wakeSize, wakeSize);

                        // Draw with a modest opacity so it blends with ocean
                        using (context.PushOpacity(0.9))
                        {
                            // No rotation available from region data here; future: rotate if direction is known
                            context.DrawImage(_boatWakeBitmap, new Rect(0, 0, _boatWakeBitmap.Size.Width, _boatWakeBitmap.Size.Height), dest);
                        }
                        */
                    }
                }
            }
            catch
            {
                // Defensive: don't crash rendering if Flag enum/value isn't available or cast fails.
            }


            HighlightSelectedRegion(context, center, hexPoints, x, y, geometry);
        }

        // TODO: draw monsters, storms, fog of war...
    }

    private void RenderOccupiedRegions(DrawingContext context)
    {
        // TODO:
        // - show island names according to option
        Debug.WriteLine("[HEXMAP-CTRL] Rendering standard plane...");

        double hexSize = HexSize * Zoom / 100.0;
        foreach (var kvp in OccupiedRegions)
        {
            var regionModel = kvp.Value;
            var flags = regionModel.Flags;
            var x = regionModel.GetX();
            var y = regionModel.GetY();

            var center = HexToPixel(x, y, hexSize);
            var hexPoints = GetHexPoints(center, hexSize);

            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(hexPoints[0], true);
                for (int i = 1; i < hexPoints.Length; i++)
                    gc.LineTo(hexPoints[i]);
                gc.LineTo(hexPoints[0]);
                gc.EndFigure(true);
            }

            // --- Fill with terrain image ---
            var terrain = regionModel.Terrain;
            var image = GetTerrainImage(terrain);

            if (image != null)
            {
                // TODO: handle Flag.SHIPTRAVEL means a boat traveled through this region (ocean)
                var bounds = geometry.Bounds;
                // Draw the image stretched to the hex's bounding box
                context.DrawImage(image, new Rect(0, 0, image.Size.Width, image.Size.Height), bounds);

                // If region is unseen, overlay a semi-transparent black to darken
                if (regionModel.IsVisible(out Flag visibilityFlag))
                {
                    // Lighten oceans where someone traveled
                    //if (terrain == Terrains.OCEAN && region.IsWithPeople())
                    if (visibilityFlag == Flag.LIGHTHOUSE)
                    {
                        if (terrain != Terrains.OCEAN)
                        {
                            // Should not happen, but just in case
                        }
                        // Overlay a semi-transparent blue to lighten the ocean
                        context.DrawGeometry(
                            // something from 50 to 80 seems good for first argb parameter
                            new SolidColorBrush(Color.FromArgb(50, 100, 255, 100)), // light blue, ~31% opacity
                            null,
                            geometry
                        );
                    }
                    /*else if (visibilityFlag == Flag.TRAVEL)
                    {
                        if (terrain != Terrains.OCEAN)
                        {
                            // Should not happen, but just in case
                            continue;
                        }
                        // Overlay a semi-transparent blue to lighten the ocean
                        context.DrawGeometry(
                            new SolidColorBrush(Color.FromArgb(100, 100, 180, 255)), // light blue, ~39% opacity
                            null,
                            geometry
                        );
                    }*/
                    // TODO: lighten oceans where a lighthouse can watch
                }
                else
                {
                    // 120 as first argb parameter is a ~47% opacity black
                    context.DrawGeometry(
                        new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                        null,
                        geometry
                    );
                }
                // Optionally, overlay a semi-transparent fill for effect
                // context.DrawGeometry(new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)), null, geometry);
            }
            else
            {
                // LATER: add a warning in console. Should not happen
                //context.DrawGeometry(Brushes.LightGray, null, geometry);
                Debug.WriteLine($"[HEXMAP-CTRL] unknown terrain {terrain} for {regionModel}...");
            }

            RenderBuildingsOnRegion(context, regionModel, center, hexSize);

            // Persisting boat wake: draw for ocean regions flagged with SHIPTRAVEL
            // Keep this simple (no animation) � uses the existing _boatWakeBitmap asset.
            // If you later add direction data you can rotate/draw per-boat similarly to DrawBoatWakes.
            try
            {
                if (terrain == Terrains.OCEAN)
                {
                    if (((flags) & (int)Flag.SHIPTRAVEL) != 0 && _boatWakeBitmap != null)
                    {
                        // Draw boat wake
                        /*
                        // size roughly half the hex bounding width (tweak if needed)
                        double wakeSize = Math.Max(4, hexSize * 0.9);
                        var dest = new Rect(center.X - wakeSize / 2, center.Y - wakeSize / 2, wakeSize, wakeSize);

                        // Draw with a modest opacity so it blends with ocean
                        using (context.PushOpacity(0.9))
                        {
                            // No rotation available from region data here; future: rotate if direction is known
                            context.DrawImage(_boatWakeBitmap, new Rect(0, 0, _boatWakeBitmap.Size.Width, _boatWakeBitmap.Size.Height), dest);
                        }
                        */
                    }
                }
            }
            catch
            {
                // Defensive: don't crash rendering if Flag enum/value isn't available or cast fails.
            }


            HighlightSelectedRegion(context, center, hexPoints, x, y, geometry);
        }

        // TODO: draw monsters, storms, fog of war...
    }

    private void RenderStandardPlane(DrawingContext context)
    {
        Debug.WriteLine("[HEXMAP-CTRL] Rendering standard plane...");
        RenderOccupiedRegions(context);
        RenderUnoccupiedRegions(context);
        RenderMovements(context);
        RenderAllShips(context);
    }

    private void RenderUnoccupiedRegionsInSpecialPlane(DrawingContext context)
    {
        Debug.WriteLine("[HEXMAP-CTRL] Rendering empty regions in special plane...");

        double hexSize = HexSize * Zoom / 100.0;
        foreach (var kvp in UnoccupiedRegions)
        {
            var regionModel = kvp.Value;
            var region = regionModel.Region;
            var flags = region!.GetFlags();

            var x = region.GetX();
            var y = region.GetY();

            var center = HexToPixel(x, y, hexSize);
            var hexPoints = GetHexPoints(center, hexSize);

            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(hexPoints[0], true);
                for (int i = 1; i < hexPoints.Length; i++)
                    gc.LineTo(hexPoints[i]);
                gc.LineTo(hexPoints[0]);
                gc.EndFigure(true);
            }

            // --- Fill with terrain image ---
            var terrain = region.GetTerrain();
            var image = GetTerrainImage(terrain);

            if (image != null)
            {
                var bounds = geometry.Bounds;
                // Draw the image stretched to the hex's bounding box
                context.DrawImage(image, new Rect(0, 0, image.Size.Width, image.Size.Height), bounds);

                // If region is unseen, overlay a semi-transparent black to darken
                if (region.IsVisible(out Flag visibilityFlag))
                {
                    /*else if (visibilityFlag == Flag.TRAVEL)
                    {
                        if (terrain != Terrains.OCEAN)
                        {
                            // Should not happen, but just in case
                            continue;
                        }
                        // Overlay a semi-transparent blue to lighten the ocean
                        context.DrawGeometry(
                            new SolidColorBrush(Color.FromArgb(100, 100, 180, 255)), // light blue, ~39% opacity
                            null,
                            geometry
                        );
                    }*/
                    // TODO: lighten oceans where a lighthouse can watch
                }
                else
                {
                    // 120 as first argb parameter is a ~47% opacity black
                    context.DrawGeometry(
                        new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                        null,
                        geometry
                    );
                }
                // Optionally, overlay a semi-transparent fill for effect
                // context.DrawGeometry(new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)), null, geometry);
            }
            else
            {
                // LATER: add a warning in console. Should not happen
                //context.DrawGeometry(Brushes.LightGray, null, geometry);
                Debug.WriteLine($"[HEXMAP-CTRL] unknown astral terrain {terrain} for {region}...");
            }

            RenderBuildingsOnRegion(context, regionModel, center, hexSize);

            HighlightSelectedRegion(context, center, hexPoints, x, y, geometry);
        }

    }

    private void RenderOccupiedRegionsInSpecialPlane(DrawingContext context)
    {
        Debug.WriteLine("[HEXMAP-CTRL] Rendering populated regions in special plane...");

        double hexSize = HexSize * Zoom / 100.0;
        foreach (var kvp in OccupiedRegions)
        {
            var regionModel = kvp.Value;
            var region = regionModel.Region;
            var flags = region!.GetFlags();

            var x = region.GetX();
            var y = region.GetY();

            var center = HexToPixel(x, y, hexSize);
            var hexPoints = GetHexPoints(center, hexSize);

            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(hexPoints[0], true);
                for (int i = 1; i < hexPoints.Length; i++)
                    gc.LineTo(hexPoints[i]);
                gc.LineTo(hexPoints[0]);
                gc.EndFigure(true);
            }

            // --- Fill with terrain image ---
            var terrain = region.GetTerrain();
            var image = GetTerrainImage(terrain);

            if (image != null)
            {
                var bounds = geometry.Bounds;
                // Draw the image stretched to the hex's bounding box
                context.DrawImage(image, new Rect(0, 0, image.Size.Width, image.Size.Height), bounds);

                // If region is unseen, overlay a semi-transparent black to darken
                if (region.IsVisible(out Flag visibilityFlag))
                {
                    /*else if (visibilityFlag == Flag.TRAVEL)
                    {
                        if (terrain != Terrains.OCEAN)
                        {
                            // Should not happen, but just in case
                            continue;
                        }
                        // Overlay a semi-transparent blue to lighten the ocean
                        context.DrawGeometry(
                            new SolidColorBrush(Color.FromArgb(100, 100, 180, 255)), // light blue, ~39% opacity
                            null,
                            geometry
                        );
                    }*/
                    // TODO: lighten oceans where a lighthouse can watch
                }
                else
                {
                    // 120 as first argb parameter is a ~47% opacity black
                    context.DrawGeometry(
                        new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                        null,
                        geometry
                    );
                }
                // Optionally, overlay a semi-transparent fill for effect
                // context.DrawGeometry(new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)), null, geometry);
            }
            else
            {
                // LATER: add a warning in console. Should not happen
                //context.DrawGeometry(Brushes.LightGray, null, geometry);
                Debug.WriteLine($"[HEXMAP-CTRL] unknown astral terrain {terrain} for {region}...");
            }

            RenderBuildingsOnRegion(context, regionModel, center, hexSize);

            HighlightSelectedRegion(context, center, hexPoints, x, y, geometry);
        }

    }


    private void RenderSpecialPlane(DrawingContext context)
    {
        Debug.WriteLine("[HEXMAP-CTRL] Rendering special plane...");
        RenderOccupiedRegionsInSpecialPlane(context);
        RenderUnoccupiedRegionsInSpecialPlane(context);
        RenderMovements(context);
        RenderAllShips(context);
    }

    private void RenderMovements(DrawingContext context)
    {
        if (IsStandardPlane)
        {
            RenderShipMovements(context);
        }

        RenderByFootMovements(context);
        RenderByHorseMovements(context);
    }

    private void RenderByFootMovements(DrawingContext context)
    {
    }

    private void RenderByHorseMovements(DrawingContext context)
    {
    }

    private sealed class ShipRouteDrawing
    {
        public ShipModel Ship { get; init; } = null!;
        public ShipVisual Visual { get; init; } = null!;
        public ShipFactionRelation Relation { get; init; }
        public int Coast { get; init; }
        public List<RegionDataBlock> Regions { get; } = [];
        public List<Point> Centers { get; } = [];
        public List<ShipRouteSegmentKey> SegmentKeys { get; } = [];
        public List<bool> ReversedSegments { get; } = [];
        public double[] SegmentLanes { get; set; } = [];
        public List<Point> Anchors { get; set; } = [];
        public bool IsSelected { get; init; }
    }

    private readonly record struct ShipRouteSegmentKey(int Plane, int AX, int AY, int BX, int BY);

    private readonly record struct RouteDestinationRing(Point Center, double Radius, bool IsOcean);

    private void RenderShipMovements(DrawingContext context)
    {
        if (_shipRouteMode == ShipRouteModes.None)
        {
            return;
        }

        double hexSize = HexSize * Zoom / 100.0;
        if (hexSize <= 0)
        {
            return;
        }

        List<ShipRouteDrawing> drawings = BuildShipRouteDrawings(hexSize);
        if (drawings.Count == 0)
        {
            return;
        }

        double laneSpacing = AssignRouteLanes(drawings, hexSize);
        // A zero or missing setting must not make routes invisible: fall back to the defaults.
        int routeOpacityPercent = _shipRouteOpacity > 0 ? _shipRouteOpacity : 35;
        int selectedRouteOpacityPercent = _selectedShipRouteOpacity > 0 ? _selectedShipRouteOpacity : 70;
        double routeOpacity = Math.Clamp(routeOpacityPercent, 10, 80) / 100.0;
        double selectedRouteOpacity = Math.Clamp(selectedRouteOpacityPercent, 10, 100) / 100.0;

        var destinations = new Dictionary<ShipRouteDrawing, RouteDestinationRing>(drawings.Count);
        foreach (ShipRouteDrawing drawing in drawings)
        {
            destinations[drawing] = GetRouteDestinationRing(drawing, hexSize);
        }

        // Draw normal routes first and the selected one last, so it stays readable above the others.
        foreach (ShipRouteDrawing drawing in drawings.OrderBy(d => d.IsSelected ? 1 : 0))
        {
            drawing.Anchors = ShipRouteRenderer.BuildParallelAnchors(
                drawing.Centers,
                drawing.SegmentLanes,
                drawing.ReversedSegments,
                laneSpacing);

            // Ocean departures start at the hex centre. Land departures start at the shared
            // edge, so the route does not cross the inside of a land region.
            if (drawing.Anchors.Count > 1)
            {
                if (IsOceanRegion(drawing.Regions[0]))
                {
                    drawing.Anchors[0] = drawing.Centers[0];
                }
                else
                {
                    drawing.Anchors.RemoveAt(0);
                }
            }

            RouteDestinationRing destination = destinations[drawing];

            // Ocean arrivals may use the region centre. A non-ocean arrival stops at the
            // shared-edge midpoint, exactly like a non-ocean departure: the route must never
            // run into the centre of a land region.
            if (!destination.IsOcean && drawing.Anchors.Count > 1)
            {
                drawing.Anchors.RemoveAt(drawing.Anchors.Count - 1);
            }

            // Stop the curve on the destination ring instead of drawing a leader through it.
            ShipRouteRenderer.TrimEndToCircle(drawing.Anchors, destination.Center, destination.Radius);
            if (drawing.Anchors.Count < 2)
            {
                continue;
            }

            // Very short routes (two adjacent regions) can be entirely covered by the arrival
            // ring. Drawing them would put a line through the ship, so leave only the ring.
            if (drawing.Anchors.All(anchor =>
                    IsInsideCircle(anchor, destination.Center, destination.Radius)))
            {
                continue;
            }

            var geometry = ShipRouteRenderer.BuildSmoothGeometry(drawing.Anchors);
            Color color = GetRouteColor(drawing);
            double thickness = Math.Max(
                drawing.IsSelected ? 4.8 : 2.4,
                hexSize * (drawing.IsSelected ? 0.120 : 0.060));

            // Completed routes are historical: draw the whole route semi-transparent so future
            // ordered routes (MOVE/ROUTE) can be layered on top with a stronger style. The
            // selected ship's route uses its own opacity so it stays clearly readable.
            double opacity = drawing.IsSelected ? selectedRouteOpacity : routeOpacity;
            using (context.PushOpacity(opacity))
            {
                // The spline is smooth but can overshoot a sharp corner. Clip it to the route's
                // navigable corridor so it can never visibly cross a land hex.
                using (context.PushGeometryClip(BuildRouteCorridorGeometry(drawing, hexSize)))
                {
                    ShipRouteRenderer.DrawRoute(context, geometry, color, thickness, drawing.IsSelected);
                    DrawRouteDirectionArrows(context, drawing.Anchors, color, hexSize, drawing.IsSelected);
                }
            }

            // The departure cross is a marker, not part of the route line. Keep it fully opaque
            // and outside the corridor clip so its tips are never cut off or faded.
            DrawRouteStart(context, drawing.Anchors[0], color, hexSize);
        }

        // Draw exactly one destination ring per region/coast. If any route in the group is
        // selected, the group uses the selected style (selected routes are sorted first here).
        foreach (ShipRouteDrawing drawing in drawings
                     .GroupBy(GetRouteDestinationKey)
                     .Select(group => group
                         .OrderByDescending(d => d.IsSelected)
                         .ThenBy(d => d.Ship.Structure.GetId())
                         .First()))
        {
            DrawRouteDestination(context, drawing, destinations[drawing]);
        }
    }

    private static Color GetRouteColor(ShipRouteDrawing drawing)
    {
        Color baseColor = ShipRouteRenderer.GetRelationColor(drawing.Relation);
        return drawing.IsSelected
            ? ShipRouteRenderer.GetSelectedRouteColor(baseColor)
            : baseColor;
    }

    private static (int RegionId, int Coast) GetRouteDestinationKey(ShipRouteDrawing drawing)
    {
        RegionDataBlock destination = drawing.Regions[^1];
        int regionId = Coordinates.GetId(
            destination.GetX(),
            destination.GetY(),
            (PlaneType)destination.GetPlane());
        int coast = drawing.Coast is >= 0 and <= 5 ? drawing.Coast : -1;
        return (regionId, coast);
    }

    private List<ShipRouteDrawing> BuildShipRouteDrawings(double hexSize)
    {
        var drawings = new List<ShipRouteDrawing>();
        var visitedShips = new HashSet<int>();

        foreach (var kvp in OccupiedRegions)
        {
            AddShipRoutesFromRegion(kvp.Value, hexSize, drawings, visitedShips);
        }

        foreach (var kvp in UnoccupiedRegions)
        {
            AddShipRoutesFromRegion(kvp.Value, hexSize, drawings, visitedShips);
        }

        return drawings;
    }

    private void AddShipRoutesFromRegion(
        RegionModel regionModel,
        double hexSize,
        List<ShipRouteDrawing> drawings,
        HashSet<int> visitedShips)
    {
        foreach (ShipModel ship in regionModel.Ships)
        {
            int shipId = ship.Structure.GetId();
            if (!visitedShips.Add(shipId) || !ShouldShowShipRoute(ship))
            {
                continue;
            }

            List<RegionDataBlock> regions = GetRouteRegions(ship);
            if (regions.Count < 2)
            {
                continue;
            }

            int coast = GetShipCoast(ship);
            var drawing = new ShipRouteDrawing
            {
                Ship = ship,

                // Ships sharing a coast are represented by one image, chosen by size. The ring
                // has to use that same image anchor, otherwise it would be offset from the sprite.
                Visual = GetRepresentativeShipVisual(regionModel, ship, coast),
                Relation = GetShipRelation(ship),
                Coast = coast,
                IsSelected = IsSelectedShip(ship),
            };

            foreach (RegionDataBlock region in regions)
            {
                drawing.Regions.Add(region);
                drawing.Centers.Add(HexToPixel(region.GetX(), region.GetY(), hexSize));
            }

            for (int i = 0; i < regions.Count - 1; i++)
            {
                RegionDataBlock a = regions[i];
                RegionDataBlock b = regions[i + 1];
                drawing.SegmentKeys.Add(GetRouteSegmentKey(a, b));
                drawing.ReversedSegments.Add(IsReversedRouteSegment(a, b));
            }

            drawing.SegmentLanes = new double[drawing.SegmentKeys.Count];
            drawings.Add(drawing);
        }
    }

    private bool ShouldShowShipRoute(ShipModel ship)
    {
        if (_shipRouteMode == ShipRouteModes.None)
        {
            return false;
        }

        // A selected ship must always keep its route visible, even in "selected region" mode
        // when the route does not pass through the selected region (for example when the ship
        // selection and the region selection come from different views).
        if (IsSelectedShip(ship))
        {
            return true;
        }

        return _shipRouteMode switch
        {
            ShipRouteModes.All => true,
            ShipRouteModes.SelectedShip => false,
            ShipRouteModes.SelectedRegion => RouteContainsSelectedRegion(ship),
            _ => false,
        };
    }

    private bool IsSelectedShip(ShipModel ship)
    {
        if (SelectedShip is null)
        {
            return false;
        }

        return ReferenceEquals(ship, SelectedShip) ||
               ship.Structure.GetId() == SelectedShip.Structure.GetId();
    }

    private bool RouteContainsSelectedRegion(ShipModel ship)
    {
        if (SelectedRegion is null)
        {
            return false;
        }

        int selectedX = SelectedRegion.GetX();
        int selectedY = SelectedRegion.GetY();
        int selectedPlane = SelectedRegion.GetPlane();

        foreach (RegionDataBlock region in GetRouteRegions(ship))
        {
            if (region.GetX() == selectedX &&
                region.GetY() == selectedY &&
                region.GetPlane() == selectedPlane)
            {
                return true;
            }
        }

        return false;
    }

    private ShipFactionRelation GetShipRelation(ShipModel ship)
    {
        int factionId = ship.OwnerFactionId;
        if (factionId <= 0)
        {
            return ShipFactionRelation.Unknown;
        }

        if (ActiveFactionIds.Contains(factionId))
        {
            return ShipFactionRelation.Active;
        }

        if (AlliedFactionIds.Contains(factionId))
        {
            return ShipFactionRelation.Allied;
        }

        return ShipFactionRelation.Enemy;
    }

    private static List<RegionDataBlock> GetRawRouteRegions(ShipModel ship)
    {
        var regions = new List<RegionDataBlock>();

        foreach (DataBlock block in ship.TravelledByRegions)
        {
            if (block is not RegionDataBlock region)
            {
                continue;
            }

            AddUniqueRegion(regions, region);
        }

        return regions;
    }

    /// <summary>
    /// Returns the route with known land regions removed from the intermediate points, then fills
    /// any resulting gap with a shortest path through non-land regions. Departure and arrival may
    /// be land (harbours), but the rest of a ship route must stay on water.
    /// </summary>
    private List<RegionDataBlock> GetRouteRegions(ShipModel ship)
    {
        List<RegionDataBlock> raw = GetRawRouteRegions(ship);
        if (raw.Count < 2)
        {
            return raw;
        }

        int plane = raw[0].GetPlane();
        if (raw.Any(region => region.GetPlane() != plane))
        {
            return raw;
        }

        var kept = new List<RegionDataBlock>(raw.Count);
        AddUniqueRegion(kept, raw[0]);

        for (int i = 1; i < raw.Count - 1; i++)
        {
            if (!IsKnownLandRegion(raw[i]))
            {
                AddUniqueRegion(kept, raw[i]);
            }
        }

        AddUniqueRegion(kept, raw[^1]);
        if (kept.Count < 2)
        {
            return raw;
        }

        var navigable = new List<RegionDataBlock>(kept.Count);
        AddUniqueRegion(navigable, kept[0]);

        for (int i = 1; i < kept.Count; i++)
        {
            RegionDataBlock from = navigable[^1];
            RegionDataBlock to = kept[i];

            if (AreRouteRegionsAdjacent(from, to))
            {
                AddUniqueRegion(navigable, to);
                continue;
            }

            List<RegionDataBlock>? path = FindNavigableRoutePath(from, to);
            if (path is not null)
            {
                for (int j = 1; j < path.Count; j++)
                {
                    AddUniqueRegion(navigable, path[j]);
                }
            }
            else
            {
                // Keep the original route rather than dropping a segment when the map is
                // incomplete. The corridor clip still prevents the spline from entering land.
                AddUniqueRegion(navigable, to);
            }
        }

        return navigable.Count >= 2 ? navigable : raw;
    }

    private static void AddUniqueRegion(List<RegionDataBlock> regions, RegionDataBlock region)
    {
        if (regions.Count == 0)
        {
            regions.Add(region);
            return;
        }

        RegionDataBlock last = regions[^1];
        if (last.GetX() != region.GetX() ||
            last.GetY() != region.GetY() ||
            last.GetPlane() != region.GetPlane())
        {
            regions.Add(region);
        }
    }

    private static bool AreRouteRegionsAdjacent(RegionDataBlock a, RegionDataBlock b)
    {
        if (a.GetPlane() != b.GetPlane())
        {
            return false;
        }

        int dx = b.GetX() - a.GetX();
        int dy = b.GetY() - a.GetY();
        foreach ((int dq, int dr) in HexSlots.NeighborDeltas)
        {
            if (dx == dq && dy == dr)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Bounded BFS between two route points. Known land is blocked except at the two endpoints,
    /// which may be harbours. Unknown regions stay traversable so an incomplete map does not
    /// erase a route.
    /// </summary>
    private List<RegionDataBlock>? FindNavigableRoutePath(RegionDataBlock from, RegionDataBlock to)
    {
        int plane = from.GetPlane();
        if (to.GetPlane() != plane)
        {
            return null;
        }

        int fromKey = GetRouteRegionKey(from);
        int toKey = GetRouteRegionKey(to);
        if (fromKey == toKey)
        {
            return [from];
        }

        int distance = HexDistance(from.GetX(), from.GetY(), to.GetX(), to.GetY());
        int margin = Math.Max(6, distance + 4);
        int minX = Math.Min(from.GetX(), to.GetX()) - margin;
        int maxX = Math.Max(from.GetX(), to.GetX()) + margin;
        int minY = Math.Min(from.GetY(), to.GetY()) - margin;
        int maxY = Math.Max(from.GetY(), to.GetY()) + margin;

        var nodes = new Dictionary<int, RegionDataBlock>
        {
            [fromKey] = from,
        };
        var previous = new Dictionary<int, int>();
        var visited = new HashSet<int> { fromKey };
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((from.GetX(), from.GetY()));

        while (queue.Count > 0)
        {
            (int x, int y) = queue.Dequeue();
            int currentKey = Coordinates.GetId(x, y, (PlaneType)plane);

            foreach ((int dq, int dr) in HexSlots.NeighborDeltas)
            {
                int nx = x + dq;
                int ny = y + dr;
                if (nx < minX || nx > maxX || ny < minY || ny > maxY)
                {
                    continue;
                }

                int nextKey = Coordinates.GetId(nx, ny, (PlaneType)plane);
                if (!visited.Add(nextKey))
                {
                    continue;
                }

                bool isTarget = nextKey == toKey;
                if (!isTarget && IsKnownLandAt(nx, ny, plane))
                {
                    continue;
                }

                previous[nextKey] = currentKey;
                nodes[nextKey] = isTarget ? to : GetRouteRegionAt(nx, ny, plane);

                if (isTarget)
                {
                    return ReconstructRoutePath(nodes, previous, fromKey, toKey);
                }

                // Guard against pathological maps while still allowing long ocean crossings.
                if (visited.Count > 20000)
                {
                    return null;
                }

                queue.Enqueue((nx, ny));
            }
        }

        return null;
    }

    private static List<RegionDataBlock> ReconstructRoutePath(
        IReadOnlyDictionary<int, RegionDataBlock> nodes,
        IReadOnlyDictionary<int, int> previous,
        int startKey,
        int targetKey)
    {
        var path = new List<RegionDataBlock>();
        int currentKey = targetKey;

        while (true)
        {
            if (!nodes.TryGetValue(currentKey, out RegionDataBlock? node))
            {
                return [];
            }

            path.Add(node);
            if (currentKey == startKey)
            {
                break;
            }

            if (!previous.TryGetValue(currentKey, out int previousKey))
            {
                return [];
            }

            currentKey = previousKey;
        }

        path.Reverse();
        return path;
    }

    private RegionDataBlock GetRouteRegionAt(int x, int y, int plane)
    {
        int key = Coordinates.GetId(x, y, (PlaneType)plane);
        if (OccupiedRegions.TryGetValue(key, out RegionModel? occupied))
        {
            return occupied.Region;
        }

        if (UnoccupiedRegions.TryGetValue(key, out RegionModel? unoccupied))
        {
            return unoccupied.Region;
        }

        return new RegionDataBlock($"{x} {y} {plane}");
    }

    private bool IsKnownLandRegion(RegionDataBlock region)
    {
        return IsKnownLandAt(region.GetX(), region.GetY(), region.GetPlane());
    }

    private bool IsKnownLandAt(int x, int y, int plane)
    {
        return TryGetNeighborTerrain(x, y, plane, out int terrain) &&
               terrain != Terrains.OCEAN &&
               terrain != Terrains.UNKNOWN;
    }

    private static int GetRouteRegionKey(RegionDataBlock region)
    {
        return Coordinates.GetId(region.GetX(), region.GetY(), (PlaneType)region.GetPlane());
    }

    private static int HexDistance(int ax, int ay, int bx, int by)
    {
        int az = -ax - ay;
        int bz = -bx - by;
        return Math.Max(Math.Abs(ax - bx), Math.Max(Math.Abs(ay - by), Math.Abs(az - bz)));
    }

    private static ShipRouteSegmentKey GetRouteSegmentKey(RegionDataBlock a, RegionDataBlock b)
    {
        int plane = a.GetPlane();
        int ax = a.GetX();
        int ay = a.GetY();
        int bx = b.GetX();
        int by = b.GetY();

        if (ax > bx || (ax == bx && ay > by))
        {
            (ax, bx) = (bx, ax);
            (ay, by) = (by, ay);
        }

        return new ShipRouteSegmentKey(plane, ax, ay, bx, by);
    }

    private static bool IsReversedRouteSegment(RegionDataBlock a, RegionDataBlock b)
    {
        int ax = a.GetX();
        int ay = a.GetY();
        int bx = b.GetX();
        int by = b.GetY();
        return ax > bx || (ax == bx && ay > by);
    }

    private static double AssignRouteLanes(List<ShipRouteDrawing> drawings, double hexSize)
    {
        var users = new Dictionary<ShipRouteSegmentKey, List<(int RouteIndex, int SegmentIndex)>>();

        for (int routeIndex = 0; routeIndex < drawings.Count; routeIndex++)
        {
            ShipRouteDrawing drawing = drawings[routeIndex];
            for (int segmentIndex = 0; segmentIndex < drawing.SegmentKeys.Count; segmentIndex++)
            {
                ShipRouteSegmentKey key = drawing.SegmentKeys[segmentIndex];
                if (!users.TryGetValue(key, out var list))
                {
                    list = [];
                    users[key] = list;
                }

                list.Add((routeIndex, segmentIndex));
            }
        }

        int maxUsers = 1;
        foreach (var pair in users)
        {
            var byRoute = pair.Value
                .GroupBy(item => item.RouteIndex)
                .Select(group => new
                {
                    RouteIndex = group.Key,
                    SegmentIndices = group.Select(item => item.SegmentIndex).ToList(),
                })
                .OrderBy(item => drawings[item.RouteIndex].Ship.Structure.GetId())
                .ToList();

            int count = byRoute.Count;
            maxUsers = Math.Max(maxUsers, count);
            double center = (count - 1) / 2.0;

            for (int i = 0; i < count; i++)
            {
                double lane = i - center;
                ShipRouteDrawing route = drawings[byRoute[i].RouteIndex];

                foreach (int segmentIndex in byRoute[i].SegmentIndices)
                {
                    // BuildParallelAnchors flips the normal for reversed route directions.
                    // Flip the lane as well, otherwise an A->B and a B->A user would end up
                    // on the same side of the shared edge and overlap exactly.
                    route.SegmentLanes[segmentIndex] = route.ReversedSegments[segmentIndex]
                        ? -lane
                        : lane;
                }
            }
        }

        // Keep the total corridor width reasonable even when many ships share a segment.
        double spacing = Math.Min(hexSize * 0.09, hexSize * 0.85 / Math.Max(1, maxUsers - 1));
        return Math.Max(1.5, spacing);
    }

    private static void DrawRouteStart(DrawingContext context, Point point, Color color, double hexSize)
    {
        // The cross replaces the old filled departure dot and keeps the same footprint as that
        // dot (which was already enlarged to stay readable on the map).
        double radius = Math.Max(5.0, hexSize * 0.125);
        double thickness = Math.Max(3.0, radius * 0.14);
        var outlinePen = new Pen(
            new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)),
            thickness + 2.4);
        var crossPen = new Pen(new SolidColorBrush(color), thickness);

        var topLeft = new Point(point.X - radius, point.Y - radius);
        var bottomRight = new Point(point.X + radius, point.Y + radius);
        var topRight = new Point(point.X + radius, point.Y - radius);
        var bottomLeft = new Point(point.X - radius, point.Y + radius);

        // Dark keyline first, then the route-coloured cross on top.
        context.DrawLine(outlinePen, topLeft, bottomRight);
        context.DrawLine(outlinePen, topRight, bottomLeft);
        context.DrawLine(crossPen, topLeft, bottomRight);
        context.DrawLine(crossPen, topRight, bottomLeft);
    }

    private static void DrawRouteDirectionArrows(
        DrawingContext context,
        IReadOnlyList<Point> anchors,
        Color color,
        double hexSize,
        bool selected)
    {
        // One arrow per region: anchors alternate region centres and shared-edge midpoints.
        List<(Point Point, Vector Tangent)> arrows = ShipRouteRenderer.GetDirectionArrows(anchors, stride: 2);
        if (arrows.Count == 0)
        {
            return;
        }

        double size = Math.Max(selected ? 5.2 : 4.5, hexSize * (selected ? 0.165 : 0.14));
        double outline = Math.Max(1.0, hexSize * 0.030);
        var fill = new SolidColorBrush(color);
        var outlinePen = new Pen(new SolidColorBrush(Color.FromArgb(215, 8, 12, 16)), outline);

        foreach ((Point point, Vector tangent) in arrows)
        {
            var normal = new Vector(-tangent.Y, tangent.X);

            Point tip = new(point.X + tangent.X * size, point.Y + tangent.Y * size);
            Point baseCenter = new(point.X - tangent.X * size * 0.8, point.Y - tangent.Y * size * 0.8);
            Point left = new(
                baseCenter.X + normal.X * size * 0.62,
                baseCenter.Y + normal.Y * size * 0.62);
            Point right = new(
                baseCenter.X - normal.X * size * 0.62,
                baseCenter.Y - normal.Y * size * 0.62);

            var geometry = new StreamGeometry();
            using (StreamGeometryContext gc = geometry.Open())
            {
                gc.BeginFigure(tip, true);
                gc.LineTo(left);
                gc.LineTo(right);
                gc.EndFigure(true);
            }

            context.DrawGeometry(fill, outlinePen, geometry);
        }
    }

    private static bool IsInsideCircle(Point point, Point center, double radius)
    {
        double dx = point.X - center.X;
        double dy = point.Y - center.Y;
        return dx * dx + dy * dy <= radius * radius;
    }

    /// <summary>
    /// Builds the water corridor in which a route is allowed to be drawn. It contains the route
    /// hexes plus their non-land neighbours, so a smooth curve can still bend naturally while
    /// being clipped before it enters a known land region.
    /// </summary>
    private Geometry BuildRouteCorridorGeometry(ShipRouteDrawing drawing, double hexSize)
    {
        var cells = new List<(int X, int Y)>(drawing.Regions.Count * 4);
        var seen = new HashSet<int>();
        int plane = drawing.Regions[0].GetPlane();

        void AddCell(int x, int y)
        {
            if (seen.Add(Coordinates.GetId(x, y, (PlaneType)plane)))
            {
                cells.Add((x, y));
            }
        }

        foreach (RegionDataBlock region in drawing.Regions)
        {
            AddCell(region.GetX(), region.GetY());
        }

        foreach (RegionDataBlock region in drawing.Regions)
        {
            foreach ((int dq, int dr) in HexSlots.NeighborDeltas)
            {
                int x = region.GetX() + dq;
                int y = region.GetY() + dr;
                if (!IsKnownLandAt(x, y, plane))
                {
                    AddCell(x, y);
                }
            }
        }

        var geometry = new StreamGeometry();
        using (StreamGeometryContext context = geometry.Open())
        {
            foreach ((int x, int y) in cells)
            {
                Point[] points = GetHexPoints(HexToPixel(x, y, hexSize), hexSize);
                context.BeginFigure(points[0], true);
                for (int i = 1; i < points.Length; i++)
                {
                    context.LineTo(points[i]);
                }
                context.EndFigure(true);
            }
        }

        return geometry;
    }

    private RouteDestinationRing GetRouteDestinationRing(
        ShipRouteDrawing drawing,
        double hexSize)
    {
        RegionDataBlock destination = drawing.Regions[^1];
        bool isOcean = IsOceanRegion(destination);

        Bitmap? image = null;
        double baseRadius;
        if (_shipStyle == ShipStyles.Image)
        {
            image = GetShipImage(ShipCatalog.GetIconPath(drawing.Visual, drawing.Coast))
                ?? GetShipImage(ShipCatalog.GetGenericIconPath(drawing.Coast));

            // Measure the sprite itself. The metrics are normalised to the hex size, so they
            // automatically follow the current zoom level.
            baseRadius = image != null
                ? ShipMarkerMetrics.GetImageRingRadius(image, hexSize)
                : hexSize * ShipMarkerMetrics.FallbackImageRingRadiusFactor;
        }
        else
        {
            baseRadius = hexSize * ShipMarkerMetrics.VectorRingRadiusFactor;
        }

        Point marker = GetShipMarkerPosition(drawing, destination, hexSize, image);
        double radius = Math.Min(baseRadius, hexSize * 0.80);

        return new RouteDestinationRing(marker, radius, isOcean);
    }

    private bool IsOceanRegion(RegionDataBlock region)
    {
        // Prefer the terrain carried by the region block itself. This also works when the
        // departure region is outside the currently loaded dictionaries.
        try
        {
            int terrain = region.GetTerrain();
            if (terrain == Terrains.OCEAN)
            {
                return true;
            }

            if (terrain != Terrains.UNKNOWN)
            {
                return false;
            }
        }
        catch
        {
            // Fall back to the dictionaries below.
        }

        int key = Coordinates.GetId(
            region.GetX(),
            region.GetY(),
            (PlaneType)region.GetPlane());

        if (OccupiedRegions.TryGetValue(key, out RegionModel? occupied))
        {
            return occupied.Terrain == Terrains.OCEAN;
        }

        if (UnoccupiedRegions.TryGetValue(key, out RegionModel? unoccupied))
        {
            return unoccupied.Terrain == Terrains.OCEAN;
        }

        // Unknown terrain: use the stronger land-style highlight rather than hiding it.
        return false;
    }

    private void DrawRouteDestination(
        DrawingContext context,
        ShipRouteDrawing drawing,
        RouteDestinationRing destination)
    {
        // The ship-identifying ring uses the same colour as its route, including the brighter
        // shade used for the selected ship. It stays fully opaque so the ship remains easy to find.
        Color color = GetRouteColor(drawing);
        Point marker = destination.Center;
        double radius = destination.Radius;

        if (drawing.IsSelected)
        {
            double glowRadius = radius * 1.45;
            var glowGeometry = new EllipseGeometry(
                new Rect(marker.X - glowRadius, marker.Y - glowRadius, glowRadius * 2.0, glowRadius * 2.0));
            context.DrawGeometry(
                new SolidColorBrush(Color.FromArgb(55, color.R, color.G, color.B)),
                null,
                glowGeometry);
        }

        var ringGeometry = new EllipseGeometry(
            new Rect(marker.X - radius, marker.Y - radius, radius * 2.0, radius * 2.0));

        // Keep a subtle fill so the ship remains visible, but make the outline fully opaque and
        // noticeably thicker on both ocean and land. A dark keyline keeps the green readable on
        // bright terrain and above route lines.
        int fillAlpha = destination.IsOcean ? 38 : 52;
        double strokeWidth = Math.Max(3.2, radius * 0.12);

        if (drawing.IsSelected)
        {
            fillAlpha = Math.Min(82, fillAlpha + 20);
            strokeWidth += 1.8;
        }

        var fill = new SolidColorBrush(Color.FromArgb((byte)fillAlpha, color.R, color.G, color.B));
        var stroke = new SolidColorBrush(color);
        context.DrawGeometry(
            null,
            new Pen(new SolidColorBrush(Color.FromArgb(220, 8, 12, 16)), strokeWidth + 2.4),
            ringGeometry);
        context.DrawGeometry(fill, new Pen(stroke, strokeWidth), ringGeometry);

        // Keep a small centre dot for the case where ships are hidden: the ring alone would not
        // show where the route actually ends.
        double dotRadius = Math.Max(1.6, radius * 0.10);
        var dotGeometry = new EllipseGeometry(
            new Rect(marker.X - dotRadius, marker.Y - dotRadius, dotRadius * 2.0, dotRadius * 2.0));
        context.DrawGeometry(new SolidColorBrush(color), null, dotGeometry);
    }

    private Point GetShipMarkerPosition(
        ShipRouteDrawing drawing,
        RegionDataBlock region,
        double hexSize,
        Bitmap? image)
    {
        Point center = HexToPixel(region.GetX(), region.GetY(), hexSize);
        int coast = drawing.Coast;

        if (_shipStyle == ShipStyles.Image && image != null)
        {
            // Align the ring with the visible centre of the rendered sprite, not with the hex edge.
            return ShipMarkerMetrics.GetImageAnchor(image, center, hexSize);
        }

        if (coast is >= 0 and <= 5)
        {
            int slot = ((coast + 4) % 6) + 1;
            return HexSlots.GetSlotCenter(slot, center, hexSize, HexSlots.EdgeRadiusFactor);
        }

        return center;
    }

    /// <summary>
    /// Draws all buildings present in a region inside its hexagon.
    /// Castles/fortifications take the center slot; buildings with a terrain affinity are pushed
    /// toward the hex edge facing that terrain; the others fill the remaining slots.
    /// </summary>
    private void RenderBuildingsOnRegion(DrawingContext context, RegionModel regionModel, Point center, double hexSize)
    {
        if (!_showBuildings)
        {
            return;
        }

        List<BuildingModel>? buildings = regionModel.Buildings;
        int count = buildings.Count;
        if (count == 0)
        {
            return;
        }

        int q = regionModel.GetX();
        int r = regionModel.GetY();

        // Resolve each building to a visual, ordered so that fortifications (layer priority 0)
        // are considered first, then the larger ones.
        var items = new List<(BuildingModel Model, BuildingVisual Visual, int Size)>(count);
        foreach (BuildingModel building in buildings)
        {
            items.Add((building, BuildingCatalog.Resolve(GetBuildingType(building)), GetBuildingSize(building)));
        }
        items.Sort((a, b) =>
        {
            int byLayer = a.Visual.LayerPriority.CompareTo(b.Visual.LayerPriority);
            return byLayer != 0 ? byLayer : b.Size.CompareTo(a.Size);
        });

        // When there are more buildings than slots, the center slot is reserved for a "+n" badge.
        bool hasOverflow = count > HexSlots.SlotCount;
        int capacity = hasOverflow ? HexSlots.PerimeterCount : HexSlots.SlotCount;

        var usedSlots = new bool[HexSlots.SlotCount];
        var assignments = new List<(int Slot, BuildingModel Model, BuildingVisual Visual, int Size)>(capacity);
        var assigned = new bool[items.Count];

        // Pass 1: the (largest) fortification goes to the center slot.
        if (!hasOverflow)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Visual.IsFortification)
                {
                    usedSlots[HexSlots.CenterIndex] = true;
                    assignments.Add((HexSlots.CenterIndex, items[i].Model, items[i].Visual, items[i].Size));
                    assigned[i] = true;
                    break;
                }
            }
        }

        // Pass 2: buildings with a terrain affinity go to the slot facing the matching neighbour.
        for (int i = 0; i < items.Count && assignments.Count < capacity; i++)
        {
            if (assigned[i] || items[i].Visual.AffinityTerrain < 0)
            {
                continue;
            }

            int slot = FindAffinitySlot(items[i].Visual.AffinityTerrain, q, r, regionModel.GetPlane());
            if (slot > HexSlots.CenterIndex && !usedSlots[slot])
            {
                usedSlots[slot] = true;
                assignments.Add((slot, items[i].Model, items[i].Visual, items[i].Size));
                assigned[i] = true;
            }
        }

        // Pass 3: everything else fills the remaining slots (center first, then perimeter).
        for (int i = 0; i < items.Count && assignments.Count < capacity; i++)
        {
            if (assigned[i])
            {
                continue;
            }

            int slot = NextFreeSlot(usedSlots, hasOverflow);
            if (slot < 0)
            {
                break;
            }

            usedSlots[slot] = true;
            assignments.Add((slot, items[i].Model, items[i].Visual, items[i].Size));
            assigned[i] = true;
        }

        // Draw the assigned buildings.
        double iconSize = ComputeBuildingIconSize(hexSize, count);
        foreach ((int slot, BuildingModel model, BuildingVisual visual, int size) in assignments)
        {
            Point slotCenter = HexSlots.GetSlotCenter(slot, center, hexSize);
            double drawSize = visual.IsFortification ? iconSize * 1.10 : iconSize;
            DrawBuilding(context, visual, model, slotCenter, drawSize, regionModel.Terrain);
        }

        // Overflow badge.
        int overflow = count - assignments.Count;
        if (overflow > 0)
        {
            DrawBuildingCountBadge(context, center, hexSize, overflow);
        }
    }

    /// <summary>Returns the perimeter slot facing a neighbour whose terrain matches the affinity, or -1.</summary>
    private int FindAffinitySlot(int affinityTerrain, int q, int r, int plane)
    {
        for (int i = 0; i < HexSlots.NeighborDeltas.Length; i++)
        {
            (int dq, int dr) = HexSlots.NeighborDeltas[i];
            if (TryGetNeighborTerrain(q + dq, r + dr, plane, out int terrain) && terrain == affinityTerrain)
            {
                return i + 1;
            }
        }
        return -1;
    }

    /// <summary>Returns the terrain of a neighbour region, if it is known.</summary>
    private bool TryGetNeighborTerrain(int q, int r, int plane, out int terrain)
    {
        terrain = Terrains.UNKNOWN;
        int key = Coordinates.GetId(q, r, (PlaneType)plane);
        if (OccupiedRegions.TryGetValue(key, out RegionModel? occupied))
        {
            terrain = occupied.Terrain;
            return true;
        }
        if (UnoccupiedRegions.TryGetValue(key, out RegionModel? unoccupied))
        {
            terrain = unoccupied.Terrain;
            return true;
        }
        return false;
    }

    private static int NextFreeSlot(bool[] usedSlots, bool skipCenter)
    {
        int first = skipCenter ? HexSlots.CenterIndex + 1 : HexSlots.CenterIndex;
        for (int slot = first; slot < usedSlots.Length; slot++)
        {
            if (!usedSlots[slot])
            {
                return slot;
            }
        }
        return -1;
    }

    /// <summary>Size of a single building icon, shrinking as the number of buildings grows.</summary>
    private static double ComputeBuildingIconSize(double hexSize, int count)
    {
        double factor = count <= 1 ? 0.78 : count <= 4 ? 0.50 : 0.40;
        return hexSize * factor;
    }

    private void DrawBuilding(DrawingContext context, BuildingVisual visual, BuildingModel building, Point center, double size, int terrain)
    {
        bool owned = IsOwnedByActiveFaction(building);

        // Fortifications owned by the player use a dedicated graphic.
        string iconPath = visual.IsFortification && owned ? OwnedCastleIconPath : visual.IconPath;

        // Contrast plate: keeps the small, mostly dark pixel-art icons readable over the
        // textured terrain images (forest, mountain, glacier...).
        DrawBuildingPlate(context, terrain, center, size * BuildingContrastModes.PlateScale, _buildingContrast);

        if (_buildingStyle != BuildingStyles.Symbols)
        {
            Bitmap? bitmap = GetBuildingImage(iconPath);
            if (bitmap != null)
            {
                DrawBuildingImage(context, bitmap, center, size);
                return;
            }
        }

        if (_buildingStyle == BuildingStyles.Icons)
        {
            // icons-only mode: nothing to draw when the bitmap is missing
            return;
        }

        DrawBuildingShape(context, visual.FallbackShape, visual.FallbackColor, center, size * 0.78);
    }

    /// <summary>
    /// Draws a light rounded plate with a dark terrain-tinted rim behind a building icon.
    /// This is what makes the tiny icons readable regardless of the terrain colors.
    /// </summary>
    private static void DrawBuildingPlate(DrawingContext context, int terrain, Point center, double plateSize, int mode)
    {
        if (mode == BuildingContrastModes.None || plateSize < 5.0)
        {
            return;
        }

        double half = plateSize / 2.0;
        double radius = plateSize * 0.22;

        var plateGeometry = new RectangleGeometry(
            new Rect(center.X - half, center.Y - half, plateSize, plateSize),
            radius,
            radius);

        SolidColorBrush rim = _plateRims.GetOrAdd(
            terrain,
            static t => new SolidColorBrush(BuildingContrast.GetPlate(t).Rim));

        if (mode == BuildingContrastModes.Rim)
        {
            // Outline only: no filled background, just a colored keyline.
            var outlinePen = new Pen(rim, Math.Max(1.0, plateSize * 0.06));
            context.DrawGeometry(null, outlinePen, plateGeometry);
            return;
        }

        double shadowOffset = Math.Max(1.0, plateSize * 0.045);

        // Soft drop shadow, slightly offset downwards.
        var shadowGeometry = new RectangleGeometry(
            new Rect(center.X - half, center.Y - half + shadowOffset, plateSize, plateSize),
            radius,
            radius);
        context.DrawGeometry(_plateShadow, null, shadowGeometry);

        SolidColorBrush fill = _plateFills.GetOrAdd(
            terrain,
            static t => new SolidColorBrush(BuildingContrast.GetPlate(t).Fill));

        var pen = new Pen(rim, Math.Max(1.0, plateSize * 0.055));
        context.DrawGeometry(fill, pen, plateGeometry);
    }

    private static void DrawBuildingImage(DrawingContext context, Bitmap bitmap, Point center, double size)
    {
        double sourceWidth = bitmap.Size.Width;
        double sourceHeight = bitmap.Size.Height;
        if (sourceWidth <= 0 || sourceHeight <= 0)
        {
            return;
        }

        double scale = Math.Min(size / sourceWidth, size / sourceHeight);
        double width = sourceWidth * scale;
        double height = sourceHeight * scale;
        var destination = new Rect(center.X - width / 2.0, center.Y - height / 2.0, width, height);

        // The icons are 16x16 pixel art: nearest-neighbor keeps them crisp when zoomed in,
        // instead of the blurry look produced by bilinear filtering.
        using (context.PushRenderOptions(new RenderOptions
        {
            BitmapInterpolationMode = BitmapInterpolationMode.None,
        }))
        {
            context.DrawImage(bitmap, new Rect(0, 0, sourceWidth, sourceHeight), destination);
        }
    }

    private static void DrawBuildingShape(DrawingContext context, BuildingShape shape, Color color, Point center, double size)
    {
        if (size <= 0)
        {
            return;
        }

        var brush = new SolidColorBrush(color);
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(200, 20, 20, 20)), Math.Max(1.0, size * 0.06));

        Geometry geometry;
        switch (shape)
        {
            case BuildingShape.Circle:
                geometry = new EllipseGeometry(new Rect(center.X - size / 2.0, center.Y - size / 2.0, size, size));
                break;
            case BuildingShape.Triangle:
                geometry = BuildPolygon(
                [
                    new Point(center.X, center.Y - size / 2.0),
                    new Point(center.X + size / 2.0, center.Y + size / 2.0),
                    new Point(center.X - size / 2.0, center.Y + size / 2.0),
                ]);
                break;
            case BuildingShape.Rhombus:
                geometry = BuildPolygon(
                [
                    new Point(center.X, center.Y - size / 2.0),
                    new Point(center.X + size / 2.0, center.Y),
                    new Point(center.X, center.Y + size / 2.0),
                    new Point(center.X - size / 2.0, center.Y),
                ]);
                break;
            default: // Square
                double half = size / 2.0;
                geometry = BuildPolygon(
                [
                    new Point(center.X - half, center.Y - half),
                    new Point(center.X + half, center.Y - half),
                    new Point(center.X + half, center.Y + half),
                    new Point(center.X - half, center.Y + half),
                ]);
                break;
        }

        context.DrawGeometry(brush, pen, geometry);
    }

    private static Geometry BuildPolygon(Point[] points)
    {
        var geometry = new StreamGeometry();
        using (var gc = geometry.Open())
        {
            gc.BeginFigure(points[0], true);
            for (int i = 1; i < points.Length; i++)
            {
                gc.LineTo(points[i]);
            }
            gc.LineTo(points[0]);
            gc.EndFigure(true);
        }
        return geometry;
    }

    private static void DrawBuildingCountBadge(DrawingContext context, Point center, double hexSize, int count)
    {
        double radius = Math.Max(6.0, hexSize * 0.22);
        var background = new SolidColorBrush(Color.FromArgb(220, 20, 20, 20));
        var pen = new Pen(Brushes.White, 1);
        var circle = new EllipseGeometry(new Rect(center.X - radius, center.Y - radius, radius * 2.0, radius * 2.0));
        context.DrawGeometry(background, pen, circle);

        var typeface = new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Bold);
        using var layout = new TextLayout(
            $"+{count}",
            typeface,
            Math.Max(8.0, radius * 0.9),
            Brushes.White,
            TextAlignment.Center,
            TextWrapping.NoWrap);
        layout.Draw(context, new Point(center.X - layout.Width / 2.0, center.Y - layout.Height / 2.0));
    }

    private static string GetBuildingType(BuildingModel building)
    {
        string type = building.Type;
        if (string.IsNullOrEmpty(type))
        {
            try
            {
                type = building.Structure.Value(KeyType.TYPE);
            }
            catch
            {
                type = string.Empty;
            }
        }
        return type;
    }

    private static int GetBuildingSize(BuildingModel building)
    {
        if (building.Size >= 0)
        {
            return building.Size;
        }
        try
        {
            return building.Structure.ValueInt(KeyType.SIZE);
        }
        catch
        {
            return -1;
        }
    }

    private bool IsOwnedByActiveFaction(BuildingModel building)
    {
        if (ActiveFactionIds.Count == 0)
        {
            return false;
        }

        int factionId;
        try
        {
            factionId = building.Structure.ValueInt(KeyType.FACTION);
        }
        catch
        {
            return false;
        }

        return factionId > 0 && ActiveFactionIds.Contains(factionId);
    }

    private static Bitmap? GetBuildingImage(string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath))
        {
            return null;
        }
        return _buildingImages.GetOrAdd(iconPath, LoadBitmap);
    }

    /// <summary>
    /// Draws all ships in a final pass, after every region has been painted. Ship images with a
    /// coast overlap the neighbouring hexagon, so drawing them inside the region loop would let
    /// the next region's terrain cover half of the ship.
    /// </summary>
    private void RenderAllShips(DrawingContext context)
    {
        if (!_showShips)
        {
            return;
        }

        double hexSize = HexSize * Zoom / 100.0;
        var visited = new HashSet<int>();

        foreach (var kvp in OccupiedRegions)
        {
            if (visited.Add(kvp.Key))
            {
                RenderShipsOnRegion(context, kvp.Value, HexToPixel(kvp.Value.GetX(), kvp.Value.GetY(), hexSize), hexSize);
            }
        }

        foreach (var kvp in UnoccupiedRegions)
        {
            if (visited.Add(kvp.Key))
            {
                RenderShipsOnRegion(context, kvp.Value, HexToPixel(kvp.Value.GetX(), kvp.Value.GetY(), hexSize), hexSize);
            }
        }
    }

    private void RenderShipsOnRegion(DrawingContext context, RegionModel regionModel, Point center, double hexSize)
    {
        if (!_showShips || hexSize <= 0)
        {
            return;
        }

        List<ShipModel>? ships = regionModel.Ships;
        int count = ships.Count;
        if (count == 0)
        {
            return;
        }

        // Group ships by coast so that several ships docked on the same edge do not overlap
        // exactly. -1 means "not docked", drawn in the center of the hexagon.
        var groups = new Dictionary<int, List<ShipModel>>();
        foreach (ShipModel ship in ships)
        {
            int coast = GetShipCoast(ship);
            int key = coast is >= 0 and <= 5 ? coast : -1;
            if (!groups.TryGetValue(key, out List<ShipModel>? group))
            {
                group = new List<ShipModel>();
                groups[key] = group;
            }
            group.Add(ship);
        }

        if (_shipStyle == ShipStyles.Image)
        {
            foreach (var pair in groups)
            {
                int coast = pair.Key;
                List<ShipModel> group = pair.Value;
                ShipModel representative = SelectRepresentativeShip(group);
                ShipVisual visual = ShipCatalog.Resolve(GetShipType(representative));

                if (!DrawShipImage(context, visual, coast, center, hexSize))
                {
                    // Missing image asset: fall back to the detailed vector silhouette.
                    DrawShipMarker(context, representative, group.Count, coast, center, hexSize, detailed: true);
                }

                if (group.Count > 1)
                {
                    Point badgeCenter = GetShipBadgeCenter(coast, center, hexSize);
                    DrawShipCountBadge(context, badgeCenter, hexSize, group.Count, additional: false);
                }
            }
            return;
        }

        bool detailed = _shipStyle == ShipStyles.Vector;
        double iconSize = ComputeShipIconSize(hexSize, count);

        foreach (var pair in groups)
        {
            int coast = pair.Key;
            List<ShipModel> group = pair.Value;
            int slot = coast is >= 0 and <= 5 ? ((coast + 4) % 6) + 1 : HexSlots.CenterIndex;
            Point edgeCenter = slot == HexSlots.CenterIndex
                ? center
                : HexSlots.GetSlotCenter(slot, center, hexSize, HexSlots.EdgeRadiusFactor);
            double angle = slot == HexSlots.CenterIndex ? 0.0 : HexSlots.GetSlotAngle(slot);
            (double tx, double ty) = slot == HexSlots.CenterIndex ? (1.0, 0.0) : HexSlots.GetSlotTangent(slot);

            // Show up to five individual ships per edge, then a "+n" badge for the rest.
            int shown = Math.Min(group.Count, 5);
            double spacing = iconSize * 0.72;
            for (int i = 0; i < shown; i++)
            {
                double offset = (i - (shown - 1) / 2.0) * spacing;
                var position = new Point(edgeCenter.X + tx * offset, edgeCenter.Y + ty * offset);
                ShipModel ship = group[i];
                ShipVisual visual = ShipCatalog.Resolve(GetShipType(ship));
                double drawSize = iconSize * visual.SizeFactor;

                if (detailed)
                {
                    ShipSilhouettes.DrawVector(context, visual, position, drawSize, angle);
                }
                else
                {
                    ShipSilhouettes.DrawSymbol(context, visual, position, drawSize, angle);
                }
            }

            if (group.Count > shown)
            {
                Point badgeCenter = slot == HexSlots.CenterIndex
                    ? new Point(center.X + iconSize * 0.9, center.Y + iconSize * 0.65)
                    : HexSlots.GetSlotCenter(slot, center, hexSize, 0.50);
                DrawShipCountBadge(context, badgeCenter, hexSize, group.Count - shown, additional: true);
            }
        }
    }

    /// <summary>
    /// Draws the pre-rendered image of a ship on its coast. The source images are 80x80 cells
    /// containing a 64x64 hexagon with 8 px margins; the inner hexagon is mapped onto the
    /// region hexagon so the ship lands exactly on the corresponding edge.
    /// </summary>
    private static bool DrawShipImage(DrawingContext context, ShipVisual visual, int coast, Point center, double hexSize)
    {
        Bitmap? bitmap = GetShipImage(ShipCatalog.GetIconPath(visual, coast));
        bitmap ??= GetShipImage(ShipCatalog.GetGenericIconPath(coast));
        if (bitmap == null)
        {
            return false;
        }

        const double sourceHexSize = 64.0;
        const double sourceMargin = 8.0;
        const double sourceImageSize = 80.0;

        double hexWidth = Math.Sqrt(3.0) * hexSize;
        double hexHeight = 2.0 * hexSize;
        double scaleX = hexWidth / sourceHexSize;
        double scaleY = hexHeight / sourceHexSize;

        var destination = new Rect(
            center.X - hexWidth / 2.0 - sourceMargin * scaleX,
            center.Y - hexHeight / 2.0 - sourceMargin * scaleY,
            sourceImageSize * scaleX,
            sourceImageSize * scaleY);

        using (context.PushRenderOptions(new RenderOptions
        {
            BitmapInterpolationMode = BitmapInterpolationMode.None,
        }))
        {
            context.DrawImage(bitmap, new Rect(0, 0, bitmap.Size.Width, bitmap.Size.Height), destination);
        }

        return true;
    }

    /// <summary>Draws one vector ship marker, used when an image asset is missing.</summary>
    private static void DrawShipMarker(
        DrawingContext context,
        ShipModel ship,
        int groupCount,
        int coast,
        Point center,
        double hexSize,
        bool detailed)
    {
        int slot = coast is >= 0 and <= 5 ? ((coast + 4) % 6) + 1 : HexSlots.CenterIndex;
        Point position = slot == HexSlots.CenterIndex
            ? center
            : HexSlots.GetSlotCenter(slot, center, hexSize, HexSlots.EdgeRadiusFactor);
        double angle = slot == HexSlots.CenterIndex ? 0.0 : HexSlots.GetSlotAngle(slot);
        ShipVisual visual = ShipCatalog.Resolve(GetShipType(ship));
        double size = ComputeShipIconSize(hexSize, Math.Max(1, groupCount)) * visual.SizeFactor;

        if (detailed)
        {
            ShipSilhouettes.DrawVector(context, visual, position, size, angle);
        }
        else
        {
            ShipSilhouettes.DrawSymbol(context, visual, position, size, angle);
        }
    }

    private static ShipModel SelectRepresentativeShip(List<ShipModel> group)
    {
        ShipModel best = group[0];
        double bestSize = ShipCatalog.Resolve(GetShipType(best)).SizeFactor;

        for (int i = 1; i < group.Count; i++)
        {
            double size = ShipCatalog.Resolve(GetShipType(group[i])).SizeFactor;
            if (size > bestSize)
            {
                best = group[i];
                bestSize = size;
            }
        }

        return best;
    }

    /// <summary>
    /// Returns the visual of the ship that is actually drawn for a coast. Several ships docked on
    /// the same edge share one image: the largest one. Route rings must use the same type.
    /// </summary>
    private static ShipVisual GetRepresentativeShipVisual(RegionModel regionModel, ShipModel fallback, int coast)
    {
        int key = coast is >= 0 and <= 5 ? coast : -1;
        ShipModel? representative = null;
        double bestSize = double.MinValue;

        foreach (ShipModel candidate in regionModel.Ships)
        {
            int candidateCoast = GetShipCoast(candidate);
            int candidateKey = candidateCoast is >= 0 and <= 5 ? candidateCoast : -1;
            if (candidateKey != key)
            {
                continue;
            }

            ShipVisual candidateVisual = ShipCatalog.Resolve(GetShipType(candidate));
            if (candidateVisual.SizeFactor > bestSize)
            {
                representative = candidate;
                bestSize = candidateVisual.SizeFactor;
            }
        }

        return ShipCatalog.Resolve(GetShipType(representative ?? fallback));
    }

    /// <summary>Size of a single ship marker, shrinking as the number of ships grows.</summary>
    private static double ComputeShipIconSize(double hexSize, int count)
    {
        double factor = count <= 1 ? 0.60 : count <= 4 ? 0.46 : 0.38;
        return hexSize * factor;
    }

    private static Point GetShipBadgeCenter(int coast, Point center, double hexSize)
    {
        if (coast is >= 0 and <= 5)
        {
            int slot = ((coast + 4) % 6) + 1;
            return HexSlots.GetSlotCenter(slot, center, hexSize, 0.62);
        }

        // Undocked ships: keep the badge clear of the ship image in the middle of the cell.
        return new Point(center.X + hexSize * 0.34, center.Y - hexSize * 0.36);
    }

    private static void DrawShipCountBadge(DrawingContext context, Point center, double hexSize, int count, bool additional)
    {
        if (count <= 0)
        {
            return;
        }

        double radius = Math.Max(5.0, hexSize * 0.16);
        var background = new SolidColorBrush(Color.FromArgb(220, 20, 20, 20));
        var pen = new Pen(Brushes.White, 1);
        var circle = new EllipseGeometry(new Rect(center.X - radius, center.Y - radius, radius * 2.0, radius * 2.0));
        context.DrawGeometry(background, pen, circle);

        string label = additional ? $"+{count}" : count.ToString();
        var typeface = new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Bold);
        using var layout = new TextLayout(
            label,
            typeface,
            Math.Max(7.0, radius * 0.95),
            Brushes.White,
            TextAlignment.Center,
            TextWrapping.NoWrap);
        layout.Draw(context, new Point(center.X - layout.Width / 2.0, center.Y - layout.Height / 2.0));
    }

    private static string GetShipType(ShipModel ship)
    {
        string type = ship.Type;
        if (string.IsNullOrEmpty(type))
        {
            try
            {
                type = ship.Structure.Value(KeyType.TYPE);
            }
            catch
            {
                type = string.Empty;
            }
        }
        return type;
    }

    private static int GetShipCoast(ShipModel ship)
    {
        if (ship.Coast >= 0)
        {
            return ship.Coast;
        }

        try
        {
            return ship.Structure.ValueInt(KeyType.COAST, -1);
        }
        catch
        {
            return -1;
        }
    }

    private static Bitmap? GetShipImage(string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath))
        {
            return null;
        }
        return _shipImages.GetOrAdd(iconPath, LoadBitmap);
    }

    // --- Add helper to cleanup and draw wakes; call DrawBoatWakes(context, hexSize) at the end of Render() ---
    private void DrawBoatWakes(DrawingContext context, double hexSize)
    {
        List<BoatWake> snapshot;
        lock (_wakesLock)
        {
            snapshot = _boatWakes.ToList();
        }

        if (snapshot.Count == 0 || _boatWakeBitmap == null)
            return;

        var now = DateTime.UtcNow;
        // Remove expired wakes (cleanup)
        lock (_wakesLock)
        {
            _boatWakes.RemoveAll(w => (now - w.Created).TotalSeconds >= w.DurationSeconds);
        }

        foreach (var w in snapshot)
        {
            var age = (now - w.Created).TotalSeconds;
            if (age >= w.DurationSeconds) continue;
            var remaining = 1.0 - (age / w.DurationSeconds); // 1->0

            // opacity fades out (keep minimum so visible)
            double opacity = Math.Max(0.15, remaining);

            // size: roughly half the hex bounding width (or use slightly less)
            double size = hexSize * 1.0; // hexSize is radius-to-corner; adjust as needed
            double scale = 0.5 + 0.4 * remaining; // vary size slightly while fading
            double drawSize = size * scale;

            // pixel center for hex
            var center = HexToPixel(w.Q, w.R, hexSize);

            // dest rectangle centered on hex
            var dest = new Rect(center.X - drawSize / 2, center.Y - drawSize / 2, drawSize, drawSize);

            // compute rotation matrix around center (PushTransform expects an Avalonia.Matrix)
            double angleRad = w.AngleDegrees * Math.PI / 180.0;
            var translateToOrigin = Matrix.CreateTranslation(-center.X, -center.Y);
            var rotation = Matrix.CreateRotation(angleRad);
            var translateBack = Matrix.CreateTranslation(center.X, center.Y);
            var mat = translateToOrigin * rotation * translateBack;

            // Use PushTransform (replacement for deprecated PushPostTransform)
            using (context.PushTransform(mat))
            using (context.PushOpacity(opacity))
            {
                context.DrawImage(_boatWakeBitmap, new Rect(0, 0, _boatWakeBitmap.Size.Width, _boatWakeBitmap.Size.Height), dest);
            }
        }
    }

    private Point HexToPixel(int q, int r, double size)
    {
        // Map origin is at (_minX, _minY)
        var pt = HexToPixelUncentered(q, r, size);
        return new Point(pt.X - _minX + size, pt.Y - _minY + size); // +size for padding
    }

    private static Point HexToPixelUncentered(int q, int r, double size)
    {
        double x = size * Math.Sqrt(3) * (q + r / 2.0);
        double y = -size * 3.0 / 2.0 * r; // Invert Y as in your main method
        return new Point(x, y);
    }

    private Point[] GetHexPoints(Point center, double size)
    {
        var points = new Point[6];
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 180 * (60 * i - 30);
            points[i] = new Point(
                center.X + size * Math.Cos(angle),
                center.Y + size * Math.Sin(angle));
        }
        return points;
    }

    /// <summary>
    /// Handles settings properties changes and save events.
    /// </summary>
    /// <param name="e">Data context changed event</param>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double hexSize = HexSize * Zoom / 100.0;
        double contentWidth = _hasBounds ? (_maxX - _minX) + hexSize * 2 : 300;
        double contentHeight = _hasBounds ? (_maxY - _minY) + hexSize * 2 : 300;

        double width = double.IsInfinity(availableSize.Width)
            ? contentWidth
            : Math.Max(contentWidth, availableSize.Width);

        double height = double.IsInfinity(availableSize.Height)
            ? contentHeight
            : Math.Max(contentHeight, availableSize.Height);

        width = double.IsNaN(width) || width < 0 ? 0 : width;
        height = double.IsNaN(height) || height < 0 ? 0 : height;

        return new Size(width, height);
    }

    // --- Public API: call this when a boat travels through a region ---
    public void AddBoatWake(int q, int r, double angleDegrees, double durationSeconds = 2.5)
    {
        lock (_wakesLock)
        {
            _boatWakes.Add(new BoatWake
            {
                Q = q,
                R = r,
                AngleDegrees = angleDegrees,
                Created = DateTime.UtcNow,
                DurationSeconds = durationSeconds
            });
        }
        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var scrollViewer = GetParentScrollViewer();
        if (scrollViewer is not null)
        {
            // record initial pointer position in viewport coords if possible (avoids double-count)
            _pointerStartViewport = e.GetPosition(scrollViewer);
            _scrollStartOffset = scrollViewer.Offset;
            try
            {
                e.Pointer.Capture(this);
                _isPointerCapturedForPanning = true;
                _isPanning = false;
            }
            catch
            {
                _isPointerCapturedForPanning = false;
            }
        }
        else
        {
            // fallback: record local control coords
            _pointerStartLocal = e.GetPosition(this);
            _scrollStartOffset = new Vector();
            try
            {
                e.Pointer.Capture(this);
                _isPointerCapturedForPanning = true;
                _isPanning = false;
            }
            catch
            {
                _isPointerCapturedForPanning = false;
            }
        }

        // Do not perform selection now. Selection is resolved on release (click vs drag).
        var localPoint = e.GetPosition(this);
        var (q, r) = PixelToHex(localPoint, HexSize * Zoom / 100.0);
        Debug.WriteLine($"[HEXMAPCONTROL] OnPointerPressed {localPoint} x-region={q}, y-region={r}");

    }

    private void OnPointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        var point = e.GetPosition(this);

        if (_isPointerCapturedForPanning)
        {
            // release capture
            try { e.Pointer.Capture(null); } catch { }
            _isPointerCapturedForPanning = false;

            if (_isPanning)
            {
                // we were panning; finish and do not treat as click
                _isPanning = false;
                return;
            }
        }

        // If no pan, treat as click if pointer stayed within the same hex as press
        Point initialPoint = _pointerStartLocal;
        // if we have a viewport start, translate it back to local coords for hex check
        var scrollViewer = GetParentScrollViewer();
        if (scrollViewer is not null)
        {
            // convert viewport point to control local point by offsetting according to scroll offset and content transform
            // easiest: use current pointer local position recorded on release (we already have 'point') and compare hex of that to hex computed from initial viewport converted
            initialPoint = e.GetPosition(this); // fallback safe: compare same local point (no pan)
        }

        var (initialQ, initialR) = PixelToHex(initialPoint, HexSize * Zoom / 100.0);
        var (q, r) = PixelToHex(point, HexSize * Zoom / 100.0);
        Debug.WriteLine($"[HEXMAPCONTROL] OnPointerReleased after: ({initialQ}, {initialR}); ({q}, {r})");
        if (initialQ != q || initialR != r)
        {
            Debug.WriteLine($"[HEXMAPCONTROL] OnPointerReleased - NO SELECTION (moved to another hex)");
            return;
        }

        UpdateSelection(q, r, false);
    }

    private void OnPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        var pointLocal = e.GetPosition(this);

        // find once the parent ScrollViewer
        var scrollViewer = this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();

        // If pointer is captured, consider panning
        if (_isPointerCapturedForPanning)
        {
            // compute delta in viewport coordinates (if we have scrollViewer), otherwise fallback to local coords
            Vector delta;
            if (scrollViewer != null)
            {
                var currentViewportPos = e.GetPosition(scrollViewer);
                delta = new Vector(currentViewportPos.X - _pointerStartViewport.X, currentViewportPos.Y - _pointerStartViewport.Y);
            }
            else
            {
                delta = new Vector(pointLocal.X - _pointerStartLocal.X, pointLocal.Y - _pointerStartLocal.Y);
            }

            double threshold = ComputePanThreshold();

            if (!_isPanning &&
                (Math.Abs(delta.X) >= threshold || Math.Abs(delta.Y) >= threshold))
            {
                _isPanning = true;
                // hide tooltip while panning
                _hoveredRegion = null;
                ToolTip.SetTip(this, null);
            }

            if (_isPanning && scrollViewer != null)
            {
                // drag moves content opposite to pointer movement
                var newOffset = new Vector(_scrollStartOffset.X - delta.X, _scrollStartOffset.Y - delta.Y);

                // clamp to valid ranges
                double maxX = Math.Max(0, scrollViewer.Extent.Width - scrollViewer.Viewport.Width);
                double maxY = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
                newOffset = new Vector(Math.Max(0, Math.Min(newOffset.X, maxX)), Math.Max(0, Math.Min(newOffset.Y, maxY)));

                scrollViewer.Offset = newOffset;
                return; // while panning, skip hover logic
            }
        }

        // Normal hover/tooltip processing when not panning
        var (q2, r2) = PixelToHex(pointLocal, HexSize * Zoom / 100.0);
        int key = Coordinates.GetId(q2, r2);
        if (OccupiedRegions.TryGetValue(key, out var populatedRegionModel))
        {
            var region = populatedRegionModel.Region;
            var terrain = region!.GetTerrain();
            if (Terrains.CanBeNamed(terrain))
            {
                // don't display tooltip if over a region that can not be named (ocean, firewall...)
                if (!ReferenceEquals(region, _hoveredRegion))
                {
                    _hoveredRegion = populatedRegionModel;
                    // Build tooltip text
                    string tip = region.GetUILabel();
                    if (string.IsNullOrEmpty(tip))
                    {
                        tip = region.Value(KeyType.NAME);
                        if (string.IsNullOrEmpty(tip))
                            tip = $"{q2},{r2}";
                    }
                    tip = AppendBuildingsToTooltip(tip, q2, r2);
                    ToolTip.SetTip(this, tip);
                }
            }
            else
            {
                _hoveredRegion = null;
                ToolTip.SetTip(this, null);
            }
            return;
        }

        if (UnoccupiedRegions.TryGetValue(key, out var emptyRegionModel))
        {
            var region = emptyRegionModel.Region;
            var terrain = region!.GetTerrain();
            if (Terrains.CanBeNamed(terrain))
            {
                // don't display tooltip if over a region that can not be named (ocean, firewall...)
                if (!ReferenceEquals(region, _hoveredRegion))
                {
                    _hoveredRegion = populatedRegionModel;
                    // Build tooltip text
                    string tip = region.GetUILabel();
                    if (string.IsNullOrEmpty(tip))
                    {
                        tip = region.Value(KeyType.NAME);
                        if (string.IsNullOrEmpty(tip))
                            tip = $"{q2},{r2}";
                    }
                    tip = AppendBuildingsToTooltip(tip, q2, r2);
                    ToolTip.SetTip(this, tip);
                }
            }
            else
            {
                _hoveredRegion = null;
                ToolTip.SetTip(this, null);
            }
        }
    }

    /// <summary>
    /// Appends the names of the buildings present in the hovered region to its tooltip.
    /// </summary>
    private string AppendBuildingsToTooltip(string baseTip, int q, int r)
    {
        if (!_showBuildings)
        {
            return baseTip;
        }

        int key = Coordinates.GetId(q, r);
        if (!OccupiedRegions.TryGetValue(key, out RegionModel? regionModel) &&
            !UnoccupiedRegions.TryGetValue(key, out regionModel))
        {
            return baseTip;
        }

        if (regionModel is null || regionModel.Buildings.Count == 0)
        {
            return baseTip;
        }

        var names = new List<string>(regionModel.Buildings.Count);
        foreach (BuildingModel building in regionModel.Buildings)
        {
            string label = Labels.Localize(
                Odyssey.Models.Tools.DataProperty.Categories.Building,
                GetBuildingType(building));
            if (!string.IsNullOrEmpty(label))
            {
                names.Add(label);
            }
        }

        if (names.Count == 0)
        {
            return baseTip;
        }

        return $"{baseTip}{Environment.NewLine}{string.Join(", ", names)}";
    }

    private void OnPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        // TODO: when mouse is over the tooltip itself, then it does not work anymore until mouse re-enters the control
        _hoveredRegion = null;
        ToolTip.SetTip(this, null);

        if (_isPointerCapturedForPanning)
        {
            try { e.Pointer.Capture(null); } catch { }
            _isPointerCapturedForPanning = false;
            _isPanning = false;
        }
    }

    private void OnSelectedRegionChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // SelectedRegion is a RegionModel (see SelectedRegionProperty), not a DataBlock.
        var previousRegion = e.OldValue as RegionModel;
        var region = e.NewValue as RegionModel;
        if (ReferenceEquals(previousRegion, region))
        {
            return;
        }

        if (region != null)
        {
            _selectedQ = region.GetX();
            _selectedR = region.GetY();
        }
        else
        {
            _selectedQ = null;
            _selectedR = null;
        }
        InvalidateVisual();
        ScrollToSelectedRegion();
    }

    private void UpdateSelection(int q, int r, bool shouldCenterNewSelection)
    {
        // Set the flag before assigning SelectedRegion: the property change synchronously
        // triggers ScrollToSelectedRegion, which consumes (and resets) this flag.
        _shouldCenterNewSelection = shouldCenterNewSelection;

        int key = Coordinates.GetId(q, r);
        if (OccupiedRegions.TryGetValue(key, out var populatedRegion))
        {
            Debug.WriteLine($"UpdateSelection q={q}, r={r}, populated region found: {populatedRegion}");
            _selectedQ = q;
            _selectedR = r;
            SelectedRegion = populatedRegion;
        }
        else if (UnoccupiedRegions.TryGetValue(key, out var emptyRegion))
        {
            Debug.WriteLine($"UpdateSelection q={q}, r={r}, empty region found: {emptyRegion}");
            _selectedQ = q;
            _selectedR = r;
            SelectedRegion = emptyRegion;
        }
        else
        {
            _selectedQ = null;
            _selectedR = null;
            SelectedRegion = null;
        }

        InvalidateVisual();
    }

    private void ScrollToSelectedRegion()
    {
        bool shouldCenterNewSelection = _shouldCenterNewSelection;

        // this is the default behavior
        _shouldCenterNewSelection = true;

        if (SelectedRegion == null)
            return;

        if (shouldCenterNewSelection)
        {
            // Calculate the pixel position of the selected region
            int q = SelectedRegion.GetX();
            int r = SelectedRegion.GetY();

            var scrollViewer = GetParentScrollViewer();
            if (scrollViewer is null)
                return;


            Vector? offset = CenterToRegionCoordinates(q, r, scrollViewer.Offset, scrollViewer.Viewport, scrollViewer.Extent, false);
            if (offset == null)
                return;

            scrollViewer.Offset = (Vector)offset;
        }
    }

    private (int q, int r) PixelToHex(Point p, double size)
    {
        // Undo the offset used in HexToPixel
        double x = p.X - size;
        double y = p.Y - size;

        // Undo the minX/minY offset
        x += _minX;
        y += _minY;

        // Invert Y as in HexToPixelUncentered
        y = -y;

        // Axial coordinates math (pointy-topped)
        double q = (Math.Sqrt(3) / 3 * x - 1.0 / 3 * y) / size;
        double r = (2.0 / 3 * y) / size;

        // Round to nearest hex
        return HexRound(q, r);
    }

    private (int q, int r) HexRound(double q, double r)
    {
        double s = -q - r;
        int rq = (int)Math.Round(q);
        int rr = (int)Math.Round(r);
        int rs = (int)Math.Round(s);

        double q_diff = Math.Abs(rq - q);
        double r_diff = Math.Abs(rr - r);
        double s_diff = Math.Abs(rs - s);

        if (q_diff > r_diff && q_diff > s_diff)
            rq = -rr - rs;
        else if (r_diff > s_diff)
            rr = -rq - rs;

        return (rq, rr);
    }

    private void UpdateTerrainImagesFromSeason()
    {
        // TODO: compare season with previous value to avoid unuseful reloads
        string seasonStr = string.Empty;
        if (_useSeasonImages)
        {
            switch (Season)
            {
                case Seasons.SPRING:
                    seasonStr = "spring";
                    break;
                case Seasons.SUMMER:
                    seasonStr = "summer";
                    break;
                case Seasons.AUTUMN:
                    seasonStr = "autumn";
                    break;
                case Seasons.WINTER:
                    seasonStr = "winter";
                    break;
                default: break;
            }
        }
        for (int terrain = Terrains.UNKNOWN; terrain < Terrains.LAST; terrain++)
        {
            Bitmap? bitmap = null;
            if (_useSeasonImages && !string.IsNullOrEmpty(seasonStr))
            {
                // Try to load the season-specific image
                bitmap = LoadBitmap(GetTerrainResourcePathname(terrain, seasonStr), terrain, false);
            }
            _terrainImages[terrain] = bitmap ?? _standardTerrainImages[terrain];
        }
    }

    private int? _selectedQ = null;
    private int? _selectedR = null;
    private readonly ConcurrentDictionary<int, Bitmap?> _terrainImages = new();

    private static readonly ConcurrentDictionary<int, Bitmap?> _standardTerrainImages = new();

    // Cache of building icon bitmaps, keyed by avares resource path.
    private static readonly ConcurrentDictionary<string, Bitmap?> _buildingImages = new();

    // Cache of ship images (one image per type and coast index), keyed by avares resource path.
    private static readonly ConcurrentDictionary<string, Bitmap?> _shipImages = new();

    // Cached brushes for the building contrast plates, keyed by terrain type.
    private static readonly ConcurrentDictionary<int, SolidColorBrush> _plateFills = new();
    private static readonly ConcurrentDictionary<int, SolidColorBrush> _plateRims = new();

    // Soft drop shadow drawn under every building plate.
    private static readonly SolidColorBrush _plateShadow = new(Color.FromArgb(55, 0, 0, 0));

    // Icon used for fortifications owned by the player's faction.
    private const string OwnedCastleIconPath = "/Assets/map/symbols/castle_owned.gif";

    private Bitmap? GetTerrainImage(int terrain)
    {
        if (!_terrainImages.TryGetValue(terrain, out var bmp))
        {
            Debug.WriteLine("[HEXMAP-CTRL] unknown terrain {terrain}...");
            bmp = _standardTerrainImages[terrain];
        }
        return bmp;
    }

    private static string GetTerrainResourcePathname(int t, string suffix)
    {
        string imageName;
        switch (t)
        {
            case Terrains.OCEAN:
                imageName = "ocean";
                break;
            case Terrains.SWAMP:
                imageName = "swamp";
                break;
            case Terrains.PLAINS:
                imageName = "plains";
                break;
            case Terrains.DESERT:
                imageName = "desert";
                break;
            case Terrains.FOREST:
                imageName = "forest";
                break;
            case Terrains.HIGHLAND:
                imageName = "highland";
                break;
            case Terrains.MOUNTAIN:
                imageName = "mountain";
                break;
            case Terrains.GLACIER:
                imageName = "glacier";
                break;
            case Terrains.VOLCANO:
                imageName = "volcano";
                break;
            case Terrains.VOLCANO_ACTIVE:
                imageName = "volcano";
                break;
            case Terrains.ICEBERG:
                imageName = "iceberg";
                break;
            case Terrains.CORRIDOR:
                imageName = "corridor";
                break;
            case Terrains.WALL:
                imageName = "wall";
                break;
            case Terrains.HALL:
                imageName = "hall";
                break;
            case Terrains.FOG:
                imageName = "fog";
                break;
            case Terrains.THICKFOG:
                imageName = "thickfog";
                break;
            case Terrains.FIREWALL:
                imageName = "firewall";
                break;
            case Terrains.MAHLSTROM:
                imageName = "mahlstrom";
                break;
            // CHECK: wanted icon for the folowing values
            case Terrains.UNKNOWN:
            case Terrains.PACKICE:
            case Terrains.ICEFLOE:
            case Terrains.LAST:
            default: return string.Empty;
        }
        string imageFullname = string.IsNullOrEmpty(suffix) ? imageName : $"{imageName}_{suffix}";
        return $"/Assets/map/terrains/{imageFullname}.png";
    }

    private static void PreloadStandardTerrainImages()
    {
        for (int terrain = Terrains.UNKNOWN; terrain < Terrains.LAST; terrain++)
        {
            var resourcePathname = GetTerrainResourcePathname(terrain, string.Empty);
            _standardTerrainImages[terrain] = LoadBitmap(resourcePathname, terrain, true);
        }
    }

    private static Bitmap? LoadBitmap(string resourcePathname)
    {
        if (string.IsNullOrEmpty(resourcePathname))
        {
            return null;
        }

        try
        {
            var uriObj = new Uri($"avares://Odyssey{resourcePathname}");
            var stream = AssetLoader.Open(uriObj);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    private static Bitmap? LoadBitmap(string resourcePathname, int terrain, bool shouldFind)
    {
        Bitmap? bitmap = null;
        if (!string.IsNullOrEmpty(resourcePathname))
        {
            // Compose the avares URI for Avalonia asset loading
            var uri = $"avares://Odyssey{resourcePathname}";
            try
            {
                var uriObj = new Uri(uri);
                var stream = AssetLoader.Open(uriObj);
                bitmap = new Bitmap(stream);
            }
            catch (Exception ex)
            {
                if (shouldFind)
                {
                    Debug.WriteLine($"[HEXMAPCONTROL] WARNING | could not load terrain image terrain={terrain} resourcePathname={resourcePathname} {ex}");
                }
            }
        }
        if (bitmap == null && shouldFind)
        {
            Debug.WriteLine($"[HEXMAPCONTROL] WARNING | could not find terrain image for terrain={terrain} resourcePathname={resourcePathname}");
        }
        return bitmap;
    }

    // Helper: pick a readable brush (black or white) depending on terrain "brightness".
    // Tweak mapping to taste � chosen to be conservative for common terrain images.
    private static IBrush GetLabelBrushForTerrain(int terrain)
    {
        switch (terrain)
        {
            case Terrains.FOREST:
            case Terrains.MOUNTAIN:
            case Terrains.VOLCANO:
            case Terrains.VOLCANO_ACTIVE:
            case Terrains.GLACIER:
            case Terrains.ICEBERG:
            case Terrains.ICEFLOE:
            case Terrains.WALL:
            case Terrains.HALL:
            case Terrains.CORRIDOR:
            case Terrains.FOG:
            case Terrains.THICKFOG:
            case Terrains.MAHLSTROM:
                // dark backgrounds -> use white text
                //return Brushes.White;
            case Terrains.DESERT:
            case Terrains.PLAINS:
            case Terrains.SWAMP:
            case Terrains.HIGHLAND:
            case Terrains.UNKNOWN:
            case Terrains.PACKICE:
            default:
                // lighter backgrounds -> use dark text
                return Brushes.Black;
        }
    }

    /* 
    PSEUDOCODE (plan d�taill�) :
    - Ajouter une m�thode private void HighlightSelectedRegion(...) qui encapsule la logique actuelle
      de dessin du contour d'un hexagone s�lectionn� ou, si non s�lectionn�, du trac� standard de la bordure.
    - Param�tres attendus :
      - DrawingContext context : contexte de dessin
      - Point center : centre du hex courant (utile pour calculs d'inset)
      - Point[] hexPoints : points des 6 sommets de l'hex
      - int x, int y : coordonn�es q/r de la r�gion courante
      - Geometry geometry : g�om�trie du hexagon (utilis�e pour la branche "else" pour dessiner la bordure grise)
    - Comportement :
      - Si _selectedQ == x && _selectedR == y :
        - Calculer �paisseur de surlignage en fonction de HexSize (min 2px)
        - Calculer inset (d�placement vers le centre pour borderGeometry)
        - Construire un StreamGeometry pour la bordure r�duite
        - Dessiner cette bordure avec un Pen semi-transparent rouge
      - Sinon :
        - Dessiner la bordure standard en gris (Pen de largeur 1)
    - Remplacer le bloc inline original dans Render() par un appel � HighlightSelectedRegion(context, center, hexPoints, x, y, geometry);
    */
    private void HighlightSelectedRegion(DrawingContext context, Point center, Point[] hexPoints, int x, int y, Geometry geometry)
    {
        // Draw highlight if selected
        if (_selectedQ == x && _selectedR == y)
        {
            // compute thickness according to the hexSize (depends on zoom factor)
            // min 2px to keep it visible
            double highlightThickness = Math.Max(2, HexSize * Zoom / 100.0 * 0.22);

            double inset = 0.15;
            var centerInset = new Avalonia.Point(center.X * inset, center.Y * inset);

            // hex in semi-transparent red
            var darkPen = new Pen(new SolidColorBrush(Color.FromArgb(120, 255, 0, 0)), highlightThickness);

            var borderGeometry = new StreamGeometry();
            using (var ctx = borderGeometry.Open())
            {
                ctx.BeginFigure(
                    new Point(hexPoints[0].X * (1 - inset) + centerInset.X, hexPoints[0].Y * (1 - inset) + centerInset.Y),
                    false // not filled
                );
                for (int i = 1; i < hexPoints.Length; i++)
                {
                    ctx.LineTo(
                        new Point(hexPoints[i].X * (1 - inset) + centerInset.X, hexPoints[i].Y * (1 - inset) + centerInset.Y)
                    );
                }
                ctx.LineTo(
                    new Point(hexPoints[0].X * (1 - inset) + centerInset.X, hexPoints[0].Y * (1 - inset) + centerInset.Y)
                );
                ctx.EndFigure(false);
            }
            context.DrawGeometry(null, darkPen, borderGeometry);
        }
        else
        {
            // Draw the hex border on top (always last, in gray)
            context.DrawGeometry(null, new Pen(new SolidColorBrush(Color.FromRgb(128, 128, 128)), 1), geometry);
        }
    }
}