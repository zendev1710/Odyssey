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
            RegionModel? region = ResolveRegion(regionNodeViewModel, locationResolver);
            SetItem(item, region, null, locationResolver);
        }

        protected static RegionModel? ResolveRegion(ExplorerNodeViewModel regionNodeViewModel, ILocationResolver locationResolver)
        {
            if (locationResolver is CRDocument report)
            {
                return report.FindRegionById(regionNodeViewModel.Id);
            }

            return locationResolver.ResolveItem(regionNodeViewModel.Id, out EntityModel? resolvedItem)
                ? resolvedItem as RegionModel
                : null;
        }
    }
}
