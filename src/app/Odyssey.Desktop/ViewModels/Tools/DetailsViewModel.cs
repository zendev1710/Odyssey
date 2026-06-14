using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Documents;
using Prism.Events;
using System;
using System.Collections.Generic;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Represents a view model that manages the display of detailed information for different types of entities such as
/// units, ships, and buildings within a document tool context.
/// </summary>
/// <remarks>The <see cref="DetailsViewModel"/> class is responsible for switching between different details view
/// models based on the current selection. It supports unit, ship, and building details, as well as a default "no
/// details" view when no specific entity is selected. This class is intended to be used within a document tool
/// environment and relies on an event aggregator for handling events.</remarks>
public partial class DetailsViewModel : DocumentToolViewModelBase
{
    private readonly NoDetailsViewModel _noDetailsViewModel;
    private readonly UnitDetailsViewModel _unitDetailsViewModel;
    private readonly ShipDetailsViewModel _shipDetailsViewModel;
    private readonly BuildingDetailsViewModel _buildingDetailsViewModel;

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    public DetailsViewModel(IEventAggregator? eventAggregator = null) : base(eventAggregator)
    {
        _noDetailsViewModel = new NoDetailsViewModel(eventAggregator);
        _unitDetailsViewModel = new UnitDetailsViewModel(eventAggregator);
        _buildingDetailsViewModel = new BuildingDetailsViewModel(eventAggregator);
        _shipDetailsViewModel = new ShipDetailsViewModel(eventAggregator);
        _currentPage = _noDetailsViewModel;
    }

    partial void OnCurrentPageChanged(ViewModelBase? oldValue, global::Odyssey.ViewModels.ViewModelBase? newValue)
    {
        // In case it would be useful
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Exclude this selector from selection change events to avoid reentrancy issues
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        ISelection sel = selectionChange.Selection;
        // Region, faction or intermediate nodes (ships node, buildings node, etc.) lead to no details view model.
        // TODO: maybe add a seen region details view model (which details to be shown ?)
        // TODO: maybe add a unseen region details view model (which details to be shown ?)
        ViewModelBase? vm = _noDetailsViewModel;
        switch (true)
        {
            case var _ when sel.IsUnitSelected():
                vm = _unitDetailsViewModel;
                break;
            case var _ when sel.IsBuildingSelected():
                vm = _buildingDetailsViewModel;
                break;
            case var _ when sel.IsShipSelected():
                vm = _shipDetailsViewModel;
                break;
        }
        SwitchToPage(vm, selectionChange);
    }

    private void SwitchToPage(ViewModelBase page, ISelectionChange selectionChange)
    {
        ISelection sel = selectionChange.Selection;
        SetSelection(sel);
        CurrentPage = page;
    }

    /// <summary>
    /// Links the current report document to the view model.
    /// </summary>
    /// <param name="cr">the report document to link</param>
    /// <returns>true if the report document has been linked to the view model and has data; otherwise false</returns>
    protected override bool SetMapFile(CRDocument cr)
    {
        return base.SetMapFile(cr);
    }
}
