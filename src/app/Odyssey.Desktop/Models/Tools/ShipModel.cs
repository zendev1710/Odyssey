
using Odyssey.Models.Data;
using System.Collections.Generic;
using Odyssey.Models.Documents;
using static Odyssey.Utils.Converters;
using Odyssey.Models.Localization;
using static Odyssey.Models.Tools.DataProperty;
using System.Diagnostics;

namespace Odyssey.Models.Tools;

public class ShipModel(DataBlock shipDataBlock): ContainerModel(shipDataBlock, Categories.Ship)
{
    public int Coast { get; private set; } = -1;
    public int DamagePercent { get; private set; }
    public int Cargo { get; private set; }
    public int Capacity { get; private set; }
    public int TotalSkill { get; private set; }

    /// <summary>
    /// Collects ship data.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="region"></param>
    /// <param name="owner"></param>
    /// <param name="containerProperty"></param>
    /// <returns></returns>
    public override bool CollectData(CRDocument report, DataBlock? region, ref DataProperty? containerProperty)
    {
        if (Container == null) { return false; }
        containerProperty = null;
        DataProperties.Clear();

        foreach (DataKey kb in Container.GetData())
        {
            if (!HandleData(kb))
            {
                // Specific detailed ship data
                switch (kb.GetKeyType())
                {
                    case KeyType.DAMAGE:
                        DamagePercent = kb.GetInt();
                        break;
                    case KeyType.COAST:
                        Coast = kb.GetInt();
                        break;
                    case KeyType.CARGO:
                        Cargo = kb.GetInt();
                        break;
                    case KeyType.CAPACITY:
                        Capacity = kb.GetInt();
                        break;
                    case KeyType.LOAD:
                    case KeyType.MAXLOAD:
                        // Ignore
                        break;
                    case KeyType.FACTION:
                        break;
                    default:
                        AddDataProperty(report, kb);
                        break;
                }
            }
        }

        string containerLabel = GetLabel();
        containerProperty = new DataProperty(Categories.Node, containerLabel, string.Empty, containerLabel, string.Empty, Container);

        CollectOwnerInfo(report);
        if (CollectUnits(region, KeyType.SHIP, Container.GetId()))
        {
            TotalSkill = ComputeTotalSkill();
        }

        CollectEffects();

        return true;
    }

    private int ComputeTotalSkill()
    {
        int total = 0;
        if (Units.Count > 0)
        {
            foreach (DataBlock unit in Units)
            {
                int number = unit.ValueInt(KeyType.NUMBER);
                DataBlock? child = null;
                if (number > 0 && CRDocument.GetSeenChild(ref child, unit, BlockType.TALENTS))
                {
                    int skill = child!.ValueSkill(DataKey.SKILL_SAILING_NAME);
                    total += skill * number;
                }
            }
        }
        return total;
    }
}
