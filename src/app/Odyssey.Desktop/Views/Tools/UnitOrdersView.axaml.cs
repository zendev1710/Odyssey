using Avalonia;
using Avalonia.Controls;
//using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
//using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.TextMate;
using Odyssea.TextMate;
using Odyssey.TextMate;
using Odyssey.ViewModels;
using Odyssey.ViewModels.Tools;
using System;
using System.ComponentModel;
using System.Diagnostics;
//using System.Collections.Generic;
//using AvaloniaEdit.Snippets;
//using Snippet = AvaloniaEdit.Snippets.Snippet;

//using System.Diagnostics;
//using TextMateSharp.Grammars;

namespace Odyssey.Views.Tools;

public partial class UnitOrdersView : UserControl
{
    private static string eresseaScopeName = "source.eressea";
    private readonly TextEditor? _textEditor;
    private AvaloniaEdit.TextMate.TextMate.Installation _textMateInstallation;
    private EresseaOrdersRegistryOptions _eresseaRegistryOptions;

    private TextBlock _statusTextBlock;
    private CustomMargin _customMargin;

    public UnitOrdersView()
    {
        InitializeComponent();

        //var control = this.FindControl<TextBlock>("StatusUnitName");
        //_textEditor is null !
        _textEditor = this.FindControl<TextEditor>("Editor");
        //_textEditor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
        //_textEditor.Background = Brushes.Transparent;
        //_textEditor.ShowLineNumbers = true;
        //_textEditor.TextArea.Background = this.Background;
        //_textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
        //_textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
        //_textEditor.Options.AllowToggleOverstrikeMode = true;
        //_textEditor.Options.EnableTextDragDrop = true;
        //_textEditor.Options.ShowBoxForControlCharacters = true;
        //_textEditor.Options.ColumnRulerPositions = new List<int>() { 80, 100 };
        //_textEditor.TextArea.IndentationStrategy = new Indentation.CSharp.CSharpIndentationStrategy(_textEditor.Options);
        //_textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        //_textEditor.TextArea.RightClickMovesCaret = true;
        //_textEditor.Options.HighlightCurrentLine = true;
        //_textEditor.Options.CompletionAcceptAction = CompletionAcceptAction.DoubleTapped;

        InitializeTextMate();

        // TODO
        //var vm = new UnitOrdersViewModel(_textMateInstallation, _eresseaRegistryOptions);
        //foreach (ExtendedThemeName themeName in Enum.GetValues<ExtendedThemeName>())
        //{
        //    var themeViewModel = new ColorThemeViewModel(themeName);
        //    mainWindowVM.AllColorThemes.Add(themeViewModel);
        //    if (themeName == ExtendedThemeName.DarkPlus)
        //    {
        //        vm.SelectedTheme = themeViewModel;
        //    }
        //}
        //DataContext = vm;

        // Subscribe to DataContext changes to attach to the ViewModel
        this.DataContextChanged += OnDataContextChanged;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeTextMate()
    {
        string scopeName = eresseaScopeName;
        _eresseaRegistryOptions = new EresseaOrdersRegistryOptions("Odyssey.Resources.Grammars.eressea.tmLanguage.json");
        var registryOptions = new TextMateSharp.Registry.Registry(_eresseaRegistryOptions);
        var grammar = registryOptions.LoadGrammar(scopeName);

        // Set the language id for Eressea Orders
        //_textMateInstallation.SetGrammar("1000");
        ///_textEditor is null !
        _textMateInstallation = _textEditor.InstallTextMate(_eresseaRegistryOptions);
        _textMateInstallation.SetGrammar(scopeName);
        _textMateInstallation.AppliedTheme += TextMateInstallationOnAppliedTheme;
    }

    private void TextMateInstallationOnAppliedTheme(object sender, AvaloniaEdit.TextMate.TextMate.Installation e)
    {
        ApplyThemeColorsToEditor(e);
        ApplyThemeColorsToWindow(e);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        // When to avoid this: If you just need to change the style of an element (like a color),
        // you should ideally use DataTriggers or Styles in your XAML bound to the SelectedTheme property instead of doing it in the code-behind.
        // Only use the code-behind approach if you need to perform complex logic (e.g., re-initializing a third-party control,
        // or heavy visual reconfiguration) that cannot be done via simple XAML bindings.

        // // Unsubscribe from old VM if necessary (avoid memory leaks)
        // if (sender is UnitOrdersViewModel oldVm) oldVm.PropertyChanged -= ViewModel_PropertyChanged;
        // if (DataContext is UnitOrdersViewModel newVm)
        // {
        //     newVm.PropertyChanged += ViewModel_PropertyChanged;
        // }

        if (DataContext is UnitOrdersViewModel vm)
        {
            vm.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UnitOrdersViewModel.SelectedTheme))
        {
            var vm = (UnitOrdersViewModel)sender!;
            // Handle your theme logic here
            ApplyThemeToView(vm.SelectedTheme);
        }
    }

    private void ApplyThemeToView(ColorThemeViewModel theme)
    {
        // Your logic for the view (e.g., changing colors, triggers, etc.)
        Debug.WriteLine($"Theme changed to: {theme.ThemeName}");
        _textMateInstallation.SetTheme(_eresseaRegistryOptions.LoadTheme(theme.ThemeName));
    }

    void ApplyThemeColorsToEditor(AvaloniaEdit.TextMate.TextMate.Installation e)
    {
        ApplyBrushAction(e, "editor.background", brush => _textEditor.Background = brush);
        ApplyBrushAction(e, "editor.foreground", brush => _textEditor.Foreground = brush);

        if (!ApplyBrushAction(e, "editor.selectionBackground",
                brush => _textEditor.TextArea.SelectionBrush = brush))
        {
            if (Application.Current!.TryGetResource("TextAreaSelectionBrush", out var resourceObject))
            {
                if (resourceObject is IBrush brush)
                {
                    _textEditor.TextArea.SelectionBrush = brush;
                }
            }
        }

        if (!ApplyBrushAction(e, "editor.lineHighlightBackground",
                brush =>
                {
                    _textEditor.TextArea.TextView.CurrentLineBackground = brush;
                    _textEditor.TextArea.TextView.CurrentLineBorder = new Pen(brush); // Todo: VS Code didn't seem to have a border but it might be nice to have that option. For now just make it the same..
                }))
        {
            _textEditor.TextArea.TextView.SetDefaultHighlightLineColors();
        }

        //Todo: looks like the margin doesn't have a active line highlight, would be a nice addition
        if (!ApplyBrushAction(e, "editorLineNumber.foreground",
                brush => _textEditor.LineNumbersForeground = brush))
        {
            _textEditor.LineNumbersForeground = _textEditor.Foreground;
        }
    }

    private void ApplyThemeColorsToWindow(AvaloniaEdit.TextMate.TextMate.Installation e)
    {
        // Try to get theme name (fallback to installation theme if available)
        //string themeName = _textMateInstallation?. Theme?.Name ?? string.Empty;
        //bool isLightTheme = themeName.IndexOf("light", StringComparison.OrdinalIgnoreCase) >= 0;
        //bool isDarkTheme = !isLightTheme;

        /*
        // Get editor colors from TextMate (fall back to existing app resources if missing)
        Color bgColor;
        Color fgColor;
        if (!e.TryGetThemeColor("editor.background", out var bgStr) || !Color.TryParse(bgStr, out bgColor))
        {
            if (Application.Current!.TryGetResource("TextControlBackground", out var existingBg) && existingBg is IBrush bgBrush && bgBrush is SolidColorBrush sbg)
                bgColor = sbg.Color;
            else
                bgColor = isDarkTheme ? Colors.Black : Colors.White;
        }
        if (!e.TryGetThemeColor("editor.foreground", out var fgStr) || !Color.TryParse(fgStr, out fgColor))
        {
            if (Application.Current!.TryGetResource("TextControlForeground", out var existingFg) && existingFg is IBrush fgBrush && fgBrush is SolidColorBrush sfg)
                fgColor = sfg.Color;
            else
                fgColor = isDarkTheme ? Colors.White : Colors.Black;
        }

        var bgBrush = new SolidColorBrush(bgColor);
        var fgBrush = new SolidColorBrush(fgColor);

        // Keys defined in App.axaml (plus a few common fluent-like keys)
        string[] backgroundKeys =
        {
            "TextControlBackground",
            "TextControlBackgroundFocused",
            "TextControlBackgroundPointerOver",
            "RegionColor",
            "AcrylicTintColor",
            "AcrylicFallbackColor",
            // Fluent-style keys you may want to override
            "ThemeBackgroundColor",
            "SystemControlBackgroundBaseLowBrush",
            "SystemControlBackgroundBaseMediumBrush",
            "SystemControlBackgroundBaseHighBrush"
        };

        string[] foregroundKeys =
        {
            "TextControlForeground",
            "TextForegroundColor",
            // Fluent-style keys you may want to override
            "SystemControlForegroundBaseHighBrush",
            "SystemAltHighColor"
        };

        // Apply to application resources (create or replace)
        foreach (var key in backgroundKeys)
        {
            Application.Current.Resources[key] = bgBrush;
        }
        foreach (var key in foregroundKeys)
        {
            Application.Current.Resources[key] = fgBrush;
        }

        // If you need a different tint for light/dark, tweak Accent/Alt keys:
        if (isDarkTheme)
        {
            Application.Current.Resources["AcrylicTintColor"] = new SolidColorBrush(Color.FromArgb(0xFF, (byte)(bgColor.R / 2), (byte)(bgColor.G / 2), (byte)(bgColor.B / 2)));
        }
        else
        {
            Application.Current.Resources["AcrylicTintColor"] = new SolidColorBrush(Color.FromArgb(0xFF, (byte)Math.Min(255, bgColor.R + 20), (byte)Math.Min(255, bgColor.G + 20), (byte)Math.Min(255, bgColor.B + 20)));
        }

        // Force already-open windows to pick up the new brushes if they don't use DynamicResource
        if (Application.Current?.Windows != null)
        {
            foreach (var w in Application.Current.Windows)
            {
                if (w is Window win)
                {
                    if (Application.Current.Resources["TextControlBackground"] is IBrush appBg)
                        win.Background = appBg;
                    if (Application.Current.Resources["TextControlForeground"] is IBrush appFg)
                        win.Foreground = appFg;

                    // If certain dock / third-party controls are not updated, you can walk
                    // their visual tree here and set Background/Foreground where needed.
                }
            }
        }

        // NOTE:
        // - Certaines librairies (Dock, DataGrid, ConfigFactory, AvaloniaEdit) utilisent
        //   leurs propres clés internes. Pour un override complet, identifie les clés
        //   dans leurs fichiers de thème (avalonia xaml) et ajoute-les aux arrays ci-dessus.
        // - Si tu veux appliquer des variantes Light/Dark différentes (p.ex. couleurs spécifiques
        //   pour boutons, surfaces), fournis deux palettes et choisis selon isLightTheme/isDarkTheme.

        */

        /*
        // StatusBar is a grid, not a panel 
        var panel = this.Find<StackPanel>("StatusBar");
        if (panel == null)
        {
            return;
        }

        if (!ApplyBrushAction(e, "statusBar.background", brush => panel.Background = brush))
        {
            panel.Background = Brushes.Purple;
        }

        if (!ApplyBrushAction(e, "statusBar.foreground", brush => _statusTextBlock.Foreground = brush))
        {
            _statusTextBlock.Foreground = Brushes.White;
        }

        if (!ApplyBrushAction(e, "sideBar.background", brush => _customMargin.BackGroundBrush = brush))
        {
            _customMargin.SetDefaultBackgroundBrush();
        }
        */

        //Applying the Editor background to the whole window for demo sake.
        ApplyBrushAction(e, "editor.background", brush => Background = brush);
        ApplyBrushAction(e, "editor.foreground", brush => Foreground = brush);
    }

    bool ApplyBrushAction(AvaloniaEdit.TextMate.TextMate.Installation e, string colorKeyNameFromJson, Action<IBrush> applyColorAction)
    {
        if (!e.TryGetThemeColor(colorKeyNameFromJson, out var colorString))
            return false;

        if (!Color.TryParse(colorString, out Color color))
            return false;

        var colorBrush = new SolidColorBrush(color);
        applyColorAction(colorBrush);
        return true;
    }
}
