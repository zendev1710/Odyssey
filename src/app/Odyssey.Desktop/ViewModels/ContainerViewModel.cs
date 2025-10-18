using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using Prism.Events;
using System;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.ViewModels;

/// <summary>
/// ViewModel for container (ship or building having one or more units).
/// </summary>
public partial class ContainerViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _ownerName;

    [ObservableProperty]
    private DataBlock? _ownerUnit;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _type;

    [ObservableProperty]
    private int _size;

    protected ContainerModel? ContainerModel { get; set; }

    protected DataBlock? Block { get; set; }

    protected DataBlock? Region { get; set; }

    public ContainerViewModel() : this(string.Empty, null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }
    public ContainerViewModel(string id, IEventAggregator? eventAggregator) : base(id, eventAggregator)
    {
        _ownerName = string.Empty;
        _description = string.Empty;
        _name = string.Empty;
        _type = string.Empty;
        _size = 0;
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
            newSelection = new SimpleItemSelection(block, Region, null);
        }

        SendSelectionChangedEvent(newSelection);
    }

    protected void AddOwner(NodeViewModel parent, bool buildingContainer)
    {
        if (!string.IsNullOrEmpty(OwnerName))
        {
            string ownerLabel = Labels.Localize(Categories.Node, buildingContainer ? Labels.OWNER : Labels.CAPTAIN);
            string ownerFullNameLabel = $"{ownerLabel}: {OwnerName}";
            DataProperty ownerProperty = new(Categories.None, ownerFullNameLabel, string.Empty, ownerFullNameLabel, "", OwnerUnit);
            AddItem(parent, ownerProperty);
        }

    }
    protected bool AddUnits(NodeViewModel parent)
    {
        if (ContainerModel == null || ContainerModel.Units.Count == 0)
        {
            return false;
        }
        string label = Labels.Localize(Categories.Node, Labels.UNITS);
        DataProperty unitsProperty = new(Categories.Node, label, string.Empty, label);
        var unitsNode = AddItem(parent, unitsProperty);
        foreach (var unit in ContainerModel.Units)
        {
            string id = unit.IdToString();
            string name = unit.Value(KeyType.NAME);
            string number = unit.Value(KeyType.NUMBER);
            string fullName = $"{name} ({id})";
            string unitlabel = $"{fullName}: {number}";
            DataProperty unitProperty = new(Categories.None, unitlabel, string.Empty, unitlabel, string.Empty, unit);
            _ = AddItem(unitsNode, unitProperty);
        }
        return true;
    }
    protected bool AddEffects(NodeViewModel parent)
    {
        if (ContainerModel?.Effects.Count > 0)
        {
            string effectsLabel = Labels.Localize(Categories.Node, Labels.EFFECTS);
            DataProperty effectsProperty = new(Categories.Node, Labels.EFFECTS, string.Empty, effectsLabel);
            NodeViewModel effectsNode = AddItem(parent, effectsProperty);
            foreach (string effectLabel in ContainerModel.Effects)
            {
                // Note : effectLabel is already localized in english
                // Example : The winds seem to favor this ship. (ic7s)
                DataProperty effectProperty = new(effectLabel);
                AddItem(effectsNode, effectProperty);
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// Append item under parent if index is -1; otherwise insert item at the specified index.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="label"></param>
    /// <param name="block"></param>
    /// <returns></returns>
    protected static TreeNodeViewModel AddItem(NodeViewModel parent, DataProperty? property, string header = "", int index = -1)
    {
        return new(parent, property, header, index);
    }
}

