
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using static Odyssey.Models.Tools.DataProperty;
using Odyssey.Models.Tools;

namespace Odyssey.Models;

public class BuildingModel : StructureModel
{
    public BuildingModel(DataBlock buildingDataBlock, int locationId/*, DataBlock? region = null*/) : base(buildingDataBlock, Categories.Building, locationId/*, region*/)
    {
    }

    protected override bool CollectDataFromBlock(CRDocument report/*, DataBlock? region*/, ref DataProperty? structureProperty)
    {
        if (Structure == null)
        {
            return false;
        }
        structureProperty = null;
        DataProperties.Clear();
        foreach (DataKey dk in Structure.GetData())
        {
            if (!HandleData(dk))
            {
                AddDataProperty(dk);
            }
        }
        string structureLabel = GetLabel();
        structureProperty = new DataProperty(Categories.Node, structureLabel, string.Empty, structureLabel, string.Empty, this/*Structure*/);
        CollectOwnerInfo(report);
        CollectUnits(report, /*region,*/ KeyType.BUILDING, Structure.GetId());
        CollectEffects();

        return true;
    }
}
