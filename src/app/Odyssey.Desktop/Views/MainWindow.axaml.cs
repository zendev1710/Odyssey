using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Odyssey.Assets;
using Odyssey.Settings;
using Odyssey.ViewModels;
using Odyssey.ViewModels.Documents;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Odyssey.Views;

public partial class MainWindow : Window
{
    private WindowState _previousWindowState = WindowState.Normal;
    private WindowDecorations _previousWindowDecorations = WindowDecorations.Full;
    private PixelPoint _lastNormalPosition;
    private Size _lastNormalClientSize;
    private bool _hasLastNormalBounds;
    private bool _applicationCloseConfirmed;
    private bool _applicationClosePromptPending;

    public MainWindow()
    {
        InitializeComponent();
        RestoreWindowBounds();
        PositionChanged += (_, _) => CaptureNormalBounds();
        SizeChanged += (_, _) => CaptureNormalBounds();
        Closing += (_, _) => SaveWindowBounds();
        Closing += MainWindow_Closing;

        this.DataContextChanged += (_, e) =>
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                vm.SaveChangesPrompt = file => SaveChangesDialog.ShowAsync(this, file.Title);
                vm.SaveFailurePrompt = file => SaveChangesDialog.ShowSaveFailureAsync(this, file.Title);
                vm.IsFullscreen = GlobalSettings.Shared.MainWindowIsFullscreen;
                // Handle ViewModel property changed events
                vm.PropertyChanged += ViewModel_PropertyChanged;
                if (vm.IsFullscreen)
                {
                    ApplyFullscreenState(true);
                }
                vm.OpenInitialDocumentIfNeeded();
            }
        };

        // Just if I want to have a custom top bar having the window title
        /*
        TopBar = this.FindControl<Border>("TopBar");
        TopBar.PointerPressed += TopBar_PointerPressed;
        */
    }

    private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (_applicationCloseConfirmed)
        {
            return;
        }

        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        e.Cancel = true;
        if (_applicationClosePromptPending)
        {
            return;
        }

        _applicationClosePromptPending = true;
        try
        {
            if (await vm.ConfirmCloseAllDocumentsAsync())
            {
                _applicationCloseConfirmed = true;
                Dispatcher.UIThread.Post(Close);
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"[MAINWINDOW] Unable to close application: {exception}");
            Console.Error.WriteLine(exception);
        }
        finally
        {
            _applicationClosePromptPending = false;
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
        if (sender is MainWindowViewModel vm && e.PropertyName == nameof(vm.IsFullscreen))
        {
            ApplyFullscreenState(vm.IsFullscreen);
        }
    }

    private void RestoreWindowBounds()
    {
        GlobalSettings settings = GlobalSettings.Shared;
        if (settings.HasSavedMainWindowBounds
            && double.IsFinite(settings.MainWindowClientWidth)
            && double.IsFinite(settings.MainWindowClientHeight)
            && settings.MainWindowClientWidth > 0
            && settings.MainWindowClientHeight > 0)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = new PixelPoint(settings.MainWindowPositionX, settings.MainWindowPositionY);
            Width = settings.MainWindowClientWidth;
            Height = settings.MainWindowClientHeight;
            _previousWindowState = settings.MainWindowWasMaximized
                ? WindowState.Maximized
                : WindowState.Normal;
            WindowState = _previousWindowState;
        }

        CaptureNormalBounds();
    }

    private void CaptureNormalBounds()
    {
        if (WindowState != WindowState.Normal)
        {
            return;
        }

        Size clientSize = ClientSize;
        if (clientSize.Width <= 0 || clientSize.Height <= 0)
        {
            clientSize = new Size(Width, Height);
        }

        if (!double.IsFinite(clientSize.Width) || !double.IsFinite(clientSize.Height)
            || clientSize.Width <= 0 || clientSize.Height <= 0)
        {
            return;
        }

        _lastNormalPosition = Position;
        _lastNormalClientSize = clientSize;
        _hasLastNormalBounds = true;
    }

    private void SaveWindowBounds()
    {
        CaptureNormalBounds();
        GlobalSettings settings = GlobalSettings.Shared;
        if (_hasLastNormalBounds)
        {
            settings.MainWindowPositionX = _lastNormalPosition.X;
            settings.MainWindowPositionY = _lastNormalPosition.Y;
            settings.MainWindowClientWidth = _lastNormalClientSize.Width;
            settings.MainWindowClientHeight = _lastNormalClientSize.Height;
            settings.HasSavedMainWindowBounds = true;
        }

        settings.MainWindowIsFullscreen = WindowState == WindowState.FullScreen;
        settings.MainWindowWasMaximized = WindowState == WindowState.Maximized
            || (WindowState == WindowState.FullScreen && _previousWindowState == WindowState.Maximized);
        settings.Save();
    }

    private void ApplyFullscreenState(bool isFullscreen)
    {
        if (isFullscreen)
        {
            CaptureNormalBounds();
            _previousWindowState = WindowState;
            _previousWindowDecorations = WindowDecorations;
            WindowState = WindowState.FullScreen;
            WindowDecorations = WindowDecorations.None;
        }
        else
        {
            WindowState = _previousWindowState;
            WindowDecorations = _previousWindowDecorations;
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void TopBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    /*
    private void OnHelpMenuClicked(object? sender, RoutedEventArgs e)
    {
        _ = HelpSystem.Instance.ShowHelp();
    }

    // Pour afficher une aide contextuelle
    private void OnShowGettingStartedHelp(object? sender, RoutedEventArgs e)
    {
        _ = HelpSystem.Instance.ShowHelp("getting-started");
    }

    // Dans le constructeur pour le raccourci F1
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F1)
        {
            _ = HelpSystem.Instance.ShowHelp();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }
    */
}
