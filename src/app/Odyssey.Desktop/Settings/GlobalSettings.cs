using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Core;
using ConfigFactory.Core.Attributes;
using Odyssey.Controls.Buildings;
using Odyssey.Controls.Ships;
using Odyssey.Models.Localization;

namespace Odyssey.Settings;

public partial class GlobalSettings : ConfigModule<GlobalSettings>, ISettingsProvider
{
    // LATER:
    // - adjust colors according to the theme mode and enable
    // - add a setting to enable/disable this converter
    public static bool EnableTerrainBackgroundColor { get; set; } = false;

    /// <summary>
    /// Raised when a setting value has changed.
    /// </summary>
    public event EventHandler<SettingChangedEventArgs>? SettingChanged;

    /// <summary>
    /// Raises <see cref="SettingChanged"/> for every setting property change.
    /// </summary>
    public GlobalSettings()
    {
        // CommunityToolkit generated setters raise PropertyChanged directly (they do not go
        // through the virtual OnPropertyChanged(PropertyChangedEventArgs)), so subscribe to the
        // event to forward changes as SettingChanged notifications.
        PropertyChanged += OnSettingPropertyChanged;
    }

    private void OnSettingPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName))
        {
            return;
        }

        try
        {
            object? value = GetType().GetProperty(e.PropertyName)?.GetValue(this);
            SettingChanged?.Invoke(this, new SettingChangedEventArgs(e.PropertyName, value));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[APPCONFIG] Failed to raise SettingChanged for '{e.PropertyName}': {ex.Message}");
        }
    }

    //private const string WindowsThemeRegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    //private const string WindowsThemeRegistryValueName = "AppsUseLightTheme";

    public const string IS_READONLY_MODE = "IsReadOnlyMode";
    public const String OPEN_LAST_REPORT_AT_STARTUP = "OpenLastReportAtStartup";

    public const string LANGUAGE = "Language";
    public const string THEME = "Theme";
    public const string THEME_MODE = "ThemeMode";
    public const string EXPLORER_ACTIVE_FACTION_UNITS_AT_FIRST = "ActiveFactionUnitsAtTop";
    public const string EXPLORER_EXPAND_TREE_ON_REPORT_OPENING = "ExpandTreeOnReportOpening";
    public const string EXPLORER_SELECT_LAST_ACTIVE_REGION_ON_REPORT_OPENING = "SelectLastActiveRegionOnReportOpening";
    public const string MAP_USE_SEASON_IMAGES = "UseSeasonImages";
    public const string MAP_SHOW_BUILDINGS = "ShowBuildings";
    public const string MAP_BUILDING_STYLE = "BuildingStyle";
    public const string MAP_BUILDING_CONTRAST = "BuildingContrast";
    public const string MAP_SHOW_SHIPS = "ShowShips";
    public const string MAP_SHIP_STYLE = "ShipStyle";
    public const string MAP_SHIP_ROUTE_MODE = "ShipRouteMode";
    public const string MAP_SHIP_ROUTE_OPACITY = "ShipRouteOpacity";
    public const string MAP_SELECTED_SHIP_ROUTE_OPACITY = "SelectedShipRouteOpacity";
    public const string MAP_SHOW_REGION_NAMES = "ShowRegionNames";
    public const string MAP_REGION_NAME_BANNER = "RegionNameBanner";

    public const string HIDE_IN_PROGRESS_FEATURES = "HideInProgressFeatures";

    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "GetLanguages",
        DisplayMemberPath = "Key",
        SelectedValuePath = "Value")]
    [property: Config(
        Header = "parm_hdr_language",
        Description = "parm_inf_language",
        Category = "parm_cat_general",
        Group = "parm_grp_appearance")]
    private string _language = SupportedUILanguage.English.ToIsoCode();

    [ObservableProperty]
    [property: DropdownConfig(
       RuntimeItemsSourceMethodName = "GetThemes",
       DisplayMemberPath = "Key",
       SelectedValuePath = "Value")]
    [property: Config(
       Header = "parm_hdr_theme",
       Description = "parm_inf_theme",
       Category = "parm_cat_general",
       Group = "parm_grp_appearance")]
    private int _theme = (int)Themes.Theme.Fluent;

    [ObservableProperty]
    [property: DropdownConfig(
       RuntimeItemsSourceMethodName = "GetThemeModes",
       DisplayMemberPath = "Key",
       SelectedValuePath = "Value")]
    [property: Config(
       Header = "parm_hdr_theme_mode",
       Description = "parm_inf_theme_mode",
       Category = "parm_cat_general",
       Group = "parm_grp_appearance")]
    private int _themeMode = (int)Core.Theme.ApplicationThemeMode.Default;

    [ObservableProperty]
    [property: Config(
        Header = "parm_hdr_readonly_mode",
        Description = "parm_inf_readonly_mode",
        Category = "parm_cat_general",
        Group = "parm_grp_behaviours")]
    private bool _isReadOnlyMode = true;

    [ObservableProperty]
    [property: Config(
    Header = "parm_hdr_open_last_report_at_startup",
    Description = "parm_inf_open_last_report_at_startup",
    Category = "parm_cat_general",
    Group = "parm_grp_behaviours")]
    private bool _openLastReportAtStartup = false;

    // whether the Explorer tree view will be fully expanded when a report is document is opened
    [ObservableProperty]
    [property: Config(
        Header = "parm_hdr_explorer_expand_tree_on_load",
        Description = "parm_inf_explorer_expand_tree_on_load",
        Category = "parm_cat_views",
        Group = "parm_grp_explorer")]
    private bool _expandTreeOnReportOpening = false;

    [ObservableProperty]
    [property: Config(
    Header = "parm_hdr_explorer_select_last_active_region",
    Description = "parm_inf_explorer_select_last_active_region",
    Category = "parm_cat_views",
    Group = "parm_grp_explorer")]
    private bool _useLastActiveRegionOnReportOpening = false;

    // At this moment, this setting is set to false by default because it has negative impact on explorer navigation (e.g. unit prev/next.),
    // because DataBlocks tree order should then not the same as in the Explorer.
    [ObservableProperty]
    [property: Config(
    Header = "parm_hdr_explorer_active_faction_units_at_top",
    Description = "parm_inf_explorer_active_faction_units_at_top",
    Category = "parm_cat_views",
    Group = "parm_grp_explorer")]
    private bool _activeFactionUnitsAtTop = false;

    // whether additional images depending on current season are used in Map
    [ObservableProperty]
    [property: Config(
        Header = "parm_hdr_map_season_images",
        Description = "parm_inf_map_season_images",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private bool _useSeasonImages = false;

    // whether buildings are displayed on the map
    [ObservableProperty]
    [property: Config(
        Header = "parm_hdr_map_show_buildings",
        Description = "parm_inf_map_show_buildings",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private bool _showBuildings = true;

    // how buildings are drawn on the map: 0 = auto (icons with vector fallback), 1 = icons only, 2 = symbols only
    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "GetBuildingStyles",
        DisplayMemberPath = "Key",
        SelectedValuePath = "Value")]
    [property: Config(
        Header = "parm_hdr_map_building_style",
        Description = "parm_inf_map_building_style",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private int _buildingStyle = 0;

    // contrast aid drawn behind building icons: 0 = plate, 1 = none, 2 = rim only
    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "GetBuildingContrastModes",
        DisplayMemberPath = "Key",
        SelectedValuePath = "Value")]
    [property: Config(
        Header = "parm_hdr_map_building_contrast",
        Description = "parm_inf_map_building_contrast",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private int _buildingContrast = BuildingContrastModes.Plate;

    // whether ships are displayed on the map
    [ObservableProperty]
    [property: Config(
        Header = "parm_hdr_map_show_ships",
        Description = "parm_inf_map_show_ships",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private bool _showShips = true;

    // how ships are drawn on the map: 0 = images, 1 = compact symbols, 2 = detailed vectors
    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "GetShipStyles",
        DisplayMemberPath = "Key",
        SelectedValuePath = "Value")]
    [property: Config(
        Header = "parm_hdr_map_ship_style",
        Description = "parm_inf_map_ship_style",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private int _shipStyle = ShipStyles.Image;

    // which ship routes are drawn on the map: 0 = all, 1 = selected ship, 2 = selected region, 3 = none
    [ObservableProperty]
    [property: DropdownConfig(
        RuntimeItemsSourceMethodName = "GetShipRouteModes",
        DisplayMemberPath = "Key",
        SelectedValuePath = "Value")]
    [property: Config(
        Header = "parm_hdr_map_ship_route_mode",
        Description = "parm_inf_map_ship_route_mode",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private int _shipRouteMode = ShipRouteModes.All;

    // opacity of completed ship routes, in percent (10..80)
    [ObservableProperty]
    [property: NumericConfig(Minimum = 10, Maximum = 80, Increment = 5)]
    [property: Config(
        Header = "parm_hdr_map_ship_route_opacity",
        Description = "parm_inf_map_ship_route_opacity",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private int _shipRouteOpacity = 35;

    // opacity of the selected ship route, in percent (10..100)
    [ObservableProperty]
    [property: NumericConfig(Minimum = 10, Maximum = 100, Increment = 5)]
    [property: Config(
        Header = "parm_hdr_map_selected_ship_route_opacity",
        Description = "parm_inf_map_selected_ship_route_opacity",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private int _selectedShipRouteOpacity = 70;

    // whether region names are drawn on the map
    [ObservableProperty]
    [property: Config(
        Header = "parm_hdr_map_show_region_names",
        Description = "parm_inf_map_show_region_names",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private bool _showRegionNames = true;

    // whether region names are drawn inside a gold banner with a dark outline
    [ObservableProperty]
    [property: Config(
        Header = "parm_hdr_map_region_name_banner",
        Description = "parm_inf_map_region_name_banner",
        Category = "parm_cat_views",
        Group = "parm_grp_map")]
    private bool _regionNameBanner = false;

    [ObservableProperty]
    [property: Config(
    Header = "parm_hdr_features_hide_in_progress",
    Description = "parm_inf_features_hide_in_progress",
    Category = "parm_cat_development",
    Group = "parm_grp_features")]
    private bool _hideInProgressFeatures = true;

    // FIXME: IStorageProvider must be defined here !?
    [ObservableProperty]
    [property: BrowserConfig(
        BrowserMode = BrowserMode.OpenFile,
        Filter = "echeck*:*.exe",
        InstanceBrowserKey = "some-browser-field-key")]
    [property: Config(
        Header = "parm_hdr_echeck_program",
        Description = "parm_inf_echeck_program",
        Category = "parm_cat_game",
        Group = "parm_grp_tools")]
    private string _echeckPathname = string.Empty;

    [ObservableProperty]
    [property: BrowserConfig(
    BrowserMode = BrowserMode.OpenFolder,
    Filter = "",
    InstanceBrowserKey = "some-browser-field-key")]
    [property: Config(
    Header = "parm_hdr_bookmarks_folder",
    Description = "parm_inf_bookmarks_folder",
    Category = "parm_cat_",
    Group = "parm_grp_")]
    private string _bookmarksFolder = string.Empty;


    public ObservableCollection<KeyValuePair<string, string>> GetLanguages()
    {
        var languages = (new KeyValuePair<string, string>[] {
            new("parm_key_lang_english", SupportedUILanguage.English.ToIsoCode()),
            new("parm_key_lang_french", SupportedUILanguage.French.ToIsoCode()),
            new("parm_key_lang_german", SupportedUILanguage.German.ToIsoCode()),
            // TODO: add system if it belongs to one of the above three languages
            //new("parm_key_system", "system"),
        }).Select(x => new KeyValuePair<string, string>(Translate(x.Key), x.Value));

        return new(languages);
    }

    public ObservableCollection<KeyValuePair<string, int>> GetThemeModes()
    {
        var options = (new KeyValuePair<string, int>[] {
            new("parm_key_system", (int)Core.Theme.ApplicationThemeMode.Default),
            new("parm_key_theme_mode_light", (int)Core.Theme.ApplicationThemeMode.Light),
            new("parm_key_theme_mode_dark", (int)Core.Theme.ApplicationThemeMode.Dark),
        }).Select(x => new KeyValuePair<string, int>(Translate(x.Key), x.Value));

        return new(options);
    }

    public ObservableCollection<KeyValuePair<string, int>> GetThemes()
    {
        var options = (new KeyValuePair<string, int>[] {
            new("parm_key_theme_fluent", (int)Themes.Theme.Fluent),
            new("parm_key_theme_simple", (int)Themes.Theme.Simple),
        }).Select(x => new KeyValuePair<string, int>(Translate(x.Key), x.Value));

        return new(options);
    }

    public ObservableCollection<KeyValuePair<string, int>> GetBuildingStyles()
    {
        var options = (new KeyValuePair<string, int>[] {
            new("parm_key_building_style_auto", BuildingStyles.Auto),
            new("parm_key_building_style_icons", BuildingStyles.Icons),
            new("parm_key_building_style_symbols", BuildingStyles.Symbols),
        }).Select(x => new KeyValuePair<string, int>(Translate(x.Key), x.Value));

        return new(options);
    }

    public ObservableCollection<KeyValuePair<string, int>> GetBuildingContrastModes()
    {
        var options = (new KeyValuePair<string, int>[] {
            new("parm_key_building_contrast_plate", BuildingContrastModes.Plate),
            new("parm_key_building_contrast_rim", BuildingContrastModes.Rim),
            new("parm_key_building_contrast_none", BuildingContrastModes.None),
        }).Select(x => new KeyValuePair<string, int>(Translate(x.Key), x.Value));

        return new(options);
    }

    public ObservableCollection<KeyValuePair<string, int>> GetShipStyles()
    {
        var options = (new KeyValuePair<string, int>[] {
            new("parm_key_ship_style_image", ShipStyles.Image),
            new("parm_key_ship_style_symbol", ShipStyles.Symbol),
            new("parm_key_ship_style_vector", ShipStyles.Vector),
        }).Select(x => new KeyValuePair<string, int>(Translate(x.Key), x.Value));

        return new(options);
    }

    public ObservableCollection<KeyValuePair<string, int>> GetShipRouteModes()
    {
        var options = (new KeyValuePair<string, int>[] {
            new("parm_key_ship_route_all", ShipRouteModes.All),
            new("parm_key_ship_route_selected_ship", ShipRouteModes.SelectedShip),
            new("parm_key_ship_route_selected_region", ShipRouteModes.SelectedRegion),
            new("parm_key_ship_route_none", ShipRouteModes.None),
        }).Select(x => new KeyValuePair<string, int>(Translate(x.Key), x.Value));

        return new(options);
    }

    /// <summary>
    /// Internationalize and localize using the embedded resource files
    /// </summary>
    public override string Translate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) {
            return input;
        }
        string result = Labels.Localize(input) ?? input;
        // TODO: replace all \n in real carriage return values 
        Debug.WriteLine($"[APPCONFIG] Translate {input} into {result}");

        return result.Replace("\n", "<LineBreak/>");
    }

    /// <summary>
    /// Returns the program name, instead of the class name (ApplicationConfig).
    /// Change the JSON configuration pathname (%localappdata%/<Name>/Config.json).
    /// </summary>
    public override string Name { get { return "Odyssey"; } }

    public override void Save()
    {
        Debug.WriteLine("[APPCONFIG] Save -> BEGIN");
        base.Save();
        Debug.WriteLine("[APPCONFIG] Save -> ENDED");
    }

    /// <summary>
    /// Loads the Config.json file from the %localappdata%\Odyssey folder.
    /// </summary>
    /// <param name="module"></param>
    public override void Load(ref GlobalSettings module)
    {
        Debug.WriteLine("[APPCONFIG] Load -> BEGIN");
        try
        {
            // A JsonException is thrown if the config.json file is empty (0 KB)
            base.Load(ref module);
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[ERROR] Failed to load settings from {module.LocalPath} file: {ex.Message}");
        }
        Debug.WriteLine("[APPCONFIG] Load -> ENDED");
    }
    /// <summary>
    /// Resets the settings to their config.json values.
    /// All modified settings using GUI are restored to their values that were last saved.
    /// </summary>
    public override void Reset()
    {
        Debug.WriteLine("[APPCONFIG] Reset -> BEGIN");
        base.Reset();
        Debug.WriteLine("[APPCONFIG] Reset -> ENDED");
    }

    private bool TryGetPropertyValue<TType>(string propertyName, out TType? value)
    {
        value = default!;
        PropertyInfo? propertyInfo = GetType().GetProperty(propertyName);
        if (propertyInfo is null)
        {
            Debug.WriteLine($"Property '{propertyName}' not found.");
            return false;
        }
        object? propertyValue = propertyInfo.GetValue(this);
        if (propertyValue is null && Nullable.GetUnderlyingType(typeof(TType)) is not null)
        {
            return true;
        }
        if (propertyValue is not TType typedValue)
        {
            Debug.WriteLine($"Property '{propertyName}' is of type {propertyInfo.PropertyType}, got {typeof(TType)}.");
            return false;
        }
        value = typedValue;
        return true;
    }

    /// <summary>
    /// Gets the value of a defined setting.
    /// </summary>
    /// <typeparam name="T">The type of value that will be retrieved.</typeparam>
    /// <param name="settingName">The name of the targeted setting.</param>
    /// <returns>Return the value of the setting or its default value.</returns>
    public T GetSetting<T>(string settingName)
    {
        if (TryGetPropertyValue<T>(settingName, out T? value))
        {
            return value!;
        }
        return default!;
    }

    /// <summary>
    /// Gets the value of a defined setting.
    /// </summary>
    /// <typeparam name="T">The type of value that will be retrieved.</typeparam>
    /// <param name="settingName">The name of the targeted setting.</param>
    /// <returns>Return the value of the setting or its default value.</returns>
    public static T Get<T>(string settingName)
    {
        return Shared.GetSetting<T>(settingName);
    }
    /*
    private static AvailableApplicationTheme GetCurrentSystemTheme()
    {
        object? registryValueObject = null;
        if (OperatingSystem.IsWindows())
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(WindowsThemeRegistryKeyPath);
            registryValueObject = key?.GetValue(WindowsThemeRegistryValueName);
        }
        if (registryValueObject == null)
        {
            return AvailableApplicationTheme.Light;
        }

        int registryValue = (int)registryValueObject;

        return registryValue > 0 ? AvailableApplicationTheme.Light : AvailableApplicationTheme.Dark;
    }
    */
}
