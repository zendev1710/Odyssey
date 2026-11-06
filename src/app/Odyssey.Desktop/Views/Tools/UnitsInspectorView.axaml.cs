using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Odyssey.Controls;
using Odyssey.ViewModels.Tools;
using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

namespace Odyssey.Views.Tools
{
    public partial class UnitsInspectorView : UserControl
    {
        private UnitsInspectorViewModel? _viewModel;
        private GridSplitter? _gridSplitter;
        private int? _pendingRevealUnitId;
        private bool _rebuildPending;

        public UnitsInspectorView()
        {
            InitializeComponent();
            _gridSplitter = this.FindControl<GridSplitter>("UnitsGridSplitter");
            if (this.FindControl<Grid>("UnitsGrid") is Grid unitsGrid)
            {
                unitsGrid.AddHandler(Button.ClickEvent, UnitsGrid_Click, RoutingStrategies.Bubble);
            }

            if (Design.IsDesignMode)
            {
                BuildDesignPreview();
                return;
            }

            DataContextChanged += (_, _) => UpdateViewModel();
            AttachedToVisualTree += (_, _) => ScrollSelectedUnitIntoView();
            UpdateViewModel();
        }

        private void UpdateViewModel()
        {
            if (_viewModel is not null)
            {
                _viewModel.SelectedUnitNeedsReveal -= ViewModel_SelectedUnitNeedsReveal;
                _viewModel.Units.CollectionChanged -= Units_CollectionChanged;
            }

            _viewModel = DataContext as UnitsInspectorViewModel;
            if (_viewModel is not null)
            {
                _viewModel.SelectedUnitNeedsReveal += ViewModel_SelectedUnitNeedsReveal;
                _viewModel.Units.CollectionChanged += Units_CollectionChanged;
            }
            ScheduleGridRebuild();
        }

        private void Units_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ScheduleGridRebuild();

        private void ScheduleGridRebuild()
        {
            if (_rebuildPending)
            {
                return;
            }

            _rebuildPending = true;
            Dispatcher.UIThread.Post(() =>
            {
                _rebuildPending = false;
                RebuildUnitsGrid();
            });
        }

        private void RebuildUnitsGrid()
        {
            if (this.FindControl<Grid>("UnitsGrid") is not Grid unitsGrid || _gridSplitter is null)
            {
                return;
            }

            IReadOnlyList<UnitViewModel> units = _viewModel is null
                ? Array.Empty<UnitViewModel>()
                : _viewModel.Units;
            PopulateUnitsGrid(unitsGrid, units);
        }

        private void BuildDesignPreview()
        {
            if (this.FindControl<Grid>("UnitsGrid") is not Grid unitsGrid)
            {
                return;
            }

            unitsGrid.IsVisible = true;
            unitsGrid.ColumnDefinitions[1].Width = new GridLength(2, GridUnitType.Star);
            if (_gridSplitter is not null)
            {
                _gridSplitter.IsVisible = true;
            }
            var sampleUnits = new[]
            {
                new UnitViewModel("001", "Design unit"),
                new UnitViewModel("002", "Another unit")
            };
            sampleUnits[0].AreSkillsVisible = true;
            sampleUnits[0].SkillItems.Add(new UnitSkillViewModel(string.Empty, "Magic", 3));
            sampleUnits[0].InventoryItems.Add(new UnitItemViewModel(string.Empty, "Sword", 1, "item"));
            PopulateUnitsGrid(unitsGrid, sampleUnits);
        }

        private void PopulateUnitsGrid(Grid unitsGrid, IReadOnlyList<UnitViewModel> units)
        {
            unitsGrid.Children.Clear();
            unitsGrid.RowDefinitions.Clear();
            for (int row = 0; row < units.Count; row++)
            {
                UnitViewModel unit = units[row];
                unitsGrid.RowDefinitions.Add(new RowDefinition(new GridLength(108)));
                var detailsGrid = new Grid { ColumnSpacing = 6, Margin = new Thickness(4) };
                detailsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(180)));
                detailsGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                Grid.SetRow(detailsGrid, row);
                Grid.SetColumn(detailsGrid, 0);
                unitsGrid.Children.Add(detailsGrid);

                AddCell(detailsGrid, unit, "UnitIdentityTemplate", 0, 0);
                AddCell(detailsGrid, unit, "UnitSkillsAndInventoryTemplate", 0, 1, new Thickness(0, 0, 6, 0));
                AddCell(unitsGrid, unit, "UnitOrdersTemplate", row, 1, new Thickness(4));

                var selectionBorder = new Border
                {
                    DataContext = unit,
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(14),
                    Background = Brushes.Transparent,
                    IsHitTestVisible = false
                };
                selectionBorder.Bind(Border.BorderBrushProperty, new Binding(nameof(UnitViewModel.FactionStatusBrush)));
                selectionBorder.Bind(Visual.IsVisibleProperty, new Binding(nameof(UnitViewModel.IsSelected)));
                Grid.SetRow(selectionBorder, row);
                Grid.SetColumnSpan(selectionBorder, 2);
                unitsGrid.Children.Add(selectionBorder);
            }

            if (units.Count > 0 && _gridSplitter is not null)
            {
                Grid.SetRow(_gridSplitter, 0);
                Grid.SetRowSpan(_gridSplitter, units.Count);
                Grid.SetColumn(_gridSplitter, 0);
                unitsGrid.Children.Add(_gridSplitter);
            }
        }

        private void AddCell(Grid grid, UnitViewModel unit, string templateKey, int row, int column, Thickness margin = default)
        {
            var cell = new ContentPresenter
            {
                Content = unit,
                ContentTemplate = (IDataTemplate)Resources[templateKey]!,
                Margin = margin
            };
            EventHandler? layoutUpdatedHandler = null;
            layoutUpdatedHandler = (_, _) =>
            {
                ScrollViewer[] scrollViewers = cell.GetVisualDescendants().OfType<ScrollViewer>().ToArray();
                if (scrollViewers.Length == 0)
                {
                    return;
                }

                cell.LayoutUpdated -= layoutUpdatedHandler;
                foreach (ScrollViewer scrollViewer in scrollViewers)
                {
                    scrollViewer.LayoutUpdated += ScrollViewer_LayoutUpdated;
                }
            };
            cell.LayoutUpdated += layoutUpdatedHandler;
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, column);
            grid.Children.Add(cell);
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
                if (_pendingRevealUnitId != selectedUnitId || !IsVisible || _viewModel is not UnitsInspectorViewModel viewModel ||
                    viewModel.SelectedUnitId != selectedUnitId)
                {
                    return;
                }

                UnitViewModel? selectedUnit = viewModel.Units.FirstOrDefault(unit => unit.Model?.Id == selectedUnitId);
                if (selectedUnit is null || this.FindControl<Grid>("UnitsGrid") is not Grid unitsGrid ||
                    this.FindControl<ScrollViewer>("UnitsScrollViewer") is not ScrollViewer scrollViewer)
                {
                    if (selectedUnit is null)
                    {
                        _pendingRevealUnitId = null;
                    }
                    return;
                }

                ContentPresenter? identityCell = unitsGrid.GetVisualDescendants().OfType<ContentPresenter>()
                    .FirstOrDefault(cell => ReferenceEquals(cell.Content, selectedUnit) && Grid.GetColumn(cell) == 0);
                Point? rowTranslated = identityCell?.TranslatePoint(new Point(0, 0), scrollViewer);
                double viewportHeight = scrollViewer.Viewport.Height;
                if (rowTranslated is null || identityCell is null || viewportHeight <= 0)
                {
                    return;
                }

                double rowTop = rowTranslated.Value.Y;
                double rowBottom = rowTop + identityCell.Bounds.Height;
                double currentOffset = scrollViewer.Offset.Y;
                double maxOffset = Math.Max(0, scrollViewer.Extent.Height - viewportHeight);

                if (rowTop >= 0 && rowBottom <= viewportHeight)
                {
                    _pendingRevealUnitId = null;
                    return;
                }

                double targetOffset;
                if (rowBottom <= 0 || rowTop >= viewportHeight)
                {
                    targetOffset = currentOffset + (rowTop + rowBottom - viewportHeight) / 2;
                }
                else if (rowTop < 0)
                {
                    targetOffset = currentOffset + rowTop;
                }
                else if (rowBottom > viewportHeight)
                {
                    targetOffset = currentOffset + rowBottom - viewportHeight;
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

        private void UnitsGrid_Click(object? sender, RoutedEventArgs e)
        {
            if (e.Source is RepeatButton arrow)
            {
                ScrollHorizontally(arrow, arrow.Content?.ToString() == "◀" ? -24 : 24);
            }
        }

        private void ScrollHorizontally(object? sender, double amount)
        {
            if (sender is RepeatButton arrow && HorizontalScrollHelper.FindScrollViewer(arrow) is ScrollViewer scrollViewer)
            {
                HorizontalScrollHelper.ScrollBy(scrollViewer, amount);
            }
        }

        private void ScrollViewer_LayoutUpdated(object? sender, EventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                HorizontalScrollHelper.UpdateArrowVisibility(scrollViewer);
            }
        }

        private void InitializeComponent()
        {
            // ensure XAML is loaded even if codegen is disabled
            AvaloniaXamlLoader.Load(this);
        }
    }
}