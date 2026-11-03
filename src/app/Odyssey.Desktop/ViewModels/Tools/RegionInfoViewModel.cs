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
        RegionModel? region = anotherRegionSelected ? selection.Region : null;
        UnitModel? unit = anotherUnitSelected ? selection.Item as UnitModel : null;

        if (region != null)
        {
            int terrain = region.Terrain;
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

            foreach (RenderedMessage message in Report.GetRegionMessages(region))
            {
                AddMessage(RegionMessages, message);
            }

            foreach (string effect in Report.GetRegionEffects(region))
            {
                AppendItem(EffectsMessages, effect, null);
            }

            foreach ((int directionValue, int percent) in Report.GetRegionRoads(region))
            {
                string labelKey = percent == 100 ? Labels.ROAD_INFO : Labels.ROAD_IN_PROGRESS_INFO;
                string direction = Labels.GetDirectionAbbreviation(directionValue);
                string label = Labels.Localize(Categories.None, labelKey, [direction, $"{percent}"]);
                AppendItem(RoadsMessages!, label, null);
            }

            foreach ((string label, List<EntityModel> targets) in Report.GetRegionTransits(region))
            {
                _ = AppendItem(TravelMessages, label, new MessageEntry(label, targets));
            }

            SortedSet<int> guardsIds = Report.GetRegionGuardingFactionIds(region);
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
            string name = unit.DisplayLabel;
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
            foreach (var message in Report.GetActiveFactionMessages(unit, region))
            {
                if (unit is not null && message.MatchesUnit)
                {
                    AddMessage(UnitMessages!, message.Message);
                }
                if (region is not null && message.MatchesRegion)
                {
                    AddMessage(RegionMessages, message.Message);
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
