using System;
using Avalonia;
using Avalonia.Controls;

namespace Odyssey.Controls;

public class InventoryGridPanel : Panel
{
    public static readonly StyledProperty<double> CellSizeProperty =
        AvaloniaProperty.Register<InventoryGridPanel, double>(nameof(CellSize), 42d);

    public static readonly StyledProperty<double> CellGapProperty =
        AvaloniaProperty.Register<InventoryGridPanel, double>(nameof(CellGap), 4d);

    public double CellSize
    {
        get => GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    public double CellGap
    {
        get => GetValue(CellGapProperty);
        set => SetValue(CellGapProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var gap = Math.Max(0, CellGap);
        var cell = Math.Max(1, CellSize);
        var count = Children.Count;
        if (count == 0)
            return default;

        var columns = availableSize.Width > 0
            ? Math.Max(1, (int)Math.Floor(availableSize.Width / (cell + gap)))
            : 1;

        var rows = Math.Max(1, (int)Math.Ceiling(count / (double)columns));
        foreach (var child in Children)
        {
            child.Measure(new Size(cell, cell));
        }

        var desiredWidth = columns * cell + Math.Max(0, columns - 1) * gap;
        var desiredHeight = rows * cell + Math.Max(0, rows - 1) * gap;
        return new Size(desiredWidth, desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var gap = Math.Max(0, CellGap);
        var cell = Math.Max(1, CellSize);
        var count = Children.Count;
        if (count == 0)
            return finalSize;

        var columns = finalSize.Width > 0
            ? Math.Max(1, (int)Math.Floor(finalSize.Width / (cell + gap)))
            : 1;

        var rows = Math.Max(1, (int)Math.Ceiling(count / (double)columns));

        for (var index = 0; index < count; index++)
        {
            var child = Children[index];
            var column = index % columns;
            var row = index / columns;
            var x = column * (cell + gap);
            var y = row * (cell + gap);
            child.Arrange(new Rect(x, y, cell, cell));
        }

        return new Size(finalSize.Width, rows * cell + Math.Max(0, rows - 1) * gap);
    }
}
