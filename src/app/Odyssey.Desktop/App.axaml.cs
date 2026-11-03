using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using Odyssey.Assets;
using Odyssey.Core.Theme;
using Odyssey.Settings;
using Odyssey.Themes;
using Odyssey.Views;
using Dock.Avalonia.Diagnostics.Controls;
using Dock.Avalonia.Diagnostics;
using Dock.Avalonia.Themes.Fluent;
using Dock.Avalonia.Themes.Simple;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using Odyssey.Core.Services;
using Avalonia.Input;
using Odyssey.ViewModels;
using Odyssey.TextMate;
using Odyssea.TextMate;

namespace Odyssey;

public class App : Application
{
    private readonly Styles _themeStylesContainer = new();

    private FluentTheme? _fluentTheme;
    private SimpleTheme? _simpleTheme;
    private IStyle? _dockFluent, _dockSimple;
    private IStyle? _dataGridFluent, _dataGridSimple;
    private IStyle? _avaloniaEditFluent, _avaloniaEditSimple;
    private IStyle? _configFactoryFluent/*, _configFactorySimple*/;
    private readonly EresseaOrdersRegistryOptions _themeRegistryOptions = new("Odyssey.Resources.Grammars.eressea.tmLanguage.json");

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

        ServiceLocator.Initialize();

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

        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        GlobalSettings.Shared.SettingChanged += OnGlobalSettingChanged;

        Theme initialTheme = (Theme)GlobalSettings.Get<int>(GlobalSettings.THEME);
        ApplicationThemeMode initialThemeMode = (ApplicationThemeMode)GlobalSettings.Get<int>(GlobalSettings.THEME_MODE);

        // Note: Simple theme does not exist for ConfigFactory, so Settings view will not be styled when Simple theme is used
        SetThemes(initialTheme);

        ThemeModeManager.SetMode(initialThemeMode);

#if DOCK_USE_GENERATED_APP_INITIALIZE_COMPONENT
        InitializeComponent();
#else
        AvaloniaXamlLoader.Load(this);
#endif

        UpdateThemeResources(initialThemeMode);
    }

    private void UpdateThemeResources(ApplicationThemeMode themeMode)
    {
        bool isDarkMode = themeMode switch
        {
            ApplicationThemeMode.Default => ActualThemeVariant == ThemeVariant.Dark,
            ApplicationThemeMode.Light => false,
            ApplicationThemeMode.Dark => true,
            _ => throw new ArgumentOutOfRangeException(nameof(themeMode), themeMode, "Unknown application theme mode.")
        };

        string settingName = isDarkMode
            ? GlobalSettings.UNIT_ORDERS_DARK_THEME
            : GlobalSettings.UNIT_ORDERS_LIGHT_THEME;
        ExtendedThemeName themeName = (ExtendedThemeName)GlobalSettings.Get<int>(settingName);
        IReadOnlyDictionary<string, string> themeColors = _themeRegistryOptions.LoadThemeColors(themeName);

        Color defaultBackground = isDarkMode ? Color.Parse("#2F2F2F") : Color.Parse("#EFEFEF");
        Color defaultForeground = isDarkMode ? Colors.White : Color.Parse("#212121");
        Color background = GetThemeColor(themeColors, "editor.background", defaultBackground);
        Color foreground = GetThemeColor(themeColors, "editor.foreground", defaultForeground);
        Color surface = GetThemeColor(themeColors, "sideBar.background", background);
        Color inputBackground = GetThemeColor(themeColors, "input.background", surface);
        Color border = GetThemeColor(themeColors, "editorGroup.border", Blend(surface, foreground, 0.2));
        Color focus = GetThemeColor(themeColors, "focusBorder", border);
        Color selection = GetThemeColor(
            themeColors,
            "list.activeSelectionBackground",
            GetThemeColor(themeColors, "editor.selectionBackground", focus));
        Color acrylicTint = GetThemeColor(themeColors, "titleBar.activeBackground", surface);

        Resources["ApplicationThemeBackgroundBrush"] = new SolidColorBrush(background);
        Resources["ApplicationThemeForegroundBrush"] = new SolidColorBrush(foreground);
        Resources["ApplicationThemeSurfaceBrush"] = new SolidColorBrush(surface);
        Resources["ApplicationThemeInputBrush"] = new SolidColorBrush(inputBackground);
        Resources["ApplicationThemeSelectionBrush"] = new SolidColorBrush(selection);
        Resources["TextControlBackground"] = new SolidColorBrush(inputBackground);
        Resources["TextControlBackgroundFocused"] = new SolidColorBrush(inputBackground);
        Resources["TextControlBackgroundPointerOver"] = new SolidColorBrush(inputBackground);
        Resources["TextControlForeground"] = new SolidColorBrush(foreground);
        Resources["TextForegroundColor"] = new SolidColorBrush(foreground);
        Resources["TextControlBorderBrush"] = new SolidColorBrush(border);
        Resources["TextControlBorderBrushPointerOver"] = new SolidColorBrush(focus);
        Resources["DockThemeBackgroundBrush"] = new SolidColorBrush(surface);
        Resources["DockThemeForegroundBrush"] = new SolidColorBrush(foreground);
        Resources["DockThemeBorderLowBrush"] = new SolidColorBrush(border);
        Resources["AcrylicTintColor"] = acrylicTint;
        Resources["AcrylicFallbackColor"] = background;
    }

    private static Color GetThemeColor(IReadOnlyDictionary<string, string> themeColors, string name, Color fallback)
    {
        if (!themeColors.TryGetValue(name, out string? value))
        {
            return fallback;
        }

        if (value.Length == 9 && value[0] == '#' &&
            uint.TryParse(value.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint rgba))
        {
            return Color.FromArgb(
                (byte)rgba,
                (byte)(rgba >> 24),
                (byte)(rgba >> 16),
                (byte)(rgba >> 8));
        }

        if (Color.TryParse(value, out Color color))
        {
            return color;
        }

        throw new InvalidDataException($"TextMate theme color '{name}' has invalid value '{value}'.");
    }

    private static Color Blend(Color background, Color foreground, double foregroundOpacity)
    {
        static byte Mix(byte backgroundChannel, byte foregroundChannel, double opacity) =>
            (byte)Math.Round(backgroundChannel * (1 - opacity) + foregroundChannel * opacity);

        return Color.FromArgb(
            byte.MaxValue,
            Mix(background.R, foreground.R, foregroundOpacity),
            Mix(background.G, foreground.G, foregroundOpacity),
            Mix(background.B, foreground.B, foregroundOpacity));
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e) =>
        UpdateThemeResources((ApplicationThemeMode)GlobalSettings.Get<int>(GlobalSettings.THEME_MODE));

    private void OnGlobalSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName != GlobalSettings.UNIT_ORDERS_LIGHT_THEME &&
            e.SettingName != GlobalSettings.UNIT_ORDERS_DARK_THEME)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
            UpdateThemeResources((ApplicationThemeMode)GlobalSettings.Get<int>(GlobalSettings.THEME_MODE)));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Debug.WriteLine($"[APPLICATION] OnFrameworkInitializationCompleted");
        //ServiceLocator.Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            var mainWindow = new MainWindow { DataContext = mainWindowViewModel };

#if DEBUG
            mainWindow.AttachDockDebug(() => mainWindowViewModel.Layout, new KeyGesture(Key.F11));
            mainWindow.AttachDockDebugOverlay(new KeyGesture(Key.F9));
#endif

            mainWindow.Closing += (_, _) => { mainWindowViewModel.CloseLayout(); };
            desktopLifetime.MainWindow = mainWindow;
            desktopLifetime.Exit += (_, _) => { mainWindowViewModel.CloseLayout();};
        }

        base.OnFrameworkInitializationCompleted();
        Debug.WriteLine($"[APPLICATION] OnFrameworkInitializationCompleted done");
    }

    /// <summary>
    /// Load application settings from config.json file.
    /// </summary>
    private static void LoadSettings()
    {
        Debug.WriteLine($"[APPLICATION] LoadSettings...");
        ConfigFactory.ConfigFactory.Build<GlobalSettings>();
        Debug.WriteLine($"[APPLICATION] LoadSettings done");
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
