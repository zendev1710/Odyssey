using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting; // added for TextLayout
using Avalonia.Platform;
using Avalonia.VisualTree;
using Odyssey.Extensions;
using Odyssey.Models;
using Odyssey.Models.Data;
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
    private DataBlock? _hoveredRegion;

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

    public static readonly StyledProperty<Dictionary<int, DataBlock>> RegionsProperty =
        AvaloniaProperty.Register<HexMapControl, Dictionary<int, DataBlock>>(nameof(Regions));

    public static readonly StyledProperty<Dictionary<int, ShipModel>> ShipsProperty =
        AvaloniaProperty.Register<HexMapControl, Dictionary<int, ShipModel>>(nameof(Ships));

    public static readonly StyledProperty<Dictionary<int, BuildingModel>> BuildingsProperty =
       AvaloniaProperty.Register<HexMapControl, Dictionary<int, BuildingModel>>(nameof(Buildings));

    public static readonly StyledProperty<Dictionary<int, RegionModel>> RegionsWithContainerProperty =
       AvaloniaProperty.Register<HexMapControl, Dictionary<int, RegionModel>>(nameof(RegionsWithContainer));

    public static readonly StyledProperty<double> HexSizeProperty =
        AvaloniaProperty.Register<HexMapControl, double>(nameof(HexSize), defaultHexSize);

    public static readonly StyledProperty<int> ZoomProperty =
        AvaloniaProperty.Register<HexMapControl, int>(nameof(Zoom), defaultZoom);

    public static readonly StyledProperty<DataBlock?> SelectedRegionProperty =
        AvaloniaProperty.Register<HexMapControl, DataBlock?>(nameof(SelectedRegion));

    public static readonly StyledProperty<bool> IsStandardPlaneProperty =
        AvaloniaProperty.Register<HexMapControl, bool>(nameof(IsStandardPlane), false);

    public Dictionary<int, DataBlock> Regions
    {
        get => GetValue(RegionsProperty) ?? new Dictionary<int, DataBlock>();
        set => SetValue(RegionsProperty, value ?? new Dictionary<int, DataBlock>());
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

    public Dictionary<int, RegionModel> RegionsWithContainer
    {
        get => GetValue(RegionsWithContainerProperty) ?? new Dictionary<int, RegionModel>();
        set => SetValue(RegionsWithContainerProperty, value ?? new Dictionary<int, RegionModel>());
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

    public DataBlock? SelectedRegion
    {
        get => GetValue(SelectedRegionProperty);
        set => SetValue(SelectedRegionProperty, value);
    }

    static HexMapControl()
    {
        PreloadStandardTerrainImages();
        RegionsProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnRegionsChanged());
        HexSizeProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnHexSizeChanged());
        ZoomProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnZoomChanged());
        SeasonProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnSeasonChanged());
        SelectedRegionProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnSelectedRegionChanged(e));
    }

    // Helper to find ancestor ScrollViewer
    private ScrollViewer? GetParentScrollViewer()
    {
        return this.GetVisualAncestors().OfType<ScrollViewer>().FirstOrDefault();
    }

    private bool _useSeasonImages;

    public HexMapControl()
    {
        Season = Seasons.UNKNOWN;
        _useSeasonImages = GlobalSettings.Get<bool>(GlobalSettings.MAP_USE_SEASON_IMAGES);
        Regions = new Dictionary<int, DataBlock>();
        UpdateTerrainImagesFromSeason();
        this.PointerPressed += OnPointerPressed;
        this.PointerReleased += OnPointerReleased;
        this.PointerMoved += OnPointerMoved;
        this.PointerExited += OnPointerExited;

        // Set a short default tooltip show delay (optional)
        ToolTip.SetShowDelay(this, 500);
        
        // Load directional wake image (add a PNG at Assets/Map/Effects/boat_wake.png)
        //_boatWakeBitmap = LoadBitmap("/Assets/Map/Effects/boat_wake.png", -1, false);
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

    private void OnRegionsChanged()
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

        if (Regions.Count == 0)
        {
            _minX = _maxX = _minY = _maxY = 0;
            return;
        }

        double hexSize = HexSize * Zoom / 100.0;

        foreach (var kvp in Regions)
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
        Debug.WriteLine($"[HEXMAP-CTRL] HexSize={HexSize} Zoom={Zoom} Number of regions={Regions.Count}");

        base.Render(context);
        double hexSize = HexSize * Zoom / 100.0;
        foreach (var kvp in Regions)
        {
            var region = kvp.Value;
            var flags = region.GetFlags();

            /*
            int planeId = region.GetId();
            if (planeId.IsStandardPlane() != IsStandardPlane)
            {
                // Only draw regions from its attached plane
                continue;
            }
            */

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
                // TODO: handle Flag.SHIPTRAVEL means a boat traveled through this region (ocean)
                var bounds = geometry.Bounds;
                // Draw the image stretched to the hex's bounding box
                context.DrawImage(image, new Rect(0, 0, image.Size.Width, image.Size.Height), bounds);

                // If region is unseen, overlay a semi-transparent black to darken
                if (region.IsSeenRegion(out Flag visibilityFlag))
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
                Debug.WriteLine($"[HEXMAP-CTRL] unknown terrain {terrain} for {region}...");
            }

            if (Terrains.CanBeNamed(terrain))
            {
                // Draw region name centered in hex (horizontally and vertically)
                try
                {
                    // Get readable name from the datablock
                    string name = region.Value(KeyType.NAME);
                    /**/
                    if (string.IsNullOrEmpty(name))
                    {
                        // fallback to lowercase name or coordinates if no name
                        name = region.Value(KeyType.LOWERCASE_NAME);
                    }
                    /**/
                    if (!string.IsNullOrEmpty(name))
                    {
                        // Can be adjusted. font size relative to hex size (tweak multiplier as needed)
                        double fontSize = Math.Max(8, hexSize * 0.33 /*0.28 / 0.35*/);
                        // Can be adjusted. Max width should be a bit smaller than hex bounding width
                        double maxTextWidth = Math.Max(10, geometry.Bounds.Width * 0.95);

                        // Create a left-aligned TextLayout with a MaxWidth then compute the measured size
                        TextDecorationCollection? textDecorations = null;
                        // var typeface = new Typeface("Segoe UI");
                        // Use SemiBold to increase contrast/visibility; change to FontWeight.Bold if you prefer stronger weight.
                        var typeface = new Typeface("Segoe UI", FontStyle.Normal, FontWeight.SemiBold);
                        double maxWidth = maxTextWidth;
                        double fontSz = fontSize;
                        // Can be adjusted. slightly stronger shadow: 220 instead of 180
                        var shadowBrush = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0));

                        // TODO: maybe white on unactive mountain, black for others
                        var labelBrush = GetLabelBrushForTerrain(terrain);

                        // Create left-aligned layout and center manually (left alignment + manual origin avoids the centering issues)
                        using (var layout = new TextLayout(
                            name,
                            typeface,
                            fontSz,
                            labelBrush,
                            TextAlignment.Left,
                            TextWrapping.NoWrap,
                            TextTrimming.CharacterEllipsis,
                            textDecorations,
                            FlowDirection.LeftToRight,
                            maxWidth))
                        using (var shadowLayout = new TextLayout(
                            name,
                            typeface,
                            fontSz,
                            shadowBrush,
                            TextAlignment.Left,
                            TextWrapping.NoWrap,
                            TextTrimming.CharacterEllipsis,
                            textDecorations,
                            FlowDirection.LeftToRight,
                            maxWidth))
                        {
                            // Prefer WidthIncludingTrailingWhitespace for visual width, fall back to Width.
                            double measuredWidth = layout.WidthIncludingTrailingWhitespace > 0 ? layout.WidthIncludingTrailingWhitespace : layout.Width;
                            measuredWidth = Math.Min(measuredWidth, maxWidth);
                            double measuredHeight = layout.Height;

                            // Center the layout box horizontally and vertically on the hex center,
                            // then shift slightly downward (a fraction of the font size) so text sits a bit lower than true center.
                            double dropFactor = 0.9; // smaller = closer to center, larger = further down; tweak as needed
                            double drop = fontSz * dropFactor;
                            var origin = new Point(center.X - measuredWidth / 2.0, center.Y - measuredHeight / 2.0 + drop);

                            // Can be adjusted
                            double raise = Math.Max(0.0, fontSz * 0.3); // tweak 0.12 to move more/less
                            origin = new Point(origin.X, origin.Y - raise);

                            // Shadow and main draw
                            var shadowOffset = Math.Max(1.0, fontSz * 0.07);
                            shadowLayout.Draw(context, new Point(origin.X + shadowOffset, origin.Y + shadowOffset));
                            layout.Draw(context, origin);

                        }
                    }
                }
                catch
                {
                    // Defensive: don't crash rendering on text layout errors
                }
            }

            // Persisting boat wake: draw for ocean regions flagged with SHIPTRAVEL
            // Keep this simple (no animation) — uses the existing _boatWakeBitmap asset.
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

            if (((flags) & (int)Flag.CASTLE) != 0)
            {
                RenderBuildingsOnRegion(region);
            }

            if (((flags) & (int)Flag.SHIP) != 0)
            {
                RenderShipsOnRegion(region);
            }

            HighlightSelectedRegion(context, center, hexPoints, x, y, geometry);
        }

        RenderShipMovements();
        // TODO: draw monsters, storms, fog of war...
    }

    private void RenderShipMovements()
    {
        // DrawBoatWakes(context, hexSize);
        foreach (var kvs in Ships)
        {
        }
    }

    private void RenderBuildingsOnRegion(DataBlock region)
    {
        if (RegionsWithContainer.TryGetValue(region.GetId(), out var regionModel))
        {
            foreach (var buildingModel in regionModel.Buildings)
            {
                // TODO
            }
            // Lighthouse (phare) :
            // cercle plein blanc avec bande circulaire rouge à demi diamètre du cercle (donc cercle blanc au milieu)
            // affiché au centre de la région hexagonale, avec 2 rayons lumineux opposés l'un à l'autre, semi-transparents, partant du cercle, allant vers l'arête de l'hexagone
            // les rayons lumineux sont présents uniquement si le propriétaire du phare a une perception suffisante
            //BURG 485590
            //"Leuchtturm"; Typ
            //"lighthouse"; Name
            //10; Groesse
            //79080; Besitzer
            //1189101; Partei

            //BURG 1267534
            //"Hafen"; Typ
            //"harbour"; Name
            //25; Groesse
            //320139; Besitzer
            //904991; Partei
        }
    }

    private void RenderShipsOnRegion(DataBlock region)
    {
        if (RegionsWithContainer.TryGetValue(region.GetId(), out var regionModel))
        {
            foreach (var shipModel in regionModel.Ships)
            {
                // TODO
            }
        }
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
        //_selector = DataContext as ISelector ?? throw new InvalidOperationException("DataContext must implement ISelector");
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
        if (Regions.TryGetValue(key, out var region))
        {
            var terrain = region.GetTerrain();
            if (Terrains.CanBeNamed(terrain))
            {
                // don't display tooltip if over a region that can not be named (ocean, firewall...)
                if (!ReferenceEquals(region, _hoveredRegion))
                {
                    _hoveredRegion = region;
                    // Build tooltip text
                    string tip = region.GetUILabel();
                    if (string.IsNullOrEmpty(tip))
                    {
                        tip = region.Value(KeyType.NAME);
                        if (string.IsNullOrEmpty(tip))
                            tip = $"{q2},{r2}";
                    }
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
        var previousRegion = e.OldValue as DataBlock;
        var region = e.NewValue as DataBlock;
        if (previousRegion == region)
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
        int key = Coordinates.GetId(q, r);
        if (Regions.TryGetValue(key, out var region))
        {
            Debug.WriteLine($"UpdateSelection q={q}, r={r}, region found: {region}");
            _selectedQ = q;
            _selectedR = r;
            SelectedRegion = region;
        }
        else
        {
            _selectedQ = null;
            _selectedR = null;
            SelectedRegion = null;
        }
        _shouldCenterNewSelection = shouldCenterNewSelection;
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
        return $"/Assets/Map/Terrains/{imageFullname}.png";
    }

    private static void PreloadStandardTerrainImages()
    {
        for (int terrain = Terrains.UNKNOWN; terrain < Terrains.LAST; terrain++)
        {
            var resourcePathname = GetTerrainResourcePathname(terrain, string.Empty);
            _standardTerrainImages[terrain] = LoadBitmap(resourcePathname, terrain, true);
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
    // Tweak mapping to taste — chosen to be conservative for common terrain images.
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
    PSEUDOCODE (plan détaillé) :
    - Ajouter une méthode private void HighlightSelectedRegion(...) qui encapsule la logique actuelle
      de dessin du contour d'un hexagone sélectionné ou, si non sélectionné, du tracé standard de la bordure.
    - Paramètres attendus :
      - DrawingContext context : contexte de dessin
      - Point center : centre du hex courant (utile pour calculs d'inset)
      - Point[] hexPoints : points des 6 sommets de l'hex
      - int x, int y : coordonnées q/r de la région courante
      - Geometry geometry : géométrie du hexagon (utilisée pour la branche "else" pour dessiner la bordure grise)
    - Comportement :
      - Si _selectedQ == x && _selectedR == y :
        - Calculer épaisseur de surlignage en fonction de HexSize (min 2px)
        - Calculer inset (déplacement vers le centre pour borderGeometry)
        - Construire un StreamGeometry pour la bordure réduite
        - Dessiner cette bordure avec un Pen semi-transparent rouge
      - Sinon :
        - Dessiner la bordure standard en gris (Pen de largeur 1)
    - Remplacer le bloc inline original dans Render() par un appel à HighlightSelectedRegion(context, center, hexPoints, x, y, geometry);
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