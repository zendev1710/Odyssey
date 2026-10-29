using CommunityToolkit.Mvvm.Input;
using Odyssey.Models.Documents;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Shows the units selected by the composite units inspector.
/// Inherits from <see cref="DocumentToolViewModelBase"/> so the Report is provided later via SetMapFile.
/// </summary>
public partial class UnitStripViewModel : DocumentToolViewModelBase
{
    public ObservableCollection<UnitViewModel> Units { get; } = new();

    // Public command property exposed to the view
    public IRelayCommand<UnitViewModel?> SelectUnitCommand { get; }

    private readonly List<int> _loadedUnitIds = [];

    public UnitStripViewModel() : base()
    {
        // Initialize command so XAML can bind to SelectUnitCommand
        SelectUnitCommand = new RelayCommand<UnitViewModel?>(SelectUnit);
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);
        _loadedUnitIds.Clear();
        Units.Clear();
    }

    public void SetUnits(IEnumerable<int> unitIds)
    {
        List<int> requestedIds = [.. unitIds];
        if (requestedIds.SequenceEqual(_loadedUnitIds))
            return;

        _loadedUnitIds.Clear();
        _loadedUnitIds.AddRange(requestedIds);
        Units.Clear();
        if (!HasDocument)
            return;

        foreach (int unitId in requestedIds)
        {
            Units.Add(UnitViewModel.FromDocument(Report, unitId));
        }
    }

    /// <summary>
    /// Called from the UnitStrip view when the user clicks a unit tile.
    /// Publishes the selected unit id so other tools (Inspector, Orders) can react.
    /// </summary>
    private void SelectUnit(UnitViewModel? unit)
    {
        if (unit == null)
            return;

        int id = 0;
        if (unit.DataBlock != null)
        {
            id = unit.DataBlock.GetId();
        }
        else
        {
            int.TryParse(unit.Id, out id);
        }

        if (id <= 0)
            return;

        // TODO: send event
    }
}