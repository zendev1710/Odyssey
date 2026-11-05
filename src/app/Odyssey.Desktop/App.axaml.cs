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
        int configuredTheme = GlobalSettings.Get<int>(settingName);
        bool hasSelectedTheme = configuredTheme != GlobalSettings.DOCK_DEFAULT_TEXT_EDITOR_THEME;
        IReadOnlyDictionary<string, string> themeColors = hasSelectedTheme
            ? _themeRegistryOptions.LoadThemeColors((ExtendedThemeName)configuredTheme)
            : new Dictionary<string, string>();

        ResetThemeResourceOverrides();
        bool isFluentTheme = (Theme)GlobalSettings.Get<int>(GlobalSettings.THEME) == Theme.Fluent;
        IBrush nativeItemHover = GetRequiredThemeBrush(
            isFluentTheme ? "TreeViewItemBackgroundPointerOver" : "ThemeControlHighlightMidBrush");
        IBrush dockWorkbench = GetRequiredThemeBrush("DockSurfaceWorkbenchBrush");
        IBrush dockEditor = GetRequiredThemeBrush("DockSurfaceEditorBrush");
        IBrush dockPanel = GetRequiredThemeBrush("DockSurfacePanelBrush");
        IBrush dockForeground = GetRequiredThemeBrush("DockTabForegroundBrush");
        IBrush dockSelection = GetRequiredThemeBrush("DockTabActiveBackgroundBrush");
        IBrush dockBorder = GetRequiredThemeBrush("DockBorderSubtleBrush");
        IBrush dockInactiveHeader = dockPanel;
        IBrush dockActiveHeader = GetRequiredThemeBrush("DockSurfaceHeaderActiveBrush");

        Color defaultBackground = GetBrushColor(
            dockWorkbench,
            isDarkMode ? Color.Parse("#2F2F2F") : Color.Parse("#EFEFEF"));
        Color defaultForeground = GetBrushColor(
            dockForeground,
            isDarkMode ? Colors.White : Color.Parse("#212121"));
        Color background = GetThemeColor(themeColors, "editor.background", defaultBackground);
        Color foreground = GetThemeColor(themeColors, "editor.foreground", defaultForeground);
        Color surface = GetThemeColor(themeColors, "sideBar.background", GetBrushColor(dockPanel, defaultBackground));
        Color inputBackground = GetThemeColor(themeColors, "input.background", GetBrushColor(dockEditor, surface));
        Color border = GetThemeColor(
            themeColors,
            "editorGroup.border",
            GetBrushColor(dockBorder, Blend(surface, foreground, 0.2)));
        Color focus = GetThemeColor(themeColors, "focusBorder", border);
        Color selection = GetThemeColor(
            themeColors,
            "list.activeSelectionBackground",
            GetThemeColor(themeColors, "editor.selectionBackground", GetBrushColor(dockSelection, focus)));
        Color selectionForeground = GetThemeColor(
            themeColors,
            "list.activeSelectionForeground",
            foreground);
        Color menuBackground = GetThemeColor(themeColors, "menu.background", surface);
        Color menuForeground = GetThemeColor(themeColors, "menu.foreground", foreground);
        Color menuSelectionForeground = GetThemeColor(themeColors, "menu.selectionForeground", foreground);
        Color menuDisabledForeground = Blend(menuBackground, menuForeground, 0.5);
        Color menuBorder = GetThemeColor(themeColors, "menu.border", border);
        IBrush itemHover = hasSelectedTheme && themeColors.ContainsKey("list.hoverBackground")
            ? new SolidColorBrush(GetMutedThemeHoverColor(
                themeColors,
                "list.hoverBackground",
                menuBackground))
            : nativeItemHover;
        Color tooltipBackground = GetThemeColor(themeColors, "editorHoverWidget.background", menuBackground);
        Color tooltipForeground = GetThemeColor(themeColors, "editorHoverWidget.foreground", foreground);
        Color tooltipBorder = GetThemeColor(themeColors, "editorHoverWidget.border", border);
        Color acrylicTint = GetThemeColor(themeColors, "titleBar.activeBackground", surface);

        Resources["ApplicationThemeBackgroundBrush"] = GetThemeBrush(themeColors, "editor.background", dockWorkbench, background);
        Resources["ApplicationThemeBackgroundColor"] = background;
        Resources["ApplicationThemeForegroundBrush"] = GetThemeBrush(themeColors, "editor.foreground", dockForeground, foreground);
        Resources["ApplicationThemeForegroundColor"] = foreground;
        Resources["ApplicationThemeSurfaceBrush"] = GetThemeBrush(themeColors, "sideBar.background", dockPanel, surface);
        Resources["ApplicationThemeSurfaceColor"] = surface;
        Resources["ApplicationThemeHoverColor"] = GetBrushColor(itemHover, menuBackground);
        Resources["ApplicationConfigItemHoverColor"] = hasSelectedTheme
            ? GetBrushColor(itemHover, menuBackground)
            : GetReadableHoverColor(surface, foreground);
        Resources["ApplicationThemeInputBrush"] = GetThemeBrush(themeColors, "input.background", dockEditor, inputBackground);
        Resources["ApplicationThemeSelectionBrush"] = GetThemeBrush(
            themeColors,
            "list.activeSelectionBackground",
            dockSelection,
            selection);
        Resources["ApplicationThemeSelectionForegroundBrush"] = GetThemeBrush(
            themeColors,
            "list.activeSelectionForeground",
            dockForeground,
            selectionForeground);
        ApplySelectionControlThemeColors(hasSelectedTheme, isFluentTheme, selection, selectionForeground);
        Resources["ApplicationThemeHoverBrush"] = itemHover;
        Resources["ApplicationThemeMenuBackgroundBrush"] = GetThemeBrush(
            themeColors,
            "menu.background",
            dockPanel,
            menuBackground);
        Resources["ApplicationThemeMenuForegroundBrush"] = GetThemeBrush(
            themeColors,
            "menu.foreground",
            dockForeground,
            menuForeground);
        Resources["ApplicationThemeMenuBorderBrush"] = GetThemeBrush(
            themeColors,
            "menu.border",
            dockBorder,
            menuBorder);
        Resources["ApplicationThemeTooltipBackgroundBrush"] = GetThemeBrush(
            themeColors,
            "editorHoverWidget.background",
            dockPanel,
            tooltipBackground);
        Resources["ApplicationThemeTooltipForegroundBrush"] = GetThemeBrush(
            themeColors,
            "editorHoverWidget.foreground",
            dockForeground,
            tooltipForeground);
        Resources["ApplicationThemeTooltipBorderBrush"] = GetThemeBrush(
            themeColors,
            "editorHoverWidget.border",
            dockBorder,
            tooltipBorder);
        Resources["ApplicationThemeFindInputBrush"] = GetThemeBrush(
            themeColors,
            "input.background",
            dockEditor,
            inputBackground);
        Resources["SearchPanelBackgroundBrush"] = dockPanel;
        Resources["SearchPanelBorderBrush"] = dockBorder;
        Resources["PanelBackgroundBrush"] = GetThemeBrush(themeColors, "sideBar.background", dockPanel, surface);
        Resources["PanelBackground"] = GetThemeBrush(themeColors, "sideBar.background", dockPanel, surface);
        Resources["TextControlBackground"] = GetThemeBrush(themeColors, "input.background", dockEditor, inputBackground);
        Resources["TextControlBackgroundFocused"] = GetThemeBrush(themeColors, "input.background", dockEditor, inputBackground);
        Resources["TextControlBackgroundPointerOver"] = GetThemeBrush(themeColors, "input.background", dockEditor, inputBackground);
        Resources["TextControlForeground"] = GetThemeBrush(themeColors, "editor.foreground", dockForeground, foreground);
        Resources["TextForegroundColor"] = GetThemeBrush(themeColors, "editor.foreground", dockForeground, foreground);
        Resources["TextControlBorderBrush"] = GetThemeBrush(themeColors, "editorGroup.border", dockBorder, border);
        Resources["TextControlBorderBrushPointerOver"] = GetThemeBrush(themeColors, "focusBorder", dockBorder, focus);

        Resources["ApplicationDockTitleBarInactiveBrush"] = GetThemeBrush(
            themeColors,
            "editor.background",
            dockInactiveHeader,
            background);
        Resources["ApplicationDockTitleBarActiveBrush"] = GetThemeBrush(
            themeColors,
            "titleBar.activeBackground",
            dockActiveHeader,
            surface);

        _fluentTheme!.Resources["ExpanderHeaderBackground"] = GetThemeBrush(
            themeColors,
            "sideBar.background",
            dockPanel,
            surface);
        _fluentTheme.Resources["ExpanderHeaderBackgroundPointerOver"] = itemHover;
        _fluentTheme.Resources["ExpanderHeaderBackgroundPressed"] = itemHover;
        _fluentTheme.Resources["ExpanderHeaderBackgroundDisabled"] = GetThemeBrush(
            themeColors,
            "sideBar.background",
            dockPanel,
            surface);
        _fluentTheme.Resources["ExpanderHeaderForeground"] = GetThemeBrush(
            themeColors,
            "editor.foreground",
            dockForeground,
            foreground);
        _fluentTheme.Resources["ExpanderHeaderForegroundPointerOver"] = GetThemeBrush(
            themeColors,
            "editor.foreground",
            dockForeground,
            foreground);
        _fluentTheme.Resources["ExpanderHeaderForegroundPressed"] = GetThemeBrush(
            themeColors,
            "editor.foreground",
            dockForeground,
            foreground);
        _fluentTheme.Resources["ExpanderHeaderBorderBrush"] = GetThemeBrush(
            themeColors,
            "editorGroup.border",
            dockBorder,
            border);
        _fluentTheme.Resources["ExpanderContentBackground"] = GetThemeBrush(
            themeColors,
            "editor.background",
            dockWorkbench,
            background);
        _fluentTheme.Resources["ExpanderContentBorderBrush"] = GetThemeBrush(
            themeColors,
            "editorGroup.border",
            dockBorder,
            border);
        _fluentTheme.Resources["ExpanderChevronForeground"] = GetThemeBrush(
            themeColors,
            "editor.foreground",
            dockForeground,
            foreground);
        _fluentTheme.Resources["ExpanderChevronForegroundPointerOver"] = GetThemeBrush(
            themeColors,
            "editor.foreground",
            dockForeground,
            foreground);
        _fluentTheme.Resources["ExpanderChevronForegroundPressed"] = GetThemeBrush(
            themeColors,
            "editor.foreground",
            dockForeground,
            foreground);

        SetThemeBrushResource("MenuFlyoutItemBackgroundPointerOver", itemHover);
        SetThemeBrushResource("MenuFlyoutItemBackgroundPressed", itemHover);
        _fluentTheme!.Resources["MenuFlyoutItemBackgroundPointerOver"] = itemHover;
        _fluentTheme.Resources["MenuFlyoutItemBackgroundPressed"] = itemHover;
        _simpleTheme.Resources["ThemeControlHighlightMidBrush"] = itemHover;
        _simpleTheme!.Resources["ThemeAccentBrush4"] = itemHover;

        if (hasSelectedTheme)
        {
            _simpleTheme.Resources["ThemeBackgroundBrush"] = new SolidColorBrush(menuBackground);
            SetThemeBrushResource("MenuFlyoutPresenterBackground", menuBackground);
            SetThemeBrushResource("MenuFlyoutPresenterBorderBrush", menuBorder);
            SetThemeBrushResource("MenuFlyoutItemBackground", menuBackground);
            SetThemeBrushResource("MenuFlyoutItemForeground", menuForeground);
            SetThemeBrushResource("MenuFlyoutItemForegroundPointerOver", menuSelectionForeground);
            SetThemeBrushResource("MenuFlyoutItemForegroundPressed", menuSelectionForeground);
            SetThemeBrushResource("MenuFlyoutItemBackgroundDisabled", menuBackground);
            SetThemeBrushResource("MenuFlyoutItemForegroundDisabled", menuDisabledForeground);
            SetThemeBrushResource("MenuFlyoutItemKeyboardAcceleratorTextForeground", menuForeground);
            SetThemeBrushResource("MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver", menuSelectionForeground);
            SetThemeBrushResource("MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed", menuSelectionForeground);
            SetThemeBrushResource("MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled", menuDisabledForeground);
            SetThemeBrushResource("MenuFlyoutSubItemChevron", menuForeground);
            SetThemeBrushResource("MenuFlyoutSubItemChevronPointerOver", menuSelectionForeground);
            SetThemeBrushResource("MenuFlyoutSubItemChevronPressed", menuSelectionForeground);
            SetThemeBrushResource("MenuFlyoutSubItemChevronDisabled", menuDisabledForeground);
            SetThemeBrushResource("MenuFlyoutSubItemChevronSubMenuOpened", menuSelectionForeground);
            SetDockThemeBrush(themeColors, "editor.background", "DockSurfaceWorkbenchBrush");
            SetDockThemeBrush(themeColors, "editor.background", "DockSurfaceEditorBrush");
            SetDockThemeBrush(themeColors, "sideBar.background", "DockSurfaceSidebarBrush");
            SetDockThemeBrush(themeColors, "sideBar.background", "DockSurfacePanelBrush");
            SetDockThemeBrush(themeColors, "titleBar.inactiveBackground", "DockSurfaceHeaderBrush");
            SetDockThemeBrush(themeColors, "titleBar.activeBackground", "DockSurfaceHeaderActiveBrush");
            SetDockThemeBrush(themeColors, "editorGroup.border", "DockBorderSubtleBrush");
        }
        Resources["AcrylicTintColor"] = acrylicTint;
        Resources["AcrylicFallbackColor"] = background;
    }

    private static readonly string[] ThemeResourceOverrideKeys =
    [
        "DockSurfaceWorkbenchBrush",
        "DockSurfaceEditorBrush",
        "DockSurfaceSidebarBrush",
        "DockSurfacePanelBrush",
        "DockSurfaceHeaderBrush",
        "DockSurfaceHeaderActiveBrush",
        "DockBorderSubtleBrush",
        "ApplicationDockTitleBarInactiveBrush",
        "ApplicationDockTitleBarActiveBrush",
        "MenuFlyoutPresenterBackground",
        "MenuFlyoutPresenterBorderBrush",
        "MenuFlyoutItemBackground",
        "MenuFlyoutItemForeground",
        "MenuFlyoutItemBackgroundPointerOver",
        "MenuFlyoutItemForegroundPointerOver",
        "MenuFlyoutItemBackgroundPressed",
        "MenuFlyoutItemForegroundPressed",
        "MenuFlyoutItemBackgroundDisabled",
        "MenuFlyoutItemForegroundDisabled",
        "MenuFlyoutItemKeyboardAcceleratorTextForeground",
        "MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver",
        "MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed",
        "MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled",
        "MenuFlyoutSubItemChevron",
        "MenuFlyoutSubItemChevronPointerOver",
        "MenuFlyoutSubItemChevronPressed",
        "MenuFlyoutSubItemChevronDisabled",
        "MenuFlyoutSubItemChevronSubMenuOpened",
    ];

    private static readonly string[] FluentSelectionBrushResourceKeys =
    [
        "ToggleButtonBackgroundChecked",
        "ToggleButtonBackgroundCheckedPointerOver",
        "ToggleButtonBackgroundCheckedPressed",
        "ToggleButtonBackgroundCheckedDisabled",
        "ToggleButtonBorderBrushChecked",
        "ToggleButtonBorderBrushCheckedPointerOver",
        "ToggleButtonBorderBrushCheckedPressed",
        "ToggleButtonBorderBrushCheckedDisabled",
        "CheckBoxBorderBrushChecked",
        "CheckBoxBorderBrushCheckedPointerOver",
        "CheckBoxBorderBrushCheckedPressed",
        "CheckBoxBorderBrushCheckedDisabled",
        "CheckBoxCheckBackgroundFillChecked",
        "CheckBoxCheckBackgroundFillCheckedPointerOver",
        "CheckBoxCheckBackgroundFillCheckedPressed",
        "CheckBoxCheckBackgroundFillCheckedDisabled",
        "CheckBoxCheckBackgroundStrokeChecked",
        "CheckBoxCheckBackgroundStrokeCheckedPointerOver",
        "CheckBoxCheckBackgroundStrokeCheckedPressed",
        "CheckBoxCheckBackgroundStrokeCheckedDisabled",
        "SliderTrackValueFill",
        "SliderTrackValueFillPointerOver",
        "SliderTrackValueFillPressed",
        "SliderTrackValueFillDisabled",
        "SliderThumbBackground",
        "SliderThumbBackgroundPointerOver",
        "SliderThumbBackgroundPressed",
        "SliderThumbBackgroundDisabled",
    ];

    private static readonly string[] FluentSelectionForegroundResourceKeys =
    [
        "ToggleButtonForegroundChecked",
        "ToggleButtonForegroundCheckedPointerOver",
        "ToggleButtonForegroundCheckedPressed",
        "ToggleButtonForegroundCheckedDisabled",
        "CheckBoxCheckGlyphForegroundChecked",
        "CheckBoxCheckGlyphForegroundCheckedPointerOver",
        "CheckBoxCheckGlyphForegroundCheckedPressed",
        "CheckBoxCheckGlyphForegroundCheckedDisabled",
    ];

    private static readonly string[] FluentExpanderResourceKeys =
    [
        "ExpanderHeaderBackground",
        "ExpanderHeaderBackgroundPointerOver",
        "ExpanderHeaderBackgroundPressed",
        "ExpanderHeaderBackgroundDisabled",
        "ExpanderHeaderForeground",
        "ExpanderHeaderForegroundPointerOver",
        "ExpanderHeaderForegroundPressed",
        "ExpanderHeaderBorderBrush",
        "ExpanderContentBackground",
        "ExpanderContentBorderBrush",
        "ExpanderChevronForeground",
        "ExpanderChevronForegroundPointerOver",
        "ExpanderChevronForegroundPressed",
    ];

    private void ResetThemeResourceOverrides()
    {
        foreach (string resourceKey in ThemeResourceOverrideKeys)
        {
            Resources.Remove(resourceKey);
        }

        _fluentTheme?.Resources.Remove("TreeViewItemBackgroundPointerOver");
        _fluentTheme?.Resources.Remove("MenuFlyoutItemBackgroundPointerOver");
        _fluentTheme?.Resources.Remove("MenuFlyoutItemBackgroundPressed");
        _simpleTheme?.Resources.Remove("ThemeControlHighlightMidBrush");
        _simpleTheme?.Resources.Remove("ThemeBackgroundBrush");
        _simpleTheme?.Resources.Remove("ThemeControlHighBrush");
        _simpleTheme?.Resources.Remove("ThemeAccentBrush4");

        foreach (string resourceKey in FluentSelectionBrushResourceKeys)
        {
            _fluentTheme?.Resources.Remove(resourceKey);
        }

        foreach (string resourceKey in FluentSelectionForegroundResourceKeys)
        {
            _fluentTheme?.Resources.Remove(resourceKey);
        }

        foreach (string resourceKey in FluentExpanderResourceKeys)
        {
            _fluentTheme?.Resources.Remove(resourceKey);
        }

        _simpleTheme?.Resources.Remove("ThemeAccentBrush");
        _simpleTheme?.Resources.Remove("HighlightBrush");
    }

    private void ApplySelectionControlThemeColors(
        bool hasSelectedTheme,
        bool isFluentTheme,
        Color selection,
        Color selectionForeground)
    {
        if (!hasSelectedTheme)
        {
            return;
        }

        IBrush selectionBrush = new SolidColorBrush(selection);
        IBrush selectionForegroundBrush = new SolidColorBrush(selectionForeground);

        if (isFluentTheme)
        {
            foreach (string resourceKey in FluentSelectionBrushResourceKeys)
            {
                _fluentTheme!.Resources[resourceKey] = selectionBrush;
            }

            foreach (string resourceKey in FluentSelectionForegroundResourceKeys)
            {
                _fluentTheme!.Resources[resourceKey] = selectionForegroundBrush;
            }
        }
        else
        {
            _simpleTheme!.Resources["ThemeAccentBrush"] = selectionBrush;
            _simpleTheme.Resources["HighlightBrush"] = selectionBrush;
            _simpleTheme.Resources["ThemeControlHighBrush"] = selectionBrush;
        }
    }

    private void SetThemeBrushResource(string resourceKey, Color color) =>
        Resources[resourceKey] = new SolidColorBrush(color);

    private void SetThemeBrushResource(string resourceKey, IBrush brush) =>
        Resources[resourceKey] = brush;

    private void SetDockThemeBrush(
        IReadOnlyDictionary<string, string> themeColors,
        string colorKey,
        string resourceKey)
    {
        if (themeColors.ContainsKey(colorKey))
        {
            Resources[resourceKey] = new SolidColorBrush(GetThemeColor(themeColors, colorKey, Colors.Transparent));
        }
    }

    private IBrush GetRequiredThemeBrush(string resourceKey)
    {
        if (TryGetResource(resourceKey, ActualThemeVariant, out object? resource) && resource is IBrush brush)
        {
            return brush;
        }

        throw new InvalidOperationException($"Theme resource '{resourceKey}' was not found or is not a brush.");
    }

    private static IBrush GetThemeBrush(
        IReadOnlyDictionary<string, string> themeColors,
        string colorKey,
        IBrush fallback,
        Color colorFallback) =>
        themeColors.ContainsKey(colorKey)
            ? new SolidColorBrush(GetThemeColor(themeColors, colorKey, colorFallback))
            : fallback;

    private static Color GetBrushColor(IBrush brush, Color fallback) =>
        brush is SolidColorBrush solidColorBrush ? solidColorBrush.Color : fallback;

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

    private static Color GetReadableHoverColor(Color background, Color foreground)
    {
        Color blendTarget = GetRelativeLuminance(background) > 0.5 ? Colors.Black : Colors.White;
        double opacity = 0.15;

        while (opacity < 1)
        {
            Color candidate = Blend(background, blendTarget, opacity);
            if (GetContrastRatio(candidate, foreground) >= 4.5)
            {
                return candidate;
            }

            opacity += 0.05;
        }

        return blendTarget;
    }

    private static double GetContrastRatio(Color first, Color second)
    {
        double firstLuminance = GetRelativeLuminance(first);
        double secondLuminance = GetRelativeLuminance(second);
        double lighter = Math.Max(firstLuminance, secondLuminance);
        double darker = Math.Min(firstLuminance, secondLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double GetRelativeLuminance(Color color)
    {
        static double Linearize(byte channel)
        {
            double value = channel / 255.0;
            return value <= 0.04045
                ? value / 12.92
                : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Linearize(color.R)
            + 0.7152 * Linearize(color.G)
            + 0.0722 * Linearize(color.B);
    }

    private static Color GetMutedThemeHoverColor(
        IReadOnlyDictionary<string, string> themeColors,
        string colorKey,
        Color background)
    {
        Color hoverColor = GetThemeColor(themeColors, colorKey, background);
        double opacity = hoverColor.A == byte.MaxValue ? 0.4 : hoverColor.A / (double)byte.MaxValue;
        return Blend(background, hoverColor, opacity);
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ApplicationThemeMode themeMode = (ApplicationThemeMode)GlobalSettings.Get<int>(GlobalSettings.THEME_MODE);
        if (themeMode == ApplicationThemeMode.Default)
        {
            UpdateThemeResources(themeMode);
        }
    }

    private void OnGlobalSettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName != GlobalSettings.UNIT_ORDERS_LIGHT_THEME &&
            e.SettingName != GlobalSettings.UNIT_ORDERS_DARK_THEME &&
            e.SettingName != GlobalSettings.THEME_MODE)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            ApplicationThemeMode themeMode = (ApplicationThemeMode)GlobalSettings.Get<int>(GlobalSettings.THEME_MODE);
            if (e.SettingName == GlobalSettings.THEME_MODE)
            {
                ApplyThemeMode(themeMode);
            }
            else
            {
                UpdateThemeResources(themeMode);
            }
        });
    }

    private void ApplyThemeMode(ApplicationThemeMode themeMode)
    {
        ThemeVariant previousVariant = ActualThemeVariant;
        ThemeModeManager?.SetMode(themeMode);

        if (themeMode != ApplicationThemeMode.Default || ActualThemeVariant == previousVariant)
        {
            UpdateThemeResources(themeMode);
        }
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
    /// - ConfigFactory (its ConfigItem template is shared by both application themes)
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
            app._themeStylesContainer[4] = app._configFactoryFluent!;
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
