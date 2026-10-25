using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static Odyssey.Models.Tools.DataProperty;
using static Odyssey.Utils.Converters;

namespace Odyssey.ViewModels;

public partial class ShipDetailsViewModel : StructureViewModel
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

    public ShipDetailsViewModel() : base(Ids.ShipDetails)
    {
        Items = Root.Children;
    }
    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this)) return;

        bool shipChanged = selType.HasFlag(SelectionChangeTypes.Ship);
        bool shipSelected = selection.HasShip();

        Debug.WriteLine($"[VM-SHIP] OnSelectionChanged: ship changed={shipChanged}, ship selected={shipSelected}");

        if (shipChanged)
        {
            MakeItems(selection);
        }
    }

    public void MakeItems(IEntitySelection selection)
    {
        Items.Clear();
        EntityModel? item = selection.Item;
        if (item is null)
            return;


        //Block = selection?.Item!.DataBlock;
        //Region = selection?.Region!.DataBlock as RegionDataBlock;
        ShipModel? shipModel = null;
        if (!Report.FindShip(item.Id/*Block!.GetId()*/, out shipModel))
        {
            return;
        }

        StructureModel = shipModel;
        DataProperty? property = null;
        if (StructureModel!.CollectData(Report/*, selection?.Region*/, ref property))
        {
            OwnerName = StructureModel.OwnerName;
            OwnerUnit = StructureModel.OwnerUnit;
            Name = StructureModel.Name;
            Type = StructureModel.Type;
            Size = StructureModel.Size;
            Description = StructureModel.Description;

            DamagePercent = shipModel.DamagePercent;
            Coast = shipModel.Coast;
            Cargo = shipModel.Cargo;
            Capacity = shipModel.Capacity;

            NodeViewModel structureNode = AddItem(Root, property, string.Empty, 0);
            AddOwner(structureNode, false);

            if (Coast >= 0)
            {
                string coastLabel = Labels.GetCoastName(Coast);
                DataProperty coastProperty = new(coastLabel);
                AddItem(structureNode, coastProperty);
            }
            if (Cargo > 0 || Capacity > 0)
            {
                string shipCapacityLabel = Labels.Localize(Categories.None, Labels.CAPACITY_SHIP_INFO, [ToStringWithDecimals(shipModel.Cargo), ToStringWithDecimals(shipModel.Capacity)]);
                DataProperty shipCapacityProperty = new(Categories.None, Labels.CAPACITY_SHIP_INFO, string.Empty, shipCapacityLabel);
                AddItem(structureNode, shipCapacityProperty);
            }
            if (DamagePercent > 0)
            {
                string damageLabel = Labels.Localize(Categories.None, Labels.SHIP_DAMAGE, [$"{DamagePercent}"]);
                DataProperty damageProperty = new(damageLabel);
                AddItem(structureNode, damageProperty);
            }
            foreach (DataProperty p in shipModel.DataProperties)
            {
                _ = AddItem(structureNode, p, $"{p.Label}: {p.Value}");
            }

            AddEffects(structureNode);

            if (shipModel.TotalSkill > 0)
            {
                string sailingSkillLabel = Labels.Localize(Categories.Node, Labels.TOTAL_SAILING_SKILL);
                string totalSkillLabel = $"{sailingSkillLabel}: {shipModel.TotalSkill}";
                DataProperty totalSkillProperty = new(Categories.None, totalSkillLabel, string.Empty, totalSkillLabel);
                AddItem(structureNode, totalSkillProperty);
            }

            AddUnits(structureNode);
        }
    }
}
