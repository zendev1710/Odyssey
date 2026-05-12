using System;
using Odyssey.Models.Data;

namespace Odyssey.Services;
public interface ISelectionService
{
    DataBlock? SelectedUnit { get; set; }
    event Action<DataBlock?> SelectedUnitChanged;
}

public class SelectionService : ISelectionService
{
    private DataBlock? _selectedUnit;
    public DataBlock? SelectedUnit
    {
        get => _selectedUnit;
        set {
            if (_selectedUnit != value) {
                _selectedUnit = value;
                SelectedUnitChanged?.Invoke(_selectedUnit);
            }
        }
    }
    public event Action<DataBlock?>? SelectedUnitChanged;
}