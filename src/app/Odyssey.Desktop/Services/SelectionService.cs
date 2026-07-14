using System;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;

namespace Odyssey.Services;

public interface ISelectionService
{
    ISelection Selection { get; }
    event Action<ISelection> SelectionChanged;

    void SelectItem(DataBlock item, DataBlock region = null, DataBlock faction = null);
    void SelectRegion(DataBlock region);
    void SelectFaction(DataBlock faction);
    void Clear();
}

public class SelectionService : ISelectionService
{
    private readonly SimpleItemSelection _selection;

    public ISelection Selection => _selection;

    public event Action<ISelection> SelectionChanged;

    public SelectionService()
    {
        _selection = new SimpleItemSelection();
    }

    public void SelectItem(DataBlock item, DataBlock region = null, DataBlock faction = null)
    {
        _selection.SetItem(item, region, faction);
        SelectionChanged?.Invoke(_selection);
    }

    public void SelectRegion(DataBlock region)
    {
        _selection.SetRegion(region);
        SelectionChanged?.Invoke(_selection);
    }

    public void SelectFaction(DataBlock faction)
    {
        _selection.SetFaction(faction);
        SelectionChanged?.Invoke(_selection);
    }

    public void Clear()
    {
        _selection.Clear();
        SelectionChanged?.Invoke(_selection);
    }
}
