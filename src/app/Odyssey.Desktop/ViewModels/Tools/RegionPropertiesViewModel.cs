using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Settings;
using System.Collections.Generic;
using System.Diagnostics;
using static Odyssey.Models.Data.RegionItem;
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

    public RegionPropertiesViewModel() : base()
    {
        _opacity = GlobalSettings.EnableTerrainBackgroundColor ? 0.9 : 1;
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this)) return;

        bool regionChanged = selType.HasFlag(SelectionChangeTypes.Region);
        bool regionSelected = selection.IsRegionSelected();

        Debug.WriteLine($"[VM-REG-PROP] OnSelectionChanged: region changed={regionChanged}, region selected={regionSelected}");

        if (regionChanged && selection.Region is RegionModel region)
        {
            CollectData(region);
        }
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

    protected void CollectData(RegionModel region)
    {
        CollectResourcesItem(region);
        CollectIncomeItem(region);
        CollectTradeItems(region);

        Description = region.Description;
        Herb = Labels.Localize(Categories.Item, region.Herb);

        CollectAdditionalData();
    }

    /// <summary>
    /// Region data collection -> region panel data
    /// </summary>
    protected void CollectResourcesItem(RegionModel region)
    {
        Dictionary<RegionItemType, RegionItem> resourceItems = [];
        //Dictionary<int, RegionItem> items = [];

        //////////////////////////////////////////////////
        // Region properties

        // For a named region, it's ok; for another, should use GetUIName()
        string shortRegionName = region.ShortName;
        string fullRegionName = region.DisplayLabel;
        int xPosition = region.GetX();
        int yPosition = region.GetY();
        int terrainType = region.Terrain;
        string terrainTypeName = Terrains.GetLabel(terrainType);

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

        int peasants = region.Peasants;
        int regionSilver = region.Silver;
        int entertainment = region.Entertainment;
        int recruits = region.Recruits;
        int horses = region.Horses;

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

        RegionInfos regionInfos = region.GetRegionInfos(Report);
        people = regionInfos.People;
        factionPeople = regionInfos.FactionPeople;
        factionSilver = regionInfos.FactionSilver;

        foreach (var r in regionInfos.Items)
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
    /// <summary>
    /// Region data collection -> region panel data
    /// </summary>
    protected void CollectIncomeItem(RegionModel region)
    {
        int salary = region.Salary;

        RegionModel.IncomeInputs incomeInputs = region.GetIncomeInputs();
        int surplus = Income.ComputeSurplus(region.Terrain, incomeInputs.Trees, incomeInputs.Saplings, incomeInputs.Peasants, salary);

        long learningCosts = region.LearningCost;
        long miscRevenue = region.GetIncome(Income.Kind.MISC);
        long entertainRevenue = region.GetIncome(Income.Kind.ENTERTAIN);
        long taxes = region.GetIncome(Income.Kind.TAXES);
        long tradeRevenue = region.GetIncome(Income.Kind.TRADE);
        long theft = region.GetIncome(Income.Kind.THEFT);
        long sorcery = region.GetIncome(Income.Kind.MAGIC);
        long tradeDuties = region.GetIncome(Income.Kind.TARIFFS);

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
    protected void CollectTradeItems(RegionModel region)
    {
        Dictionary<RegionItemType, RegionItem> tradeItems = [];

        string soldItemName = "";
        int soldPrice = 0;

        foreach (RegionModel.TradePrice good in region.GetTradePrices())
        {
            int price = good.Price;
            if (price < 0)
            {
                soldPrice = -price;
                soldItemName = good.Name;
            }
            AddTradeItem(tradeItems, good.Name, (ulong)(price < 0 ? -price : price));
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

    protected void CollectAdditionalData()
    {
        // TODO: handle additional data :
        // - description in a textbox if exist
        // - description as a tooltip (on full region name - 1st expander header) if exist

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
