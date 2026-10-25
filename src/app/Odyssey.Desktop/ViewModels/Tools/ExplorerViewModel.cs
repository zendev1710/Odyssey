using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Controls.Buildings;
using Odyssey.Controls.Ships;
using Odyssey.Icons;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using Odyssey.Services;
using Odyssey.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Tools.DataProperty;
using static Odyssey.ViewModels.ExplorerNodeViewModel;

namespace Odyssey.ViewModels.Tools;

public partial class ExplorerViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    private ExplorerNodeViewModel? selectedItem;

    private readonly ExplorerNodeViewModel _root;
    private readonly Dictionary<int, FactionInfo> _factionsInfo = [];

    /// <summary>
    /// when is true, units of the active faction are grouped together inside a faction node at the top of the factions list;
    /// otherwise units of the active faction are inserted directly as child of region node, after buildings).
    /// BUG: at this time, when it's false (no faction node for active faction) it has annoying side effects on tree navigation (prev/next unit and so on)
    /// </summary>
    private readonly bool _activeFactionGroup;

    /// <summary>
    /// when is true, the tree will be fully expanded after document is opened.
    /// </summary>
    private readonly bool _expandTreeOnDocumentLoading;

    protected ExplorerNodeViewModel Root { get { return _root; } }

    protected bool ActiveFactionGroup { get { return _activeFactionGroup; } }
    protected bool ExpandTreeOnDocumentLoading { get { return _expandTreeOnDocumentLoading; } }

    /// <summary>
    /// Tree of nodes ordered like this :
    /// Root (not visible)
    ///   Region 1
    ///     Ships (only if region has ships)
    ///       Ship 1
    ///       Ship 2
    ///     Buildings (only if region has buildings)
    ///       Building 1
    ///       Building 2
    ///     Active Faction 1
    ///       Unit AC1
    ///       UniT AC2
    ///     Active Faction 2
    ///       Unit AC2.1
    ///       UniT AC2.2
    ///     Allied Faction 1
    ///       Unit AL1
    ///       Unit AL2
    ///     Allied Faction 2
    ///       Unit AL2.1
    ///       Unit AL2.2
    ///    NOT Allied Faction 1
    ///       Unit NA1
    ///       Unit NA2
    ///    NOT Allied Faction 2
    ///       Unit NA2.1
    ///       Unit NA2.2
    ///   Region 2
    ///     ...
    /// </summary>
    public ObservableCollection<ExplorerNodeViewModel> Items { get; }

    public IReadOnlyList<MenuItemViewModel> NodeMenuItems { get; set; }

    public struct FactionInfo
    {
        public FactionInfo()
        {
        }
        public DataBlock? Block { get; set; }
        public FactionStatus Status { get; set; } = FactionStatus.UNKNOWN;
        public string Name { get; set; } = "";
        public FactionRace Race { get; set; } = FactionRace.Unknown;
        public int Id { get; set; } = -1;
    }

    public ExplorerViewModel() : base()
    {
        _root = new ExplorerNodeViewModel();

        _activeFactionGroup = !GlobalSettings.Get<bool>(GlobalSettings.EXPLORER_ACTIVE_FACTION_UNITS_AT_FIRST);
        _expandTreeOnDocumentLoading = GlobalSettings.Get<bool>(GlobalSettings.EXPLORER_EXPAND_TREE_ON_REPORT_OPENING);

        Items = _root.Children;

        NodeMenuItems =
            [
                //new MenuItemViewModel { Header = "_Open...", Command = OpenCommand },
                //new MenuItemViewModel { Header = "Save", Command = SaveCommand },
                new MenuItemViewModel { Header = "-" },
                /*
                new MenuItemViewModel
                {
                    Header = "Recent",
                    Items = new[]
                    {
                        new MenuItemViewModel
                        {
                            Header = "File1.txt",
                            Command = OpenRecentCommand,
                            CommandParameter = @"c:\foo\File1.txt"
                        },
                        new MenuItemViewModel
                        {
                            Header = "File2.txt",
                            Command = OpenRecentCommand,
                            CommandParameter = @"c:\foo\File2.txt"
                        },
                    }
                },
                */
            ];
    }

    /// <summary>
    /// Update selection state with the selected item.
    /// Called automatically via [ObservableProperty] attribute when the SelectedItem property has been modified.
    /// </summary>
    partial void OnSelectedItemChanged(ExplorerNodeViewModel? oldValue, ExplorerNodeViewModel? newValue)
    {
        // LATER: check what to do if newValue is null (maybe it never happens !?)
        if (oldValue != newValue && newValue != null)
        {
            SendSelectionChangedEvent(new ExplorerNodeSelection(newValue, Report));
        }
    }


    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this))
            return;

        // Handle selection changed event from all selectors except itself and BookMarksViewModel (which can select a unseen region).
        // unseen region selection change event will be received only from map view.

        if (!HandleSelection(selection))
        {
            Debug.WriteLine("[EXPLORER] selected item did not change");
        }
    }

    /// <summary>
    /// Find the tree node of the specified entity, or null when it is not in the tree.
    /// </summary>
    private ExplorerNodeViewModel? FindNode(EntityModel item)
    {
        // A region node is identified by the region key (RegionModel.Id); the id of the region data
        // block is the plane, which is not unique to a region.
        if (item is RegionModel regionModel)
        {
            return FindImmediateTreeItem(Root, regionModel.Id);
        }
        return FindTreeItem(Root, item.DataBlock.GetId());
    }

    /// <summary>
    /// Handle active document changed event.
    /// Rebuilds the tree from the new active document data.
    /// </summary>
    /// <param name="cr"></param>
    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

        Clear();
        if (HasDocument)
        {
            RebuildTree();
        }
    }

    private bool HandleSelection(IEntitySelection selection)
    {
        if (selection is ExplorerNodeSelection explorerNodeSel)
        {
            // Store the inner selector (the one who raised the sel change event)
            // to avoid it to react when auto-publishing (in) this event foir the other views
            SelectedItem = explorerNodeSel.NodeViewModel;
            return true;
        }

        // Some senders do not carry the region of the item they select (bookmarks, history, details
        // views, ...): in that case, the matching node can still be found from the selected item itself.
        if (selection.Region is null && selection.Item is not null)
        {
            ExplorerNodeViewModel? itemNode = FindNode(selection.Item);
            if (itemNode is not null)
            {
                SelectItem(itemNode);
                return true;
            }
        }

        ExplorerNodeViewModel? regionNodeViewModel = null;
        RegionModel? regionModel = selection.Region;
        if (regionModel is null)
        {
            Debug.WriteLine("[EXPLORER] WARNING | Region model not found for selection.");
            return false;
        }

        if (!selection.IsRegionSelected())
        {
            RegionDataBlock selectedRegionBlock = (regionModel.DataBlock as RegionDataBlock)!;
            if (selection.Item is null || !FindRegionFrom(ref selectedRegionBlock, selection.Item.DataBlock))
            {
                Debug.WriteLine("[EXPLORER] WARNING | Region parent not found for {sel.Item}.");
                return false;
            }
        }

        // TODO: check selectedRegion is always not null here

        // Select a not-region item, region being known
        if (!selection.HasOnlyRegion())
        {
            // If the selection comes from the explorer, it already carries the region node.
            if (selection is ExplorerChildNodeSelection explorerChildNodeSel)
            {
                regionNodeViewModel = explorerChildNodeSel.RegionNodeViewModel;
            }

            // Otherwise, find the region node from the selected region.
            regionNodeViewModel ??= FindImmediateTreeItem(Root, regionModel.Id);
        }
        else
        {
            // Only regions with people are in the tree
            if (regionModel.IsWithPeople)
            {
                regionNodeViewModel = FindImmediateTreeItem(Root, regionModel.Id);
            }
        }

        // To avoid to search a faction node that does not exist, remove FACTION from selection if it's the active faction
        /*
        // ActiveFactionGroup is always enabled, so for the moment no need to process
        CRDocument cr = GetDocument();
        if (!ActiveFactionGroup && cr.HasActiveFaction()  && sel.IsFactionSelected() && cr.IsActiveFaction(sel.Faction))
        {
            Debug.WriteLine("[EXPLORER] WARNING | TODO :remove FACTION mask from selection");
            // TODO: remove FACTION from selection
            // TODO: if item is the faction, then it should select the first unit of the active faction (first node in region after buildings and ships
            //sel.Disable(ItemTypes.FACTION);
        }
        */

        if (regionNodeViewModel != null)
        {
            // selection.Item can be null for a region-only selection: FindTreeItem then returns
            // null and the region node is selected instead (see fallback below).
            int itemId = selection.Item?.DataBlock.GetId() ?? -1;
            ExplorerNodeViewModel? item = FindTreeItem(regionNodeViewModel, itemId);

            // Container nodes (buildings/ships lists) have no id: find the matching child node.
            if (item == null && selection.HasBuildings())
            {
                item = FindImmediateChildNode(regionNodeViewModel, NodeTypes.BUILDINGS);
            }
            if (item == null && selection.HasShips())
            {
                item = FindImmediateChildNode(regionNodeViewModel, NodeTypes.SHIPS);
            }

            if (item == null)
            {
                Debug.WriteLine($"[EXPLORER] WARNING | item {regionNodeViewModel} not found.");
                ExplorerNodeViewModel? cur = SelectedItem;
                if (cur?.Parent == regionNodeViewModel)
                {
                    item = cur;
                }
                else
                {
                    item = regionNodeViewModel;
                }
            }

            if (item != null)
            {
                SelectItem(item);
                return true;
            }
        }
        else
        {
            // Happens when unseen region is selected from map view
            SelectedItem = null;
            //SetSelection(sel);
            // TODO: choose another way to force an "unseen region has been selected" event
            // event won't be received because SelectionService won't send it (same values)
            SendSelectionChangedEvent(selection);
        }
        return false;
    }

    /// <summary>
    /// Select the specified item (node) in the tree.
    /// Ensure the parent items are expanded before selecting the item.
    /// Selecting an item means to set the SelectedItem property.
    /// Note: if the specified is null or id the currently selected item (SelectedItem), nothing is done.
    /// </summary>
    /// <param name="itemToSelect"></param>
    private void SelectItem(ExplorerNodeViewModel itemToSelect)
    {
        // Expand the parents even when the item is already the selected one: the item may be hidden
        // because one of its parents has been collapsed since it was selected.
        ExpandParents(itemToSelect);

        if (itemToSelect == SelectedItem)
        {
            Debug.WriteLine($"[EXPLORER] WARNING Select Item: {itemToSelect} and selected item are the same ");
            return;
        }

        SelectedItem = itemToSelect;

        // LATER:
        // when selected item was not visible in the client area, it becomes visible but as first or last item of the client area.
        // It would be better to vertically center the position of the selected item
    }

    /// <summary>
    /// Expand all the parents of the specified item (except the root) which are not already expanded.
    /// </summary>
    private void ExpandParents(ExplorerNodeViewModel item)
    {
        // Get hierachy items from the root parent to item
        List<ExplorerNodeViewModel> parents = [];

        ExplorerNodeViewModel? parent = item.Parent;
        while (parent != null && parent != _root)
        {
            parents.Insert(0, parent);
            parent = parent?.Parent;
        }

        foreach (ExplorerNodeViewModel p in parents)
        {
            p.IsExpanded = true;
        }
    }

    private static ExplorerNodeViewModel? FindImmediateTreeItem(ExplorerNodeViewModel? node, int id/*DataBlock? block*/)
    {
        if (node == null)
        {
            return null;
        }

        foreach (ExplorerNodeViewModel child in node.Children)
        {
            if (child.Id == id)
            //if (child.Block == block)
            {
                return child;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the immediate child of the specified node having the given node type
    /// (used for container nodes like the buildings/ships list which have no id).
    /// </summary>
    private static ExplorerNodeViewModel? FindImmediateChildNode(ExplorerNodeViewModel? node, NodeTypes nodeType)
    {
        if (node == null)
        {
            return null;
        }

        foreach (ExplorerNodeViewModel child in node.Children)
        {
            if (child.NodeType == nodeType)
            {
                return child;
            }
        }
        return null;
    }


    private static ExplorerNodeViewModel? FindTreeItem(ExplorerNodeViewModel? node, int id/*DataBlock? block*/)
    {
        if (node == null)
        {
            return null;
        }

        if (id == -1)
        //if (block == null)
        {
            // If block is null, return the node itself
            return null;
        }

        if (node.Id == id)
        //if (node.Block == block)
        {
            return node;
        }

        foreach (ExplorerNodeViewModel child in node.Children)
        {
            ExplorerNodeViewModel? found = FindTreeItem(child, id);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    /// <summary>
    /// Rebuilds the tree from the data in mapFile.
    /// </summary>
    private void RebuildTree()
    {
        Debug.WriteLine("[EXPLORER] Data collection...");

        // Clear tree and build a new one from newly opened document data
        Items.Clear();

        if (Report.Planes.Count == 0)
        {
            return;
        }

        int regionsNumber = 0;
        var regionModels = Report.Planes[0].OccupiedRegions;

        foreach (var r in regionModels)
        {
            int activeUnitInsertionIndex = 0;
            ExplorerNodeViewModel? regionNode = null;
            ExplorerNodeViewModel? firstFactionNode = null;
            ExplorerNodeViewModel? shipsNode = null;
            ExplorerNodeViewModel? buildingsNode = null;
            Dictionary<int, ExplorerNodeViewModel> regionFactionsNodes = [];
            bool regionHasBuilding = false;
            bool regionHasShip = false;
            // Process each region model
            RegionModel regionModel = r.Value;
            DataBlock region = regionModel.DataBlock;
            LinkedListNode<DataBlock>? firstBlockNode = region.Node;
            // First block after region data block
            DataBlock? block = firstBlockNode!.Value.GetNextBlock();
            bool regionChanged = false;
            while (block is not null && !regionChanged)
            {
                BlockType type = block.GetBlockType();
                if (type == BlockType.REGION)
                {
                    regionFactionsNodes.Clear();
                    break;
                }

                switch (type)
                {
                    case BlockType.UNIT:
                        // Bullet image :
                        // RED for unknown or traitor
                        // BLUE for own faction
                        // GREEN for alliance
                        // GRAY for anonymous

                        // Get faction id, -1 means unknown faction or stealth/anonymous
                        int factionId = GetFactionIdForUnit(block);
                        ExplorerNodeViewModel? factionNode = null;
                        FactionInfo factionInfo = RetrieveAndStoreFactionInfo(factionId);
                        if (!regionFactionsNodes.TryGetValue(factionId, out factionNode))
                        {
                            regionNode ??= AddRegionNode(Root, regionModel, ref regionsNumber);
                            if (ActiveFactionGroup || factionInfo.Status != FactionStatus.ACTIVE)
                            {
                                // Target order in region is:
                                // - ships
                                // - buildings
                                // - factions
                                // The CR block order is not guaranteed; PromoteContainerNodes() enforces
                                // the ships/buildings part at the end of the region build.
                                int factionIndex = regionHasBuilding ? 1 : 0;
                                int indexWhenActiveGroup = firstFactionNode == null ? factionIndex : activeUnitInsertionIndex;
                                int index = factionInfo.Status == FactionStatus.ACTIVE ? indexWhenActiveGroup : -1;
                                // Add faction node, as child of region node
                                factionNode = new ExplorerNodeViewModel(regionNode, factionInfo.Name, factionInfo.Id, NodeTypes.FACTION, false, factionInfo.Status, index, raceIconUri: GetFactionRaceIconUri(factionInfo.Race));
                                regionFactionsNodes[factionId] = factionNode;
                                int factionNodeIndex = index == -1 ? regionNode.Children.Count - 1 : index;
                                if (firstFactionNode == null)
                                {
                                    activeUnitInsertionIndex = factionNodeIndex;
                                    firstFactionNode = factionNode;
                                }
                            }
                        }

                        // With ActiveFactionGroup to false, units of own faction are inserted directly as child of region node, after buildings.
                        // Add unit node, as child of faction node (ActiveFactionGroup set to true) or of a region
                        AddUnit(factionNode ?? (regionNode ??= AddRegionNode(Root, regionModel, ref regionsNumber)), block, factionNode != null ? -1 : activeUnitInsertionIndex);
                        if (factionNode == null)
                        {
                            // Keep track of the first faction node to set the activeUnitIndexPosition
                            activeUnitInsertionIndex++;
                        }
                        break;
                    case BlockType.SHIP:
                        // Append the region node if not already done
                        regionNode ??= AddRegionNode(Root, regionModel, ref regionsNumber);
                        shipsNode ??= AddShipsNode(regionNode, 0);
                        string rawShipType = $"{block.Value(KeyType.TYPE)}";
                        string shipType = Labels.Localize(Categories.Ship, rawShipType);
                        string shipSizeLabel = Labels.Localize(Categories.Node, Labels.SIZE, [$"{block.Value(KeyType.SIZE)}"]);
                        string shipLabel = $"{block.GetUILabel()}, {shipType}, {shipSizeLabel}";
                        BelongsToStatus shipStatus = Report.IsActiveFaction(block.ValueInt(KeyType.FACTION)) ? BelongsToStatus.None : BelongsToStatus.ShipInNotActiveFaction;
                        AddShip(shipsNode, shipLabel, block, shipStatus, ShipCatalog.GetExplorerIconPath(rawShipType));
                        break;
                    case BlockType.BUILDING:
                        regionHasBuilding = true;
                        regionNode ??= AddRegionNode(Root, regionModel, ref regionsNumber);
                        buildingsNode ??= AddBuildingsNode(regionNode);
                        string rawBuildingType = $"{block.Value(KeyType.TYPE)}";
                        string buildingType = Labels.Localize(Categories.Building, rawBuildingType);
                        string buildingSizeLabel = Labels.Localize(Categories.Node, Labels.SIZE, [$"{block.Value(KeyType.SIZE)}"]);
                        string buildingLabel = $"{block.GetUILabel()}, {buildingType}, {buildingSizeLabel}";
                        BelongsToStatus buildingStatus = Report.IsActiveFaction(block.ValueInt(KeyType.FACTION)) ? BelongsToStatus.None : BelongsToStatus.BuildingInNotActiveFaction;
                        AddBuilding(buildingsNode, buildingLabel, block, buildingStatus, BuildingCatalog.GetExplorerIconPath(rawBuildingType));
                        break;
#if DEBUG
                    case BlockType.RESOURCE:
                    case BlockType.PRICES:
                    case BlockType.COMMANDS:
                    case BlockType.TALENTS:
                    case BlockType.ITEMS:
                    case BlockType.SPELLS:
                    case BlockType.DURCHREISE:
                    case BlockType.MESSAGE:
                    case BlockType.UNKNOWN:
                    case BlockType.EFFECTS:
                    case BlockType.DURCHSCHIFFUNG:
                    case BlockType.COMBATSPELL:
                        break;
                    // out of the current loop, will process the next region
                    case BlockType.REGION: regionChanged = true; break;
                    default: break;
#endif
                }

                block = block.GetNextBlock();
            }

            // The CR file does not guarantee that ships/buildings are described before
            // units/factions. Promote the container nodes so the region always reads:
            // ships, buildings, then factions (and their units).
            if (regionNode is not null)
            {
                PromoteContainerNodes(regionNode);
            }
        }

        if (ExpandTreeOnDocumentLoading)
        {
            ExpandAllItems();
        }

        // Force selection of origin region (0,0) to avoid no selection at all when opening a document
        HandleSelection(new EntitySelection(Report, 0, 0, 0));

        if (regionsNumber != Report.Planes[0].OccupiedRegions.Count)
        {
            Debug.WriteLine($"[EXPLORER] WARNING | {regionsNumber} regions found instead of {Report.Planes[0].OccupiedRegions.Count} known regions ");
        }

        Debug.WriteLine($"[EXPLORER] regions: {regionsNumber} known: {Report.Planes[0].OccupiedRegions.Count}");
        Debug.WriteLine("[EXPLORER] Data collection done");
    }

    private void Clear()
    {
        Items.Clear();
        _factionsInfo.Clear();
        SelectedItem = null;
    }

    private FactionInfo RetrieveAndStoreFactionInfo(int factionId)
    {
        // TODO: dirctly use Report.Factions FactionModel objects
        FactionInfo factionInfo;
        if (!_factionsInfo.TryGetValue(factionId, out factionInfo))
        {
            // retrieve faction status
            if (factionId == (int)SpecialFaction.ANONYMOUS)
            {
                factionInfo.Status = FactionStatus.ANONYMOUS;
            }
            else if (factionId == (int)SpecialFaction.TRAITOR)
            {
                factionInfo.Status = FactionStatus.TRAITOR;
            }
            else
            {
                FactionModel? factionModel = null;
                if (Report.GetFaction(ref factionModel, factionId))
                {
                    factionInfo.Id = factionModel.Id;
                    factionInfo.Block = factionModel!.Data;
                    factionInfo.Status = factionModel.IsActive ? FactionStatus.ACTIVE : FactionStatus.UNKNOWN;
                }
                if (Report.HasActiveFaction() && factionInfo.Status != FactionStatus.ACTIVE)
                {
                    // get first active faction
                    // LATER : maybe it should be changed when several active factions are possible
                    // TODO
                    DataBlock? activeFaction = Report.ActiveFaction.Data;
                    DataBlock? block = activeFaction?.GetNextBlock();
                    while (block != null)
                    {
                        BlockType refType = block.GetBlockType();
                        if (refType != BlockType.ALLIANCE &&
                            refType != BlockType.ITEMS &&
                            refType != BlockType.OPTIONS &&
                            refType != BlockType.GROUP)
                        {
                            break;
                        }

                        if (refType == BlockType.ALLIANCE && block.GetId() == factionId)
                        {
                            // change icon to green, if alliance status to faction is set
                            factionInfo.Status = FactionStatus.ALLIED;
                            break;
                        }
                        block = block.GetNextBlock();
                    }

                }
            }
            factionInfo.Name = Report.GetFactionName(factionId);
            factionInfo.Race = Report.GetFaction(factionId)?.Race ?? FactionRace.Unknown;
            _factionsInfo[factionId] = factionInfo;
        }
        return factionInfo;
    }

    private static string? GetFactionRaceIconUri(FactionRace race) =>
        IconCatalogService.ResolvePath(IconCategories.Race, race.ToString(), "race")
        ?? "/Assets/races/race.gif";

    private static void AddShip(ExplorerNodeViewModel parentNode, string label, DataBlock block, BelongsToStatus status, string? iconUri = null)
    {
        _ = new ExplorerNodeViewModel(parentNode, label, block.GetId(), NodeTypes.SHIP, false, status, -1, iconUri);
    }

    private static void AddBuilding(ExplorerNodeViewModel parentNode, string label, DataBlock block, BelongsToStatus status, string? iconUri = null)
    {
        _ = new ExplorerNodeViewModel(parentNode, label, block.GetId(), NodeTypes.BUILDING, false, status, -1, iconUri);
    }

    private ExplorerNodeViewModel AddRegionNode(ExplorerNodeViewModel parent, RegionModel regionModel, ref int regionsNumber)
    {
        //string label = regionBlock.GetUILabel();
        //int t = regionBlock.GetTerrain();
        int t = regionModel.Terrain;
        string label = t == Terrains.OCEAN
            ? Labels.Localize(Categories.Node, Labels.OCEAN, [$"{regionModel.GetX()}", $"{regionModel.GetY()}"])
            : regionModel.Name;
        regionsNumber++;
        return new ExplorerNodeViewModel(parent, label, regionModel.Id, NodeTypes.REGION, false, t);
    }

    private ExplorerNodeViewModel AddShipsNode(ExplorerNodeViewModel parent, int index = -1)
    {
        return new ExplorerNodeViewModel(parent, Labels.Localize(Categories.Node, Labels.SHIPS), -1, NodeTypes.SHIPS, false, null, index);
    }

    private ExplorerNodeViewModel AddBuildingsNode(ExplorerNodeViewModel parent, int index = -1)
    {
        return new ExplorerNodeViewModel(parent, Labels.Localize(Categories.Node, Labels.BUILDINGS), -1, NodeTypes.BUILDINGS, false, null, index);
    }

    /// <summary>
    /// Moves the ships and buildings container nodes to the front of the region children,
    /// preserving the relative order of all other nodes. The report block order is not
    /// guaranteed, so this enforces the explorer contract: ships, buildings, then factions.
    /// </summary>
    private static void PromoteContainerNodes(ExplorerNodeViewModel regionNode)
    {
        int targetIndex = 0;
        MoveFirstChildOfType(regionNode, NodeTypes.SHIPS, ref targetIndex);
        MoveFirstChildOfType(regionNode, NodeTypes.BUILDINGS, ref targetIndex);
    }

    private static void MoveFirstChildOfType(ExplorerNodeViewModel parent, NodeTypes nodeType, ref int targetIndex)
    {
        for (int i = targetIndex; i < parent.Children.Count; i++)
        {
            if (parent.Children[i].NodeType != nodeType)
            {
                continue;
            }

            if (i != targetIndex)
            {
                parent.Children.Move(i, targetIndex);
            }
            targetIndex++;
            return;
        }
    }

    private void AddUnit(ExplorerNodeViewModel parentNode, DataBlock unitBlock, int index)
    {
        string unitlabel = $"{unitBlock.GetUILabel()}: {unitBlock.ValueInt(KeyType.NUMBER)}";
        BelongsToStatus status = BelongsToStatus.None;
        if (unitBlock.HasKey(KeyType.BUILDING))
        {
            status |= BelongsToStatus.UnitInBuilding;
        }
        else
        {
            if (unitBlock.HasKey(KeyType.SHIP))
            {
                status |= BelongsToStatus.UnitInShip;
            }
        }
        _ = new ExplorerNodeViewModel(parentNode, unitlabel, unitBlock.GetId(), NodeTypes.UNIT, !Report.IsConfirmed(unitBlock), status, index);
    }

    private void ExpandAllItems()
    {
        foreach (ExplorerNodeViewModel p in Root.Children)
        {
            p.IsExpanded = true;
        }
    }

    private void CollapseAllItems()
    {
        Root.IsExpanded = false;
    }
}
