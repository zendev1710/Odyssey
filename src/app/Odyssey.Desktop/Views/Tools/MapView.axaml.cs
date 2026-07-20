using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.Messaging;
using Odyssey.Controls;
using Odyssey.Messages;
using Odyssey.Models.Data;
using Odyssey.ViewModels.Tools;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.Threading.Tasks;

namespace Odyssey.Views.Tools;

public partial class MapView : UserControl, IRecipient<CenterMapMessage>
{
    public MapView()
    {
        InitializeComponent();

        // Center after layout is complete
        this.AttachedToVisualTree += (_, __) => CenterMap();

        // La vue écoute directement le signal de raccourci clavier
        WeakReferenceMessenger.Default.Register<CenterMapMessage>(this);
    }

    private Slider? _zoomSlider;
    private Popup? _zoomPopup;

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

    /*
    private void ScrollToHex(ScrollViewer scrollViewer, double hexX, double hexY)
    {
        // Vos formules de conversion d'hexagones en pixels
        double pixelX = ConvertHexToPixelX(hexX);
        double pixelY = ConvertHexToPixelY(hexY);

        // Centrage dans le viewport du ScrollViewer
        double targetOffsetX = pixelX - (scrollViewer.Viewport.Width / 2);
        double targetOffsetY = pixelY - (scrollViewer.Viewport.Height / 2);

        scrollViewer.Offset = new Avalonia.Vector(targetOffsetX, targetOffsetY);
    }

    private double ConvertHexToPixelX(double hexX) => hexX * 60.0 * 0.75;
    private double ConvertHexToPixelY(double hexY) => hexY * 52.0;
    */

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
            if (DataContext is not MapViewModel viewModel || viewModel.SelectedRegion == null)
                return;
            var selected = viewModel.SelectedRegion;
            Debug.WriteLine($"[MAPVIEWMODEL] CenterMapOnSelection: {selected} {viewModel.Zoom}");
            ScrollToSelectedRegion(selected);
        }, DispatcherPriority.Background);
    }

    private void ScrollToSelectedRegion(DataBlock selectedRegion)
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

    /*
    // Version sur sélection, utilisant la même approche propre du Dispatcher
    private async Task CenterMapOnSelection()
    {

                // 1. Vos formules de conversion d'hexagones en pixels
                double pixelX = ConvertHexToPixelX(selected.X);
                double pixelY = ConvertHexToPixelY(selected.Y);

                // 2. Calcul du décalage pour centrer le pixel ciblé
                double targetX = pixelX - (MapScrollViewer.Viewport.Width / 2);
                double targetY = pixelY - (MapScrollViewer.Viewport.Height / 2);

                Debug.WriteLine($"[MAPVIEW] Centering on Selection ({selected.X},{selected.Y}) -> targetX={targetX} targetY={targetY}");
                MapScrollViewer.Offset = new Vector(
                    Math.Max(0, targetX),
                    Math.Max(0, targetY)
                );
    }
    */

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

        // Find controls
        _zoomSlider = this.FindControl<Slider>("ZoomSlider");
        _zoomPopup = this.FindControl<Popup>("ZoomPopup");

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
