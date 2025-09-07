using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Odyssey.ViewModels.Tools;
using System;

namespace Odyssey.Views.Tools;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();

        // Center after layout is complete
        this.AttachedToVisualTree += (_, __) => CenterMap();
    }

    private async void CenterMap()
    {
        // Wait for layout to complete
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            MapScrollViewer = this.FindControl<ScrollViewer>("MapScrollViewer") as ScrollViewer;
            if (MapScrollViewer?.Content is Control mapControl)
            {
                double targetX = (mapControl.Bounds.Width - MapScrollViewer.Bounds.Width) / 2;
                double targetY = (mapControl.Bounds.Height - MapScrollViewer.Bounds.Height) / 2;

                MapScrollViewer.Offset = new Avalonia.Vector(
                    Math.Max(0, targetX),
                    Math.Max(0, targetY)
                );
            }
        }, DispatcherPriority.Background);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Handles settings properties changes and save events.
    /// </summary>
    /// <param name="e">Data context changed event</param>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
    }
}
