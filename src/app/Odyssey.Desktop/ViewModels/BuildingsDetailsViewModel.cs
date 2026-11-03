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

    protected override IEnumerable<StructureModel> GetStructures(CRDocument report, RegionModel region)
    {
        return report.GetBuildingsInRegion(region);
    }

    protected override bool IsContainerChanged(SelectionChangeTypes selType)
    {
        return selType.HasFlag(SelectionChangeTypes.Buildings);
    }
}
