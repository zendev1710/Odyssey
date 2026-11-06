using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using Odyssey.Settings;
using Odyssey.TextMate;
using Odyssea.TextMate;
using System;

namespace Odyssey.Controls;

public class UnitOrdersViewer : UserControl
{
    public static readonly StyledProperty<string?> OrdersTextProperty =
        AvaloniaProperty.Register<UnitOrdersViewer, string?>(nameof(OrdersText));

    public static readonly StyledProperty<bool?> IsConfirmedProperty =
        AvaloniaProperty.Register<UnitOrdersViewer, bool?>(nameof(IsConfirmed));

    public static readonly StyledProperty<bool> CanEditOrdersProperty =
        AvaloniaProperty.Register<UnitOrdersViewer, bool>(nameof(CanEditOrders));

    private const string GrammarScopeName = "source.eressea";
    private static readonly EresseaOrdersRegistryOptions RegistryOptions =
        new("Odyssey.Resources.Grammars.eressea.tmLanguage.json");
    private readonly TextEditor _editor = new();
    private readonly AvaloniaEdit.TextMate.TextMate.Installation _textMateInstallation;
    private Color? _themeEditorBackground;
    private Application? _themeApplication;
    private GlobalSettings? _settings;

    public string? OrdersText
    {
        get => GetValue(OrdersTextProperty);
        set => SetValue(OrdersTextProperty, value);
    }

    public bool? IsConfirmed
    {
        get => GetValue(IsConfirmedProperty);
        set => SetValue(IsConfirmedProperty, value);
    }

    public bool CanEditOrders
    {
        get => GetValue(CanEditOrdersProperty);
        set => SetValue(CanEditOrdersProperty, value);
    }

    public UnitOrdersViewer()
    {
        _editor.IsReadOnly = true;
        _editor.ShowLineNumbers = false;
        _editor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
        _editor.VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
        _editor.Options.HighlightCurrentLine = false;
        _editor.TextArea.RightClickMovesCaret = false;
        UnitOrdersEditorContextMenu.Attach(_editor, allowPaste: false);
        Content = _editor;

        _textMateInstallation = _editor.InstallTextMate(RegistryOptions);
        _textMateInstallation.SetGrammar(GrammarScopeName);
        _textMateInstallation.AppliedTheme += TextMateInstallationOnAppliedTheme;

        AttachedToVisualTree += (_, _) => AttachThemeSettings();
        DetachedFromVisualTree += (_, _) => DetachThemeSettings();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrdersTextProperty)
        {
            _editor.Text = OrdersText ?? string.Empty;
        }
        else if (change.Property == IsConfirmedProperty || change.Property == CanEditOrdersProperty)
        {
            UpdateOrderConfirmationAppearance();
        }
    }

    private void AttachThemeSettings()
    {
        if (_themeApplication is null && Application.Current is Application application)
        {
            _themeApplication = application;
            _themeApplication.ActualThemeVariantChanged += Application_ActualThemeVariantChanged;
        }

        if (_settings is null)
        {
            _settings = GlobalSettings.Shared;
            _settings.SettingChanged += Settings_SettingChanged;
        }

        UpdateConfiguredThemeForApplicationMode();
    }

    private void DetachThemeSettings()
    {
        if (_themeApplication is not null)
        {
            _themeApplication.ActualThemeVariantChanged -= Application_ActualThemeVariantChanged;
            _themeApplication = null;
        }

        if (_settings is not null)
        {
            _settings.SettingChanged -= Settings_SettingChanged;
            _settings = null;
        }
    }

    private ThemeVariant CurrentApplicationThemeVariant =>
        (_themeApplication ?? Application.Current)?.ActualThemeVariant ?? ThemeVariant.Default;

    private void UpdateConfiguredThemeForApplicationMode()
    {
        ExtendedThemeName themeName = UnitOrdersThemeHelper.GetConfiguredTheme(CurrentApplicationThemeVariant);
        _textMateInstallation.SetTheme(RegistryOptions.LoadTheme(themeName));
    }

    private void TextMateInstallationOnAppliedTheme(object? sender, AvaloniaEdit.TextMate.TextMate.Installation installation)
    {
        _themeEditorBackground = null;
        ApplyBrushAction(installation, "editor.background", brush =>
        {
            _themeEditorBackground = (brush as SolidColorBrush)?.Color;
            Background = brush;
            _editor.Background = brush;
        });
        ApplyBrushAction(installation, "editor.foreground", brush => _editor.Foreground = brush);
        ApplyBrushAction(installation, "editor.selectionBackground", brush => _editor.TextArea.SelectionBrush = brush);
        UpdateOrderConfirmationAppearance();
    }

    private void Settings_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName is GlobalSettings.UNIT_ORDERS_LIGHT_THEME or GlobalSettings.UNIT_ORDERS_DARK_THEME)
        {
            UpdateConfiguredThemeForApplicationMode();
        }
    }

    private void Application_ActualThemeVariantChanged(object? sender, EventArgs e) =>
        UpdateConfiguredThemeForApplicationMode();

    private void UpdateOrderConfirmationAppearance()
    {
        if (_themeEditorBackground is not Color themeBackground)
        {
            return;
        }

        bool showUnconfirmedState = CanEditOrders && IsConfirmed == false;
        Color background = UnitOrdersThemeHelper.GetConfirmationBackground(
            themeBackground,
            showUnconfirmedState,
            CurrentApplicationThemeVariant == ThemeVariant.Dark);
        var backgroundBrush = new SolidColorBrush(background);
        Background = backgroundBrush;
        _editor.Background = backgroundBrush;
        _editor.TextArea.Background = backgroundBrush;
    }

    private static bool ApplyBrushAction(
        AvaloniaEdit.TextMate.TextMate.Installation installation,
        string colorKey,
        Action<IBrush> applyColorAction)
    {
        if (!installation.TryGetThemeColor(colorKey, out string colorString) || !Color.TryParse(colorString, out Color color))
        {
            return false;
        }

        applyColorAction(new SolidColorBrush(color));
        return true;
    }
}
