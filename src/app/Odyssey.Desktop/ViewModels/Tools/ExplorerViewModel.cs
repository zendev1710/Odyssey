using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using Odyssey.Settings;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Tools.DataProperty;

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
    ///     Unit 1 of active faction (only if ActiveFactionGroup is false)
    ///     Unit 2 of active faction (only if ActiveFactionGroup is false)
    ///     Active Faction (only if ActiveFactionGroup is true)
    ///       Unit 1
    ///       UniT 2
    ///     Faction 2
    ///       Unit 2.1
    ///       Unit 2.2
    ///     Faction 3
    ///       Unit 3.1
    ///       Unit 3.2
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
    }

    public ExplorerViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    public ExplorerViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
        _root = new ExplorerNodeViewModel();

        _activeFactionGroup = !GlobalSettings.Get<bool>(GlobalSettings.EXPLORER_ACTIVE_FACTION_UNITS_AT_FIRST);
        _expandTreeOnDocumentLoading = GlobalSettings.Get<bool>(GlobalSettings.EXPLORER_EXPAND_TREE_ON_REPORT_OPENING);
        
        Items = _root.Children;

        NodeMenuItems = new[]
            {
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
            };
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
            ISelection sel = new ExplorerNodeSelection(newValue);
            SetSelection(sel);
            // Notify the other views about the selection change
            PublishSelectionChangedEvent(new SelectionChange(sel, this, InnerSelector));
        }
    }

    /// <summary>
    /// Handle selection change event sent by another view (selector).
    /// Will select the corresponding item in the tree if any.
    /// </summary>
    /// <param name="selectionChange"></param>
    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Handle selection changed event from all selectors except itself.
        // Exclude this selector from selection change events to avoid reentrancy issues
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        if (!HandleSelection(selectionChange))
        {
            Debug.WriteLine("[EXPLORER] selected item did not change");
        }
    }

    /// <summary>
    /// Handle active document changed event.
    /// Rebuilds the tree from the new active document data.
    /// </summary>
    /// <param name="cr"></param>
    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            RebuildTree();
        }
    }

    protected override void OnActiveDocumentClosed(CRDocument cr)
    {
        Clear();
        // reset to an empty report document
        SetMapFile(new CRDocument());
    }

    private bool HandleSelection(ISelectionChange selectionChange)
    {
        ISelection sel = selectionChange.Selection;
        if (IsSelected(sel))
        {
            // Selection is already set, so no need to update it
            return false;
        }

        if (sel is ExplorerNodeSelection explorerNodeSel)
        {
            // Store the inner selector (the one who raised the sel change event)
            // to avoid it to react when auto-publishing (in) this event foir the other views
            InnerSelector = selectionChange.Selector;
            SelectedItem = explorerNodeSel.NodeViewModel;
            return true;
        }

        ExplorerNodeViewModel? regionNodeViewModel = null;
        DataBlock? selectedRegion = sel.Region;
        if (!sel.IsRegionSelected())
        {
            if (sel.Item == null || !GetKnownParent(ref selectedRegion, sel.Item))
            //if (sel.Item == null || !CRDocument.GetParent(ref selectedRegion, sel.Item))
            {
                Debug.WriteLine("[EXPLORER] WARNING | Region parent not found for {sel.Item}.");
                return false;
            }
        }

        // Select a not-region item, region being known
        if (selectedRegion == Selection?.Region)
        {
            // Region is not changed, so no need to search for region node
            if (Selection is ExplorerChildNodeSelection explorerChildNodeSel)
            {
                regionNodeViewModel = explorerChildNodeSel.RegionNodeViewModel;
            }
            else
            {
                List<ExplorerNodeViewModel> nodeViewModelSubtree = ExplorerNodeViewModel.GetNodeSubTree(SelectedItem!, false, false, false).ToList();
                regionNodeViewModel = nodeViewModelSubtree.FirstOrDefault();
            }
        }
        else
        {
            regionNodeViewModel = FindImmediateTreeItem(Root, selectedRegion);
        }

        // To avoid to search a faction node that does not exist, remove FACTION from selection if it's the active faction
        CRDocument cr = GetDocument();
        if (!ActiveFactionGroup && cr.HasActiveFaction()  && sel.IsFactionSelected() && sel.Faction == cr.GetActiveFaction())
        {
            Debug.WriteLine("[EXPLORER] WARNING | TODO :remove FACTION mask from selection");
            // TODO: remove FACTION from selection
            // TODO: if item is the faction, then it should select the first unit of the active faction (first node in region after buildings and ships
            //sel.Disable(ItemTypes.FACTION);
        }

        if (regionNodeViewModel != null)
        {
            ExplorerNodeViewModel? item = FindTreeItem(regionNodeViewModel, sel.Item);
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
            Debug.WriteLine("[EXPLORER] WARNING | Should kill selection.");
            //KillSelection();
        }
        return false;
    }

    /// <summary>
    /// Select the specified item (node) in the tree.
    /// Ensure the parent items are expanded before selecting the item.
    /// Selectng an item means to set the SelectedItem property.
    /// Note: if the specified is null or id the currently selected item (SeletedItem), nothing is done.
    /// </summary>
    /// <param name="itemToSelect"></param>
    private void SelectItem(ExplorerNodeViewModel itemToSelect)
    {
        if (itemToSelect == SelectedItem)
        {
            Debug.WriteLine($"[EXPLORER] WARNING Select Item: {itemToSelect} and selected item are the same ");
            return;
        }

        // Get hierachy items from the root parent to itemToSelect
        List<ExplorerNodeViewModel> parents = [];

        // LATER: check if parent.IsChecked = true is sufficient
        ExplorerNodeViewModel? parent = itemToSelect.Parent;
        while (parent != null && parent != _root)
        {
            parents.Insert(0, parent);
            parent = parent?.Parent;
        }

        // Expand all parents of the item if they are not already expanded
        foreach (ExplorerNodeViewModel p in parents)
        {
            p.IsExpanded = true;
        }

        SelectedItem = itemToSelect;

        // LATER:
        // when selected item was not visible in the client area, it becomes visible but as first or last item of the client area.
        // It would be better to vertically center the position of the selected item
    }

    private static ExplorerNodeViewModel? FindImmediateTreeItem(ExplorerNodeViewModel? node, DataBlock? block)
    {
        if (node == null)
        {
            return null;
        }

        foreach (ExplorerNodeViewModel child in node.Children)
        {
            if (child.Block == block)
            {
                return child;
            }
        }
        return null;
    }


    private static ExplorerNodeViewModel? FindTreeItem(ExplorerNodeViewModel? node, DataBlock? block)
    {
        if (node == null) {
            return null;
        }

        if (block == null)
        {
            // If block is null, return the node itself
            return null;
        }

        if (node.Block == block)
        {
            return node;
        }

        foreach (ExplorerNodeViewModel child in node.Children)
        {
            ExplorerNodeViewModel? found = FindTreeItem(child, block);
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

        CRDocument cr = GetDocument();

        ExplorerNodeViewModel? regionNode = null;
        int activeUnitInsertionIndex = 0;
        ExplorerNodeViewModel? firstFactionNode = null;
        ExplorerNodeViewModel? shipsNode = null;
        ExplorerNodeViewModel? buildingsNode = null;
        DataBlock? region = null;
        Dictionary<int, ExplorerNodeViewModel> regionFactionsNodes = [];
        bool regionHasBuilding = false;
        bool regionHasShip = false;

        LinkedListNode<DataBlock>? firstBlockNode = cr.FirstBlock;

        // OPTIMIZE: pre-filter regions in order to not browse regions excluded from explorer (regions with no people or neighbours, including a lot of oceans)
        int excludedRegionsNumber = 0;
        int regionsNumber = 0;
        DataBlock? previousBlock = null;

        for (var block = firstBlockNode?.Value; block != null; block = block.GetNextBlock())
        //for (var node = firstBlockNode; node != null; node = node.Next)
        {
            //DataBlock block = node.Value;
            BlockType type = block.GetBlockType();
            if (type == BlockType.REGION)
            {
                if (regionNode == null && region != null)
                {
                    //excludedRegionNumber++;
                }

                regionHasBuilding = false;
                regionHasShip = false;
                regionFactionsNodes.Clear();
                regionNode = null;
                activeUnitInsertionIndex = 0;
                firstFactionNode = null;
                shipsNode = null;
                buildingsNode = null;
                region = null;
                RegionAttachment? regionInfo = block.GetAttachment() as RegionAttachment;
                if (regionInfo == null || regionInfo.People.Count == 0)
                {
                    // A region with no people is not added to the regions tree explorer
                    excludedRegionsNumber++;
                    continue;
                }
                // Set as current region
                region = block;
            }
            else if (region != null)
            {
                switch (type)
                {
                    case BlockType.UNIT:
                        // Bullet image :
                        // RED for unknown or traitor
                        // BLUE for own faction
                        // GREEN for alliance
                        // GRAY for anonymous

                        // Get faction id, -1 means unknown faction (or stealth/anonymous)
                        int factionId = GetFactionIdForUnit(block);
                        ExplorerNodeViewModel? factionNode = null;
                        FactionInfo factionInfo = RetrieveAndStoreFactionInfo(factionId);
                        if (!regionFactionsNodes.TryGetValue(factionId, out factionNode))
                        {
                            regionNode ??= AddRegionNode(Root, region, ref regionsNumber);
                            if (ActiveFactionGroup || factionInfo.Status != FactionStatus.ACTIVE)
                            {
                                // Order in region is :
                                // - ships
                                // - buildings
                                // - factions
                                // handling order depends on the elements description order for the region in the CR file
                                // units/factions on a region are sometimes handled before buildings (to be confirmed)
                                // ships seem to be always handled after buildings and before units/factions (to be confirmed)
                                int factionIndex = regionHasBuilding ? 1 : 0;
                                int indexWhenActiveGroup = firstFactionNode == null ? factionIndex : activeUnitInsertionIndex;
                                int index = factionInfo.Status == FactionStatus.ACTIVE ? indexWhenActiveGroup : -1;
                                // Add faction node, as child of region node
                                factionNode = new ExplorerNodeViewModel(regionNode, factionInfo.Name, factionInfo.Block, false, factionInfo.Status, index);
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
                        AddUnit(factionNode ?? (regionNode ??= AddRegionNode(Root, region, ref regionsNumber)), block, factionNode != null ? -1 : activeUnitInsertionIndex);
                        if (factionNode == null)
                        {
                            // Keep track of the first faction node to set the activeUnitIndexPosition
                            activeUnitInsertionIndex++;
                        }
                        break;
                    case BlockType.SHIP:
                        // Append the region node if not already done
                        regionNode ??= AddRegionNode(Root, region, ref regionsNumber);
                        shipsNode ??= AddShipsNode(regionNode, 0);
                        string shipType = Labels.Localize(Categories.Ship, $"{block.Value(KeyType.TYPE)}");
                        string shipSizeLabel = Labels.Localize(Categories.Node, Labels.SIZE, [$"{block.Value(KeyType.SIZE)}"]);
                        string shipLabel = $"{block.GetUILabel()}, {shipType}, {shipSizeLabel}";
                        BelongsToStatus shipStatus = block.ValueInt(KeyType.FACTION) != Report.GetActiveFactionId() ? BelongsToStatus.ShipInNotActiveFaction : BelongsToStatus.None;
                        AddShip(shipsNode, shipLabel, block, shipStatus);
                        break;
                    case BlockType.BUILDING:
                        regionHasBuilding = true;
                        regionNode ??= AddRegionNode(Root, region, ref regionsNumber);
                        buildingsNode ??= AddBuildingsNode(regionNode);
                        string buildingType = Labels.Localize(Categories.Building, $"{block.Value(KeyType.TYPE)}");
                        string buildingSizeLabel = Labels.Localize(Categories.Node, Labels.SIZE, [$"{block.Value(KeyType.SIZE)}"]);
                        string buildingLabel = $"{block.GetUILabel()}, {buildingType}, {buildingSizeLabel}";
                        BelongsToStatus buildingStatus = block.ValueInt(KeyType.FACTION) != Report.GetActiveFactionId() ? BelongsToStatus.BuildingInNotActiveFaction : BelongsToStatus.None;
                        AddBuilding(buildingsNode, buildingLabel, block, buildingStatus);
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
                    default: break;
#endif
                }
            }
            previousBlock = block;
        }

        if (ExpandTreeOnDocumentLoading)
        {
            ExpandAllItems();
        }

        if (excludedRegionsNumber != 0)
        {
            Debug.WriteLine($"[EXPLORER] WARNING | {excludedRegionsNumber} unknown regions found but iteration should be only on known regions ");
        }

        if (regionsNumber != cr.KnownRegionsNumber)
        {
            Debug.WriteLine($"[EXPLORER] WARNING | {regionsNumber} regions found instead of {cr.KnownRegionsNumber} known regions ");
        }

        Debug.WriteLine($"[EXPLORER] regions: {regionsNumber} excluded: {excludedRegionsNumber}) known: {cr.KnownRegionsNumber}");
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
                DataBlock? factionBlock = null;
                if (Report.GetFaction(ref factionBlock, factionId))
                {
                    factionInfo.Block = factionBlock;
                }
                if (factionId == Report.GetActiveFactionId())
                {
                    factionInfo.Status = FactionStatus.ACTIVE;
                }
                else if (Report.HasActiveFaction())
                {
                    DataBlock? activeFaction = Report.GetActiveFaction();

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
            _factionsInfo[factionId] = factionInfo;
        }
        return factionInfo;
    }

    private static void AddShip(ExplorerNodeViewModel parentNode, string label, DataBlock block, BelongsToStatus status)
    {
        _ = new ExplorerNodeViewModel(parentNode, label, block, false, status);
    }

    private static void AddBuilding(ExplorerNodeViewModel parentNode, string label, DataBlock block, BelongsToStatus status)
    {
        _ = new ExplorerNodeViewModel(parentNode, label, block, false, status);
    }

    /// <summary>
    /// Adds a region node to the tree, as child of the specified parent.
    /// </summary>
    /// <param name="regionBlock">Data block linked to the node.</param>
    /// <returns></returns>
    private ExplorerNodeViewModel AddRegionNode(ExplorerNodeViewModel parent, DataBlock regionBlock, ref int regionsNumber)
    {
        string label = regionBlock.GetUILabel();
        int t = regionBlock.GetTerrain();
        regionsNumber++;
        return new ExplorerNodeViewModel(parent, label, regionBlock, false, t);
    }

    private ExplorerNodeViewModel AddShipsNode(ExplorerNodeViewModel parent, int index = -1)
    {
        return new ExplorerNodeViewModel(parent, Labels.Localize(Categories.Node, Labels.SHIPS), null, false, null, index);
    }

    private ExplorerNodeViewModel AddBuildingsNode(ExplorerNodeViewModel parent, int index = -1)
    {
        return new ExplorerNodeViewModel(parent, Labels.Localize(Categories.Node, Labels.BUILDINGS), null, false, null, index);
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
        _ = new ExplorerNodeViewModel(parentNode, unitlabel, unitBlock, !Report.IsConfirmed(unitBlock), status, index);
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
