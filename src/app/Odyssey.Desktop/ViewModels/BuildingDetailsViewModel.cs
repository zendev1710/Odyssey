using Avalonia.Controls;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;

namespace Odyssey.ViewModels;

public partial class BuildingDetailsViewModel : ContainerViewModel
{
    private readonly NodeViewModel _root = new();

    protected NodeViewModel Root { get { return _root; } }

    public ObservableCollection<NodeViewModel> Items { get; }

    public BuildingDetailsViewModel(): this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    public BuildingDetailsViewModel(IEventAggregator? eventAggregator) : base(Ids.BuildingDetails, eventAggregator)
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
        if (sel.IsBuildingSelected() && !IsSelected(sel))
        {
           
            MakeItems(sel);
        }
    }

    private void MakeItems(ISelection sel)
    {
        Items.Clear();
        ISelection? selection = SetSelection(sel);
        Block = selection?.Item;
        Region = selection?.Region;
        BuildingModel buildingModel = new(Block!);
        ContainerModel = buildingModel;

        DataProperty? property = null;

        if (ContainerModel.CollectData(Report, selection?.Region, ref property))
        {
            OwnerName = ContainerModel.OwnerName;
            OwnerUnit = ContainerModel.OwnerUnit;
            Name = ContainerModel.Name;
            Type = ContainerModel.Type;
            Size = ContainerModel.Size;
            Description = ContainerModel.Description;

            NodeViewModel containerNode = AddItem(Root, property, string.Empty, 0);
            AddOwner(containerNode, true);
            foreach (DataProperty p in ContainerModel.DataProperties)
            {
                _ = AddItem(containerNode, p, $"{p.Label}: {p.Value}");
            }
            AddEffects(containerNode);
            AddUnits(containerNode);
        }
    }
}
