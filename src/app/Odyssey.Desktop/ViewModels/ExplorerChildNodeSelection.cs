using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;


namespace Odyssey.ViewModels
{
    public class ExplorerChildNodeSelection : EntitySelection
    {
        public ExplorerNodeViewModel? RegionNodeViewModel { get; protected set; }

        public ILocationResolver? LocationResolver { get; protected set; }

        public ExplorerChildNodeSelection()
        {
            // Default constructor
        }

        public ExplorerChildNodeSelection(ExplorerNodeViewModel regionNodeViewModel, EntityModel? item, ILocationResolver locationResolver)
        {
            RegionNodeViewModel = regionNodeViewModel;
            LocationResolver = locationResolver;
            // The region node id is the region's id (and location id), so it can be resolved directly.
            RegionModel? region = locationResolver.ResolveItem(regionNodeViewModel.Id, out EntityModel? resolvedItem)
                ? resolvedItem as RegionModel
                : null;
            SetItem(item, region, null, locationResolver);
        }
    }
}
