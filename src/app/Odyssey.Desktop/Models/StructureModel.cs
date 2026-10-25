
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using System.Collections.Generic;
using static Odyssey.Utils.Converters;
using static Odyssey.Models.Tools.DataProperty;
using Odyssey.Models.Tools;

namespace Odyssey.Models;

/// <summary>
/// Represents a structure, which is an entity that can hold units.
/// A structure can be a building or a ship.
/// </summary>
public abstract class StructureModel : EntityModel
{
    private readonly Categories _category;
    private bool _isDataCollected;
    private DataProperty? _structureProperty;

    protected RegionDataBlock? Region { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public int Size { get; private set; }
    public int OwnerId { get; private set; }
    public UnitModel? OwnerUnit { get; protected set; }

    public void SetOwnerUnit(UnitModel? ownerUnit)
    {
        OwnerUnit = ownerUnit;
    }

    public string OwnerName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DataBlock Structure { get { return DataBlock; } }
    protected Categories Category { get { return _category; } }

    public List<UnitModel> Units { get; private set; } = [];
    //public List<DataBlock> Units { get; private set; } = [];
    public List<string> Effects { get; private set; } = [];
    protected StructureModel(DataBlock structureDataBlock, Categories category, int locationId/*, DataBlock? region = null*/) : base(structureDataBlock, locationId)
    {
        _category = category;
        //Region = region;
        // TODO: use location id
        Region = null;
        Size = -1;
        OwnerId = -1;
    }

    /// <summary>
    /// True when the structure data has already been collected from its data block.
    /// </summary>
    public bool IsDataCollected { get { return _isDataCollected; } }

    /// <summary>
    /// Collects the structure data (its own properties, its owner, its units and its effects)
    /// from its data block.
    /// The collection is done only once: the collected data is then reused by any view model
    /// displaying this structure.
    /// </summary>
    /// <param name="report">the CR report</param>
    /// <param name="structureProperty">the property to be displayed as the structure node</param>
    /// <returns>true when the structure data has been collected; otherwise, false</returns>
    public bool CollectData(CRDocument report, ref DataProperty? structureProperty)
    {
        if (!_isDataCollected)
        {
            _structureProperty = null;
            _isDataCollected = CollectDataFromBlock(report, ref _structureProperty);
        }
        structureProperty = _structureProperty;
        return _isDataCollected;
    }

    /// <summary>
    /// Collects the structure data from its data block.
    /// Called once only, by <see cref="CollectData"/>.
    /// </summary>
    protected abstract bool CollectDataFromBlock(CRDocument report, ref DataProperty? structureProperty);

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
        string label = $"{Name} ({Structure.IdToString()})";
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

    protected bool CollectOwnerInfo(in CRDocument report)
    { 
        if (OwnerId > 0)
        {
            if (OwnerUnit != null)
            {
                // Already collected
                return true;
            }
            DataBlock? ownerUnit = null;
            if (report.FindUnit(OwnerId, out UnitModel? ownerUnitModel))
            {
                ownerUnit = ownerUnitModel!.DataBlock;
                OwnerName = report.GetUnitName(in ownerUnit, true);
                OwnerUnit = ownerUnitModel;
                return true;
            }
        }
        return false;
    }

    protected bool CollectUnits(in CRDocument report, KeyType key, int structureId)
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

        // TODO: another way without using DataBlock
        DataBlock? startBlock = Region.GetNextBlock();
        for (DataBlock? block = startBlock; block != null; block = block.GetNextBlock())
        {
            BlockType t = block.GetBlockType();
            if (t == BlockType.REGION)
            {
                break;
            }
            if (t == BlockType.UNIT && block.ValueInt(key) == structureId)
            {
                if (report.FindUnit(block.GetId(), out UnitModel? unitModel))
                {
                    Units.Add(unitModel!);
                }
            }
        }
        return Units.Count > 0;
    }

    protected bool CollectEffects()
    {
        DataBlock? effects = null;
        DataBlock? startBlock = Structure.GetNextBlock();
        for (DataBlock? n = startBlock; n != null && n.GetDepth() > Structure.GetDepth(); n = n.GetNextBlock())
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

    protected override bool AddDataProperty(DataKey dk)
    {
        return AddNameValueProperty(dk.GetKeyFromType(), dk.GetValue(), this);
    }
}
