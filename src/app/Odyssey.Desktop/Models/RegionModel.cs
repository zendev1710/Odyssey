
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System.Collections.Generic;

namespace Odyssey.Models;

public class RegionModel : EntityModel, ILocation
{
    public readonly record struct IncomeInputs(int Trees, int Saplings, int Peasants);
    public readonly record struct TradePrice(string Name, int Price);

    private int X { get; set; }
    private int Y { get; set; }
    private int Plane { get; }
    private int _terrain;
    private int _flags;

    int ILocation.X
    {
        get => X;
        set => X = value;
    }
    int ILocation.Y
    {
        get => Y;
        set => Y = value;
    }
    
    PlaneType ILocation.Plane => (PlaneType)Plane;

    public int GetX() => X;
    public int GetY() => Y;
    public int GetPlane() => Plane;

    public int Terrain => _terrain;
    public int Flags => _flags;

    public bool IsWithPeople => (Flags & (int)Flag.REGION_SEEN) != 0;

    public string Name { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string IslandName { get; private set; } = string.Empty;
    public string ShortName => Region.Value(KeyType.NAME);
    public string Description => Region.Value(KeyType.DESCRIPTION);
    public string Herb => Region.Value(KeyType.HERB);
    public int Peasants => Region.ValueInt(KeyType.PEASANTS, -1);
    public int Silver => Region.ValueInt(KeyType.SILVER, -1);
    public int Entertainment => Region.ValueInt(RegionItem.ENTERTAINMENT_NAME, -1);
    public int Recruits => Region.ValueInt(RegionItem.RECRUITS_NAME, -1);
    public int Horses => Region.ValueInt(RegionItem.HORSES_NAME, -1);
    public int Salary => Region.ValueInt(KeyType.SALARY);
    public int Trees => Region.ValueInt(KeyType.TREES);
    public int Saplings => Region.ValueInt(KeyType.SAPLINGS);
    public long LearningCost => RegionAttachment.LearnCost;

    // TODO: set island id instead of 0 when island is defined for the region
    public RegionModel(in RegionDataBlock region) : base(region, 0)
    {
        X = region.GetX();
        Y = region.GetY();
        Plane = region.GetPlane();
        _id = Coordinates.GetId(X, Y, (PlaneType)Plane);
        //_encodedId = Utils.Converters.IdToString(_id);
        // TODO: later, we should set an island Id
        _locationId = _id;
    }

    public RegionDataBlock Region { get => (DataBlock as RegionDataBlock)!; }

    public bool IsVisible(out Flag visibilityFlag)
    {
        visibilityFlag = Flag.None;
        if ((Flags & (int)Flag.REGION_SEEN) != 0)
        {
            visibilityFlag |= Flag.REGION_SEEN;
        }
        else if ((Flags & (int)Flag.TRAVEL) != 0)
        {
            visibilityFlag |= Flag.TRAVEL;
        }
        else if ((Flags & (int)Flag.LIGHTHOUSE) != 0)
        {
            visibilityFlag |= Flag.LIGHTHOUSE;
        }
        return visibilityFlag != Flag.None;
    }

    public void CollectData()
    {
        _terrain = Region.GetTerrain();
        _flags = Region.GetFlags();
        Label = Region.GetUILabel();

        string name = Region.Value(KeyType.NAME);
        if (string.IsNullOrEmpty(name))
        {
            // fallback to lowercase name or coordinates if no name
            name = Region.Value(KeyType.LOWERCASE_NAME);
        }
        Name = name;

        // TODO: set main properties :
        // - Name
        // - Description...
    }

    public long GetIncome(Income.Kind kind) => RegionAttachment.GetIncome(kind);

    public IncomeInputs GetIncomeInputs()
    {
        int trees = Trees;
        int saplings = Saplings;
        int peasants = Peasants;

        DataBlock? firstChild = Region.GetNextBlock();
        int depth = Region.GetDepth();
        for (DataBlock? block = firstChild; block is not null && block.GetDepth() > depth; block = block.GetNextBlock())
        {
            if (block.GetBlockType() != BlockType.RESOURCE)
            {
                continue;
            }

            string value = block.Value(KeyType.RESOURCE_TYPE);
            KeyType type = (KeyType)((int)(DataKey.ParseType(value, BlockType.RESOURCE)) & ((1 << 7) - 1));
            if (type == KeyType.PEASANTS)
            {
                peasants = block.ValueInt(KeyType.RESOURCE_COUNT);
            }
            else if (type == KeyType.TREES)
            {
                trees = block.ValueInt(KeyType.RESOURCE_COUNT);
            }
            else if (type == KeyType.SAPLINGS)
            {
                saplings = block.ValueInt(KeyType.RESOURCE_COUNT);
            }
        }

        return new IncomeInputs(trees, saplings, peasants);
    }

    public RegionInfos GetRegionInfos(CRDocument report)
    {
        RegionAttachment attachment = RegionAttachment;
        if (attachment.RegionInfos is RegionInfos existingInfos)
        {
            return existingInfos;
        }

        RegionInfos regionInfos = attachment.SetRegionInfos(new RegionInfos());
        bool isActiveFaction = false;
        int regionDepth = Region.GetDepth();
        for (DataBlock? block = Region.GetNextBlock(); block is not null; block = block.GetNextBlock())
        {
            if (block.GetDepth() <= regionDepth)
            {
                break;
            }

            BlockType blockType = block.GetBlockType();
            if (blockType == BlockType.RESOURCE)
            {
                string resourceType = block.Value(KeyType.RESOURCE_TYPE);
                if ($"{DataKey.TREES_NAME}|{DataKey.SILVER_NAME}|{DataKey.HORSES_NAME}".Contains(resourceType))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(resourceType))
                {
                    int skill = block.ValueInt(KeyType.RESOURCE_SKILL);
                    int number = block.ValueInt(KeyType.RESOURCE_COUNT);
                    AddResource(regionInfos.Items, resourceType, (ulong)number, skill);
                }
            }
            else if (blockType == BlockType.UNIT)
            {
                isActiveFaction = report.IsActiveFaction(CRDocument.GetFactionIdForUnit(block));
                int number = block.ValueInt(KeyType.NUMBER, -1);
                if (number > 0)
                {
                    regionInfos.People += number;
                    if (isActiveFaction)
                    {
                        regionInfos.FactionPeople += number;
                    }
                }
            }
            else if (blockType == BlockType.OLDUNIT)
            {
                isActiveFaction = false;
            }
            else if (blockType == BlockType.ITEMS && isActiveFaction)
            {
                int silver = block.ValueInt(KeyType.SILVER, -1);
                if (silver > 0)
                {
                    regionInfos.FactionSilver += silver;
                }
            }
        }

        return regionInfos;
    }

    public IReadOnlyList<TradePrice> GetTradePrices()
    {
        List<TradePrice> prices = [];
        DataBlock? pricesBlock = null;
        if (CRDocument.GetSeenChild(ref pricesBlock, Region, BlockType.PRICES))
        {
            foreach (DataKey dataKey in pricesBlock!.GetData())
            {
                string name = dataKey.GetKeyFromType();
                string value = dataKey.GetValue();
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    prices.Add(new TradePrice(name, Utils.Converters.StringToInt(value)));
                }
            }
        }

        return prices;
    }

    private RegionAttachment RegionAttachment => (Region.GetAttachment() as RegionAttachment)!;

    private static void AddResource(Dictionary<RegionItemType, RegionItem> items, string name, ulong value, int skill)
    {
        KeyType keyType = DataKey.ParseType(name, BlockType.RESOURCE);
        RegionItemType id = keyType == KeyType.UNKNOWN
            ? RegionItem.GetId(name)
            : RegionItem.ConvertToItemType(keyType);

        if (id == RegionItemType.UNKNOWN)
        {
            return;
        }

        if (items.TryGetValue(id, out RegionItem? item))
        {
            item.Value += value;
            if (skill < item.Skill)
            {
                item.Skill = skill;
            }
            return;
        }

        items[id] = new RegionItem(name, value, RegionItemCategory.NATURAL_RESOURCE, skill);
    }

    internal void UpdateIslandName()
    {
        IslandName = (Region.GetAttachment() as RegionAttachment)?.Island ?? string.Empty;
    }
}
