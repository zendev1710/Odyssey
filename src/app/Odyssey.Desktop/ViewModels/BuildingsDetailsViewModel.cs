using Odyssey.Models;
using Odyssey.Models.Documents;
using System.Collections.Generic;

namespace Odyssey.ViewModels;

/// <summary>
/// Displays the list of the buildings of the selected region.
/// Used when the "buildings" node of a region is selected in the explorer.
/// </summary>
public partial class BuildingsDetailsViewModel : StructureListViewModel
{
    public BuildingsDetailsViewModel() : base(Ids.BuildingsDetails, true)
    {
    }

    protected override IEnumerable<StructureModel> GetStructures(RegionModel region)
    {
        return region.Buildings;
    }

    protected override bool IsContainerChanged(SelectionChangeTypes selType)
    {
        return selType.HasFlag(SelectionChangeTypes.Buildings);
    }
}
