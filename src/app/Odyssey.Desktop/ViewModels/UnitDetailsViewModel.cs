using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Odyssey.Models.Tools;
using Odyssey.Models.Localization;
using Odyssey.Models.Documents;

using static Odyssey.Utils.Converters;
using static Odyssey.Models.Tools.DataProperty;
using static Odyssey.Models.Tools.UnitModel;

namespace Odyssey.ViewModels;

public partial class UnitDetailsViewModel : ViewModelBase
{
    private readonly NodeViewModel _root = new();

    protected NodeViewModel Root { get { return _root; } }
    public ObservableCollection<NodeViewModel> Items { get; }

    [ObservableProperty]
    private string _description;

    /// <summary>
    /// Unit name, with "<name> (id)" format
    /// </summary>
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _factionName;

    [ObservableProperty]
    private string _otherFactionName;

    [ObservableProperty]
    private string _groupName;

    [ObservableProperty]
    private string _mageNameIfFamiliar;

    [ObservableProperty]
    private bool _isFamiliar;

    [ObservableProperty]
    private bool _isHero;

    [ObservableProperty]
    private bool _isStarving;

    [ObservableProperty]
    private bool _isGuarding;

    [ObservableProperty]
    private bool _isFactionDisguised;

    [ObservableProperty]
    private int _peopleNumber;

    [ObservableProperty]
    private string _race;

    [ObservableProperty]
    private string _aura;

    [ObservableProperty]
    private string _auramax;

    [ObservableProperty]
    private string _hero;

    [ObservableProperty]
    private string _hp;

    [ObservableProperty]
    private string _hungry;

    [ObservableProperty]
    private string _combatStatus;

    [ObservableProperty]
    private string _guards;

    [ObservableProperty]
    private int _familiarMage;

    [ObservableProperty]
    private int _weight;
    public UnitDetailsViewModel() : this(null) 
    { 
    }
    public UnitDetailsViewModel(IEventAggregator? eventAggregator) : base(Ids.UnitDetails, eventAggregator)
    {
        Items = Root.Children;

        _description = string.Empty;
        _name = string.Empty;
        _factionName = string.Empty;
        _otherFactionName = string.Empty;
        _groupName = string.Empty;
        _mageNameIfFamiliar = string.Empty;
        _isFamiliar = false;
        _isHero = false;
        _peopleNumber = 0;
        _race = string.Empty;
        _aura = string.Empty;
        _auramax = string.Empty;
        _hero = string.Empty;
        _hp = string.Empty;
        _hungry = string.Empty;
        _combatStatus = string.Empty;
        _guards = string.Empty;
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Exclude this selector from selection change events to avoid reentrancy issues
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        ISelection sel = selectionChange.Selection;
        if (sel.IsUnitSelected() && !IsSelected(sel))
        {
            MakeItems(sel);
        }
    }

    public void MakeItems(ISelection sel)
    {
        Items.Clear();
        ISelection selection = SetSelection(sel);
        DataBlock? unitDataBlock = selection.Item;
        DataBlock? regionDataBlock = selection.IsRegionSelected() ? selection.Region : null;
        string factionName = string.Empty;
        string otherFactionName = string.Empty;
        string mageNameIfFamiliar = string.Empty;

        bool isFamiliar = false;
        bool isHero = false;
        bool isStarving;
        bool isGuarding;
        bool isFactionDisguised = false;

        UnitModel unitModel = new(unitDataBlock!);

        // Get groups of properties; a group will be a root child node and will have each property as a child
        unitModel.CollectCategoriesData();

        unitModel.CollectData(unitDataBlock!.GetData(), Report);

        string unitName = $"{unitModel.Name} ({unitDataBlock.IdToString()})";
        DataProperty unitProperty = new(Categories.None, unitName, string.Empty, unitName, string.Empty, unitDataBlock);
        NodeViewModel unitNode = AddItem(Root, unitProperty);

        //////////
        // FACTION
         
        DataBlock? factionDataBlock = null;
        string factionLabel = string.Empty;
        if (unitModel.FactionId < 0 && unitModel.OtherFactionId < 0)
        {
            factionLabel = Labels.Localize(Labels.DISGUISED);
            isFactionDisguised = true;
        }
        else
        {
            DataBlock? faction = null;
            if (unitModel.FactionId > 0)
            {
                if (Report.GetFaction(ref faction, unitModel.FactionId))
                {
                    factionDataBlock = faction;
                    factionLabel = factionDataBlock!.Value(KeyType.FACTIONNAME);
                    if (string.IsNullOrEmpty(factionLabel))
                    {
                        factionLabel = Labels.Localize(Labels.DISGUISED);
                    }
                    else
                    {
                        factionLabel = $"{factionLabel} ({factionDataBlock.IdToString()})";
                    }
                }
                else
                {
                    string factionId = IdToString(unitModel.FactionId);
                    factionLabel = Labels.Localize(Labels.UNKNOWN_ID, [factionId]);
                }
                factionName = factionLabel;
            }

            if (unitModel.OtherFactionId > 0)
            {
                string otherFactionLabel = string.Empty;
                DataBlock? anotherFaction = null;
                if (Report.GetFaction(ref anotherFaction, unitModel.OtherFactionId))
                {
                    factionDataBlock = anotherFaction!;
                    otherFactionLabel = factionDataBlock.Value(KeyType.FACTIONNAME);
                    if (string.IsNullOrEmpty(otherFactionLabel))
                    {
                        otherFactionLabel = Labels.Localize(Labels.FACTION_DISGUISED_AS_FACTION);
                    }
                    else
                    {
                        string otherFactionId = IdToString(unitModel.OtherFactionId);
                        otherFactionLabel = Labels.Localize(Labels.FACTION_DISGUISED_AS, [$"{otherFactionLabel} ({otherFactionId})"]);
                    }
                }
                else
                {
                    string otherFactionId = IdToString(unitModel.OtherFactionId);
                    // emapdetailspanel.node.unknownfaction
                    otherFactionLabel = Labels.Localize(Labels.UNKNOWN_ID, [otherFactionId]);
                }
                otherFactionName = otherFactionLabel;
                factionLabel += $", {otherFactionLabel}";
            }
        }

        DataProperty factionProperty = new(Categories.None, factionName, string.Empty, factionLabel, string.Empty);
        _ = AddItem(unitNode, factionProperty);

        ////////
        // GROUP

        DataProperty? groupProperty = null;
        if (unitModel.Group > 0)
        {
            int group = unitModel.Group;
            string groupNameValue;
            DataBlock? match = null;
            if (Report.GetGroup(ref match, group))
            {
                groupNameValue = match!.Value(KeyType.LOWERCASE_NAME);
            }
            else
            {
                groupNameValue = ToStringVal(group);
            }
           //groupName = groupNameValue;
            string groupLabel = Labels.Localize(Categories.Node, Labels.GROUP, [groupNameValue]);
            groupProperty = new DataProperty(Categories.Node, groupNameValue, groupLabel);
            _ = AddItem(unitNode, groupProperty);
        }

        ///////////
        // FAMILIAR

        DataProperty? familiarProperty = null;
        if (unitModel.FamiliarMageId > 0)
        {
            isFamiliar = true;
            DataBlock? mage = null;
            if (Report.GetUnit(ref mage, unitModel.FamiliarMageId))
            {
                string mageName = mage!.Value(KeyType.NAME);
                string id = IdToString(unitModel.FamiliarMageId);
                string familiarMageName = $"{mageName} ({id})";
                string familiarLabel = Labels.Localize(Categories.None, Labels.FAMILIAR_INFO, [familiarMageName]);
                familiarProperty = new DataProperty(Categories.None, Labels.FAMILIAR_INFO, familiarMageName, familiarLabel, string.Empty, mage);
                _ = AddItem(unitNode, familiarProperty);
                mageNameIfFamiliar = familiarLabel;
            }
        }

        ///////
        // UNIT
     
        string raceName = string.IsNullOrEmpty(unitModel.Race) ? Labels.PERSONS : unitModel.Race;
        string raceLabel = Labels.LocalizeCountable(Categories.Race, raceName, unitModel.PeopleNumber == 1);
        if (!string.IsNullOrEmpty(unitModel.Prefix))
        {
            string racePrefixLabel = Labels.Localize(unitModel.Prefix);
            raceLabel = Labels.Localize(Labels.RACE_INFO, [racePrefixLabel, raceLabel]);
        }
        if (!string.IsNullOrEmpty(unitModel.TrueRaceType))
        {
            // add true race name as a suffix
            raceLabel += $" ({Labels.LocalizeCountable(Categories.Race, unitModel.TrueRaceType, unitModel.PeopleNumber == 1)})";
        }

        // number + persons + prefix + race (+ hero)
        string fullNameLabel = $"{unitModel.PeopleNumber} {raceLabel}";
        if (!string.IsNullOrEmpty(unitModel.Hero))
        {
            isHero = true;
            string heroesLabel = Labels.LocalizeCountable(Categories.None, Labels.HEROES, unitModel.PeopleNumber == 1, 2);
            fullNameLabel += $", {heroesLabel}";
        }

        DataProperty fullNameProperty = new DataProperty(fullNameLabel);
        NodeViewModel unitFirstChildNote = AddItem(unitNode, fullNameProperty);

        // DE: hungert / "Starving"
        string combatStatusLabel = (unitModel.CombatStatus == CombatStatusType.Undefined) ? string.Empty : Labels.Localize(Categories.CombatStatus, $"{(int)unitModel.CombatStatus}");
        string hungryLabel = Labels.Localize(Categories.Status, $"{unitModel.Hungry}");
        string guardsLabel = unitModel.IsGuarding ? Labels.Localize(Categories.Status, Labels.GUARDING) : string.Empty;
        string hpLabel = Labels.Localize(Categories.Status, $"{unitModel.Hp}");

        isStarving = !string.IsNullOrEmpty(hungryLabel);
        isGuarding = !string.IsNullOrEmpty(guardsLabel);

        string statusLabel = string.Empty;

        statusLabel += string.IsNullOrEmpty(combatStatusLabel) ? string.Empty : $"{combatStatusLabel},";
        statusLabel += string.IsNullOrEmpty(hungryLabel) ? string.Empty : $"{hungryLabel},"; // hunger
        statusLabel += string.IsNullOrEmpty(guardsLabel) ? string.Empty : $"{guardsLabel},";
        statusLabel += string.IsNullOrEmpty(hpLabel) ? string.Empty : $"{hpLabel},";
        statusLabel = statusLabel.TrimEnd(',').Replace(",", ", ");
        if (!string.IsNullOrEmpty(statusLabel))
        {
            DataProperty statusProperty = new(statusLabel);
            unitFirstChildNote = AddItem(unitNode, statusProperty);
        }

        if (!string.IsNullOrEmpty(unitModel.Aura) || !string.IsNullOrEmpty(unitModel.AuraMax))
        {
            string auraLabel = Labels.Localize(Labels.AURA_FROM_AURAMAX, [unitModel.Aura, unitModel.AuraMax]);
            DataProperty auraProperty = new DataProperty(auraLabel);
            unitFirstChildNote = AddItem(unitNode, auraProperty);
        }

        int weight = unitModel.Weight;
        if (weight > 0)
        {
            string weightValue = ToStringWithDecimals(weight);
            //string weightValue = weight % 100 == 0 ? $"{weight / 100}" : $"{(weight / 100.0F):F2}";
            string weightLabel = Labels.Localize(Categories.Node, Labels.WEIGHT_INFO, [weightValue]);
            DataProperty weightProperty = new(Categories.Node, Labels.WEIGHT_INFO, string.Empty, weightLabel);
            unitFirstChildNote = AddItem(unitNode, weightProperty);
        }

        foreach (DataProperty k in unitModel.DataProperties)
        {
            _ = AddItem(unitNode, k);
        }

        /////////
        // SPELLS

        if (unitModel.Spells != null)
        {
            string spellsLabel = Labels.Localize(Categories.Node, Labels.SPELLS);
            DataProperty spellsProperty = new(Categories.Node, Labels.SPELLS, string.Empty, spellsLabel);
            NodeViewModel spellsNode = AddItem(unitNode, spellsProperty);
            foreach (DataKey d in unitModel.Spells.GetData())
            {
                // Values in CR are already translated in English for EN CR
                string spellName = d.GetValue();
                string spellLabel = Labels.Localize(Categories.Spell, spellName);
                string spellTip = Labels.Localize(Categories.Spell, $"{spellName}_tip");
                DataProperty spellProperty = new(Categories.Spell, spellName, string.Empty, spellLabel, spellTip);
                _ = AddItem(spellsNode, spellProperty);
            }
        }

        ////////////////
        // COMBAT SPELLS

        if (unitModel.CombatSpells.Count > 0) 
        {
            string combatSpellsLabel = Labels.Localize(Categories.Node, Labels.COMBAT_SPELLS);
            DataProperty combatSpellsProperty = new(Categories.Node, Labels.COMBAT_SPELLS, string.Empty, combatSpellsLabel);
            NodeViewModel combatSpellsNode = AddItem(unitNode, combatSpellsProperty);
            for (SortedDictionary<int, DataBlock>.Enumerator itor = unitModel.CombatSpells.GetEnumerator(); itor.MoveNext();)
            {
                DataBlock block = itor.Current.Value;
                int period = itor.Current.Key;
                if (period > (int)CombatSpellPeriod.PostCombat) { period = 1; }
                string combatSpellPeriodLabel = Labels.Localize($"{Labels.COMBAT_SPELL_PERIOD}_{period}");
                // TODO: replace localization key from rules_spell to rules_combatspell_ in resx files
                string spell = Labels.Localize(Categories.Spell, block.Value(DataKey.KEYNAME_NAME));
                string level = block.Value(DataKey.KEYNAME_LEVEL);
                // type spell level
                string combatSpellLabel = $"{combatSpellPeriodLabel}: {spell} ({level})";
                DataProperty spellProperty = new(Categories.CombatSpell, combatSpellLabel, string.Empty, combatSpellLabel);
                AddItem(combatSpellsNode, spellProperty);
            }
        }

        if (unitModel.Effects != null) 
        {
            string effectsLabel = Labels.Localize(Categories.Node, Labels.EFFECTS);
            DataProperty effectsProperty = new(Categories.Node, Labels.EFFECTS, string.Empty, effectsLabel);
            NodeViewModel effectsNode = AddItem(unitNode, effectsProperty);
            foreach (DataKey effectKey in unitModel.Effects.GetData())
            {
                string effectName = effectKey.GetValue();
                string effectValue = string.Empty;
                string effectLabel;
                // if effectValue starts with a numeric value; extracts effectname to localize it
                string[] effectParts = effectName.Split(' ', 2);
                if (effectParts.Length == 2 && int.TryParse(effectParts[0], out _))
                {
                    effectValue = effectParts[0];
                    effectName = effectParts[1];
                    effectLabel = $"{effectValue} {Labels.Localize(Categories.Effect, effectName)}";
                }
                else
                {
                    effectLabel = effectName;
                    //effectLabel = effectValue.StartsWith('"') ? effectName : Labels.Localize(Categories.Effect, effectValue);
                }
                DataProperty effectProperty = new(Categories.None, effectName, effectValue, effectLabel);
                AddItem(effectsNode, effectProperty);
            }
        }

        List<DataProperty> skillsProperties = [];
        List<DataProperty> itemsProperties = [];

        if (unitModel.CollectSkills(ref skillsProperties))
        {
            string skillsLabel = Labels.Localize(Categories.Node, Labels.SKILLS);
            DataProperty skillsProperty = new(Categories.Node, Labels.SKILLS, string.Empty, skillsLabel);
            NodeViewModel skillsNode = AddItem(unitNode, skillsProperty);
            foreach (DataProperty sp in skillsProperties)
            {
                _ = AddItem(skillsNode, sp, $"{sp.Label} {sp.Value}");
            }
        }

        ////////
        // ITEMS

        if (unitModel.CollectItems(ref itemsProperties))
        {
            string itemsLabel = Labels.Localize(Categories.Node, Labels.ITEMS);
            DataProperty itemsProperty = new(Categories.Node, Labels.ITEMS, string.Empty, itemsLabel);
            NodeViewModel itemsNode = AddItem(unitNode, itemsProperty);
            foreach (DataProperty itemProperty in itemsProperties)
            {
                _ = AddItem(itemsNode, itemProperty, $"{itemProperty.Value} {itemProperty.Label}");
            }
        }

        int walkCapacity;
        int rideCapacity;
        int maxHorsesNumber;

        // walking capacity
        (walkCapacity, maxHorsesNumber) = unitModel.ComputeWalkingCapacity(Report);
        string capacityLabel;
        if (unitModel.HorsesNumber > maxHorsesNumber)
        {
            int n = unitModel.HorsesNumber - maxHorsesNumber;
            capacityLabel = n == 1 ? $"{walkCapacity / 100.0f:F2} ({n} horse too much)" : $"{walkCapacity / 100.0f:F2} ({n} horses too much)";
        }
        else
        {
            capacityLabel = $"{walkCapacity / 100.0f:F2}";
        }
        string capacityOnFootLabel = Labels.Localize(Labels.CAPACITY_ON_FOOT_INFO, [capacityLabel]);
        DataProperty capacityOnFootProperty = new(Categories.None, Labels.CAPACITY_ON_FOOT_INFO, string.Empty, capacityOnFootLabel);
        _ = InsertItem(unitNode, unitFirstChildNote, capacityOnFootProperty);

        // Riding capacity
        (rideCapacity, maxHorsesNumber) = unitModel.ComputeRidingCapacity();
        if (maxHorsesNumber > 0)
        {
            string ridingCapacityLabel;
            if (unitModel.HorsesNumber > maxHorsesNumber)
            {
                int n = unitModel.HorsesNumber - maxHorsesNumber;
                // TODO: localize "horses too much"
                ridingCapacityLabel = n == 1 ? $"{rideCapacity / 100.0f} ({n} horses too much)" : "";
                // label = n == 1 ? "%.2f (%d horses too much)" : "%.2f (%d horses too much)"), ride_cap / 100.0f, n) : "";
            }
            else
            {
                ridingCapacityLabel = $"{(rideCapacity / 100.0F):F2}";
            }
            string capacityOnHorseLabel = Labels.Localize(Categories.None, Labels.CAPACITY_ON_HORSE_INFO, [ridingCapacityLabel]);
            DataProperty capacityOnHorseProperty = new(Categories.None, Labels.CAPACITY_ON_HORSE_INFO, string.Empty, capacityOnHorseLabel);
            InsertItem(unitNode, unitFirstChildNote, capacityOnHorseProperty);
        }

        NodeViewModel? containerNode = null;
        // OPTIMIZE - no building when region is an ocean => could just checl ship

        ///////////
        // BUILDING
        DataProperty? containerProperty = null;
        if (unitModel.CollectBuildingData(Report, regionDataBlock, ref containerProperty))
        {
            containerNode = AddItem(Root, containerProperty!, string.Empty, 0);
            _ = AddContainerOwner(containerNode, unitModel, true);
            foreach (DataProperty p in unitModel.ContainerDataProperties)
            {
                _ = AddItem(containerNode, p, $"{p.Label}: {p.Value}");
            }
        }

        //////////////////////////////////////////////////////
        // SHIP - ONLY IF UNIT IS NOT ALREADY INSIDE A BULDING
        if (containerNode == null && unitModel.CollectShipData(Report, regionDataBlock, ref containerProperty))
        {
            containerNode = AddItem(Root, containerProperty, string.Empty, 0);
            _ = AddContainerOwner(containerNode, unitModel, false);

            if (unitModel.ShipPercentDamage > 0)
            {
                string damageLabel = Labels.Localize(Categories.None, Labels.SHIP_DAMAGE, [$"{unitModel.ShipPercentDamage}"]);
                DataProperty damageProperty = new(damageLabel);
                AddItem(containerNode, damageProperty);
            }
            if (unitModel.ShipCoast >= 0)
            {
                string coastLabel = Labels.GetCoastName(unitModel.ShipCoast);
                DataProperty coastProperty = new(coastLabel);
                AddItem(containerNode, coastProperty);
            }
            if (unitModel.ShipCargo > 0 || unitModel.ShipCapacity > 0)
            {
                string shipCapacityLabel = Labels.Localize(Categories.None, Labels.CAPACITY_SHIP_INFO, [ToStringWithDecimals(unitModel.ShipCargo), ToStringWithDecimals(unitModel.ShipCapacity)]);
                DataProperty shipCapacityProperty = new(Categories.None, Labels.CAPACITY_SHIP_INFO, string.Empty, shipCapacityLabel);
                AddItem(containerNode, shipCapacityProperty);
            }
        }

        if (containerNode != null && unitModel.DataProperties.Count > 0)
        {
            foreach (DataProperty p in unitModel.DataProperties)
            {
                _ = AddItem(containerNode, p, $"{p.Label}: {p.Value}");
            }
        }

        /////////////////////////////////////////
        // CONTAINER - BUILDING OR SHIP - EFFECTS

        if (unitModel.ContainerEffects.Count > 0 && containerNode != null)
        {
            string effectsLabel = Labels.Localize(Categories.Node, Labels.EFFECTS);
            DataProperty effectsProperty = new(Categories.Node, Labels.EFFECTS, string.Empty, effectsLabel);
            NodeViewModel effectsNode = AddItem(containerNode, effectsProperty);
            foreach (string effectName in unitModel.ContainerEffects)
            {
                // effectName is already localized in english (or german) in report
                // LATER: localize in any current other language than english
                DataProperty effectProperty = new(effectName);
                AddItem(effectsNode, effectProperty);
            }
        }

        ////////////////////////////

        Name = unitModel.Name;
        PeopleNumber = unitModel.PeopleNumber;
        Race = unitModel.Race;
        Aura = unitModel.Aura;
        Auramax = unitModel.AuraMax;
        Hero = unitModel.Hero;
        Hp = unitModel.Hp;
        Hungry = unitModel.Hungry;
        FamiliarMage = unitModel.FamiliarMageId;
        Weight = unitModel.Weight;

        IsFamiliar = isFamiliar;
        IsHero = isHero;
        IsStarving = isStarving;
        IsGuarding = isGuarding;
        IsFactionDisguised = isFactionDisguised;
        OtherFactionName = otherFactionName;
        MageNameIfFamiliar = mageNameIfFamiliar;

        Description = unitDataBlock.Value(KeyType.DESCRIPTION);

        /*
        // TODO: what about the folowing ?
        _factionName = string.Empty;
        _groupName = string.Empty;
        _combatStatus = string.Empty;
        _guards = string.Empty;
        */
    }
    private static bool AddContainerOwner(NodeViewModel parent, UnitModel unitModel, bool buildingContainer)
    {
        if (!string.IsNullOrEmpty(unitModel.ContainerOwnerName))
        {
            string ownerLabel = Labels.Localize(Categories.Node, buildingContainer ? Labels.OWNER : Labels.CAPTAIN);
            string ownerFullNameLabel = $"{ownerLabel}: {unitModel.ContainerOwnerName}";
            DataProperty ownerProperty = new(Categories.None, ownerFullNameLabel, string.Empty, ownerFullNameLabel, "", unitModel.ContainerOwnerUnit);
            AddItem(parent, ownerProperty);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Dispatch the select node Event using the event aggregator.
    /// Normally called by a double-click or a space key pressed on the currently selected item of this tree messages list in the corresponding view.
    /// Event is published only if the selected item is linked to a DataBlock (selection state has changed).
    /// </summary>
    /// <param name="node"></param>
    public void DispatchSelectedNode(NodeViewModel? node)
    {
        if (node == null)
        {
            return;
        }
        DataBlock? block = node?.Data?.Reference;
        if (block == null)
        {
            return;
        }
        ISelection sel = Selection!;
        if (sel.Item == block)
        {
            // If the block is already selected, we do not change the selection state.
            // This avoids reentrancy issues when the selection is changed by the view.
            return;
        }

        ISelection? newSelection;
        if (sel is ExplorerChildNodeSelection childNodeSel)
        {
            newSelection = new ExplorerChildNodeSelection(childNodeSel.RegionNodeViewModel!, block);
        }
        else
        {
            newSelection = new SimpleItemSelection(block, sel.Region, null);
        }

        // LATER: should I keep this newSel ?
        // SetSelection(newSelection);
        PublishSelectionChangedEvent(new SelectionChange(newSelection, this, null));
    }

    /// <summary>
    /// Append item under parent if index is -1; otherwise insert item at the specified index.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="label"></param>
    /// <param name="block"></param>
    /// <returns></returns>
    private static TreeNodeViewModel AddItem(NodeViewModel parent, DataProperty? property, string header = "", int index = -1)
    {
        return new(parent, property, header, index);
    }
    /// <summary>
    /// Insert item just before the specified brother node.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="brother"></param>
    /// <param name="property"></param>
    /// <param name="header"></param>
    /// <returns></returns>
    private static TreeNodeViewModel InsertItem(NodeViewModel parent, NodeViewModel brother, DataProperty? property, string header = "")
    {
        return new(parent, property, header, parent.Children.IndexOf(brother));
    }

}

