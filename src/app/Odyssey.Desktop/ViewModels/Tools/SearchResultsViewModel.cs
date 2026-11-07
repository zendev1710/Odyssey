using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Assets;
using Odyssey.Models;
using Odyssey.Models.Documents;
using Odyssey.Search;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Odyssey.ViewModels.Tools;

public partial class SearchResultsViewModel : DocumentToolViewModelBase
{
    private bool _suppressFilterRefresh;

    public ObservableCollection<GlobalSearchResult> Results { get; } = [];

    [ObservableProperty]
    private IReadOnlyList<GlobalSearchResult> _filteredResults = [];

    [ObservableProperty]
    private bool _showRegions = true;

    [ObservableProperty]
    private bool _showUnits = true;

    [ObservableProperty]
    private bool _showBuildings = true;

    [ObservableProperty]
    private bool _showShips = true;

    [ObservableProperty]
    private bool _showFactions = true;

    [ObservableProperty]
    private string _searchStatus = string.Empty;

    [ObservableProperty]
    private GlobalSearchResult? _selectedResult;

    public bool HasResults => Results.Count > 0;
    public string CategorySelectionActionLabel => AreAllCategoriesSelected
        ? Lang.find_filter_deselect_all
        : Lang.find_filter_select_all;
    public string RegionCategoryFilterLabel => GetCategoryFilterLabel(Lang.find_result_kind_region, GlobalSearchCategory.Region);
    public string UnitCategoryFilterLabel => GetCategoryFilterLabel(Lang.find_result_kind_unit, GlobalSearchCategory.Unit);
    public string BuildingCategoryFilterLabel => GetCategoryFilterLabel(Lang.find_result_kind_building, GlobalSearchCategory.Building);
    public string ShipCategoryFilterLabel => GetCategoryFilterLabel(Lang.find_result_kind_ship, GlobalSearchCategory.Ship);
    public string FactionCategoryFilterLabel => GetCategoryFilterLabel(Lang.find_result_kind_faction, GlobalSearchCategory.Faction);

    public SearchResultsViewModel() : base()
    {
    }

    public void ReplaceResults(IEnumerable<GlobalSearchResult> results, string status)
    {
        SelectedResult = null;
        Results.Clear();
        foreach (GlobalSearchResult result in results)
        {
            Results.Add(result);
        }

        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(RegionCategoryFilterLabel));
        OnPropertyChanged(nameof(UnitCategoryFilterLabel));
        OnPropertyChanged(nameof(BuildingCategoryFilterLabel));
        OnPropertyChanged(nameof(ShipCategoryFilterLabel));
        OnPropertyChanged(nameof(FactionCategoryFilterLabel));
        RefreshFilteredResults();
        SearchStatus = status;
        SelectedResult = FilteredResults.FirstOrDefault();
    }

    public void SelectFirstResultAndApply()
    {
        SelectedResult = FilteredResults.FirstOrDefault();
        ApplySelectedResult();
    }

    public void MoveSelection(int direction)
    {
        if (FilteredResults.Count == 0)
        {
            return;
        }

        int currentIndex = IndexOfSelectedFilteredResult();
        if (currentIndex < 0)
        {
            currentIndex = direction >= 0 ? -1 : 0;
        }

        int nextIndex = (currentIndex + direction + FilteredResults.Count) % FilteredResults.Count;
        SelectedResult = FilteredResults[nextIndex];
        ApplySelectedResult();
    }

    [RelayCommand]
    private void ToggleAllCategories()
    {
        bool selectAll = !AreAllCategoriesSelected;
        _suppressFilterRefresh = true;
        try
        {
            ShowRegions = selectAll;
            ShowUnits = selectAll;
            ShowBuildings = selectAll;
            ShowShips = selectAll;
            ShowFactions = selectAll;
        }
        finally
        {
            _suppressFilterRefresh = false;
        }

        OnPropertyChanged(nameof(CategorySelectionActionLabel));
        RefreshFilteredResults();
    }

    public void ApplySelectedResult()
    {
        if (SelectedResult is GlobalSearchResult result && result.Entity is EntityModel entity)
        {
            SendSelectionChangedEvent(new EntitySelection(entity, result.Region, locatedFaction: result.LocatedFaction));
        }
        else if (SelectedResult?.LocatedFaction is LocatedFactionModel locatedFaction)
        {
            SendSelectionChangedEvent(new EntitySelection(null, locatedFaction.Region, locatedFaction: locatedFaction));
        }
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);
        ReplaceResults([], string.Empty);
    }

    partial void OnShowRegionsChanged(bool value) => OnCategoryFilterChanged();
    partial void OnShowUnitsChanged(bool value) => OnCategoryFilterChanged();
    partial void OnShowBuildingsChanged(bool value) => OnCategoryFilterChanged();
    partial void OnShowShipsChanged(bool value) => OnCategoryFilterChanged();
    partial void OnShowFactionsChanged(bool value) => OnCategoryFilterChanged();

    private void OnCategoryFilterChanged()
    {
        if (_suppressFilterRefresh)
        {
            return;
        }

        OnPropertyChanged(nameof(CategorySelectionActionLabel));
        RefreshFilteredResults();
    }

    private bool AreAllCategoriesSelected => ShowRegions && ShowUnits && ShowBuildings && ShowShips && ShowFactions;

    private void RefreshFilteredResults()
    {
        FilteredResults = Results.Where(IsCategoryVisible).ToArray();
        if (SelectedResult is not null && !FilteredResults.Contains(SelectedResult))
        {
            SelectedResult = FilteredResults.FirstOrDefault();
        }
    }

    private bool IsCategoryVisible(GlobalSearchResult result) => result.Category switch
    {
        GlobalSearchCategory.Region => ShowRegions,
        GlobalSearchCategory.Unit => ShowUnits,
        GlobalSearchCategory.Building => ShowBuildings,
        GlobalSearchCategory.Ship => ShowShips,
        GlobalSearchCategory.Faction => ShowFactions,
        _ => false
    };

    private string GetCategoryFilterLabel(string label, GlobalSearchCategory category) =>
        $"{label} ({Results.Count(result => result.Category == category)})";

    private int IndexOfSelectedFilteredResult()
    {
        for (int i = 0; i < FilteredResults.Count; i++)
        {
            if (ReferenceEquals(FilteredResults[i], SelectedResult))
            {
                return i;
            }
        }

        return -1;
    }

}
