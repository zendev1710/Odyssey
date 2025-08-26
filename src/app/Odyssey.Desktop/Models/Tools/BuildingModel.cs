
using Odyssey.Models.Data;
using System.Collections.Generic;
using Odyssey.Models.Documents;
using static Odyssey.Utils.Converters;
using Odyssey.Models.Localization;
using static Odyssey.Models.Tools.DataProperty;
using System.Reactive;
using System.Diagnostics;
using Avalonia.Data;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace Odyssey.Models.Tools;

public class BuildingModel(DataBlock buildingDataBlock) : ContainerModel(buildingDataBlock, Categories.Building)
{
    public override bool CollectData(CRDocument report, DataBlock? region, ref DataProperty? containerProperty)
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
        CollectUnits(region, KeyType.BUILDING, Container.GetId());
        CollectEffects();

        return true;
    }
}
