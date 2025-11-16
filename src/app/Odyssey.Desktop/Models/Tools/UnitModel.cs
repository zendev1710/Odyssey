
using Odyssey.Models.Data;
using System;
using System.Collections.Generic;
using Odyssey.Models.Documents;
using static Odyssey.Utils.Converters;
using Odyssey.Models.Localization;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.Models.Tools;

public class UnitModel : EntityModel
{
    public enum CombatStatusType
    {
        Undefined = -1,
        Aggressive = 0,
        InFront = 1,
        Rear = 2,
        Defensive = 3,
        DoesntFight = 4,
        Flees = 5,
        Unknown = 6,
    }
    public enum CombatSpellPeriod
    {
        PreCombat = 0,
        DuringCombat = 1,
        PostCombat = 2,
    }
    private readonly List<DataProperty> _containerDataProperties;

    private DataBlock? Items { get; set; }
    private DataBlock? Skills { get; set; }
    public DataBlock? Spells { get; private set; }
    public DataBlock? Effects { get; private set; }
    public SortedDictionary<int, DataBlock> CombatSpells { get; private set; } = [];
    // UNIT PROPERTIES
    public string Name { get; private set; } = string.Empty;
    public int FactionId { get; private set; } = -1;
    public int OtherFactionId { get; private set; } = -1;
    public int PeopleNumber { get; private set; }
    public string Race { get; private set; } = string.Empty;
    public string Aura { get; private set; } = string.Empty;
    public string AuraMax { get; private set; } = string.Empty;
    public int Weight { get; private set; }
    public int Group { get; private set; }
    public string Prefix { get; private set; } = string.Empty;
    public CombatStatusType CombatStatus { get; private set; } = CombatStatusType.Undefined;
    public string Hp { get; private set; } = string.Empty;
    public int FamiliarMageId { get; private set; }
    public bool IsGuarding { get; private set; } = false;
    public string Hungry { get; private set; } = string.Empty;
    public string Hero { get; private set; } = string.Empty;
    public string TrueRaceType { get; private set; } = string.Empty;
    // CONTAINER - BUILDING OR SHIP - PROPERTIES
    /// <summary>
    /// Container is a building or a ship.
    /// </summary>
    private DataBlock? Container { get; set; }
    public KeyType ContainerType { get; private set; }
    public DataBlock? ContainerOwnerUnit { get; private set; }
    public string ContainerOwnerName { get; private set; } = string.Empty;
    public string ContainerName { get; private set; } = string.Empty;
    public string ContainerTypeName { get; private set; } = string.Empty;
    public int ContainerSize { get; private set; }
    public List<string> ContainerEffects { get; private set; } = [];
    public List<DataProperty> ContainerDataProperties { get { return _containerDataProperties; } }
    // SHIP SPECIFIC PROPERTIES
    public int ShipPercentDamage { get; private set; }
    public int ShipCoast { get; private set; } = -1;
    public int ShipCargo { get; private set; }
    public int ShipCapacity { get; private set; }
    // COMPUTED DATA
    public int RidingSkill {get; private set; }
    public int HorsesNumber { get; private set; }
    public int TrollsCartsNumber { get; private set; }
    public int CartsNumber { get; private set; }
    public int CartsNumberMax { get; private set; }
    public int CatapultsNumber { get; private set; }
    public int CatapultsNumberMax { get; private set; }

    public UnitModel(DataBlock unitDataBlock): base(unitDataBlock)
    {
        _containerDataProperties = [];
    }

    public void CollectCategoriesData()
    {
        int minDepth = DataBlock.GetDepth();
        DataBlock? unitChild = DataBlock.GetNextBlock();
        for (DataBlock? block = unitChild; block != null && block.GetDepth() > minDepth; block = block.GetNextBlock())
        {
            BlockType btype = block.GetBlockType();
            if (btype == BlockType.ITEMS)
            {
                Items = block;
            }
            else if (btype == BlockType.TALENTS)
            {
                Skills = block;
            }
            else if (btype == BlockType.SPELLS)
            {
                Spells = block;
            }
            else if (btype == BlockType.EFFECTS)
            {
                Effects = block;
            }
            else if (btype == BlockType.COMBATSPELL)
            {
                CombatSpells[block.GetId()] = block;
            }
        }
    }

    public void CollectData(List<DataKey> data, CRDocument report)
    {
        foreach (DataKey udk in data)
        {
            switch (udk.GetKeyType())
            {
                case KeyType.NAME:
                    Name = udk.GetValue();
                    break;
                case KeyType.FACTION:
                    FactionId = StringToInt(udk.GetValue());
                    break;
                case KeyType.OTHER_FACTION:
                    OtherFactionId = StringToInt(udk.GetValue());
                    break;
                case KeyType.NUMBER:
                    PeopleNumber = StringToInt(udk.GetValue());
                    break;
                case KeyType.TYPE:
                    Race = udk.GetValue();
                    break;
                case KeyType.AURA:
                    Aura = udk.GetValue();
                    break;
                case KeyType.AURAMAX:
                    AuraMax = udk.GetValue();
                    break;
                case KeyType.BUILDING:
                    DataBlock? building = null;
                    if (report.GetBuilding(ref building, StringToInt(udk.GetValue()))) 
                    { 
                        Container = building;
                        ContainerType = KeyType.BUILDING;
                    }
                    break;
                case KeyType.SHIP:
                    DataBlock? ship = null;
                    if (report.GetShip(ref ship, StringToInt(udk.GetValue()))) 
                    {
                        Container = ship;
                        ContainerType = KeyType.SHIP;
                    }
                    break;
                case KeyType.WEIGHT:
                    Weight = StringToInt(udk.GetValue());
                    break;
                case KeyType.GROUP:
                    Group = StringToInt(udk.GetValue());
                    break;
                case KeyType.PREFIX:
                    Prefix = udk.GetValue();
                    break;
                case KeyType.STATUS:
                    CombatStatus = (CombatStatusType)StringToInt(udk.GetValue());
                    break;
                case KeyType.HITPOINTS:
                    Hp = udk.GetValue();
                    break;
                case KeyType.FAMILIARMAGE:
                    FamiliarMageId = StringToInt(udk.GetValue());
                    break;
                case KeyType.GUARDING:
                    IsGuarding = udk.GetValue() == "1";
                    break;
                case KeyType.HUNGER:
                    Hungry = udk.GetValue();
                    break;
                case KeyType.HERO:
                    Hero = udk.GetValue();
                    break;
                case KeyType.ORDERS_CONFIRMED:
                case KeyType.FACTIONSTEALTH:
                case KeyType.DESCRIPTION:
                case KeyType.NOTES:
                    // do not show
                    break;
                case KeyType.UNKNOWN:
                    switch (udk.GetKeyFromType())
                    {
                        // ignored :
                        case Strings.NEUTRAL_ALIAS:
                        case Strings.NEUTRAL_TEMP:
                            break;
                        case Strings.DE_TRUE_RACE_TYPE:
                            TrueRaceType = udk.GetValue();
                            break;
                        default:
                            AddDataProperty(report, udk);
                            break;
                    }
                    break;
                default:
                    AddDataProperty(report, udk);
                    break;
            }
        }

    }
    public bool CollectItems(ref List<DataProperty> itemsProperties)
    {
        itemsProperties = [];
        HorsesNumber = 0;
        CartsNumber = 0;
        CatapultsNumber = 0;
        if (Items != null)
        {
            foreach (DataKey itemKey in Items.GetData())
            {
                string itemName = itemKey.GetKeyFromType();
                int value = StringToInt(itemKey.GetValue());
                switch (itemName)
                {
                    case Strings.DE_HORSE:
                    case Strings.DE_ELFEN_HORSE:
                        HorsesNumber += value;
                        break;
                    case Strings.DE_CART:
                    // case "Dare": ???
                        CartsNumber += value;
                        break;
                    case Strings.DE_CATAPULT:
                        CatapultsNumber += value;
                        break;
                    default: break;
                }
                string itemLevel = $"{value}";
                string itemLabel = Labels.Localize(Categories.Item, itemName, [itemLevel]);
                itemsProperties.Add(new DataProperty(Categories.Item, itemName, itemLevel, itemLabel));
            }
            itemsProperties.Sort(CompareProperties);
        }
        return itemsProperties.Count > 0;
    }

    public bool CollectSkills(ref List<DataProperty> skillsProperties)
    {
        skillsProperties = [];
        RidingSkill = 0;
        if (Skills != null)
        {
            foreach (DataKey skillKey in Skills.GetData())
            {
                string skillName = skillKey.GetKeyFromType();
                int value = skillKey.GetValueInt(1);
                string skillLevel = $"{value}";
                string skillLabel = Labels.Localize(Categories.Skill, skillName, [skillLevel]);
                skillsProperties.Add(new DataProperty(Categories.Skill, skillName, skillLevel, skillLabel));
                if (RidingSkill == 0 && skillName == Strings.DE_RIDING) 
                {
                    RidingSkill = value;
                }
            }
            skillsProperties.Sort(CompareProperties);
        }
        return skillsProperties.Count > 0;
    }
    
    public bool CollectBuildingData(CRDocument report, /*DataBlock? region,*/ ref DataProperty? buildingProperty)
    {
        if (Container == null || ContainerType != KeyType.BUILDING) { return false; }
        BuildingModel? buildingModel = null;
        if (!report.FindBuilding(Container!.GetId(), out buildingModel))
        {
            return false;
        }
        //BuildingModel buildingModel = new(Container);
        if (buildingModel.CollectData(report/*, region*/, ref buildingProperty))
        {
            ContainerOwnerName = buildingModel.OwnerName;
            ContainerOwnerUnit = buildingModel.OwnerUnit;
            ContainerName = buildingModel.Name;
            ContainerTypeName = buildingModel.Type;
            ContainerSize = buildingModel.Size;
            ContainerDataProperties.AddRange(buildingModel.DataProperties);
            ContainerEffects.AddRange(buildingModel.Effects);
            // No specific building data
            return true;
        }

        return false;
    }

    /// <summary>
    /// Collect ship data if unit is inside a ship.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="shipProperty"></param>
    /// <returns>true if unit is inside a ship and ship data has benn successfully collectexd; otherwise, false</returns>
    public bool CollectShipData(CRDocument report, /*DataBlock? region,*/ ref DataProperty? shipProperty)
    {
        if (Container == null || ContainerType != KeyType.SHIP) { return false; }
        ShipModel? shipModel = null;
        if (!report.FindShip(Container!.GetId(), out shipModel))
        {
            return false;
        }
        //ShipModel shipModel = new(Container);
        if (shipModel!.CollectData(report, /*region,*/ ref shipProperty))
        {
            ContainerOwnerName = shipModel.OwnerName;
            ContainerOwnerUnit = shipModel.OwnerUnit;
            ContainerName = shipModel.Name;
            ContainerTypeName = shipModel.Type;
            ContainerSize = shipModel.Size;
            ContainerDataProperties.AddRange(shipModel.DataProperties);
            ContainerEffects.AddRange(shipModel.Effects);
            // Specific ship data
            ShipPercentDamage = shipModel.DamagePercent;
            ShipCoast = shipModel.Coast;
            ShipCargo = shipModel.Cargo;
            ShipCapacity = shipModel.Capacity;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Compute unit walking capacity.
    /// </summary>
    /// <param name="report">report document, useful to know if catapult is a vehicult, depending on the rules.</param>
    /// <returns>a couple of numeric values, riding capacity and max number of horses</returns>
    public (int, int) ComputeWalkingCapacity(CRDocument report)
    {
        // Catapult became a vehicle in Eressea version 28.4 and later
        bool catapultIsVehicle = string.Compare(report.Version, "28.4") >= 0;
        int self = Rules.WEIGHT_PERSON;
        int carry = Rules.CAPACITY_PERSON;
        int maxHorsesNumberCapacity = (RidingSkill * 4 + 1) * PeopleNumber;
        if (maxHorsesNumberCapacity > HorsesNumber)
        {
            maxHorsesNumberCapacity = HorsesNumber;
        }
        CartsNumberMax = maxHorsesNumberCapacity / 2;
        TrollsCartsNumber = Race == Strings.DE_TROLLS ? PeopleNumber / 4 : 0;
        carry = Race == Strings.DE_GOBLINS ? Rules.CAPACITY_GOBLIN : carry;
        carry = Race == Strings.DE_TROLLS ? Rules.CAPACITY_TROLL : carry;
        self = Race == Strings.DE_GOBLINS ? Rules.WEIGHT_GOBLIN : self;
        self = Race == Strings.DE_TROLLS ? Rules.WEIGHT_TROLL : self;
        CartsNumberMax += TrollsCartsNumber;
        if (!catapultIsVehicle)
        {
            CatapultsNumber = 0;
        }
        if (CartsNumberMax > CartsNumber)
        {
            CatapultsNumberMax = CartsNumberMax - CartsNumber;
            CartsNumberMax = CartsNumber;
        }
        if (CatapultsNumberMax > CatapultsNumber)
        {
            CatapultsNumberMax = CatapultsNumber;
        }
        int walkingCapacity = CartsNumberMax * Rules.CAPACITY_CART + maxHorsesNumberCapacity * Rules.CAPACITY_HORSE + CatapultsNumberMax * Rules.CAPACITY_CATAPULT;
        walkingCapacity += carry * PeopleNumber;
        walkingCapacity -= Weight;

        // the following are included in weight, but we don't have to carry them
        walkingCapacity += CatapultsNumberMax * Rules.WEIGHT_CATAPULT;
        walkingCapacity += CartsNumberMax * Rules.WEIGHT_CART;
        walkingCapacity += HorsesNumber * Rules.WEIGHT_HORSE;
        walkingCapacity += self * PeopleNumber;

        return (walkingCapacity, maxHorsesNumberCapacity);
    }
    /// <summary>
    /// Compute unit riding capacity.
    /// </summary>
    /// <returns>a couple of numeric values, riding capacity and max number of horses</returns>
    public (int, int) ComputeRidingCapacity()
    {
        int ridingCapacity = 0;
        int maxHorsesNumberCapacity = 0;
        if (HorsesNumber > 0)
        {
            maxHorsesNumberCapacity = RidingSkill * 2 * PeopleNumber;
            if (maxHorsesNumberCapacity > HorsesNumber)
            {
                maxHorsesNumberCapacity = HorsesNumber;
            }
            int maxCartsNumber = maxHorsesNumberCapacity / 2;
            int maxCatapultsNumber = 0;
            if (maxCartsNumber > CartsNumber)
            {
                maxCatapultsNumber = maxCartsNumber - CartsNumber;
                maxCartsNumber = CartsNumber;
            }
            if (maxCatapultsNumber > CatapultsNumber)
            {
                maxCatapultsNumber = CatapultsNumber;
            }
            ridingCapacity = maxCartsNumber * Rules.CAPACITY_CART + maxHorsesNumberCapacity * Rules.CAPACITY_HORSE + maxCatapultsNumber * Rules.CAPACITY_CATAPULT;
            ridingCapacity -= Weight;
            // the following are included in weight, but we don't have to carry them:
            ridingCapacity += maxCatapultsNumber * Rules.WEIGHT_CATAPULT;
            ridingCapacity += maxCartsNumber * Rules.WEIGHT_CART;
            ridingCapacity += HorsesNumber * Rules.WEIGHT_HORSE;
        }
        return (ridingCapacity, maxHorsesNumberCapacity);
    }
    /// <summary>
    /// Compare two properties alphabetically according to their label.
    /// </summary>
    /// <param name="a">the first property</param>
    /// <param name="b">the second property to compare with</param>
    /// <returns></returns>
    private static int CompareProperties(DataProperty a, DataProperty b)
    {
        return string.Compare(a.Label, b.Label, StringComparison.InvariantCulture);
    }
}
