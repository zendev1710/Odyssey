using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Snippets;
using Snippet = AvaloniaEdit.Snippets.Snippet;

using System.Diagnostics;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using Odyssea.TextMate;

namespace Odyssey.Views.Tools;

public partial class UnitOrdersView : UserControl
{
    private readonly TextEditor? _textEditor;
    private TextMate.Installation _textMateInstallation;
    public UnitOrdersView()
    {
        InitializeComponent();

        //var control = this.FindControl<TextBlock>("StatusUnitName");
        _textEditor = this.FindControl<TextEditor>("Editor");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void InitializeTextMate()
    {
        var registryOptions = new EresseaOrdersRegistryOptions();
        _textMateInstallation = _textEditor.InstallTextMate(registryOptions);

        // Set the language id for Eressea Orders
        _textMateInstallation.SetGrammar("1000");
    }
}
