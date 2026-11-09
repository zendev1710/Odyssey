using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Odyssey.Controls;
using Odyssey.Messages;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.ViewModels.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Odyssey.Views.Tools;

public partial class MapView : UserControl, IRecipient<CenterMapMessage>
{
    public MapView()
    {
        InitializeComponent();
        _mapScrollViewer = this.FindControl<ScrollViewer>("MapScrollViewer");
        _hexMap = this.FindControl<HexMapControl>("HexMap");

        if (_mapScrollViewer is not null)
        {
            _mapScrollViewer.SizeChanged += (_, _) => TryCenterPendingSelection();
        }
        if (_hexMap is not null)
        {
            _hexMap.LayoutUpdated += (_, _) => TryCenterPendingSelection();
        }

        // Center after layout is complete
        this.AttachedToVisualTree += (_, __) =>
        {
            if (DataContext is MapViewModel { IsSelectionCenterPending: true })
            {
                ScheduleSelectionCenterCheck();
            }
            else
            {
                _ = CenterMap();
            }
        };

        // La vue écoute directement le signal de raccourci clavier
        WeakReferenceMessenger.Default.Register<CenterMapMessage>(this);
    }

    private Slider? _zoomSlider;
    private Popup? _zoomPopup;
    private ScrollViewer? _mapScrollViewer;
    private HexMapControl? _hexMap;
    private MapViewModel? _subscribedViewModel;
    private bool _selectionCenterCheckPosted;

    public async void Receive(CenterMapMessage message)
    {
        if (message.IsGlobal)
        {
            await CenterMap();
        }
        else
        {
            await CenterMapOnSelection();
        }
    }

    private async Task CenterMap()
    {
        // Wait for layout to complete
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (DataContext is not MapViewModel viewModel)
                return;

            if (!viewModel.HasDocument)
                return;

            var scrollViewer = this.FindControl<ScrollViewer>("MapScrollViewer") as ScrollViewer;
            if (scrollViewer?.Content is Control mapControl)
            {
                double targetX = (mapControl.Bounds.Width - scrollViewer.Bounds.Width) / 2;
                double targetY = (mapControl.Bounds.Height - scrollViewer.Bounds.Height) / 2;

                Debug.WriteLine($"[MAPVIEW] targetX={targetX} targetY={targetY}");
                scrollViewer.Offset = new Vector(
                    Math.Max(0, targetX),
                    Math.Max(0, targetY)
                );
            }
        }, DispatcherPriority.Background);
    }

    private async Task CenterMapOnSelection()
    {
        // Wait for layout to complete
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (DataContext is not MapViewModel viewModel || viewModel.SelectedRegion == null
                || viewModel.SelectedRegion.GetPlane() != (int?)viewModel.SelectedPlane?.Type)
                return;
            var selected = viewModel.SelectedRegion;
            Debug.WriteLine($"[MAPVIEWMODEL] CenterMapOnSelection: {selected} {viewModel.Zoom}");
            ScrollToSelectedRegion(selected);
        }, DispatcherPriority.Background);
    }

    private void ScrollToSelectedRegion(RegionModel selectedRegion)
    {
        if (selectedRegion == null)
            return;

        var scrollViewer = this.FindControl<ScrollViewer>("MapScrollViewer") as ScrollViewer;
        if (scrollViewer?.Content is HexMapControl mapControl)
        {
            int q = selectedRegion.GetX();
            int r = selectedRegion.GetY();

            Vector ? offset = mapControl.CenterToRegionCoordinates(q, r, scrollViewer.Offset, scrollViewer.Viewport, scrollViewer.Extent, true);
            if (offset is not null)
            {
                scrollViewer.Offset = (Vector)offset;
            }
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property.Name != "IsPointerOver")
        {
            Debug.WriteLine($"[MAPVIEW-XAML] OnPropertyChanged {change.Property.Name} > {change.NewValue}");
        }
        base.OnPropertyChanged(change);
    }

    /// <summary>
    /// Handles settings properties changes and save events.
    /// </summary>
    /// <param name="e">Data context changed event</param>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // Wire slider value updates to ViewModel.Zoom so the popup text updates live
        var vm = this.DataContext as MapViewModel;
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.PropertyChanged -= MapViewModel_PropertyChanged;
        }
        _subscribedViewModel = vm;
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.PropertyChanged += MapViewModel_PropertyChanged;
        }

        // Find controls
        _zoomSlider = this.FindControl<Slider>("ZoomSlider");
        _zoomPopup = this.FindControl<Popup>("ZoomPopup");
        if (vm is not null && this.FindControl<HexMapControl>("HexMap") is { } hexMap)
        {
            hexMap.Zoom = Math.Clamp((int)Math.Round(vm.Zoom), 20, 400);
        }

        // Unsubscribe previous event handler if any
        if (_zoomSlider is not null)
        {
            _zoomSlider.PropertyChanged -= ZoomSlider_PropertyChanged;
        }

        if (_zoomSlider is not null)
        {
            // Ensure pointer events are wired in case they were not wired in XAML
            _zoomSlider.PointerPressed += Slider_PointerPressed;
            _zoomSlider.PointerReleased += Slider_PointerReleased;
            // Attach property-changed handler to update VM.Zoom live
            _zoomSlider.PropertyChanged += ZoomSlider_PropertyChanged;
        }

        if (vm?.IsSelectionCenterPending == true)
        {
            ScheduleSelectionCenterCheck();
        }
    }

    private void MapViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not MapViewModel viewModel)
        {
            return;
        }

        if (e.PropertyName == nameof(viewModel.IsSelectionCenterPending)
            || (viewModel.IsSelectionCenterPending && e.PropertyName is nameof(viewModel.SelectedRegion)
                or nameof(viewModel.OccupiedRegions)
                or nameof(viewModel.UnoccupiedRegions)))
        {
            if (viewModel.IsSelectionCenterPending)
            {
                ScheduleSelectionCenterCheck();
            }
        }
    }

    private void ScheduleSelectionCenterCheck()
    {
        if (_selectionCenterCheckPosted)
        {
            return;
        }

        _selectionCenterCheckPosted = true;
        Dispatcher.UIThread.Post(() =>
        {
            _selectionCenterCheckPosted = false;
            TryCenterPendingSelection();
        }, DispatcherPriority.Background);
    }

    private void TryCenterPendingSelection()
    {
        if (DataContext is not MapViewModel viewModel
            || !viewModel.IsSelectionCenterPending
            || viewModel.SelectedRegion is not RegionModel selectedRegion
            || _mapScrollViewer is null
            || _hexMap is null
            || _mapScrollViewer.Viewport.Width <= 0
            || _mapScrollViewer.Viewport.Height <= 0
            || _mapScrollViewer.Extent.Width <= 0
            || _mapScrollViewer.Extent.Height <= 0
            || (!_hexMap.OccupiedRegions.ContainsValue(selectedRegion)
                && !_hexMap.UnoccupiedRegions.ContainsValue(selectedRegion)))
        {
            return;
        }

        Vector? offset = _hexMap.CenterToRegionCoordinates(
            selectedRegion.GetX(),
            selectedRegion.GetY(),
            _mapScrollViewer.Offset,
            _mapScrollViewer.Viewport,
            _mapScrollViewer.Extent,
            evenIfAlreadyVisible: true);
        if (offset is null)
        {
            return;
        }

        _mapScrollViewer.Offset = offset.Value;
        viewModel.CompleteSelectionCenterRequest();
    }

    private void Slider_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (this.DataContext is MapViewModel vm)
        {
            vm.IsDragging = true;
        }
        // Optionally open popup directly if not bound
        var vm2 = this.DataContext as MapViewModel;
        if (_zoomPopup is not null && !(vm2?.IsDragging ?? false))
        {
            _zoomPopup.IsOpen = true;
        }
    }

    private void Slider_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (this.DataContext is MapViewModel vm)
        {
            vm.IsDragging = false;
        }
        if (_zoomPopup is not null)
        {
            _zoomPopup.IsOpen = false;
        }
    }

    private void ZoomSlider_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Slider.ValueProperty)
        {
            if (this.DataContext is MapViewModel vm)
            {
                try
                {
                    vm.Zoom = Convert.ToDouble(e.NewValue);
                }
                catch
                {
                    // ignore conversion errors
                }
            }
        }
    }
}
