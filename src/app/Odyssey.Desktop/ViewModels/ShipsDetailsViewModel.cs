using Odyssey.Models;
using Odyssey.Models.Documents;
using System.Collections.Generic;

namespace Odyssey.ViewModels;

/// <summary>
/// Displays the list of the ships of the selected region.
/// Used when the "ships" node of a region is selected in the explorer.
/// </summary>
public partial class ShipsDetailsViewModel : StructureListViewModel
{
    public ShipsDetailsViewModel() : base(Ids.ShipsDetails, false)
    {
    }

    protected override IEnumerable<StructureModel> GetStructures(RegionModel region)
    {
        return region.Ships;
    }

    protected override bool IsContainerChanged(SelectionChangeTypes selType)
    {
        return selType.HasFlag(SelectionChangeTypes.Ships);
    }
}
