using Odyssey.Models;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Odyssey.ViewModels;

/// <summary>
/// Base view model displaying the list of structures (buildings or ships) of the selected region.
/// It is used when the "buildings" or "ships" container node of a region is selected in the explorer.
/// </summary>
public abstract partial class StructureListViewModel : StructureViewModel
{
    private readonly NodeViewModel _root = new();
    private readonly bool _isBuildingStructure;

    protected NodeViewModel Root { get { return _root; } }

    public ObservableCollection<NodeViewModel> Items { get; }

    /// <param name="id">The view model id (see <see cref="Ids"/>).</param>
    /// <param name="isBuildingStructure">True for buildings, false for ships (used for the owner label).</param>
    protected StructureListViewModel(string id, bool isBuildingStructure) : base(id)
    {
        _isBuildingStructure = isBuildingStructure;
        Items = _root.Children;
    }

    /// <summary>
    /// Gets the structures of the specified region (its buildings or its ships).
    /// </summary>
    protected abstract IEnumerable<StructureModel> GetStructures(RegionModel region);

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

        Items.Clear();
    }

    public void MakeItems(IEntitySelection selection)
    {
        Items.Clear();

        Region = selection.Region;
        if (Region is null)
        {
            return;
        }

        foreach (StructureModel structure in GetStructures(Region))
        {
            DataProperty? structureProperty = null;
            if (!structure.CollectData(Report, ref structureProperty) || structureProperty is null)
            {
                continue;
            }

            // Only the structure owner and its properties are displayed: the full details
            // are available by double-clicking the structure (which selects it).
            StructureModel = structure;
            OwnerName = structure.OwnerName;
            OwnerUnit = structure.OwnerUnit;

            NodeViewModel structureNode = AddItem(Root, structureProperty, structureProperty.Label);
            AddOwner(structureNode, _isBuildingStructure);
            foreach (DataProperty property in structure.DataProperties)
            {
                _ = AddItem(structureNode, property, $"{property.Label}: {property.Value}");
            }
        }
    }
}
