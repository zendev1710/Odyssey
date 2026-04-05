using Avalonia;
using Avalonia.Controls;
//using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
//using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.TextMate;
using Odyssea.TextMate;
using System;
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
        _textEditor = this.FindControl<TextEditor>("TextEditor");
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
