using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Odyssey.Models.Data;
using Odyssey.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using TextMateSharp.Internal.Rules;
using Avalonia.VisualTree;
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
    // Default size of hexagon as the width of the bounding square in pixels
    private static readonly double defaultHexSideSize = 64;

    // Default size of hexagon as from radius to corner
    private static readonly double defaultHexSize = defaultHexSideSize / Math.Sqrt(3);

    // 1. Change the property type and registration:
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
        PreloadTerrainResources();
        RegionsProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnRegionsChanged());
        HexSizeProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnHexSizeChanged());
        SelectedRegionProperty.Changed.AddClassHandler<HexMapControl>((ctrl, e) => ctrl.OnSelectedRegionChanged(e));
    }

    //private ISelector _selector = null!; // LATER: use it or remove it

    public HexMapControl()
    {
        Regions = new Dictionary<int, DataBlock>();
        this.PointerPressed += OnPointerPressed;
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
                // Optionally, overlay a semi-transparent fill for effect
                // context.DrawGeometry(new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)), null, geometry);
            }
            else
            {
                context.DrawGeometry(Brushes.LightGray, null, geometry);
            }

            // Draw highlight if selected
            if (_selectedQ == x && _selectedR == y)
            {
                // 1. Calcul dynamique de l'épaisseur selon le zoom
                double highlightThickness = Math.Max(2, HexSize * 0.22); // min 2px pour rester visible

                // Définition des variables d'inset
                double inset = 0.15;
                var centerInset = new Avalonia.Point(center.X * inset, center.Y * inset);

                // --- DESSIN DES ARÊTES FIXES ---
                var darkPen = new Pen(
                    new SolidColorBrush(Color.FromArgb(120, 255, 0, 0)),
                    //new SolidColorBrush(Color.FromArgb(150, 60, 60, 60)),
                    highlightThickness/*,
                    lineCap: PenLineCap.Round,
                    lineJoin: PenLineJoin.Round*/
                );

                // Crée une geometry fermée pour le contour
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
                // Draw the hex border on top (always last, now light gray)
                context.DrawGeometry(null, new Pen(new SolidColorBrush(Color.FromRgb(128, 220, 220)), 1), geometry);
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

    // Example: Efficient lookup in OnPointerPressed
    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetPosition(this);
        var (q, r) = PixelToHex(point, HexSize);

        // Find the region with the specified position
        int key = (int)new Coordinates(q, r, (int)PlaneType.WORLD);
        if (Regions.TryGetValue(key, out var region))
        {
            System.Diagnostics.Debug.WriteLine($"Clicked hex: q={q}, r={r}, region found: {region}");
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

    private int? _selectedQ = null;
    private int? _selectedR = null;

    private static readonly ConcurrentDictionary<int, Bitmap?> _terrainImages = new();

    private static Bitmap? GetTerrainImage(int terrain)
    {
        _terrainImages.TryGetValue(terrain, out var bmp);
        return bmp;
    }

    private static string? GetTerrainResource(int t)
    {
        string? iconName;
        switch (t)
        {
            case Terrains.OCEAN:
                iconName = "ocean";
                break;
            case Terrains.SWAMP:
                iconName = "swamp"; // Marais
                break;
            case Terrains.PLAINS:
                iconName = "plains";
                break;
            case Terrains.DESERT:
                iconName = "desert";
                break;
            case Terrains.FOREST:
                iconName = "forest";
                break;
            case Terrains.HIGHLAND:
                iconName = "highland";
                break;
            case Terrains.MOUNTAIN:
                iconName = "mountain";
                break;
            case Terrains.GLACIER:
                iconName = "glacier";
                break;
            case Terrains.VOLCANO:
                iconName = "volcano";
                break;
            case Terrains.VOLCANO_ACTIVE:
                iconName = "volcano";
                break;
            case Terrains.ICEBERG:
                iconName = "iceberg";
                break;
            case Terrains.CORRIDOR:
                iconName = "corridor";
                break;
            case Terrains.WALL:
                iconName = "wall";
                break;
            case Terrains.HALL:
                iconName = "hall";
                break;
            case Terrains.FOG:
                iconName = "fog";
                break;
            case Terrains.THICKFOG:
                iconName = "thickfog";
                break;
            case Terrains.FIREWALL:
                iconName = "firewall";
                break;
            case Terrains.MAHLSTROM:
                iconName = "mahlstrom";
                break;
            // CHECK: wanted icon for the folowing values
            case Terrains.UNKNOWN:
            case Terrains.PACKICE:
            case Terrains.ICEFLOE:
            case Terrains.LAST:
            default: return null;
        }
        return $"/Assets/Map/Terrains/{iconName}.gif";
    }

    private static readonly int[] KnownTerrains = new[]
    {
        Terrains.OCEAN, Terrains.SWAMP, Terrains.PLAINS, Terrains.DESERT, Terrains.FOREST,
        Terrains.HIGHLAND, Terrains.MOUNTAIN, Terrains.GLACIER, Terrains.VOLCANO, Terrains.VOLCANO_ACTIVE,
        Terrains.ICEBERG, Terrains.CORRIDOR, Terrains.WALL, Terrains.HALL, Terrains.FOG,
        Terrains.THICKFOG, Terrains.FIREWALL, Terrains.MAHLSTROM
        // Add more if needed
    };

    private static void PreloadTerrainResources()
    {
        foreach (var terrain in KnownTerrains)
        {
            // Get the resource path for this terrain
            var resourcePath = GetTerrainResource(terrain);
            if (string.IsNullOrEmpty(resourcePath))
                continue;

            // Compose the avares URI for Avalonia asset loading
            var uri = $"avares://Odyssey{resourcePath}";
            try
            {
                var uriObj = new Uri(uri);
                var stream = AssetLoader.Open(uriObj);
                var bitmap = new Bitmap(stream);
                _terrainImages[terrain] = bitmap;
            }
            catch
            {
                // Optionally log or handle missing/invalid images
                _terrainImages[terrain] = null;
            }
        }
    }
}