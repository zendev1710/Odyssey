using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Settings;
using HarfBuzzSharp;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Odyssey.Models.Data.RegionItem;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Represents the view model for region properties, providing data and functionality for displaying and interacting
/// with region-specific information in the application.
/// Content is updated each time a region is newly selected in the Explorer viezw.
/// </summary>
/// <remarks>This view model includes properties for various region attributes such as names, terrain types,
/// positions, and resources. It also handles data collection and updates based on region selection changes. The class
/// is designed to be used within a document tool context, inheriting from <see
/// cref="DocumentToolViewModelBase"/>.</remarks>
public partial class RegionPropertiesViewModel : DocumentToolViewModelBase
{
    //////////////////////////////////////////////////
    // Region properties

    [ObservableProperty]
    private double _opacity;

    /// <summary>
    /// Full name of the region. Includes coordinates and id.
    /// </summary>
    [ObservableProperty]
    private string? _fullRegionName;

    /// <summary>
    /// Short name of the region.
    /// </summary>
    [ObservableProperty]
    private string? _shortRegionName;

    /// <summary>
    /// Terrain type name.
    /// </summary>
    [ObservableProperty]
    private string? _terrainTypeName;

    /// <summary>
    /// Terrain type.
    /// </summary>
    [ObservableProperty]
    private int _terrainType;

    [ObservableProperty]
    private int _xPosition;

    [ObservableProperty]
    private int _yPosition;

    [ObservableProperty]
    private bool _isUnknown;

    [ObservableProperty]
    private bool _isNeighbour;

    [ObservableProperty]
    private bool _canHaveNaturalResources;

    // IMPROVE FEATURE: could get the more precise peaceful status (battle / only alliance / other not allied factions / enemy factions)
    [ObservableProperty]
    private bool _hasBattle;

    // IMPROVE FEATURE: could expose more information about guarding (which unit/faction is guarding)
    [ObservableProperty]
    private bool _isGuarded;

    [ObservableProperty]
    private string? _description;

    //////////////////////////////////////////////////
    // IMPROVE FEATURE: what about buildings and ships ?

    //////////////////////////////////////////////////
    // Region people

    [ObservableProperty]
    private int _people;

    [ObservableProperty]
    private int _factionPeople;

    [ObservableProperty]
    private int _factionSilver;

    //////////////////////////////////////////////////
    // Region activity

    [ObservableProperty]
    private int _silver;

    /// <summary>
    /// Number of peasants in the region.
    /// </summary>
    [ObservableProperty]
    private int _peasants;

    [ObservableProperty]
    private int _recruits;

    [ObservableProperty]
    private int _entertainment;

    //////////////////////////////////////////////////
    // Business and costs

    /// <summary>
    /// Surplus. Means Farm yield.
    /// </summary>
    [ObservableProperty]
    private int _surplus;

    /// <summary>
    /// Salary. Means peasants wages.
    /// </summary>
    [ObservableProperty]
    private int _salary;

    [ObservableProperty]
    private long _entertainRevenue;

    /// <summary>
    /// Miscellaneous income from other activities.
    /// </summary>
    [ObservableProperty]
    private long _miscRevenue;

    /// <summary>
    /// Taxes (steer).
    /// </summary>
    [ObservableProperty]
    private long _taxes;

    /// <summary>
    /// Income from trade.
    /// </summary>
    [ObservableProperty]
    private long _tradeRevenue;

    /// <summary>
    /// Income from trade.
    /// </summary>
    [ObservableProperty]
    private long _trade;

    /// <summary>
    /// Income from sorcery.
    /// </summary>
    [ObservableProperty]
    private long _sorcery;

    /// <summary>
    /// Income from theft.
    /// </summary>
    [ObservableProperty]
    private long _theft;

    /// <summary>
    /// Trade duties (cost).
    /// </summary>
    [ObservableProperty]
    private long _tradeDuties;

    /// <summary>
    /// Learning costs.
    /// </summary>
    [ObservableProperty]
    private long _learningCosts;

    //////////////////////////////////////////////////
    // Region resources

    /// <summary>
    /// Number of trees.
    /// If region has mallorn, this is the number of mallorn trees.
    /// </summary>
    [ObservableProperty]
    private int _trees;

    /// <summary>
    /// Number of saplings. Saplings are young trees.
    /// If region has mallorn, this is the number of mallorn saplings.
    /// </summary>
    [ObservableProperty]
    private int _saplings;

    [ObservableProperty]
    private int _horses;

    [ObservableProperty]
    private int _iron;

    [ObservableProperty]
    private int _stone;

    [ObservableProperty]
    private int _laen;

    [ObservableProperty]
    private bool _hasLaen;

    [ObservableProperty]
    private bool _hasMallorn;

    [ObservableProperty]
    private string? _treesType;

    [ObservableProperty]
    private string? _saplingsType;

    /// <summary>
    /// Herb present in the region.
    /// </summary>
    [ObservableProperty]
    private string? _herb;

    //////////////////////////////////////////////////
    // Region trade resources

    // OPTIMIZE : use converter for that
    [ObservableProperty]
    private string? _tradeCategoryLabel;

    [ObservableProperty]
    private int _balm;

    [ObservableProperty]
    private int _spice;

    [ObservableProperty]
    private int _gem;

    [ObservableProperty]
    private int _myrrh;

    [ObservableProperty]
    private int _oil;

    [ObservableProperty]
    private int _silk;

    [ObservableProperty]
    private int _incense;

    public RegionPropertiesViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }
    public RegionPropertiesViewModel(IEventAggregator? eventAggregator): base(eventAggregator)
    {
        _opacity = GlobalSettings.EnableTerrainBackgroundColor ? 0.9 : 1;
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
        if (sel.IsRegionSelected() && !IsSelectedRegion(sel))
        {
            SetSelection(sel);

            // TODO: update data : 
            // - description in a textbox if exist
            // - description as a tooltip (on full region name - 1st expander header) if exist 
            CollectData(Selection.Region!);
        }
        Debug.WriteLine($"[VM-REG-PROP] SELSTATE CHANGED {sel} -> ENDED");
    }

    /// <summary>
    /// Links the report document to the view model.
    /// </summary>
    /// <param name="cr">the report document to link</param>
    /// <returns>true if the report document has been linked to the view model and has data; otherwise false</returns>
    protected override bool SetMapFile(CRDocument cr)
    {
        return base.SetMapFile(cr);
    }

    protected static void AddResourceItem(RegionItemCategory category, RegionItemType pid, Dictionary<RegionItemType, RegionItem> items, string name, ulong value, int skill = 0)
    {
        RegionItemType id = pid;
        if (pid == RegionItemType.UNKNOWN)
        {
            KeyType kt = DataKey.ParseType(name, BlockType.RESOURCE);
            if (kt == KeyType.UNKNOWN)
            {
                id = GetId(name);
            }
            else
            {
                id = RegionItem.ConvertToItemType(kt);
            }
        }

        if (id == RegionItemType.UNKNOWN)
        {
            // Adamantium will be displayed as unknown
            Debug.WriteLine($"[VM-REG-PROP] !!! AddDataItem: Unknown resource {name}");
            return;
        }

        if (items.TryGetValue(id, out var item))
        {
            // Update the resource
            item.Value += value;
            if (skill < item.Skill)
            {
                item.Skill = skill;
                if (skill != 0)
                {
                    Debug.WriteLine($"[VM-REG-PROP] AddItem {name} -> skill {skill}");
                }
            }
            return;
        }
        items[id] = new RegionItem(name, value, category, skill);
    }

    protected static void AddTradeItem(Dictionary<RegionItemType, RegionItem> items, string name, ulong value)
    {
        RegionItemType id = GetId(name);
        if (items.TryGetValue(id, out var item))
        {
            // Update the resource
            item.Value += value;
            return;
        }
        items[id] = new RegionItem(name, value, RegionItemCategory.TRADE);
    }

    protected void CollectData(DataBlock region)
    {
        CollectResourcesItem(region);
        CollectIncomeItem(region);
        CollectTradeItems(region);

        Description = region.Value(KeyType.DESCRIPTION);
        Herb = Labels.Localize(Categories.Item,region.Value(KeyType.HERB));
    }

    /// <summary>
    /// Region data collection -> region panel data
    /// </summary>
    protected void CollectResourcesItem(DataBlock region)
    {
        Dictionary<RegionItemType, RegionItem> resourceItems = [];
        //Dictionary<int, RegionItem> items = [];

        //////////////////////////////////////////////////
        // Region properties

        // For a named region, it's ok; for another, should use GetUIName()
        string shortRegionName = region.Value(KeyType.NAME);
        string fullRegionName = region.GetUILabel();
        int xPosition = region.GetX();
        int yPosition = region.GetY();
        int terrainType = region.GetTerrain();
        string terrainTypeName = Terrains.GetLabel(TerrainType);

        ///////////////////////////////////////////////
        /// OCEAN STATUS
        /// - not seen: not highlighted and not a neighbor)
        /// - neighbor: not highlighted
        /// - travel: highlighted
        /// - seen directly: seen by active/ally faction 
        /// 
        /// Only ocean with ship is in the region explorer
        /// ocean information are only ships (and who they belong it) - and coordinates
        ///////////////////////////////////////////////

        // LATER: compute this property according to the Selection.SelectedRegions
        // An unknown region can not be selected by the region explorer, only by the world map explorer
        bool isUnknown = false;

        // TODO: compute and set the value of these properties
        bool isNeighbour = false;
        bool canHaveNaturalResources = false;
        bool hasBattle = false;
        bool isGuarded = false;

        //////////////////////////////////////////////////
        // Collect region resources from the region block
        // OPTIMIZE : Should be done at report file loading

        int peasants = region.ValueInt(KeyType.PEASANTS, -1);
        int regionSilver = region.ValueInt(KeyType.SILVER, -1);
        int entertainment = region.ValueInt(ENTERTAINMENT_NAME, -1);
        int recruits = region.ValueInt(RECRUITS_NAME, -1);
        int horses = region.ValueInt(HORSES_NAME, -1);

        if (entertainment >= 0)
        {
            AddResourceItem(RegionItemCategory.ACTIVITY, RegionItemType.ENTERTAINMENT_MAX, resourceItems, ENTERTAINMENT_MAX_NAME, (ulong)entertainment);
        }
        if (recruits >= 0)
        {
            AddResourceItem(RegionItemCategory.ACTIVITY, RegionItemType.RECRUITS, resourceItems, RECRUITS_NAME, (ulong)recruits, 0);
        }
        if (horses >= 0)
        {
            AddResourceItem(RegionItemCategory.NATURAL_RESOURCE, RegionItemType.HORSES, resourceItems, HORSES_NAME, (ulong)horses, 0);
        }

        int people = 0;
        int factionPeople = 0;
        int factionSilver = 0;

        RegionAttachment? regionAttachment = region.GetAttachment() as RegionAttachment;
        RegionInfos? regionInfos = regionAttachment?.RegionInfos;
        if (regionInfos != null)
        {
            people = regionInfos.People;
            factionPeople = regionInfos.FactionPeople;
            factionSilver = regionInfos.FactionSilver;
        }
        else
        {
            // Compute and store region information
            // no regionInfo -> collect information (resources and peole...) from other blocks
            bool isActiveFaction = false;
            regionInfos = regionAttachment!.SetRegionInfos(new RegionInfos());
            var startBlock = region.GetNextBlock();
            int depth = region.GetDepth();
            for (var b = startBlock; b != null; b = b.GetNextBlock())
            {
                if (b.GetDepth() <= depth)
                {
                    // The block is not a child, just a brother or parent
                    break;
                }
                BlockType btype = b.GetBlockType();
                if (btype == BlockType.RESOURCE)
                {
                    string resourceType = b.Value(KeyType.RESOURCE_TYPE);
                    // don't double-count resources in REGION block
                    if ($"{DataKey.TREES_NAME}|{DataKey.SILVER_NAME}|{DataKey.HORSES_NAME}".Contains(resourceType))
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(resourceType))
                    {
                        int skill = b.ValueInt(KeyType.RESOURCE_SKILL);
                        int number = b.ValueInt(KeyType.RESOURCE_COUNT);
                        AddResourceItem(RegionItemCategory.NATURAL_RESOURCE, RegionItemType.UNKNOWN, regionAttachment.RegionInfos.Items, resourceType, (ulong)number, skill);
                    }
                }
                else if (btype == BlockType.UNIT)
                {
                    // collect people from units to :
                    // - compute total number of people in the region
                    // - compute number of people from the active faction
                    DataBlock unitBlock = b;
                    int factionId = GetFactionIdForUnit(unitBlock);
                    isActiveFaction = Report.IsActiveFaction(factionId);
                    int number = unitBlock.ValueInt(KeyType.NUMBER, -1);
                    if (number > 0)
                    {
                        people += number;
                        if (isActiveFaction)
                        {
                            factionPeople += number;
                        }
                    }
                }
                else if (btype == BlockType.ITEMS)
                {
                    if (isActiveFaction)
                    {
                        int silver = b.ValueInt(KeyType.SILVER, -1);
                        if (silver > 0)
                        {
                            factionSilver += silver;
                        }
                    }

                }
            }

            regionInfos.People = people;
            regionInfos.FactionPeople = factionPeople;
            regionInfos.FactionSilver = factionSilver;
        }

        foreach (var r in regionAttachment.RegionInfos.Items)
        {
            AddResourceItem(RegionItemCategory.NATURAL_RESOURCE, RegionItemType.UNKNOWN, resourceItems, r.Value.Name, r.Value.Value, r.Value.Skill);
        }

        if (people > 0)
        {
            AddResourceItem(RegionItemCategory.ACTIVITY, RegionItemType.PEOPLE, resourceItems, "People", (ulong)people, 0);
        }
        if (factionPeople > 0)
        {
            AddResourceItem(RegionItemCategory.ACTIVITY, RegionItemType.FACTION_PEOPLE, resourceItems, "Faction people", (ulong)factionPeople, 0);
            AddResourceItem(RegionItemCategory.ACTIVITY, RegionItemType.FACTION_SILVER, resourceItems, "Faction silver", (ulong)factionSilver, 0);
        }

        //////////////////////////////////////////////////
        // Set all properties

        ShortRegionName = shortRegionName;
        FullRegionName = fullRegionName;
        XPosition = xPosition;
        YPosition = yPosition;
        TerrainType = terrainType;
        TerrainTypeName = terrainTypeName;

        YPosition = region.GetY();
        TerrainType = region.GetTerrain();
        TerrainTypeName = Terrains.GetLabel(TerrainType);

        IsUnknown = isUnknown;
        IsNeighbour = isNeighbour;
        CanHaveNaturalResources = canHaveNaturalResources;
        HasBattle = hasBattle;
        IsGuarded = isGuarded;

        // OPTIMIZE: use RegionItemViewModel class

        People = resourceItems.TryGetValue(RegionItemType.PEOPLE, out RegionItem? item) ? (int)item.Value : people;
        FactionPeople = resourceItems.TryGetValue(RegionItemType.FACTION_PEOPLE, out item) ? (int)item.Value : factionPeople;
        FactionSilver = resourceItems.TryGetValue(RegionItemType.FACTION_SILVER, out item) ? (int)item.Value : factionSilver;

        Silver = resourceItems.TryGetValue(RegionItemType.SILVER, out item) ? (int)item.Value : regionSilver;
        Peasants = resourceItems.TryGetValue(RegionItemType.PEASANTS, out item) ? (int)item.Value : peasants;
        Recruits = resourceItems.TryGetValue(RegionItemType.RECRUITS, out item) ? (int)item.Value : -1;
        Surplus = resourceItems.TryGetValue(RegionItemType.PEASANTS_WAGES, out item) ? (int)item.Value : -1;
        // TO add translation in DE tooltip : Maximale Anzahl Silber, dass per Unterhaltung eingenommen werden kann 
        Entertainment = resourceItems.TryGetValue(RegionItemType.ENTERTAINMENT_MAX, out item) ? (int)item.Value : -1; 
        int trees = resourceItems.TryGetValue(RegionItemType.TREES, out item) ? (int)item.Value : -1;
        int saplings = resourceItems.TryGetValue(RegionItemType.SAPLINGS, out item) ? (int)item.Value : -1;

        Horses = resourceItems.TryGetValue(RegionItemType.HORSES, out item) ? (int)item.Value : -1;
        Iron = resourceItems.TryGetValue(RegionItemType.IRON, out item) ? (int)item.Value : -1;
        Stone = resourceItems.TryGetValue(RegionItemType.STONES, out item) ? (int)item.Value : -1;

        Laen = resourceItems.TryGetValue(RegionItemType.LAEN, out item) ? (int)item.Value : -1;
        int mallorn = resourceItems.TryGetValue(RegionItemType.MALLORN, out item) ? (int)item.Value : -1;
        Trees = mallorn > 0 ? mallorn : trees;
        Saplings = resourceItems.TryGetValue(RegionItemType.MALLORN_SAPLINGS, out item) ? (int)item.Value : saplings;

        HasLaen = Laen > 0;
        HasMallorn = mallorn > 0;
        TreesType = Labels.Localize(Categories.Node, HasMallorn ? Labels.MALLORN_TREES : Labels.TREES);
        SaplingsType = Labels.Localize(Categories.Node, HasMallorn ? Labels.MALLORN_SAPLINGS : Labels.SAPLINGS);
    }
    protected static (int, int, int) FindInfoForIncome(DataBlock region)
    {
        // Find peasants, trees and saplings
        int trees = region.ValueInt(KeyType.TREES);
        int saplings = region.ValueInt(KeyType.SAPLINGS);
        int peasants = region.ValueInt(KeyType.PEASANTS);

        var startBlock = region.GetNextBlock();
        int depth = region.GetDepth();
        for (DataBlock? resourceBlock = startBlock; resourceBlock != null; resourceBlock = resourceBlock.GetNextBlock())
        {
            if (resourceBlock.GetDepth() <= depth)
            {
                break;
            }
            if (resourceBlock.GetBlockType() != BlockType.RESOURCE)
            {
                continue;
            }
            string value = resourceBlock.Value(KeyType.RESOURCE_TYPE);
            KeyType type = (KeyType)((int)(DataKey.ParseType(value, BlockType.RESOURCE)) & ((1 << 7) - 1));
            if (type == KeyType.PEASANTS)
            {
                peasants = resourceBlock.ValueInt(KeyType.RESOURCE_COUNT);
            }
            else if (type == KeyType.TREES)
            {
                trees = resourceBlock.ValueInt(KeyType.RESOURCE_COUNT);
            }
            else if (type == KeyType.SAPLINGS)
            {
                saplings = resourceBlock.ValueInt(KeyType.RESOURCE_COUNT);
            }
        }

        return (trees, saplings, peasants);
    }

    /// <summary>
    /// Region data collection -> region panel data
    /// </summary>
    protected void CollectIncomeItem(DataBlock region)
    {
        Dictionary<Income.Kind, RegionItem> incomeItems = [];

        int peasants;
        int trees;
        int saplings;

        int salary = region.ValueInt(KeyType.SALARY);

        long taxes = 0;
        long tradeRevenue = 0;
        long sorcery = 0;
        long theft = 0;
        long miscRevenue = 0;
        long entertainRevenue = 0;
        long tradeDuties = 0;
        long learningCosts = 0;
        
        (trees, saplings, peasants) = FindInfoForIncome(region);
        int surplus = Income.ComputeSurplus(region.GetTerrain(), trees, saplings, peasants, salary);

        RegionAttachment? att = region.GetAttachment() as RegionAttachment;
        learningCosts = att!.LearnCost;
        miscRevenue = att.GetIncome(Income.Kind.MISC);
        entertainRevenue = att.GetIncome(Income.Kind.ENTERTAIN);
        taxes = att.GetIncome(Income.Kind.TAXES);
        tradeRevenue = att.GetIncome(Income.Kind.TRADE);
        theft = att.GetIncome(Income.Kind.THEFT);
        sorcery = att.GetIncome(Income.Kind.MAGIC);
        tradeDuties = att.GetIncome(Income.Kind.TARIFFS);

        //////////////////////
        // Income and cost - original code stats panel

        Surplus = surplus;
        Salary = salary;
        EntertainRevenue = entertainRevenue;

        MiscRevenue = miscRevenue;
        Taxes = taxes;
        LearningCosts = learningCosts;
        TradeRevenue = tradeRevenue;
        Theft = theft;
        Sorcery = sorcery;
        TradeDuties = tradeDuties;

    }

    /// <summary>
    /// Trade data collection on the region -> trade panel data
    /// </summary>
    protected void CollectTradeItems(DataBlock region)
    {
        // Search prices block of this region.
        // They are retrieved each time from the region block (not stored).
        // OPTIMIZE : keep the prices information in the region block, as they do not change.
        Dictionary<RegionItemType, RegionItem> tradeItems = [];

        string soldItemName = "";
        int soldPrice = 0;

        DataBlock? pricesBlock = null;
        //LinkedListNode<DataBlock>? regionNode = region.Node;
        if (GetSeenChild(ref pricesBlock, region, BlockType.PRICES))
        {
            /*	PREISE
				96;Balsam		// balm is purchased
				-5;Seide		// silk is sold
			*/
            foreach (var good in pricesBlock!.GetData())
            {
                string name = good.GetKeyFromType();
                string value = good.GetValue();
                if (name.Length != 0 && value.Length != 0)
                {
                    int price = Utils.Converters.StringToInt(value);
                    if (price < 0)
                    {

                        // sold trade item
                        price = -price;
                        soldPrice = price;
                        soldItemName = name;
                    }
                    // CGN - use translated text to fill info
                    //createLabels("Purchase price " + goods.translatedKey(), Globals.thousandsPoints(new FXint(price)), -1); // -1 == topmatrix
                    //createLabels("Purchase price " + goods->key(), thousandsPoints(price), -1);	// -1 == topmatrix
                    AddTradeItem(tradeItems, name, (ulong)price);
                }
            }
        }

        // OPTIMIZE: use a TradeItemViewModel class
        RegionItem? tradeItem = null;
        Trade = tradeItems.TryGetValue(RegionItemType.TRADE, out tradeItem) ? (int)tradeItem.Value : -1;
        Balm = tradeItems.TryGetValue(RegionItemType.BALM, out tradeItem) ? (int)tradeItem.Value : -1;
        Spice = tradeItems.TryGetValue(RegionItemType.SPICE, out tradeItem) ? (int)tradeItem.Value : -1;
        Gem = tradeItems.TryGetValue(RegionItemType.GEM, out tradeItem) ? (int)tradeItem.Value : -1;
        Myrrh = tradeItems.TryGetValue(RegionItemType.MYRRH, out tradeItem) ? (int)tradeItem.Value : -1;
        Oil = tradeItems.TryGetValue(RegionItemType.OIL, out tradeItem) ? (int)tradeItem.Value : -1;
        Silk = tradeItems.TryGetValue(RegionItemType.SILK, out tradeItem) ? (int)tradeItem.Value : -1;
        Incense = tradeItems.TryGetValue(RegionItemType.INCENSE, out tradeItem) ? (int)tradeItem.Value : -1;
        TradeCategoryLabel = $"Trade {Trade} - Sell {soldItemName} at price={soldPrice}";
    }

    protected void UpdateData() 
    {
        if (!Selection.IsRegionSelected())
        {
            return;
        }

        DataBlock? region = Selection.Item;
        // RETRIEVE REGION NAME
        /*
        string name = region.value(KeyType.NAME);
        if (string.IsNullOrEmpty(name))
        {
            name = region.terrainTranslatedString();
        }
        if (string.IsNullOrEmpty(name))
        {
            name = "Unknown";
        }
        string label;
        tags.name.setText(label.format("%s (%d,%d)", name.text(), region.x(), region.y()));
        label.format("%s from %s (%d,%d)", region.terrainTranslatedString().text(), name.text(), region.x(), region.y());
        */

        //
        /*

        // Description in the description field
        tags.desc.setText(description);

        // Description as Tooltip
        for (label.clear(); description.length();)
        {
            if (label.length() != 0)
            {
                label += "\n";
            }
            label += description.before(' ', 7);
            description = description.after(' ', 7);
        }
        tags.name.setTipText(label);
        */

        /*
			// select terrain image
			FXint terrain = region.terrain();
			tags.name.setIcon(terrainIcons[terrain]);

			// collect information (Farmers, Silver, Horses...)
			List<RegionInfo> info = new List<RegionInfo>();
        	// Bauern, Silber, Unterhaltung (Unterh), Rekruten, Parteisilber => Farmers, Silver, Entertainment (company), Recruits, Faction silver
			// Pferde, Laen, Eisen, Baeume, Schoesslinge => Horses, horses, iron, trees, saplings
			collectData(info, selection.region);

			// apply information entries (Farmers, Silver, Horses...)
			setInfo(info);
		}
		*/

        // UNKNOWN REGION
        /*
			if ((selection.selected & selection.UNKNOWN_REGION) != 0)
			{
				FXString label = new FXString();
				tags.name.setText(label.format("Unknown (%d,%d)", selection.sel_x, selection.sel_y));
			}
			else
			{
				tags.name.setText("");
			}

			tags.name.setHelpText("");
			tags.name.setIcon(terrainIcons[data.Globals.TERRAIN_UNKNOWN]);

			tags.matrixsep.hide();
			tags.matrixframe.hide();
			clearLabels();

			tags.desc.setText("");
		}
        */
    }
}
