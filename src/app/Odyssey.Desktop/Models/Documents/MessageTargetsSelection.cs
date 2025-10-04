using System.Collections.Generic;
using Odyssey.Models.Data;

namespace Odyssey.Models.Documents;

public class MessageTargetsSelection : SimpleItemSelection
{
    public DataBlock? Unit { get; private set; }
    public DataBlock? Ship { get; private set; }

    public DataBlock? Building { get; private set; }

    public MessageTargetsSelection(List<DataBlock> targets)
    {
        DataBlock? region = null;
        DataBlock? faction = null;
        foreach (DataBlock target in targets)
        {
            var bt = target.GetBlockType();
            if (bt == BlockType.REGION)
            {
                region = target;
            }
            else if (bt == BlockType.UNIT)
            {
                Unit = target;
            }
            else if (bt == BlockType.SHIP)
            {
                Ship = target;
            }
            else if (bt == BlockType.BUILDING)
            {
                Building = target;
            }
        }

        SetItem(GetDefaultTarget() ?? region, region, faction);
    }

    public MessageTargetsSelection()
    {
        Clear();
    }

    public MessageTargetsSelection(CRDocument report, int positionX, int positionY, int plane = 0)
    {
        Clear();
        DataBlock? region = null;
        if (report.FindRegionFromPosition(ref region, positionX, positionY, plane))
        {
            SetItem(region, null, null);
        }
    }

    public DataBlock? GetDefaultTarget()
    {
        return Unit ?? Ship ?? Building ?? Item ?? Region;
    }

    public override string ToString()
    {
        return ItemType != ItemTypes.None ? $"sel[{Item!}]" : string.Empty;
    }
}
