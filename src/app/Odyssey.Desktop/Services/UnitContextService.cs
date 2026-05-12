using Odyssey.Models.Data;
using Odyssey.Models.Tools;
using Odyssey.ViewModels.Tools;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Odyssey.Models.Documents;

namespace Odyssey.Services;

public interface IUnitContextService
{
    DataBlock? SelectedUnit { get; set; }
    ObservableCollection<UnitItemViewModel> InventoryItems { get; }
    event Action<DataBlock?>? SelectedUnitChanged;
}

public class UnitContextService : IUnitContextService
{
    private DataBlock? _selectedUnit;
    private readonly CRDocument _report; // Assurez-vous d'avoir accès au document courant

    public ObservableCollection<UnitItemViewModel> InventoryItems { get; } = new();
    public event Action<DataBlock?>? SelectedUnitChanged;

    public UnitContextService(CRDocument report)
    {
        _report = report;
    }

    public DataBlock? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (_selectedUnit != value)
            {
                _selectedUnit = value;
                UpdateInventory();
                SelectedUnitChanged?.Invoke(_selectedUnit);
            }
        }
    }

    private void UpdateInventory()
    {
        InventoryItems.Clear();
        if (_selectedUnit == null) return;

        UnitModel unitModel = new(_selectedUnit);
        unitModel.CollectCategoriesData();

        List<DataProperty> itemsProperties = [];
        if (unitModel.CollectItems(ref itemsProperties))
        {
            foreach (var prop in itemsProperties)
            {
                int quantity = int.TryParse(prop.Value, out int q) ? q : 0;
                // TODO: set correct type
                InventoryItems.Add(new UnitItemViewModel(prop.Label, quantity, "item"));
            }
        }
    }
}
