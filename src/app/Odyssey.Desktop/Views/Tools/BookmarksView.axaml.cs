using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Odyssey.ViewModels.Tools;

namespace Odyssey.Views.Tools;

public partial class BookmarksView : UserControl
{
    public BookmarksView()
    {
        InitializeComponent();

        // LATER: as for Messages in ReportInfoView, the Name XAML property, auto-generated in the partial
        // code-behind, is null here: the list box is retrieved by name.
        var bookmarksList = this.Find<ListBox>("BookmarksList");
        bookmarksList!.Tapped += BookmarksList_Tapped;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Reveals the clicked bookmark in the explorer.
    /// This is needed at least when the user clicks the bookmark which is already the selected one:
    /// SelectedBookmark does not change then, so the view model is not notified by the binding and the
    /// bookmark would not be shown in the explorer.
    /// </summary>
    private void BookmarksList_Tapped(object? sender, TappedEventArgs e)
    {
        // Reveal the bookmark only when a bookmark has been clicked (and not the empty area of the list)
        if (e.Source is Visual visual && visual.FindAncestorOfType<ListBoxItem>() is not null &&
            DataContext is BookmarksViewModel viewModel)
        {
            viewModel.RevealSelectedBookmark();
        }
    }
}
