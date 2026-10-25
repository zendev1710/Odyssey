using CommunityToolkit.Mvvm.ComponentModel;
using Markdig.Syntax;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.ViewModels;

/// <summary>
/// ViewModel for a structure (ship or building having one or more units).
/// </summary>
public partial class StructureViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _ownerName;

    [ObservableProperty]
    private UnitModel? _ownerUnit;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _type;

    [ObservableProperty]
    private int _size;

    protected StructureModel? StructureModel { get; set; }

    //protected DataBlock? Block { get; set; }

    protected RegionModel? Region { get; set; }
    //protected RegionDataBlock? Region { get; set; }

    public StructureViewModel(string id) : base(id)
    {
        _ownerName = string.Empty;
        _description = string.Empty;
        _name = string.Empty;
        _type = string.Empty;
        _size = 0;
    }
    /// <summary>
    /// Dispatch the select node Event.
    /// Normally called by a double-click or a space key pressed on the currently selected item of this tree messages list in the corresponding view.
    /// Event is published only if the selected item is linked to a DataBlock (selection state has changed).
    /// </summary>
    /// <param name="node"></param>
    public void DispatchSelectedNode(NodeViewModel? node)
    {
        EntityModel? entityModel = node?.Data?.Reference;
        //DataBlock? block = node?.Data?.Reference;
        if (entityModel is not null /*&& CurrentSelection.Item!.DataBlock != block*/)
        //if (block is not null && CurrentSelection.Item!.DataBlock != block)
        {
            // TODO: set selection to a ExplorerChildNodeSelection objet systematically
            EntitySelection? newSelection;
            if (CurrentSelection is ExplorerChildNodeSelection childNodeSel)
            {
                newSelection = new ExplorerChildNodeSelection(childNodeSel.RegionNodeViewModel!, entityModel, Report);
            }
            else
            {
                newSelection = new EntitySelection(entityModel, Region, null);
            }

            SendSelectionChangedEvent(newSelection);
        }
    }

    protected void AddOwner(NodeViewModel parent, bool buildingStructure)
    {
        if (!string.IsNullOrEmpty(OwnerName))
        {
            string ownerLabel = Labels.Localize(Categories.Node, buildingStructure ? Labels.OWNER : Labels.CAPTAIN);
            string ownerFullNameLabel = $"{ownerLabel}: {OwnerName}";
            DataProperty ownerProperty = new(Categories.None, ownerFullNameLabel, string.Empty, ownerFullNameLabel, "", OwnerUnit);
            AddItem(parent, ownerProperty);
        }

    }
    protected bool AddUnits(NodeViewModel parent)
    {
        if (StructureModel == null || StructureModel.Units.Count == 0)
        {
            return false;
        }
        string label = Labels.Localize(Categories.Node, Labels.UNITS);
        DataProperty unitsProperty = new(Categories.Node, label, string.Empty, label, null, StructureModel);
        var unitsNode = AddItem(parent, unitsProperty);
        foreach (var unit in StructureModel.Units)
        {
            string id = unit.DataBlock.IdToString();
            string name = unit.DataBlock.Value(KeyType.NAME);
            string number = unit.DataBlock.Value(KeyType.NUMBER);
            string fullName = $"{name} ({id})";
            string unitlabel = $"{fullName}: {number}";
            DataProperty unitProperty = new(Categories.None, unitlabel, string.Empty, unitlabel, string.Empty, unit);
            _ = AddItem(unitsNode, unitProperty);
        }
        return true;
    }
    protected bool AddEffects(NodeViewModel parent)
    {
        if (StructureModel?.Effects.Count > 0)
        {
            string effectsLabel = Labels.Localize(Categories.Node, Labels.EFFECTS);
            DataProperty effectsProperty = new(Categories.Node, Labels.EFFECTS, string.Empty, effectsLabel);
            NodeViewModel effectsNode = AddItem(parent, effectsProperty);
            foreach (string effectLabel in StructureModel.Effects)
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

