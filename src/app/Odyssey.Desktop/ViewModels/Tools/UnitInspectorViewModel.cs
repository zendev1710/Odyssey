using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Documents;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.ViewModels.Tools;

public partial class UnitInspectorViewModel : DocumentToolViewModelBase //: ObservableObject
{
    private readonly CRDocument _report;

    [ObservableProperty]
    private UnitViewModel? _selectedUnit;

    public UnitInspectorViewModel() : base()
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

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this)) return;

        bool regionChanged = selType.HasFlag(SelectionChangeTypes.Region);
        bool regionSelected = selection.IsRegionSelected();

        Debug.WriteLine($"[VM-UNIT-INSPECTOR] OnSelectionChanged: region changed={regionChanged}, region selected={regionSelected}");

        if (regionChanged)
        {
            HandleSelection(selection);
        }
    }

    private void HandleSelection(IEntitySelection sel)
    {
        // TODO
    }
}