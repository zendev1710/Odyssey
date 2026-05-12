using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Snippets;


using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.TextMate;
using Odyssea.TextMate;
using Odyssey.TextMate;
using Odyssey.ViewModels;
using Odyssey.ViewModels.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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
    private CompletionWindow _completionWindow;

    private TextBlock _statusTextBlock;
    private CustomMargin _customMargin;

    private CompletionEngine _engine;

    public UnitOrdersView()
    {
        InitializeComponent();

        //var control = this.FindControl<TextBlock>("StatusUnitName");
        _textEditor = this.FindControl<TextEditor>("Editor");

        if (_textEditor is not null)
        {
            // TODO: check if this is modifyed by theme
            //_textEditor.FontFamily = new FontFamily("Cascadia Code,Consolas,Menlo,Monospace");
            //_textEditor.FontWeight = FontWeight.Light
            //_textEditor.FontSize = 14;

            _textEditor.VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
            _textEditor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
            //_textEditor.Background = Brushes.Transparent;
            _textEditor.ShowLineNumbers = false;
            _textEditor.TextArea.Background = this.Background;
            _textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            _textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            _textEditor.Options.AllowToggleOverstrikeMode = true;
            _textEditor.Options.EnableTextDragDrop = true;
            _textEditor.Options.ShowBoxForControlCharacters = false;
            _textEditor.Options.ColumnRulerPositions = new List<int>() { 80, 100 };

            // or textmatesharp indenations ?
            _textEditor.TextArea.IndentationStrategy = new AvaloniaEdit.Indentation.DefaultIndentationStrategy();

            _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            _textEditor.TextArea.RightClickMovesCaret = true;
            _textEditor.Options.HighlightCurrentLine = true;
            _textEditor.Options.CompletionAcceptAction = CompletionAcceptAction.DoubleTapped;
        }

        //_insertSnippetButton = this.FindControl<Button>("insertSnippetBtn");
        //_insertSnippetButton.Click += InsertSnippetButton_Click;

        // For custom elements in the text view (e.g., error markers, inline controls), you can create and add your own ElementGenerator.
        //_textEditor.TextArea.TextView.ElementGenerators.Add(_generator);

        InitializeTextMate();

        // Garder le TextArea focalisé, mais rediriger les touches fléchées vers la fenêtre
        _textEditor!.TextArea.AddHandler(KeyDownEvent, (s, e) =>
        {
            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.Space)
            {
                // On trl+space, display the list of available orders
                var caret = _textEditor.TextArea.Caret.Offset;
                var line = _textEditor.Document.GetLineByOffset(caret);
                var lineText = _textEditor.Document.GetText(line.Offset, caret - line.Offset);

                // Appel du moteur qui gère maintenant la logique du MaxParameters
                var proposals = _engine.GetProposals(lineText, isManualTrigger: true);

                if (proposals.Count > 0)
                {
                    ShowCompletionWindow(proposals);
                }
                else
                {
                    // Optionnel : un petit feedback visuel si la commande est déjà complète
                    Debug.WriteLine("Commande complète ou aucun paramètre possible.");
                }

                e.Handled = true;
                return;
            }

            if (_completionWindow != null)
            {
                if (e.Key == Key.Up || e.Key == Key.Down)
                {
                    // Redirige vers la ListBox sans voler le focus
                    var listBox = _completionWindow.CompletionList.ListBox;
                    var args = new KeyEventArgs { RoutedEvent = KeyDownEvent, Key = e.Key };
                    listBox.RaiseEvent(args);
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter || e.Key == Key.Tab)
                {
                    _completionWindow.CompletionList.RequestInsertion(e);
                    e.Handled = true;
                }
            }
        }, RoutingStrategies.Tunnel);

        _textEditor.TextArea.TextView.LineTransformers.Add(new UnderlineAndStrikeThroughTransformer());

        _statusTextBlock = this.Find<TextBlock>("StatusText");

        this.AddHandler(PointerWheelChangedEvent, (o, i) =>
        {
            if (i.KeyModifiers != KeyModifiers.Control) return;
            if (i.Delta.Y > 0) _textEditor.FontSize++;
            else _textEditor.FontSize = _textEditor.FontSize > 1 ? _textEditor.FontSize - 1 : 1;
        }, RoutingStrategies.Bubble, true);

        // Add a custom margin at the left of the text area, which can be clicked.
        _customMargin = new CustomMargin();
        _textEditor.TextArea.LeftMargins.Insert(0, _customMargin);

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
            _engine = new CompletionEngine(vm);
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

    private void textEditor_TextArea_TextEntering(object sender, TextInputEventArgs e)
    {
        // Si l'utilisateur appuie sur Enter, on ferme tout proprement
        if (e.Text == "\r" || e.Text == "\n")
        {
            _completionWindow?.Close();
            return;
        }

        // Si la fenêtre est ouverte, on laisse l'utilisateur taper normalement
        if (e.Text.Length > 0 && _completionWindow != null)
        {
            // Optionnel : si l'utilisateur tape un espace, on valide l'item sélectionné
            if (e.Text == " ")
            {
                _completionWindow.CompletionList.RequestInsertion(e);
                e.Handled = true; // Empêche l'espace d'être ajouté
            }
        }
    }

    private void textEditor_TextArea_TextEntered(object sender, TextInputEventArgs e)
    {
        // Ne rien faire si on appuie sur Enter, Tab ou Escape
        // Les touches spéciales sont déjà gérées par le KeyDown handler
        if (e.Text == "\r" || e.Text == "\n" || e.Text == "\t") return;

        var caret = _textEditor.TextArea.Caret.Offset;
        var line = _textEditor.Document.GetLineByOffset(caret);
        var lineText = _textEditor.Document.GetText(line.Offset, caret - line.Offset);

        // Ne pas ouvrir si la ligne est vide (ou contient seulement des espaces)
        if (string.IsNullOrWhiteSpace(lineText))
        {
            _completionWindow?.Close();
            return;
        }

        // On récupère les propositions automatiquement
        var proposals = _engine.GetProposals(lineText, isManualTrigger: false);
        if (proposals.Count > 0)
        {
            // Si elle est déjà ouverte, on se contente de mettre à jour les données
            if (_completionWindow != null)
            {
                _completionWindow.CompletionList.CompletionData.Clear();
                foreach (var p in proposals) _completionWindow.CompletionList.CompletionData.Add(p);
            }
            else
            {
                ShowCompletionWindow(proposals);
            }
        }
        else
        {
            _completionWindow?.Close();
        }

        // previous version
        /*
        // Ne rien faire si on appuie sur Enter, Tab ou Escape
        // Les touches spéciales sont déjà gérées par le KeyDown handler
        if (e.Text == "\r" || e.Text == "\n" || e.Text == "\t") return;

        // Retrieve the current word being typed (from the start of the line to the caret)
        var caret = _textEditor.TextArea.Caret.Offset;
        var line = _textEditor.Document.GetLineByOffset(caret);
        var lineText = _textEditor.Document.GetText(line.Offset, caret - line.Offset);

        if (lineText == " " && e.Text == " ")
        {
            // A space at beginning open the list with all possible values
        }
        else
        {
            // Ne pas ouvrir si la ligne est vide (ou contient seulement des espaces)
            if (string.IsNullOrWhiteSpace(lineText))
            {
                _completionWindow?.Close();
                return;
            }
        }

        // Si la fenêtre existe déjà, on met juste à jour ses données
        if (_completionWindow != null)
        {
            _completionWindow.CompletionList.CompletionData.Clear();

            foreach (var p in _engine.GetProposals(lineText, isManualTrigger: false))
                _completionWindow.CompletionList.CompletionData.Add(p);

            // Si plus rien ne matche, fermer
            if (_completionWindow.CompletionList.CompletionData.Count == 0)
                _completionWindow.Close();
        }
        else
        {
            // Création initiale
            var proposals = _engine.GetProposals(lineText, isManualTrigger: false);
            if (proposals.Count > 0)
            {
                _completionWindow = new CompletionWindow(_textEditor.TextArea);
                foreach (var p in proposals) _completionWindow.CompletionList.CompletionData.Add(p);

                _completionWindow.Closed += (o, args) => _completionWindow = null;
                _completionWindow.Show();
                _completionWindow.CompletionList.ListBox.SelectedIndex = 0;
            }
        }
        */
    }

    private void ShowCompletionWindow(IEnumerable<ICompletionData> proposals)
    {
        // Si déjà ouverte, on ferme
        _completionWindow?.Close();

        _completionWindow = new CompletionWindow(_textEditor.TextArea);
        _completionWindow.Closed += (o, args) => _completionWindow = null;

        // 3. Ajouter les propositions
        foreach (var p in proposals)
        {
            _completionWindow.CompletionList.CompletionData.Add(p);
        }

        if (_completionWindow.CompletionList.CompletionData.Count > 0)
        {
            _completionWindow.Show();
            _completionWindow.CompletionList.ListBox.SelectedIndex = 0;
            // 4. Configurer la fenêtre
            //_completionWindow.CompletionList.ListBox.Focusable = true;
            //_completionWindow.Show();

            // 5. Sélectionner le premier élément par défaut
            //_completionWindow.CompletionList.ListBox.SelectedIndex = 0;
            //_completionWindow.CompletionList.ListBox.Focus();
        }

        /*
        var data = _completionWindow.CompletionList.CompletionData;

        // Récupérer toutes les propositions (en passant une chaîne vide pour tout lister)
        var proposals = _engine.GetProposals(initialText, isManualTrigger: true);
        foreach (var p in proposals) data.Add(p);

        if (data.Count > 0)
        {
            _completionWindow.Closed += (o, args) => _completionWindow = null;
            _completionWindow.Show();
            _completionWindow.CompletionList.ListBox.SelectedIndex = 0;
            _completionWindow.CompletionList.ListBox.Focus();
        }
        */
    }
    private void Caret_PositionChanged(object sender, EventArgs e)
    {
        // _statusTextBlock.Text = string.Format("Line {0} Column {1}", _textEditor!.TextArea.Caret.Line, _textEditor.TextArea.Caret.Column);
    }

    // TODO: dispose _textMateInstallation when closing window
    //protected override void OnClosed(EventArgs e)
    //{
    //    base.OnClosed(e);
    //    _textMateInstallation.Dispose();
    //}

    private void RemoveUnderlineAndStrikethroughTransformer()
    {
        for (int i = _textEditor.TextArea.TextView.LineTransformers.Count - 1; i >= 0; i--)
        {
            if (_textEditor.TextArea.TextView.LineTransformers[i] is UnderlineAndStrikeThroughTransformer)
            {
                _textEditor.TextArea.TextView.LineTransformers.RemoveAt(i);
            }
        }
    }

    class UnderlineAndStrikeThroughTransformer : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            if (line.LineNumber == 2)
            {
                string lineText = this.CurrentContext.Document.GetText(line);

                int indexOfUnderline = lineText.IndexOf("underline");
                int indexOfStrikeThrough = lineText.IndexOf("strikethrough");

                if (indexOfUnderline != -1)
                {
                    ChangeLinePart(
                        line.Offset + indexOfUnderline,
                        line.Offset + indexOfUnderline + "underline".Length,
                        visualLine =>
                        {
                            if (visualLine.TextRunProperties.TextDecorations != null)
                            {
                                var textDecorations = new TextDecorationCollection(visualLine.TextRunProperties.TextDecorations) { TextDecorations.Underline[0] };

                                visualLine.TextRunProperties.SetTextDecorations(textDecorations);
                            }
                            else
                            {
                                visualLine.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
                            }
                        }
                    );
                }

                if (indexOfStrikeThrough != -1)
                {
                    ChangeLinePart(
                        line.Offset + indexOfStrikeThrough,
                        line.Offset + indexOfStrikeThrough + "strikethrough".Length,
                        visualLine =>
                        {
                            if (visualLine.TextRunProperties.TextDecorations != null)
                            {
                                var textDecorations = new TextDecorationCollection(visualLine.TextRunProperties.TextDecorations) { TextDecorations.Strikethrough[0] };

                                visualLine.TextRunProperties.SetTextDecorations(textDecorations);
                            }
                            else
                            {
                                visualLine.TextRunProperties.SetTextDecorations(TextDecorations.Strikethrough);
                            }
                        }
                    );
                }
            }
        }
    }

    public class EresseaCompletionData : ICompletionData
    {
        private readonly string _desc;

        public IImage Image => null;

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => _contentControl ??= BuildContentControl();

        public object Description => _desc ?? "Description for " + Text;

        private Control BuildContentControl()
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = Text;
            textBlock.Margin = new Thickness(5);
            return textBlock;
        }
        Control _contentControl;

        public string Text { get; }
        public Func<TextArea, string, List<ICompletionData>>? GetNextContext { get; }

        public EresseaCompletionData(string text, string desc, Func<TextArea, string, List<ICompletionData>>? nextContext = null)
        {
            Text = text;
            _desc = desc;
            GetNextContext = nextContext;
        }

        public double Priority { get; } = 0;

        public void Complete(TextArea textArea, ISegment segment, EventArgs e)
        {
            // 1. Calculer le début du mot actuel pour être sûr de tout remplacer
            var document = textArea.Document;
            var caretOffset = textArea.Caret.Offset;

            // Reculer pour trouver le début du mot (le premier caractère non-alphanumérique)
            int startOffset = caretOffset;
            while (startOffset > 0 && char.IsLetterOrDigit(document.GetCharAt(startOffset - 1)))
            {
                startOffset--;
            }

            // 2. Calculer la longueur du mot partiel déjà tapé
            int lengthToReplace = caretOffset - startOffset;

            // 3. Remplacer tout le mot partiel par le mot complet
            document.Replace(startOffset, lengthToReplace, Text);

            // 4. Ajouter l'espace seulement si nécessaire
            if (GetNextContext != null)
            {
                document.Insert(startOffset + Text.Length, " ");
            }
        }
        /*
        public void Complete(TextArea textArea, ISegment segment, EventArgs e)
        {
            // Au lieu de remplacer simplement le segment, vous devez inclure le caractère tapé
            // si la fenêtre a été ouverte après l'insertion du caractère.

            // Si votre segment est vide (longueur 0), c'est qu'AvaloniaEdit ne sait pas quoi remplacer.
            // Forcez le remplacement du mot courant.
            var doc = textArea.Document;
            var start = segment.Offset;

            // Petite astuce : remonter pour inclure le caractère tapé
            if (start > 0 && char.IsLetterOrDigit(doc.GetCharAt(start - 1)))
                start--;

            textArea.Document.Replace(start, segment.Length + (segment.Offset - start), Text);

            // // 1. Insérer le texte
            // textArea.Document.Replace(segment, Text);
            // // 2. Si une suite est définie, ouvrir automatiquement la prochaine fenêtre
            // if (GetNextContext != null)
            // {
            //     // Ajouter un espace avant de proposer la suite
            //     textArea.Document.Insert(segment.Offset + Text.Length, " ");
            //     // Relancer la complétion (implémentez une méthode TriggerCompletion)
            // }
        }
        */
    }

    private void InsertSnippetButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: use an "ADD TEMP UNIT" snippet
        var className = new SnippetReplaceableTextElement { Text = "Name" };
        var snippet = new Snippet
        {
            Elements =
                {
                    new SnippetTextElement { Text = "public class " },
                    className,
                    new SnippetTextElement
                    {
                        Text = "\n{\n    public "
                    },
                    new SnippetBoundElement { TargetElement = className },
                    new SnippetTextElement { Text = "()\n    {\n        " },
                    new SnippetCaretElement(),
                    new SnippetTextElement { Text = "\n    }\n}" }
                }
        };

        snippet.Insert(_textEditor.TextArea);
        _textEditor.Focus();
    }
}
