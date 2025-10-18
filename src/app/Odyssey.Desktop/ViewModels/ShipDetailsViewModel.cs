using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Odyssey.Utils.Converters;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;
using static Odyssey.Models.Tools.DataProperty;
using Odyssey.Models.Tools;
using Odyssey.Models.Localization;
using Avalonia.Controls;
using System;
using System.Diagnostics;
using Odyssey.Models.Documents;

namespace Odyssey.ViewModels;

public partial class ShipDetailsViewModel : ContainerViewModel
{
    private readonly NodeViewModel _root = new();
    protected NodeViewModel Root { get { return _root; } }
    public ObservableCollection<NodeViewModel> Items { get; }

    [ObservableProperty]
    private int _coast;

    [ObservableProperty]
    private int _damagePercent;

    [ObservableProperty]
    private int _cargo;

    [ObservableProperty]
    private int _capacity;

    public ShipDetailsViewModel(): this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }
    public ShipDetailsViewModel(IEventAggregator? eventAggregator) : base(Ids.ShipDetails, eventAggregator)
    {
        Items = Root.Children;
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
        if (sel.IsShipSelected() && !IsSelected(sel))
        {
            MakeItems(sel);
        }
    }

    public void MakeItems(ISelection sel)
    {
        Items.Clear();
        ISelection? selection = SetSelection(sel);
        Block = selection?.Item;
        Region = selection?.Region;
        ShipModel shipModel = new(Block!);
        ContainerModel = shipModel; 
        DataProperty? property = null;
        if (ContainerModel.CollectData(Report, selection?.Region, ref property))
        {
            OwnerName = ContainerModel.OwnerName;
            OwnerUnit = ContainerModel.OwnerUnit;
            Name = ContainerModel.Name;
            Type = ContainerModel.Type;
            Size = ContainerModel.Size;
            Description = ContainerModel.Description;

            DamagePercent = shipModel.DamagePercent;
            Coast = shipModel.Coast;
            Cargo = shipModel.Cargo;
            Capacity = shipModel.Capacity;

            NodeViewModel containerNode = AddItem(Root, property, string.Empty, 0);
            AddOwner(containerNode, false);

            if (Coast >= 0)
            {
                string coastLabel = Labels.GetCoastName(Coast);
                DataProperty coastProperty = new(coastLabel);
                AddItem(containerNode, coastProperty);
            }
            if (Cargo > 0 || Capacity > 0)
            {
                string shipCapacityLabel = Labels.Localize(Categories.None, Labels.CAPACITY_SHIP_INFO, [ToStringWithDecimals(shipModel.Cargo), ToStringWithDecimals(shipModel.Capacity)]);
                DataProperty shipCapacityProperty = new(Categories.None, Labels.CAPACITY_SHIP_INFO, string.Empty, shipCapacityLabel);
                AddItem(containerNode, shipCapacityProperty);
            }
            if (DamagePercent > 0)
            {
                string damageLabel = Labels.Localize(Categories.None, Labels.SHIP_DAMAGE, [$"{DamagePercent}"]);
                DataProperty damageProperty = new(damageLabel);
                AddItem(containerNode, damageProperty);
            }
            foreach (DataProperty p in shipModel.DataProperties)
            {
                _ = AddItem(containerNode, p, $"{p.Label}: {p.Value}");
            }

            AddEffects(containerNode);

            if (shipModel.TotalSkill > 0)
            {
                string sailingSkillLabel = Labels.Localize(Categories.Node, Labels.TOTAL_SAILING_SKILL);
                string totalSkillLabel = $"{sailingSkillLabel}: {shipModel.TotalSkill}";
                DataProperty totalSkillProperty = new(Categories.None, totalSkillLabel, string.Empty, totalSkillLabel);
                AddItem(containerNode, totalSkillProperty);
            }

            AddUnits(containerNode);
        }
    }
}
