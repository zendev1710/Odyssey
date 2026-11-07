using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Controls.Buildings;
using Odyssey.Controls.Ships;
using Markdig.Syntax;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.ViewModels;

public enum StructureKind
{
    Building,
    Ship,
}

/// <summary>
/// ViewModel for a structure (ship or building having one or more units).
/// </summary>
public partial class StructureViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOwner))]
    [NotifyPropertyChangedFor(nameof(HasUnlinkedOwner))]
    private string _ownerName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOwnerUnit))]
    [NotifyPropertyChangedFor(nameof(HasUnlinkedOwner))]
    [NotifyPropertyChangedFor(nameof(OwnerTooltip))]
    private UnitModel? _ownerUnit;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StructureTypeIconPath))]
    [NotifyPropertyChangedFor(nameof(StructureTypeTooltip))]
    private string _type;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSize))]
    private int _size;

    [ObservableProperty]
    private string _structureId = string.Empty;

    public StructureKind StructureType { get; }
    public ObservableCollection<StructureUnitLinkViewModel> Occupants { get; } = [];

    public bool HasOwner => !string.IsNullOrWhiteSpace(OwnerName);
    public bool HasOwnerUnit => OwnerUnit is not null;
    public bool HasUnlinkedOwner => HasOwner && OwnerUnit is null;
    public bool HasSize => Size >= 0;
    public string OwnerLabel => Labels.Localize(Categories.Node, StructureType == StructureKind.Ship ? Labels.CAPTAIN : Labels.OWNER);
    public string SizeLabel => Labels.Localize(Categories.Node, Labels.SIZE, [$"{Size}"]);
    public string OccupantsLabel => Labels.Localize(Categories.Node, StructureType == StructureKind.Ship ? Labels.CREW : Labels.UNITS);
    public string StructureTypeTooltip => Labels.Localize(StructureType == StructureKind.Ship ? Categories.Ship : Categories.Building, Type);
    public string StructureTypeIconPath => string.IsNullOrWhiteSpace(Type)
        ? string.Empty
        : StructureType == StructureKind.Ship
            ? ShipCatalog.GetExplorerIconPath(Type)
            : BuildingCatalog.Resolve(Type).IconPath;
    public string OwnerTooltip
    {
        get
        {
            return OwnerUnit is UnitModel ownerUnit ? GetUnitTooltip(ownerUnit) : string.Empty;
        }
    }

    protected string GetUnitTooltip(UnitModel unit)
    {
        string unitName = string.IsNullOrWhiteSpace(unit.Name) ? unit.DisplayName : unit.Name;
        string unitFullName = $"{unitName} ({unit.EncodedId})";
        string factionName = Report.GetFactionName(unit.EffectiveFactionId);
        string peopleLabel = Labels.LocalizeCountable(Categories.Node, Labels.PERSONS, unit.PeopleNumber == 1);
        return $"{unitFullName}{Environment.NewLine}{factionName}{Environment.NewLine}{unit.PeopleNumber} {peopleLabel}";
    }

    protected StructureModel? StructureModel { get; set; }

    //protected DataBlock? Block { get; set; }

    protected RegionModel? Region { get; set; }
    //protected RegionDataBlock? Region { get; set; }

    public StructureViewModel(string id, StructureKind structureKind = StructureKind.Building) : base(id)
    {
        StructureType = structureKind;
        _ownerName = string.Empty;
        _description = string.Empty;
        _name = string.Empty;
        _type = string.Empty;
        _size = 0;
    }

    protected void PopulateOccupants(bool ownerFirst)
    {
        Occupants.Clear();
        NodeViewModel unitsRoot = new();
        if (!AddUnits(unitsRoot, ownerFirst) || unitsRoot.Children.Count == 0)
        {
            return;
        }

        foreach (NodeViewModel unitNode in unitsRoot.Children[0].Children)
        {
            if (unitNode.Data is DataProperty property && property.Reference is UnitModel unit)
            {
                Occupants.Add(new StructureUnitLinkViewModel(
                    property,
                    GetUnitTooltip(unit),
                    new RelayCommand(() => SendSelectionChangedEvent(new EntitySelection(unit, Region)))));
            }
        }
    }

    [RelayCommand]
    private void SelectOwner()
    {
        if (OwnerUnit is UnitModel ownerUnit)
        {
            SendSelectionChangedEvent(new EntitySelection(ownerUnit, Region));
        }
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
        if (entityModel is not null)
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
    protected bool AddUnits(NodeViewModel parent, bool ownerFirst = false)
    {
        if (StructureModel == null || (StructureModel.Units.Count == 0 && (!ownerFirst || OwnerUnit is null)))
        {
            return false;
        }
        string label = Labels.Localize(Categories.Node, Labels.UNITS);
        DataProperty unitsProperty = new(Categories.Node, label, string.Empty, label, null, StructureModel);
        var unitsNode = AddItem(parent, unitsProperty);

        void AddUnit(UnitModel unit)
        {
            string id = unit.StringId;
            string name = unit.Name;
            string number = unit.PeopleNumber.ToString();
            string fullName = $"{name} ({id})";
            string unitlabel = $"{fullName}: {number}";
            DataProperty unitProperty = new(Categories.None, unitlabel, string.Empty, unitlabel, string.Empty, unit);
            _ = AddItem(unitsNode, unitProperty);
        }

        if (ownerFirst && OwnerUnit is UnitModel ownerUnit)
        {
            AddUnit(ownerUnit);
        }

        foreach (var unit in StructureModel.Units)
        {
            if (ownerFirst && OwnerUnit?.Id == unit.Id)
            {
                continue;
            }
            AddUnit(unit);
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

public sealed class StructureUnitLinkViewModel
{
    public DataProperty Property { get; }
    public string DisplayName => Property.Label;
    public string Tooltip { get; }
    public ICommand SelectCommand { get; }

    public StructureUnitLinkViewModel(DataProperty property, string tooltip, ICommand selectCommand)
    {
        Property = property;
        Tooltip = tooltip;
        SelectCommand = selectCommand;
    }
}

