
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using System.Collections.Generic;
using static Odyssey.Utils.Converters;
using static Odyssey.Models.Tools.DataProperty;
using Odyssey.Models.Tools;

namespace Odyssey.Models;

/// <summary>
/// Represents a container, which is an entity that can hold units.
/// A container can be a building or a ship.
/// </summary>
public abstract class ContainerModel : EntityModel
{
    private readonly Categories _category;

    protected DataBlock? Region { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public int Size { get; private set; }
    public int OwnerId { get; private set; }
    public DataBlock? OwnerUnit { get; private set; }
    public string OwnerName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DataBlock Container { get { return DataBlock; } }
    protected Categories Category { get { return _category; } }
    public List<DataBlock> Units { get; private set; } = [];
    public List<string> Effects { get; private set; } = [];
    protected ContainerModel(DataBlock containerDataBlock, Categories category, DataBlock? region = null) : base(containerDataBlock)
    {
        _category = category;
        Region = region;
        Size = -1;
        OwnerId = -1;
    }

    public abstract bool CollectData(CRDocument report/*, DataBlock? region*/, ref DataProperty? containerProperty);

    protected bool HandleData(DataKey kb)
    {
        bool handled = true;
        switch (kb.GetKeyType())
        {
            case KeyType.NAME:
                Name = kb.GetValue();
                break;
            case KeyType.TYPE:
                Type = kb.GetValue();
                break;
            case KeyType.SIZE:
                Size = kb.GetInt();
                break;
            case KeyType.DESCRIPTION:
                Description = kb.GetValue();
                break;
            case KeyType.CAPTAIN:
            case KeyType.OWNER:
                OwnerId = StringToInt(kb.GetValue());
                break;
            case KeyType.FACTION:
            case KeyType.NOTES:
                // LATER : check this property meaning (number/anzahl: 1 for t51h boat in 1387-woza)
            case KeyType.NUMBER:
                // Ignore
                break;
            default:
                handled = false;
                break;
        }
        return handled;
    }
    protected string GetLabel()
    {
        string label = $"{Name} ({Container.IdToString()})";
        if (!string.IsNullOrEmpty(Type))
        {
            label += $", {Labels.Localize(Category, Type)}";
        }
        if (Size >= 0)
        {
            string sizeLabel = Labels.Localize(Categories.Node, Labels.SIZE, [$"{Size}"]);
            label += $", {sizeLabel}";
        }
        return label;
    }

    protected bool CollectOwnerInfo(CRDocument report)
    { 
        if (OwnerId > 0)
        {
            if (OwnerUnit != null)
            {
                // Already collected
                return true;
            }
            DataBlock? ownerUnit = null;
            if (report.GetUnit(ref ownerUnit, OwnerId))
            {
                OwnerName = report.GetUnitName(in ownerUnit!, true);
                OwnerUnit = ownerUnit;
                return true;
            }
        }
        return false;
    }

    protected bool CollectUnits(/*DataBlock? region,*/ KeyType key, int containerId)
    {
        if (Region == null)
        { 
            return false; 
        }
        /**/
        Units.Clear();
        /*
        if (region == null) {
            return false; 
        }
        */

        DataBlock? startBlock = Region.GetNextBlock();
        for (DataBlock? block = startBlock; block != null; block = block.GetNextBlock())
        {
            BlockType t = block.GetBlockType();
            if (t == BlockType.REGION)
            {
                break;
            }
            if (t == BlockType.UNIT && block.ValueInt(key) == containerId)
            {
                Units.Add(block);
            }
        }
        return Units.Count > 0;
    }

    protected bool CollectEffects()
    {
        DataBlock? effects = null;
        DataBlock? startBlock = Container.GetNextBlock();
        for (DataBlock? n = startBlock; n != null && n.GetDepth() > Container.GetDepth(); n = n.GetNextBlock())
        {
            if (n.GetBlockType() == BlockType.EFFECTS)
            {
                effects = n;
            }
        }
        if (effects != null)
        {
            Effects.Clear();
            foreach (DataKey kb in effects.GetData())
            {
                Effects.Add(kb.GetValue());
            }
            return true;
        }
        return false;
    }

    protected override bool AddDataProperty(CRDocument report, DataKey dk)
    {
        KeyType kt = dk.GetKeyType();
        string name = dk.GetKeyFromType();
        string value = dk.GetValue();
        DataBlock? reference = null;
        if (kt == KeyType.OWNER || (kt == KeyType.UNKNOWN && name == Strings.DE_OWNER))
        {
            OwnerId = StringToInt(value);
            if (CollectOwnerInfo(report))
            {
                // Owner property is added apart in AddOwner method
                return true;
            }
        }
        return AddNameValueProperty(name, value, reference);
    }
}
