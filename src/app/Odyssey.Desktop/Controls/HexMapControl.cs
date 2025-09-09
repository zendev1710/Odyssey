using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
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
    // TODO: scrollbars should stay when map view is in float mode
    // TODO: zoom slider as a map setting "dispaly zoom slider at the top of the map"

    // Default size of hexagon as the width of the bounding square in pixels
    private static readonly double defaultHexSideSize = 64; //64 or 80

    // Default size of hexagon as from radius to corner
    private static readonly double defaultHexSize = defaultHexSideSize / Math.Sqrt(3);

    public static readonly StyledProperty<Seasons> SeasonProperty =
        AvaloniaProperty.Register<HexMapControl, Seasons>(nameof(Season), Seasons.UNKNOWN);

    public static readonly StyledProperty<Dictionary<int, DataBlock>> RegionsProperty =
        AvaloniaProperty.Register<HexMapControl, Dictionary<int, DataBlock>>(nameof(Regions));

    public static readonly StyledProperty<double> HexSizeProperty =
        AvaloniaProperty.Register<HexMapControl, double>(nameof(HexSize), defaultHexSize);

    public static readonly StyledProperty<DataBlock?> SelectedRegionProperty =
        AvaloniaProperty.Register<HexMapControl, DataBlock?>(nameof(SelectedRegion));

    // Update property to never return null
    public Dictionary<int, DataBlock> Regions
    {
        get => GetValue(RegionsProperty) ?? new Dictionary<int, DataBlock>();
        set => SetValue(RegionsProperty, value ?? new Dictionary<int, DataBlock>());
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
        SeasonProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnSeasonChanged());
        SelectedRegionProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnSelectedRegionChanged(e));
    }

    private bool _useSeasonImages;

    public HexMapControl()
    {
        _useSeasonImages = GlobalSettings.Get<bool>(GlobalSettings.MAP_USE_SEASON_IMAGES);
        Regions = new Dictionary<int, DataBlock>();
        UpdateTerrainImagesFromSeason(Season);
        Season = Seasons.UNKNOWN;
        this.PointerPressed += OnPointerPressed;
    }

    private void OnSeasonChanged()
    {
        UpdateTerrainImagesFromSeason(Season);
    }

    private void OnHexSizeChanged()
    {
        ComputeRegionBounds();
        InvalidateMeasure();
        InvalidateVisual();
    }

    private void OnRegionsChanged()
    {
        ComputeRegionBounds();
        InvalidateMeasure();
        InvalidateVisual();
    }

    private double _minX, _maxX, _minY, _maxY;
    private bool _hasBounds = false;

    private void ComputeRegionBounds()
    {
        double hexSize = HexSize;
        _hasBounds = false;
        foreach (var kvp in Regions)
        {
            var region = kvp.Value;
            int q = region.GetX();
            int r = region.GetY();
            var pt = HexToPixelUncentered(q, r, hexSize);

            if (!_hasBounds)
            {
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
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        double hexSize = HexSize;
        foreach (var kvp in Regions)
        {
            var region = kvp.Value;

            // LATER: handle other planes to be displayerd as an alternative map
            var plane = (PlaneType)region.GetId();
            if (plane != PlaneType.WORLD)
            {
                // Only draw regions on the main plane for now
                continue;
            }

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
                // TODO: exclude also regions visible by travel, lighthouse...
                if (region.IsUnseenRegion())
                {
                    // 160 as first argb parameter is a ~63% opacity black
                    context.DrawGeometry(
                        new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)), 
                        null,
                        geometry
                    );
                }
                else
                { 
                }
                // Optionally, overlay a semi-transparent fill for effect
                // context.DrawGeometry(new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)), null, geometry);
            }
            else
            {
                // LATER: add a warning in console. Should not happen
                //context.DrawGeometry(Brushes.LightGray, null, geometry);
            }

            // Draw highlight if selected
            if (_selectedQ == x && _selectedR == y)
            {
                // compute thickness according t the hexSize (depends on zoom factor)
                // min 2px to keep it visible
                double highlightThickness = Math.Max(2, HexSize * 0.22);

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
        double hexSize = HexSize;
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

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);
        var (q, r) = PixelToHex(point, HexSize);

        // Find the region with the specified position
        int key = (int)new Coordinates(q, r, (int)PlaneType.WORLD);
        if (Regions.TryGetValue(key, out var region))
        {
            //System.Diagnostics.Debug.WriteLine($"Clicked hex: q={q}, r={r}, region found: {region}");
            _selectedQ = q;
            _selectedR = r;
            SelectedRegion = region;
            InvalidateVisual();

        }
        else
        {
            _selectedQ = null;
            _selectedR = null;
            SelectedRegion = null;
            InvalidateVisual();
        }
    }

    private void OnSelectedRegionChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var region = e.NewValue as DataBlock;
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

    private void ScrollToSelectedRegion()
    {
        if (SelectedRegion == null)
            return;

        // Calculate the pixel position of the selected region
        int q = SelectedRegion.GetX();
        int r = SelectedRegion.GetY();
        var center = HexToPixel(q, r, HexSize);

        // Find the parent ScrollViewer
        var scrollViewer = this.GetVisualAncestors()
            .OfType<ScrollViewer>()
            .FirstOrDefault();

        if (scrollViewer == null)
            return;

        // The visible area in content coordinates
        var viewLeft = scrollViewer.Offset.X;
        var viewTop = scrollViewer.Offset.Y;
        var viewRight = viewLeft + scrollViewer.Viewport.Width;
        var viewBottom = viewTop + scrollViewer.Viewport.Height;

        // Optionally, add a margin so the region isn't right at the edge
        double margin = HexSize * 0.5;

        // Check if the center is already visible (with margin)
        if (center.X >= viewLeft + margin && center.X <= viewRight - margin &&
            center.Y >= viewTop + margin && center.Y <= viewBottom - margin)
        {
            // Already visible, do nothing
            return;
        }

        // Center the region in the ScrollViewer
        double targetX = center.X - scrollViewer.Viewport.Width / 2;
        double targetY = center.Y - scrollViewer.Viewport.Height / 2;

        // Clamp to valid scroll range
        targetX = Math.Max(0, Math.Min(targetX, scrollViewer.Extent.Width - scrollViewer.Viewport.Width));
        targetY = Math.Max(0, Math.Min(targetY, scrollViewer.Extent.Height - scrollViewer.Viewport.Height));

        scrollViewer.Offset = new Vector(targetX, targetY);
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

    private void UpdateTerrainImagesFromSeason(Seasons season)
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
    private ConcurrentDictionary<int, Bitmap?> _terrainImages = new();

    private static readonly ConcurrentDictionary<int, Bitmap?> _standardTerrainImages = new();

    private Bitmap? GetTerrainImage(int terrain)
    {
        _terrainImages.TryGetValue(terrain, out var bmp);
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
        else
        {

        }
        if (bitmap == null && shouldFind)
        {
            Debug.WriteLine($"[HEXMAPCONTROL] WARNING | could not find terrain image for terrain={terrain} resourcePathname={resourcePathname}");
        }
        return bitmap;
    }
}