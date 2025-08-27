using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Odyssey.Help;
using Odyssey.ViewModels;
using System.ComponentModel;

namespace Odyssey.Views;

public partial class MainWindow : Window
{
    private WindowState _previousWindowState = WindowState.Normal;
    private SystemDecorations _previousSystemDecorations = SystemDecorations.Full;

    public MainWindow()
    {
        InitializeComponent();

        this.DataContextChanged += (_, e) =>
        {
            if (this.DataContext is MainWindowViewModel vm)
            {
                // Handle ViewModel property changed events
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        };

        // Just if I want to have a custom top bar having the window title
        /*
        TopBar = this.FindControl<Border>("TopBar");
        TopBar.PointerPressed += TopBar_PointerPressed;
        */
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
            if (vm.IsFullscreen)
            {
                // Store the window state before going fullscreen
                this._previousWindowState = this.WindowState;
                this._previousSystemDecorations = this.SystemDecorations;
                this.WindowState = WindowState.FullScreen;
                this.SystemDecorations = SystemDecorations.None;
            }
            else
            {
                // Set the Window state to the previous one, stored on the last toggle fullscreen command
                this.WindowState = this._previousWindowState;
                this.SystemDecorations = _previousSystemDecorations;
            }
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
