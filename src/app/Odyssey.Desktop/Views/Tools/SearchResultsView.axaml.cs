using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Odyssey.ViewModels.Tools;

namespace Odyssey.Views.Tools;

public partial class SearchResultsView : UserControl
{
    private const double MinimumWidthForCategoryFilter = 240;

    public SearchResultsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ResultsListBox_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is SearchResultsViewModel viewModel)
        {
            viewModel.ApplySelectedResult();
        }
    }

    private void ResultsListBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is SearchResultsViewModel viewModel)
        {
            e.Handled = true;
            viewModel.ApplySelectedResult();
        }
    }

    private void ResultsListBox_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if ((e.Key == Key.Up || e.Key == Key.Down) && DataContext is SearchResultsViewModel viewModel)
        {
            viewModel.ApplySelectedResult();
        }
    }

    private void HeaderGrid_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateCategoryFilterVisibility(e.NewSize.Width);
    }

    private void SearchResultsView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (this.FindControl<Grid>("HeaderGrid") is Grid headerGrid)
        {
            UpdateCategoryFilterVisibility(headerGrid.Bounds.Width);
        }
    }

    private void UpdateCategoryFilterVisibility(double width)
    {
        if (this.FindControl<Button>("CategoryFilterButton") is Button filterButton)
        {
            filterButton.IsVisible = width >= MinimumWidthForCategoryFilter;
        }
    }
}
