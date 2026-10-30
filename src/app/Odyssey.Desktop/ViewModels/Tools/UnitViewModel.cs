using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;

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
    public UnitModel? Model { get; private set; }

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _factionId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IdentityBackground))]
    private bool _isConfirmed;

    [ObservableProperty]
    private int _peopleNumber;

    [ObservableProperty]
    private bool _areSkillsVisible;

    [ObservableProperty]
    private int _x;

    [ObservableProperty]
    private int _y;

    [ObservableProperty]
    private string _summary = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IdentityBackground))]
    [NotifyPropertyChangedFor(nameof(IdentityForeground))]
    private bool _isSelected;

    public string OrdersSummary { get; private set; } = "-";
    public IBrush FactionStatusBrush { get; private set; } = CreateFactionStatusBrush(FactionStatus.UNKNOWN);
    public IBrush FactionStatusBackground { get; private set; } = CreateFactionStatusBackground(FactionStatus.UNKNOWN);
    public IBrush FactionStatusSelectedBackground { get; private set; } = CreateFactionStatusSelectedBackground(FactionStatus.UNKNOWN);
    public IBrush FactionStatusConfirmedBackground { get; private set; } = CreateDarkenedBrush(CreateFactionStatusBackground(FactionStatus.UNKNOWN));
    public IBrush FactionStatusSelectedConfirmedBackground { get; private set; } = CreateDarkenedBrush(CreateFactionStatusSelectedBackground(FactionStatus.UNKNOWN));
    public IBrush IdentityBackground => IsSelected
        ? IsConfirmed ? FactionStatusSelectedConfirmedBackground : FactionStatusSelectedBackground
        : IsConfirmed ? FactionStatusConfirmedBackground : FactionStatusBackground;
    public IBrush IdentityForeground => IsSelected ? Brushes.White : Brushes.Black;
    public ObservableCollection<UnitItemViewModel> InventoryItems { get; } = new();
    public ObservableCollection<UnitSkillViewModel> SkillItems { get; } = new();

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
    public static UnitViewModel FromDocument(CRDocument doc, int unitId, bool includeSummary = false/*, int x, int y*/)
    {
        // create with id as fallback; fields will be populated if DataBlock found
        var vm = new UnitViewModel(unitId.ToString(), string.Empty/*, x, y*/);
        if (doc.FindUnit(unitId, out UnitModel? unit))
        {
            vm.Id = unit!.EncodedId;
            vm.Name = unit.Name;
            vm.FactionId = unit.FactionId;
            vm.IsConfirmed = unit.IsConfirmed;
            vm.PeopleNumber = unit.PeopleNumber;
            vm.Model = unit;
            FactionStatus factionStatus = doc.GetFactionStatus(CRDocument.GetFactionIdForUnit(unit.DataBlock));
            vm.AreSkillsVisible = factionStatus is FactionStatus.ACTIVE or FactionStatus.ALLIED;
            vm.FactionStatusBrush = CreateFactionStatusBrush(factionStatus);
            vm.FactionStatusBackground = CreateFactionStatusBackground(factionStatus);
            vm.FactionStatusSelectedBackground = CreateFactionStatusSelectedBackground(factionStatus);
            vm.FactionStatusConfirmedBackground = CreateDarkenedBrush(vm.FactionStatusBackground);
            vm.FactionStatusSelectedConfirmedBackground = CreateDarkenedBrush(vm.FactionStatusSelectedBackground);
            if (includeSummary)
            {
                vm.LoadUnitSummary(unit);
            }
            //vm.X = x;
            //vm.Y = y;
            vm.Summary = $"{vm.Name} ({vm.Id})";
        }
        return vm;
    }

    private static IBrush CreateFactionStatusBrush(FactionStatus status)
    {
        string color = status switch
        {
            FactionStatus.ACTIVE => "#3B82F6",
            FactionStatus.ALLIED => "#22C55E",
            FactionStatus.ANONYMOUS => "#9CA3AF",
            _ => "#EF4444",
        };
        return new SolidColorBrush(Color.Parse(color));
    }

    private static IBrush CreateFactionStatusBackground(FactionStatus status)
    {
        string color = status switch
        {
            FactionStatus.ACTIVE => "#DCEBFF",
            FactionStatus.ALLIED => "#DDF4E3",
            FactionStatus.ANONYMOUS => "#E5E7EB",
            _ => "#FCE0E0",
        };
        return new SolidColorBrush(Color.Parse(color));
    }

    private static IBrush CreateFactionStatusSelectedBackground(FactionStatus status)
    {
        string color = status switch
        {
            FactionStatus.ACTIVE => "#1D4ED8",
            FactionStatus.ALLIED => "#15803D",
            FactionStatus.ANONYMOUS => "#4B5563",
            _ => "#B91C1C",
        };
        return new SolidColorBrush(Color.Parse(color));
    }

    private static IBrush CreateDarkenedBrush(IBrush brush)
    {
        var baseColor = ((SolidColorBrush)brush).Color;
        return new SolidColorBrush(Color.FromArgb(
            baseColor.A,
            (byte)(baseColor.R / 2),
            (byte)(baseColor.G / 2),
            (byte)(baseColor.B / 2)));
    }

    private void LoadUnitSummary(UnitModel unit)
    {
        List<DataProperty> properties = [];
        if (AreSkillsVisible && unit.CollectSkills(ref properties))
        {
            foreach (DataProperty property in properties)
            {
                SkillItems.Add(new UnitSkillViewModel(
                    property.Name,
                    property.Label,
                    int.TryParse(property.Value, out int level) ? level : 0,
                    property.Change));
            }
        }

        properties = [];
        if (unit.CollectItems(ref properties))
        {
            foreach (DataProperty property in properties)
            {
                InventoryItems.Add(new UnitItemViewModel(
                    property.Name,
                    property.Label,
                    int.TryParse(property.Value, out int quantity) ? quantity : 0,
                    property.Category.ToString()));
            }
        }

        DataBlock? commandsBlock = null;
        if (CRDocument.GetSeenCommands(ref commandsBlock, unit.DataBlock) && commandsBlock is not null)
        {
            OrdersAttachment? attachment = commandsBlock.GetAttachment() as OrdersAttachment;
            IEnumerable<string> commands = attachment is not null
                ? attachment.Commands
                : commandsBlock.GetData().Select(data => data.GetValue());
            string summary = string.Join(Environment.NewLine, commands.Where(command => !string.IsNullOrWhiteSpace(command)));
            if (!string.IsNullOrWhiteSpace(summary))
            {
                OrdersSummary = summary;
            }
        }
    }

    public override string ToString() => $"{Name} ({Id})";
}