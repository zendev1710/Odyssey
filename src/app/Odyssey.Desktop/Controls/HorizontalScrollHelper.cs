using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Odyssey.Controls;

public static class HorizontalScrollHelper
{
    public static ScrollViewer? FindScrollViewer(RepeatButton arrow)
    {
        Grid? buttonRow = arrow.GetVisualAncestors().OfType<Grid>().FirstOrDefault();
        return buttonRow?.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
    }

    public static void ScrollBy(ScrollViewer scrollViewer, double amount)
    {
        double maxOffset = Math.Max(0, scrollViewer.Extent.Width - scrollViewer.Viewport.Width);
        double offset = Math.Clamp(scrollViewer.Offset.X + amount, 0, maxOffset);
        scrollViewer.Offset = new Vector(offset, scrollViewer.Offset.Y);
        UpdateArrowVisibility(scrollViewer);
    }

    public static void UpdateArrowVisibility(ScrollViewer scrollViewer)
    {
        Grid? buttonRow = scrollViewer.GetVisualAncestors().OfType<Grid>().FirstOrDefault();
        RepeatButton[] arrows = buttonRow?.Children.OfType<RepeatButton>().ToArray() ?? [];
        if (arrows.Length != 2)
        {
            return;
        }

        RepeatButton? leftArrow = arrows.FirstOrDefault(arrow => Grid.GetColumn(arrow) == 0);
        RepeatButton? rightArrow = arrows.FirstOrDefault(arrow => Grid.GetColumn(arrow) == 2);
        if (leftArrow is null || rightArrow is null)
        {
            return;
        }

        double offset = scrollViewer.Offset.X;
        double viewportEnd = offset + scrollViewer.Viewport.Width;
        bool canScrollLeft = offset > 1;
        bool canScrollRight = viewportEnd < scrollViewer.Extent.Width - 1;
        leftArrow.IsVisible = canScrollLeft;
        rightArrow.IsVisible = canScrollRight;
    }
}
