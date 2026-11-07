using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;

namespace Odyssey.ViewModels;

/// <summary>
/// Base view model displaying the list of structures (buildings or ships) of the selected region.
/// It is used when the "buildings" or "ships" container node of a region is selected in the explorer.
/// </summary>
public abstract partial class StructureListViewModel : StructureViewModel
{
    public ObservableCollection<StructureListItemViewModel> Structures { get; } = new();

    /// <param name="id">The view model id (see <see cref="Ids"/>).</param>
    /// <param name="isBuildingStructure">True for buildings, false for ships.</param>
    protected StructureListViewModel(string id, bool isBuildingStructure)
        : base(id, isBuildingStructure ? StructureKind.Building : StructureKind.Ship)
    {
    }

    /// <summary>
    /// Gets the structures of the specified region (its buildings or its ships).
    /// </summary>
    protected abstract IEnumerable<StructureModel> GetStructures(CRDocument report, RegionModel region);

    /// <summary>
    /// Returns true when the changed selection concerns the container handled by this view model.
    /// </summary>
    protected abstract bool IsContainerChanged(SelectionChangeTypes selType);

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this))
            return;

        if (IsContainerChanged(selType))
        {
            MakeItems(selection);
        }
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

        Structures.Clear();
    }

    public void MakeItems(IEntitySelection selection)
    {
        Structures.Clear();

        RegionModel? region = selection.Region;
        Region = region;
        if (region is null)
        {
            return;
        }

        foreach (StructureModel structure in GetStructures(Report, region))
        {
            DataProperty? structureProperty = null;
            if (!structure.CollectData(Report, ref structureProperty) || structureProperty is null)
            {
                continue;
            }

            Structures.Add(new StructureListItemViewModel(
                structureProperty.Label,
                new RelayCommand(() => SendSelectionChangedEvent(new EntitySelection(structure, region)))));
        }
    }
}

public sealed class StructureListItemViewModel
{
    public string DisplayName { get; }
    public ICommand SelectCommand { get; }

    public StructureListItemViewModel(string displayName, ICommand selectCommand)
    {
        DisplayName = displayName;
        SelectCommand = selectCommand;
    }
}
