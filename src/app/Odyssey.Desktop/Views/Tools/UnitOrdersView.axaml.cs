using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Snippets;
using Snippet = AvaloniaEdit.Snippets.Snippet;

using System.Diagnostics;
using AvaloniaEdit;

namespace Odyssey.Views.Tools;

public partial class UnitOrdersView : UserControl
{
    //private readonly TextEditor? _textEditor;
    public UnitOrdersView()
    {
        InitializeComponent();

        //var control = this.FindControl<TextBlock>("StatusUnitName");
        //_textEditor = this.FindControl<TextEditor>("Editor");
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
