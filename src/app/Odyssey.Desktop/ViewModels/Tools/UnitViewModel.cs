using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using static Odyssey.Models.Documents.CRDocument;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// ViewModel léger représentant une unité pour les vues d'édition d'ordres.
/// Conçu pour être créé à partir d'un <see cref="DataBlock"/> ou manuellement.
/// </summary>
public partial class UnitViewModel : ObservableObject
{
    /// <summary>
    /// Underlying data block (nullable si construit manuellement).
    /// </summary>
    public DataBlock? DataBlock { get; }

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _factionId;

    [ObservableProperty]
    private bool _isConfirmed;

    [ObservableProperty]
    private int _x;

    [ObservableProperty]
    private int _y;

    [ObservableProperty]
    private string _summary = string.Empty;

    public UnitViewModel(string id, string name, /*int x, int y,*/ int factionId = 0, bool isConfirmed = false)
    {
        DataBlock = null;
        _id = id ?? string.Empty;
        _name = name ?? string.Empty;
        _factionId = factionId;
        _isConfirmed = isConfirmed;
        //_x = x;
        //_y = y;
        _summary = $"{_name} ({_id})";
    }

    public UnitViewModel(DataBlock block, int x, int y)
    {
        DataBlock = block ?? throw new ArgumentNullException(nameof(block));

        // Populate common properties from DataBlock (defensive - rely on CRDocument API)
        try
        {
            // Display label / name
            _name = block.GetUILabel() ?? string.Empty;

            // Try to read common keys; fallback to sensible defaults
            try { _factionId = block.ValueInt(KeyType.FACTION); } catch { _factionId = 0; }
            try { _isConfirmed = block.ValueInt(KeyType.ORDERS_CONFIRMED) != 0; } catch { _isConfirmed = false; }

            // Id: prefer string key if available, otherwise use int id
            try
            {
                _id = block.IdToString();
            }
            catch
            {
                try { _id = block.ValueInt(KeyType.ID).ToString(); } catch { _id = string.Empty; }
            }

            // Coordinates (if present)
            // TODO: check if coordinates are needed
            _x = x;
            _y = y;

            _summary = $"{_name} ({_id})";
        }
        catch
        {
            // Ensure object is still usable even if DataBlock layout differs
            _id ??= string.Empty;
            _name ??= string.Empty;
            _summary ??= $"{_name} ({_id})";
        }
    }

    // Factory légère : construit un UnitViewModel à partir d'un unit id dans CRDocument
    public static UnitViewModel FromDocument(CRDocument doc, int unitId/*, int x, int y*/)
    {
        // create with id as fallback; fields will be populated if DataBlock found
        var vm = new UnitViewModel(unitId.ToString(), string.Empty/*, x, y*/);
        if (doc.Units.TryGetValue(unitId, out var block) && block != null)
        {
            vm.Id = block.IdToString();
            vm.Name = block.GetUILabel() ?? string.Empty;
            vm.FactionId = block.ValueInt(KeyType.FACTION, 0);
            vm.IsConfirmed = block.ValueInt(KeyType.ORDERS_CONFIRMED, 0) != 0;
            //vm.X = x;
            //vm.Y = y;
            vm.Summary = $"{vm.Name} ({vm.Id})";
        }
        return vm;
    }

    public override string ToString() => $"{Name} ({Id})";
}