
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
    private readonly List<DataProperty> _structureDataProperties;
    private readonly List<DataProperty> _itemsProperties;
    private readonly List<DataProperty> _skillsProperties;

    // The data of a unit is collected only once (see CollectData): the collected data is then reused.
    private bool _isDataCollected;
    private bool _areCategoriesCollected;
    private bool _areItemsCollected;
    private bool _areSkillsCollected;
    private bool _isContainerDataCollected;
    private DataProperty? _containerProperty;

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
    // STRUCTURE - BUILDING OR SHIP - PROPERTIES
    /// <summary>
    /// Structure is a building or a ship.
    /// </summary>
    private DataBlock? Structure { get; set; }
    public KeyType StructureType { get; private set; }
    public ShipModel? CommandedShip { get; private set; }
    public UnitModel? StructureOwnerUnit { get; private set; }
    //public DataBlock? StructureOwnerUnit { get; private set; }
    internal void SetCommandedShip(ShipModel? ship)
    {
        CommandedShip = ship;
    }

    public string StructureOwnerName { get; private set; } = string.Empty;
    public string StructureName { get; private set; } = string.Empty;
    public string StructureTypeName { get; private set; } = string.Empty;
    public int StructureSize { get; private set; }
    public List<string> StructureEffects { get; private set; } = [];
    public List<DataProperty> StructureDataProperties { get { return _structureDataProperties; } }
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

    public bool IsConfirmed { get; private set; }

    public UnitModel(DataBlock unitDataBlock, int locationId): base(unitDataBlock, locationId)
    {
        _structureDataProperties = [];
        _itemsProperties = [];
        _skillsProperties = [];
        PopulateImmediateProperties();
        // TODO
        IsConfirmed = false;
    }

    private bool IsImmediateProperty(DataKey dataKey)
    {
        switch (dataKey.GetKeyType())
        {
            case KeyType.NAME:
            case KeyType.FACTION:
            case KeyType.OTHER_FACTION:
            case KeyType.NUMBER:
            case KeyType.TYPE:
            case KeyType.WEIGHT:
            case KeyType.GROUP:
            case KeyType.PREFIX:
            case KeyType.STATUS:
            case KeyType.HITPOINTS:
            case KeyType.GUARDING:
            case KeyType.HUNGER:
            case KeyType.HERO:
                return true;
            case KeyType.UNKNOWN:
                return dataKey.GetKeyFromType() == Strings.DE_TRUE_RACE_TYPE;
            default:
                return false;
        }
    }

    private void PopulateImmediateProperties()
    {
        foreach (DataKey udk in DataBlock.GetData())
        {
            if (!IsImmediateProperty(udk))
            {
                continue;
            }

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
                case KeyType.GUARDING:
                    IsGuarding = udk.GetValue() == "1";
                    break;
                case KeyType.HUNGER:
                    Hungry = udk.GetValue();
                    break;
                case KeyType.HERO:
                    Hero = udk.GetValue();
                    break;
                case KeyType.UNKNOWN:
                    if (udk.GetKeyFromType() == Strings.DE_TRUE_RACE_TYPE)
                    {
                        TrueRaceType = udk.GetValue();
                    }
                    break;
            }
        }
    }

    // Dans UnitModel.cs, vous pourriez ajouter une méthode utilitaire :
    public bool PrepareAndCollectItems(ref List<DataProperty> itemsProperties)
    {
        CollectCategoriesData();
        return CollectItems(ref itemsProperties);
    }

    public void ColelctIsConfirmedStatus()
    {
        // TODO: compute IsConfirmed
    }

    /// <summary>
    /// Collects the data blocks of the unit categories (items, talents, spells, effects...).
    /// The collection is done only once: the collected data is then reused.
    /// </summary>
    public void CollectCategoriesData()
    {
        if (_areCategoriesCollected)
        {
            return;
        }
        _areCategoriesCollected = true;

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

    /// <summary>
    /// Collects the unit data from its data block: its categories (items, talents, spells...)
    /// and its own properties.
    /// The collection is done only once: the collected data is then reused by any view model
    /// displaying this unit.
    /// </summary>
    /// <param name="report">the CR report</param>
    public void CollectData(CRDocument report)
    {
        if (_isDataCollected)
        {
            return;
        }

        _isDataCollected = true;
        CollectCategoriesData();

        foreach (DataKey udk in DataBlock.GetData())
        {
            if (IsImmediateProperty(udk))
            {
                continue;
            }

            switch (udk.GetKeyType())
            {
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
                        Structure = building;
                        StructureType = KeyType.BUILDING;
                    }
                    break;
                case KeyType.SHIP:
                    if (report.FindShip(StringToInt(udk.GetValue()), out ShipModel? ship))
                    {
                        Structure = ship.Structure;
                        StructureType = KeyType.SHIP;
                    }
                    break;
                case KeyType.FAMILIARMAGE:
                    FamiliarMageId = StringToInt(udk.GetValue());
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
                        case Strings.NEUTRAL_ALIAS:
                        case Strings.NEUTRAL_TEMP:
                            break;
                        default:
                            AddDataProperty(udk);
                            break;
                    }
                    break;
                default:
                    AddDataProperty(udk);
                    break;
            }
        }
    }
    /// <summary>
    /// Collects the items carried by the unit. The collection is done only once.
    /// </summary>
    /// <param name="itemsProperties">the properties of the collected items</param>
    /// <returns>true when the unit carries at least one item; otherwise, false</returns>
    public bool CollectItems(ref List<DataProperty> itemsProperties)
    {
        if (!_areItemsCollected)
        {
            _areItemsCollected = true;
            CollectCategoriesData();
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
                    _itemsProperties.Add(new DataProperty(Categories.Item, itemName, itemLevel, itemLabel));
                }
                _itemsProperties.Sort(CompareProperties);
            }
        }
        itemsProperties = _itemsProperties;
        return _itemsProperties.Count > 0;
    }

    /// <summary>
    /// Collects the skills of the unit. The collection is done only once.
    /// </summary>
    /// <param name="skillsProperties">the properties of the collected skills</param>
    /// <returns>true when the unit has at least one skill; otherwise, false</returns>
    public bool CollectSkills(ref List<DataProperty> skillsProperties)
    {
        if (!_areSkillsCollected)
        {
            _areSkillsCollected = true;
            CollectCategoriesData();
            RidingSkill = 0;
            if (Skills != null)
            {
                foreach (DataKey skillKey in Skills.GetData())
                {
                    string skillName = skillKey.GetKeyFromType();
                    int value = skillKey.GetValueInt(1);
                    string skillLevel = $"{value}";
                    string skillLabel = Labels.Localize(Categories.Skill, skillName, [skillLevel]);
                    int change = skillKey.GetValueInt(2);
                    _skillsProperties.Add(new DataProperty(Categories.Skill, skillName, skillLevel, skillLabel, change: change));
                    if (RidingSkill == 0 && skillName == Strings.DE_RIDING)
                    {
                        RidingSkill = value;
                    }
                }
                _skillsProperties.Sort(CompareProperties);
            }
        }
        skillsProperties = _skillsProperties;
        return _skillsProperties.Count > 0;
    }
    
    /// <summary>
    /// Collects the data of the structure (a building or a ship) containing this unit, if any.
    /// The collection is done only once: the collected data is then reused by any view model.
    /// </summary>
    /// <param name="report">the CR report</param>
    /// <param name="containerProperty">the property to be displayed as the container node</param>
    /// <returns>true when the unit is inside a building or a ship whose data has been collected</returns>
    public bool CollectContainerData(CRDocument report, ref DataProperty? containerProperty)
    {
        if (!_isContainerDataCollected)
        {
            _containerProperty = null;
            _isContainerDataCollected = StructureType == KeyType.SHIP
                ? CollectShipData(report, ref _containerProperty)
                : CollectBuildingData(report, ref _containerProperty);
        }
        containerProperty = _containerProperty;
        return _isContainerDataCollected;
    }

    /// <summary>
    /// Collects the data of the building containing this unit.
    /// Called once only, by <see cref="CollectContainerData"/>.
    /// </summary>
    private bool CollectBuildingData(CRDocument report, /*DataBlock? region,*/ ref DataProperty? buildingProperty)
    {
        if (Structure == null || StructureType != KeyType.BUILDING) { return false; }
        BuildingModel? buildingModel = null;
        if (!report.FindBuilding(Structure!.GetId(), out buildingModel))
        {
            return false;
        }
        //BuildingModel buildingModel = new(Structure);
        if (buildingModel.CollectData(report/*, region*/, ref buildingProperty))
        {
            StructureOwnerName = buildingModel.OwnerName;
            StructureOwnerUnit = buildingModel.OwnerUnit;
            StructureName = buildingModel.Name;
            StructureTypeName = buildingModel.Type;
            StructureSize = buildingModel.Size;
            StructureDataProperties.AddRange(buildingModel.DataProperties);
            StructureEffects.AddRange(buildingModel.Effects);
            // No specific building data
            return true;
        }

        return false;
    }

    /// <summary>
    /// Collects the data of the ship containing this unit.
    /// Called once only, by <see cref="CollectContainerData"/>.
    /// </summary>
    private bool CollectShipData(CRDocument report, /*DataBlock? region,*/ ref DataProperty? shipProperty)
    {
        if (Structure == null || StructureType != KeyType.SHIP) { return false; }
        ShipModel? shipModel = null;
        if (!report.FindShip(Structure!.GetId(), out shipModel))
        {
            return false;
        }
        //ShipModel shipModel = new(Structure);
        if (shipModel!.CollectData(report, /*region,*/ ref shipProperty))
        {
            StructureOwnerName = shipModel.OwnerName;
            StructureOwnerUnit = shipModel.OwnerUnit;
            StructureName = shipModel.Name;
            StructureTypeName = shipModel.Type;
            StructureSize = shipModel.Size;
            StructureDataProperties.AddRange(shipModel.DataProperties);
            StructureEffects.AddRange(shipModel.Effects);
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
