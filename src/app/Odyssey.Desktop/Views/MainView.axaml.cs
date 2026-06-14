using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Serializer;
using Dock.Settings;
using Odyssey.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Odyssey.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        Debug.WriteLine("[MAIN VIEW] BEGIN");
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
        Debug.WriteLine("[MAIN VIEW] END");
    }

    /////// Drag and drop management ///////

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
                    //this.WindowDecorations = WindowDecorations.None;
                }
                else
                {
                    // Set the Window state to the previous one, stored on the last toggle fullscreen command
                    //this.WindowState = this._previousWindowState;
                    //this.WindowDecorations = WindowDecorations.Full;
                }
            }
        }
    }

    private void InitializeThemes()
    {
        // TODO: like Dock sample or not ?
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

    /////// Other view features management ///////

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
