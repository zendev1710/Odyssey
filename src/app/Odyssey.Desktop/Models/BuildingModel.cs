
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using static Odyssey.Models.Tools.DataProperty;
using Odyssey.Models.Tools;

namespace Odyssey.Models;

public class BuildingModel : ContainerModel
{
    public BuildingModel(DataBlock buildingDataBlock, DataBlock? region = null) : base(buildingDataBlock, Categories.Building, region)
    {
    }

    public override bool CollectData(CRDocument report/*, DataBlock? region*/, ref DataProperty? containerProperty)
    {
        if (Container == null)
        {
            return false;
        }
        containerProperty = null;
        DataProperties.Clear();
        foreach (DataKey dk in Container.GetData())
        {
            if (!HandleData(dk))
            {
                AddDataProperty(report, dk);
            }
        }
        string containerLabel = GetLabel();
        containerProperty = new DataProperty(Categories.Node, containerLabel, string.Empty, containerLabel, string.Empty, Container);
        CollectOwnerInfo(report);
        CollectUnits(/*region,*/ KeyType.BUILDING, Container.GetId());
        CollectEffects();

        return true;
    }
}
