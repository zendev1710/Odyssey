using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using Odyssey.Assets;
using Odyssey.Core.Theme;
using Odyssey.Settings;
using Odyssey.Themes;
using Odyssey.Views;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using Prism.DryIoc;
using Prism.Ioc;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Odyssey;

public class App : PrismApplication
{
    private readonly Styles _themeStylesContainer = new();

    private FluentTheme? _fluentTheme;
    private SimpleTheme? _simpleTheme;
    private IStyle? _dockFluent, _dockSimple;
    private IStyle? _dataGridFluent, _dataGridSimple;
    private IStyle? _avaloniaEditFluent, _avaloniaEditSimple;
    private IStyle? _configFactoryFluent/*, _configFactorySimple*/;

    public static IThemeModeManager? ThemeModeManager;

    public override void Initialize()
    {
        Styles.Add(_themeStylesContainer);

        var currentCulture = CultureInfo.CurrentCulture;
        var currentUICulture = CultureInfo.CurrentUICulture;

        Debug.WriteLine($"[APPLICATION] Initialize Culture={currentCulture} UICulture={currentUICulture}");

        LoadSettings();

        string lang = GlobalSettings.Get<string>(GlobalSettings.LANGUAGE);
        Lang.Culture = new CultureInfo(lang);
        CultureInfo.CurrentCulture = new CultureInfo(lang);
        CultureInfo.CurrentUICulture = new CultureInfo(lang);

        var currentCultureAfter = CultureInfo.CurrentCulture;
        var currentUICultureAfter = CultureInfo.CurrentUICulture;
        Debug.WriteLine($"[APPLICATION] Culture after={currentCultureAfter} UICulture after={currentUICultureAfter}");

        ThemeModeManager = new ModeThemeManager();

        // LATER: check when it should ideally called in this method
        AvaloniaXamlLoader.Load(this);

        // Dock Fluent and Simple themes
        Resources.Add("DockFluent", new DockFluentTheme());
        Resources.Add("DockSimple", new DockSimpleTheme());

        // Avalonia Fluent and Simple themes
        // LATER: check if app.xaml Avalonia two themes definition should better be defined here (new FluentTheme()...)
        _fluentTheme = (FluentTheme)Resources["FluentTheme"]!;
        _simpleTheme = (SimpleTheme)Resources["SimpleTheme"]!;
        //_fluentTheme = new FluentTheme();
        //_simpleTheme = new SimpleTheme();

        _dockFluent = (IStyle)Resources["DockFluent"]!;
        _dockSimple = (IStyle)Resources["DockSimple"]!;
        _dataGridFluent = (IStyle)Resources["DataGridFluent"]!;
        _dataGridSimple = (IStyle)Resources["DataGridSimple"]!;
        _avaloniaEditFluent = (IStyle)Resources["AvaloniaEditFluent"]!;
        _avaloniaEditSimple = (IStyle)Resources["AvaloniaEditSimple"]!;
        _configFactoryFluent = (IStyle)Resources["ConfigFactoryFluent"]!;
        //_configFactorySimple = (IStyle)Resources["ConfigFactorySimple"]!;

        Theme initialTheme = (Theme)GlobalSettings.Get<int>(GlobalSettings.THEME);
        ApplicationThemeMode initialThemeMode = (ApplicationThemeMode)GlobalSettings.Get<int>(GlobalSettings.THEME_MODE);

        UpdateThemeResources(initialTheme, initialThemeMode);

        //AvaloniaXamlLoader.Load(this);

        // Note: Simple theme does not exist for ConfigFactory, so Settings view will not be styled when Simple theme is used
        SetThemes(initialTheme);

        ThemeModeManager.SetMode(initialThemeMode);

        // With it, create a MainWindowViewModel via Prism application initialization
        base.Initialize();
    }

    private void UpdateThemeResources(Theme theme, ApplicationThemeMode themeMode)
    {
        // TODO: update mainWindow resource AcrylicMaterial TintColor property according to theme
        // - FluentTheme => SystemAltHighColor dynamic resource should be set to TintColor
        // - SimpleTHeme => ThemeBackgroundColor dynamic resource should be set to TintColor

        // TODO: update mainWindow RenderOptions.BitmapInterpolationMode property according to theme
        // - FluentTheme => HighQuality value should be set
        // - SimpleTHeme => what value should be set ? it seams HighQuality value should be ok

        // - FluentTheme => app.Resources["AcrylicTintColor"] = app.Resources["SystemAltHighColor"]!;
        // app.Resources["AcrylicTintColor"] = app.Resources["ThemeBackgroundColor"]!;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // TODO: handle Args as a file to open
            //this.
            //desktop.Args;
            /*
            DataContext = Container.Resolve<<IMainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = DataContext
            };
            */

            /*
#if DEBUG
            mainWindow.AttachDockDebug(() => mainWindowViewModel.Layout, new KeyGesture(Key.F11));
            mainWindow.AttachDockDebugOverlay(new KeyGesture(Key.F9));
#endif
            */
            // CHECK: DataContext for MainWindow and closing system (cf Dock examples apps)
            // var mainWindow = Container.Resolve<MainWindow>();
            // mainWindow.Closing += (_, _) => { mainWindowViewModel.CloseLayout(); };
            // desktopLifetime.MainWindow = mainWindow;
            // desktopLifetime.Exit += (_, _) => { mainWindowViewModel.CloseLayout();};
        }

        base.OnFrameworkInitializationCompleted();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    /// <summary>
    /// Prism feature. Register types in the container.
    /// </summary>
    /// <param name="containerRegistry"></param>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        Debug.WriteLine("[APPLICATION] RegisterTypes");

        // Wire-up services and navigation Views here.

        // Resolve to get an instance of the service        
        //containerRegistry.RegisterSingleton<IMySQervice, MyService>();
    }

    // Prism
    /// <summary>User interface entry point, called after Register and ConfigureModules.</summary>
    /// <returns>Startup View.</returns>
    /// 
    protected override AvaloniaObject CreateShell()
    {
        Debug.WriteLine("[APPLICATION] Create shell");
        return Container.Resolve<MainWindow>();
    }

    /// <summary>
    /// Load application settings from config.json file.
    /// </summary>
    private static void LoadSettings()
    {
        ConfigFactory.ConfigFactory.Build<GlobalSettings>();
    }

    private Theme _prevTheme;

    public static Theme CurrentTheme => ((App)Current!)._prevTheme;

    /// <summary>
    /// Set the current theme with the specified one.
    /// It overrides application styles according to the theme, for the following controls :
    /// - Avalonia
    /// - Dock
    /// - DataGrid
    /// - AvaloniaEdit
    /// - ConfigFactory (limitation: only Fluent theme is available for these controls)
    /// </summary>
    /// <param name="theme"></param>
    public static void SetThemes(Theme theme)
    {
        var app = (App)Current!;
        var prevTheme = app._prevTheme;
        app._prevTheme = theme;
        var shouldReopenWindow = prevTheme != theme;

        if (app._themeStylesContainer.Count == 0)
        {
            app._themeStylesContainer.Add(new Style());
            app._themeStylesContainer.Add(new Style());
            app._themeStylesContainer.Add(new Style());
            app._themeStylesContainer.Add(new Style());
            app._themeStylesContainer.Add(new Style());
        }

        if (theme == Theme.Fluent)
        {
            app._themeStylesContainer[0] = app._fluentTheme!;
            app._themeStylesContainer[1] = app._dockFluent!;
            app._themeStylesContainer[2] = app._dataGridFluent!;
            app._themeStylesContainer[3] = app._avaloniaEditFluent!;
            app._themeStylesContainer[4] = app._configFactoryFluent!;

        }
        else if (theme == Theme.Simple)
        {
            app._themeStylesContainer[0] = app._simpleTheme!;
            app._themeStylesContainer[1] = app._dockSimple!;
            app._themeStylesContainer[2] = app._dataGridSimple!;
            app._themeStylesContainer[3] = app._avaloniaEditSimple!;
            //app._themeStylesContainer[4] = app._configFactorySimple!;
        }

        if (shouldReopenWindow)
        {
            if (app.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                // TODO: fix window reopening when switching theme between SimpleTheme and FluentTheme
                /*
                var oldWindow = desktopLifetime.MainWindow;
                var newWindow = new MainWindow();
                desktopLifetime.MainWindow = newWindow;
                newWindow.Show();
                oldWindow?.Close();
                */
            }
            /*
            else if (app.ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
            {
                singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainView { DataContext = new MainWindowViewModel() };
            }
            */
            else if (app.ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                singleViewLifetime.MainView = new MainView();
            }
        }
    }
    /*
    public static string GetAppVersion()
    {
        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        return version ?? "Unknown";
    }
    */
}
