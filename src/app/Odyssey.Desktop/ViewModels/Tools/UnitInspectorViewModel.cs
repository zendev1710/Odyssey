using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Events;
using Odyssey.Models.Documents;
using Odyssey.ViewModels.Tools;
using Prism.Events;
using System;
using System.Collections.Generic;

namespace Odyssey.ViewModels.Tools;

public partial class UnitInspectorViewModel : DocumentToolViewModelBase //: ObservableObject
{
    private readonly CRDocument _report;

    [ObservableProperty]
    private UnitViewModel? _selectedUnit;

    public UnitInspectorViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }
    public UnitInspectorViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
        //_report = report;
        // subscribe to selection changes
        // TODO: consider using a more specific event type or filter by source to avoid unnecessary updates
        //_ea.GetEvent<UnitSelectedEvent>().Subscribe(OnUnitSelected, ThreadOption.UIThread, true);
    }

    private void OnUnitSelected(int unitId)
    {
        // Build lightweight UnitViewModel from document (factory)
        // TODO: retrieve selected region
        SelectedUnit = UnitViewModel.FromDocument(Report, unitId/*, 0, 0*/);
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Handle event only if it comes from Explorer view
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        HandleSelection(selectionChange.Selection);
    }

    private void HandleSelection(ISelection? sel)
    {
        if (sel is not null && sel.IsUnitSelected() && IsSelected(sel))
        {
            // if currently selected unit is the specified selection unit, nothing is done
            return;
        }

        // Handle all kind of selection :
        // - unit selection => update orders view with unit orders and update confirmed status
        // - other selection => empty orders view content and disable commands related to orders edition
        SetSelection(sel);
        //OnEditableUnitSelected();
        // TODO: update confirmed status (check/unchecked) when navigating from one unit to another
    }
}