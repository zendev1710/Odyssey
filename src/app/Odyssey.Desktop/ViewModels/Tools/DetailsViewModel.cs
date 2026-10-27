using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Documents;
using Odyssey.ViewModels;
using System.Collections.Generic;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Represents a view model that manages the display of detailed information for different types of entities such as
/// units, ships, and buildings within a document tool context.
/// </summary>
/// <remarks>The <see cref="DetailsViewModel"/> class is responsible for switching between different details view
/// models based on the current selection. It supports unit, ship, and building details (and, for the
/// "buildings"/"ships" container nodes of a region, the list of the region's buildings/ships), as well as a default
/// "no details" view when no specific entity is selected.</remarks>
public partial class DetailsViewModel : DocumentToolViewModelBase
{
    private readonly NoDetailsViewModel _noDetailsViewModel;
    private readonly UnitDetailsViewModel _unitDetailsViewModel;
    private readonly ShipDetailsViewModel _shipDetailsViewModel;
    private readonly BuildingDetailsViewModel _buildingDetailsViewModel;
    private readonly BuildingsDetailsViewModel _buildingsDetailsViewModel;
    private readonly ShipsDetailsViewModel _shipsDetailsViewModel;
    private readonly FactionLocationDetailsViewModel _factionLocationDetailsViewModel;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    public DetailsViewModel() : base()
    {
        _noDetailsViewModel = new NoDetailsViewModel();
        _unitDetailsViewModel = new UnitDetailsViewModel();
        _buildingDetailsViewModel = new BuildingDetailsViewModel();
        _shipDetailsViewModel = new ShipDetailsViewModel();
        _buildingsDetailsViewModel = new BuildingsDetailsViewModel();
        _shipsDetailsViewModel = new ShipsDetailsViewModel();
        _factionLocationDetailsViewModel = new FactionLocationDetailsViewModel();
        _currentPage = _noDetailsViewModel;
    }

    partial void OnCurrentPageChanged(ViewModelBase? oldValue, global::Odyssey.ViewModels.ViewModelBase? newValue)
    {
        // In case it would be useful
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this)) return;

        // Region, faction or intermediate nodes (ships node, buildings node, etc.) lead to no details view model.
        // Note: the "buildings"/"ships" container nodes have their own list view models.
        // TODO: maybe add a seen region details view model (which details to be shown ?)
        // TODO: maybe add a unseen region details view model (which details to be shown ?)
        ViewModelBase? vm = _noDetailsViewModel;
        switch (true)
        {
            // Container nodes first: they are not entity selections
            // (a region's buildings/ships list is displayed as a dedicated view).
            case var _ when selection.HasBuildings():
                vm = _buildingsDetailsViewModel;
                break;
            case var _ when selection.HasShips():
                vm = _shipsDetailsViewModel;
                break;
            case var _ when selection.HasUnit():
                vm = _unitDetailsViewModel;
                break;
            case var _ when selection.HasBuilding():
                vm = _buildingDetailsViewModel;
                break;
            case var _ when selection.HasShip():
                vm = _shipDetailsViewModel;
                break;
            case var _ when selection.HasFaction() && selection.Data is ExplorerNodeViewModel { NodeType: ExplorerNodeViewModel.NodeTypes.FACTION }:
                vm = _factionLocationDetailsViewModel;
                break;
            /*
            case var _ when selection.HasFaction():
                // TODO: handle a faction details view for the selected faction on the current region
                break;
            */
            case var _ when selection.HasRegion():
                // TODO: handle a region details view for the selected region
                break;
        }
        SwitchToPage(vm);
    }

    private void SwitchToPage(ViewModelBase page)
    {
        CurrentPage = page;
    }
}
