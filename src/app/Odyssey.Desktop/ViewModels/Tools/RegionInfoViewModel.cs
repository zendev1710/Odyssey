using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using System.Collections.Generic;
using System.Diagnostics;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// View model sharing region messages to be issued by the region information view.
/// Shared messages are the one of the selected region, for the active report document.
/// Messages are stored into a tree.
/// Messages are grouped by category. Categories are :
/// - general messages (those going into no other category)
/// - effects messages
/// - travel messages
/// - built roads information (roads are called "streets" in Eressea)
/// - guarding messages
/// - unit messages (for the currently selected unit if any)
/// Each category is a root item of the tree.
/// Unit messages category exists only if a unit is selected in the explorer view.
/// Roads and guards messages category do not exist if an ocean is selected in the explorer view.
/// Each message is a leaf item of its parent category node.
/// Battle-related messages are not handled (battle-related messages are handled in the battles view model).
/// </summary>
public partial class RegionInfoViewModel : MessagesViewModel
{
    /// <summary>
    /// Parent node containing othert uncategorized messages in the region.
    /// </summary>
    private readonly Node _regionMessages;
    /// <summary>
    /// parent node containing the messages related to the effects in the region.
    /// </summary>
    private readonly Node _effectsMessages;
    /// <summary>
    /// parent node containing the messages related to the movements in the region.
    /// </summary>
    private readonly Node _travelMessages;
    /// <summary>
    /// parent node containing the messages related to the roads in the region.
    /// </summary>
    private Node? _roadsMessages;
    /// <summary>
    /// parent node containing the messages related to the guarding in the region.
    /// </summary>
    private Node? _guardsMessages;
    /// <summary>
    /// Parent node containing the messages related to the units in the region.
    /// LATER: is this information really needed ?
    /// </summary>
    private Node? _unitMessages;

    private Node RegionMessages
    {
        get { return _regionMessages; }
    }
    private Node EffectsMessages
    {
        get { return _effectsMessages; }
    }
    private Node TravelMessages
    {
        get { return _travelMessages; }
    }
    private Node? RoadsMessages { get { return _roadsMessages; } }
    private Node? GuardsMessages { get { return _guardsMessages; } }
    private Node? UnitMessages { get { return _unitMessages; } }

    public RegionInfoViewModel() : base()
    {
        // TODO: check if an unseen region can have some messages
        // anyway, handle unseen regions with specific info here, isn't it ?
        _regionMessages = AppendItem(Root, Labels.Localize(Categories.Node, Labels.MESSAGES), null);
        _effectsMessages = AppendItem(Root, Labels.Localize(Categories.Node, Labels.EFFECTS), null);
        _travelMessages = AppendItem(Root, Labels.Localize(Categories.Node, Labels.TRANSIT), null);
        _roadsMessages = null;
        _guardsMessages = null;
        _unitMessages = null;
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this))
            return;
        CollectDataFromRegion(selection, selType);
    }

    private void CollectDataFromRegion(IEntitySelection selection, SelectionChangeTypes selType)
    {
        bool unitChanged = selType.HasFlag(SelectionChangeTypes.Unit);
        bool regionChanged = selType.HasFlag(SelectionChangeTypes.Region);
        bool regionSelected = selection.IsRegionSelected();
        bool unitSelected = selection.HasUnit();

        Debug.WriteLine($"[VM-REGION-INF] OnSelectionChanged: region changed={regionChanged}, region selected={regionSelected} unit changed={unitChanged}, unit selected={unitSelected}");

        bool anotherRegionSelected = regionChanged;
        bool anotherUnitSelected = unitChanged;
        if (anotherRegionSelected)
        {
            RegionMessages.Children.Clear();
            EffectsMessages.Children.Clear();
            TravelMessages.Children.Clear();
            RoadsMessages?.Children.Clear();
            GuardsMessages?.Children.Clear();
            UnitMessages?.Children.Clear();
        }

        if (!anotherUnitSelected)
        {
            RemoveItemIfExists(ref _unitMessages);
        }

        if (!Report.HasActiveFaction())
        {
            return;
        }

        // Note: a container selection (the buildings/ships list of a region) is a region selection
        // without any item: the region to consider is the selected region itself, not the item.
        // (selection.Item is only the region for a region-only selection.)
        DataBlock? region = anotherRegionSelected ? selection.Region?.DataBlock : null;
        DataBlock? unit = anotherUnitSelected ? selection.Item?.DataBlock : null;

        if (region != null)
        {
            int terrain = region.GetTerrain();
            bool needRoadsNode = Terrains.CanHaveRoad(terrain);
            bool needGuardsNode = Terrains.CanBeGuarded(terrain);

            if (!needRoadsNode)
            {
                RemoveItemIfExists(ref _roadsMessages);
            }
            else if (_roadsMessages == null)
            {
                _roadsMessages = AppendItem(Root, Labels.Localize(Categories.Node, Labels.ROADS), null);
            }

            if (!needGuardsNode)
            {
                RemoveItemIfExists(ref _guardsMessages);
            }
            else if (_guardsMessages == null)
            {
                _guardsMessages = AppendItem(Root, Labels.Localize(Categories.Node, Labels.GUARDS), null);
            }

            SortedSet<int> guardsIds = [];
            DataBlock? startNode = region.GetNextBlock();
            int regionDepth = region.GetDepth();
            for (DataBlock? block = startNode; block != null && block.GetDepth() > regionDepth; block = block.GetNextBlock())
            {
                BlockType btype = block.GetBlockType();
                if (btype == BlockType.MESSAGE)
                {
                    AddMessage(RegionMessages, block);
                }
                else if (block.GetDepth() > regionDepth + 1)
                {
                    continue;
                }
                else if (btype == BlockType.UNIT)
                {
                    int guard = block.ValueInt(Strings.DE_UNIT_IS_GUARDING);
                    if (guard != 0)
                    {
                        int faction = block.ValueInt(Strings.DE_UNIT_FACTION_ID);
                        guardsIds.Add(faction);
                    }
                }
                else if (btype == BlockType.EFFECTS)
                {
                    foreach (DataKey msg in block.GetData())
                    {
                        AppendItem(EffectsMessages, msg.GetValue(), null);
                    }
                }
                else if (btype == BlockType.BORDER)
                {
                    int directionValue = block.ValueInt(Strings.DE_REGION_ROAD_DIRECTION, 6);
                    int percent = block.ValueInt(Strings.DE_ROAD_PERCENT, 0);
                    // LATER: check if block.Value("typ") can be something else than "Straße"
                    string labelKey = percent == 100 ? Labels.ROAD_INFO : Labels.ROAD_IN_PROGRESS_INFO;
                    string direction = Labels.GetDirectionAbbreviation(directionValue);
                    string label = Labels.Localize(Categories.None, labelKey, [direction, $"{percent}"]);
                    AppendItem(RoadsMessages!, label, null);
                }
                else if (btype == BlockType.DURCHREISE || btype == BlockType.DURCHSCHIFFUNG)
                {
                    // TODO: find "from region" and "to region" information
                    // transit (DURCHREISE)
                    // "Wagon driver to Xorlosch (g5te)"
                    // for ships : The...
                    // TODO: localize it
                    string prefix = btype == BlockType.DURCHSCHIFFUNG ? "The " : "";
                    //string prefix = btype == BlockType.DURCHSCHIFFUNG ? "Die " : "";
                    // retrieve the targets
                    foreach (DataKey msg in block.GetData())
                    {
                        List<EntityModel> targets = [];
                        ShipModel? movingShip = null;
                        UnitModel? movingUnit = null;
                        // get string inside (...) and convert to int
                        string label = msg.GetValue();
                        int id = DataBlock.ExtractId(label);
                        if (id != -1)
                        {
                            if (block.GetBlockType() == BlockType.DURCHSCHIFFUNG)
                            {
                                if (!Report.FindShip(id, out movingShip))
                                {
                                    Debug.WriteLine($"[REGION-INF] WARNING | Ship with {id} id not found");
                                }
                            }
                            else if (!Report.FindUnit(id, out movingUnit))
                            {
                                Debug.WriteLine($"[REGION-INF] WARNING | Ship with {id} id not found");
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"[REGION-INF] WARNING | Failed to extract id from : {label}");
                        }
                        List<EntityModel?> entityModels = [movingUnit, movingShip];
                        foreach (var item in entityModels)
                        {
                            if (item is not null)
                                targets.Add(item);
                        }
                        _ = AppendItem(TravelMessages, label, new MessageEntry(label, targets));
                    }

                }
            }
            if (guardsIds.Count > 0)
            {
                foreach (int id in guardsIds)
                {
                    FactionModel? factionModel = null;
                    if (Report.GetFaction(ref factionModel, id))
                    {
                        _ = AppendItem(GuardsMessages!, factionModel!.Name, null);
                    }
                }
            }
        }

        if (unit != null)
        {
            string name = unit.GetUILabel();
            if (_unitMessages == null)
            {
                // Insert Unit item at last position
                _unitMessages = InsertItem(Root, name, null, -1);

            }
            else
            {
                _ = UpdateItem(ref _unitMessages, name, null);
            }
        }

        if (unit != null || region != null)
        {
            // Search for first MESSAGE block beginning from ActiveFaction block
            // TODO: check if it's what that should be done
            DataBlock? firstBlock = Report.ActiveFaction.Data.GetNextBlock();
            DataBlock? firstMessageBlock = null;
            for (var block = firstBlock; block != null; block = block.GetNextBlock())
            {
                if (block.GetBlockType() == BlockType.MESSAGE)
                {
                    firstMessageBlock = block;
                    break;
                }
            }

            // Note: the below unit messages are duplicated in region messages node. Is it what we want ?
            // LATER: maybe we should avoid duplicating them
            for (var msg = firstMessageBlock; msg != null; msg = msg.GetNextBlock())
            {
                if (unit != null && msg.HasReference(unit))
                {
                    AddMessage(UnitMessages!, msg);
                }
                if (region != null && msg.HasReference(region))
                {
                    AddMessage(RegionMessages, msg);
                }
            }
        }
    }

    private Node UpdateItem(ref Node? oldItem, string label, MessageEntry? entry)
    {
        Node? parent = (oldItem == null ? Root : oldItem.Parent) ?? Root;
        int index = parent == null || oldItem == null ? -1 : parent.Children.IndexOf(oldItem);
        Node newNode = new Node(parent, label, entry, index);
        RemoveItemIfExists(ref oldItem);
        oldItem = newNode;
        return newNode;
    }

    private static void RemoveItemIfExists(ref Node? item)
    {
        item?.Parent?.Children.Remove(item);
        item = null;
    }
}
