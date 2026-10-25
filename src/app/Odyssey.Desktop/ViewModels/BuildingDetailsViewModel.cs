using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Odyssey.ViewModels;

public partial class BuildingDetailsViewModel : StructureViewModel
{
    private readonly NodeViewModel _root = new();

    protected NodeViewModel Root { get { return _root; } }

    public ObservableCollection<NodeViewModel> Items { get; }

    public BuildingDetailsViewModel() : base(Ids.BuildingDetails)
    {
        Items = Root.Children;
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this))
            return;

        bool buildingChanged = selType.HasFlag(SelectionChangeTypes.Building);
        bool buildingSelected = selection.HasBuilding();

        Debug.WriteLine($"[VM-BUILDING] OnSelectionChanged: building changed={buildingChanged}, building selected={buildingSelected}");

        if (buildingChanged)
        {
            MakeItems(selection);
        }
    }

    private void MakeItems(IEntitySelection selection)
    {
        Items.Clear();
        EntityModel? item = selection.Item;
        if (item is null)
            return;

        //Block = selection.Item!.DataBlock;
        Region = selection.Region;
        //StructureModel = selection.Item as StructureModel;

        BuildingModel? buildingModel = null;
        if (!Report.FindBuilding(item.Id/*Block!.GetId()*/, out buildingModel))
        {
            return;
        }

        StructureModel = buildingModel;

        DataProperty? property = null;

        if (StructureModel!.CollectData(Report/*, selection?.Region*/, ref property))
        {
            OwnerName = StructureModel.OwnerName;
            OwnerUnit = StructureModel.OwnerUnit;
            Name = StructureModel.Name;
            Type = StructureModel.Type;
            Size = StructureModel.Size;
            Description = StructureModel.Description;

            NodeViewModel structureNode = AddItem(Root, property, string.Empty, 0);
            AddOwner(structureNode, true);
            foreach (DataProperty p in StructureModel.DataProperties)
            {
                _ = AddItem(structureNode, p, $"{p.Label}: {p.Value}");
            }
            AddEffects(structureNode);
            AddUnits(structureNode);
        }
    }
}
