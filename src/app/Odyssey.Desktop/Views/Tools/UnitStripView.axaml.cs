using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Linq;
using Odyssey.ViewModels.Tools;

namespace Odyssey.Views.Tools;

public partial class UnitStripView : UserControl
{
    private UnitStripViewModel? _viewModel;
    private int? _pendingRevealUnitId;

    public UnitStripView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => UpdateViewModel();
        AttachedToVisualTree += (_, _) => ScrollSelectedUnitIntoView();
        UpdateViewModel();
    }

    private void UpdateViewModel()
    {
        if (_viewModel is not null)
        {
            _viewModel.SelectedUnitNeedsReveal -= ViewModel_SelectedUnitNeedsReveal;
        }

        _viewModel = DataContext as UnitStripViewModel;
        if (_viewModel is not null)
        {
            _viewModel.SelectedUnitNeedsReveal += ViewModel_SelectedUnitNeedsReveal;
        }
    }

    private void ViewModel_SelectedUnitNeedsReveal(int unitId)
    {
        _pendingRevealUnitId = unitId;
        ScrollSelectedUnitIntoView();
    }

    private void ScrollSelectedUnitIntoView()
    {
        if (_pendingRevealUnitId is not int selectedUnitId)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (_pendingRevealUnitId != selectedUnitId || !IsVisible || _viewModel is not UnitStripViewModel viewModel ||
                viewModel.SelectedUnitId != selectedUnitId)
            {
                return;
            }

            UnitViewModel? selectedUnit = viewModel.Units.FirstOrDefault(unit => unit.Model?.Id == selectedUnitId);
            if (selectedUnit is null || this.FindControl<ItemsControl>("UnitsList") is not ItemsControl unitsList ||
                this.FindControl<ScrollViewer>("UnitsScrollViewer") is not ScrollViewer scrollViewer ||
                unitsList.ContainerFromItem(selectedUnit) is not Control itemContainer)
            {
                if (selectedUnit is null)
                {
                    _pendingRevealUnitId = null;
                }
                return;
            }

            Button? badge = itemContainer.GetVisualDescendants().OfType<Button>().FirstOrDefault();
            Point? translated = badge?.TranslatePoint(new Point(0, 0), scrollViewer);
            Point? rowTranslated = itemContainer.TranslatePoint(new Point(0, 0), scrollViewer);
            double viewportHeight = scrollViewer.Viewport.Height;
            if (translated is null || rowTranslated is null || badge is null || viewportHeight <= 0)
            {
                return;
            }

            double badgeTop = translated.Value.Y;
            double badgeBottom = badgeTop + badge.Bounds.Height;
            double rowTop = rowTranslated.Value.Y;
            double rowBottom = rowTop + itemContainer.Bounds.Height;
            double currentOffset = scrollViewer.Offset.Y;
            double maxOffset = Math.Max(0, scrollViewer.Extent.Height - viewportHeight);

            if (badgeTop >= 0 && badgeBottom <= viewportHeight)
            {
                _pendingRevealUnitId = null;
                return;
            }

            double targetOffset;
            if (badgeTop < 0)
            {
                targetOffset = currentOffset + rowBottom - viewportHeight;
            }
            else if (badgeBottom > viewportHeight)
            {
                targetOffset = currentOffset + rowTop;
            }
            else
            {
                _pendingRevealUnitId = null;
                return;
            }

            targetOffset = Math.Clamp(targetOffset, 0, maxOffset);
            if (Math.Abs(scrollViewer.Offset.Y - targetOffset) > 0.5)
            {
                scrollViewer.Offset = new Vector(scrollViewer.Offset.X, targetOffset);
            }
            _pendingRevealUnitId = null;
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}