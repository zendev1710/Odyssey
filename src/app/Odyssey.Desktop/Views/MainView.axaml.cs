using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Odyssey.ViewModels;
using Dock.Settings;
using System.ComponentModel;
using System.Diagnostics;

namespace Odyssey.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        InitializeThemes();
        InitializeMenu();

        // Enable drag & drop to open files in the desktop application
        AddHandler(DragDrop.DropEvent, DropHandler);
        AddHandler(DragDrop.DragOverEvent, DragOverHandler);

        this.DataContextChanged += (_, e) =>
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                // Handle ViewModel property changed events
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DragOverHandler(object? sender, DragEventArgs e)
    {
        if (DataContext is IDropTarget dropTarget)
        {
            dropTarget.DragOver(sender, e);
        }
    }

    private void DropHandler(object? sender, DragEventArgs e)
    {
        if (DataContext is IDropTarget dropTarget)
        {
            dropTarget.Drop(sender, e);
        }
    }

    /// <summary>
    /// Called when a property in the ViewModel changes. 
    /// Toggles the window fullscreen state when the value of ViewModel IsFullscreen property changed.
    /// </summary>
    /// <param name="sender">This window</param>
    /// <param name="e">Contains event data like property name, old and new values</param>
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is MainWindowViewModel vm)
        {
            // You can add logging here to debug property changes
            Debug.WriteLine($"[MAINVIEW-AX] Property changed: {e.PropertyName}");
            if (e.PropertyName == nameof(vm.IsFullscreen))
            {
                if (vm.IsFullscreen)
                {
                    // Store the window state before going fullscreen
                   // this._previousWindowState = this.WindowState;
                    //this.WindowState = WindowState.FullScreen;
                    //this.SystemDecorations = SystemDecorations.None;
                }
                else
                {
                    // Set the Window state to the previous one, stored on the last toggle fullscreen command
                    //this.WindowState = this._previousWindowState;
                    //this.SystemDecorations = SystemDecorations.Full;
                }
            }
        }
    }

    private void InitializeThemes()
    {
        var theme = this.Find<Button>("ThemeButton");
        if (theme is { })
        {
            theme.Click += (_, _) =>
            {
                App.ThemeModeManager?.Switch();
            };
        }
    }

    private void InitializeMenu()
    {
        // Enable drag & drop
        if (VisualRoot is Window window)
        {
            window.SetValue(DockProperties.IsDragEnabledProperty, true);
            window.SetValue(DockProperties.IsDropEnabledProperty, true);
        }
    }
}
