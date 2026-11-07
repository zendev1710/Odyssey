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
/// Conçu pour être créé à partir d'un <see cref="UnitModel"/> ou manuellement.
/// </summary>
public partial class UnitViewModel : ObservableObject
{
    public UnitModel? Model { get; private set; }

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _factionId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IdentityBackground))]
    private bool? _isConfirmed;

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
    public bool CanEditOrders { get; private set; }
    public IBrush FactionStatusBrush { get; private set; } = FactionStatusPalette.GetBorderBrush(FactionStatus.UNKNOWN);
    public IBrush FactionStatusBackground { get; private set; } = FactionStatusPalette.GetBackgroundBrush(FactionStatus.UNKNOWN);
    public IBrush FactionStatusSelectedBackground { get; private set; } = FactionStatusPalette.GetSelectedBackgroundBrush(FactionStatus.UNKNOWN);
    public IBrush FactionStatusConfirmedBackground { get; private set; } = FactionStatusPalette.Darken(FactionStatusPalette.GetBackgroundBrush(FactionStatus.UNKNOWN));
    public IBrush FactionStatusSelectedConfirmedBackground { get; private set; } = FactionStatusPalette.Darken(FactionStatusPalette.GetSelectedBackgroundBrush(FactionStatus.UNKNOWN));
    public IBrush IdentityBackground => IsSelected
        ? IsConfirmed == true ? FactionStatusSelectedConfirmedBackground : FactionStatusSelectedBackground
        : IsConfirmed == true ? FactionStatusConfirmedBackground : FactionStatusBackground;
    public IBrush IdentityForeground => IsSelected ? Brushes.White : Brushes.Black;
    public ObservableCollection<UnitItemViewModel> InventoryItems { get; } = new();
    public ObservableCollection<UnitSkillViewModel> SkillItems { get; } = new();

    public UnitViewModel(string id, string name, /*int x, int y,*/ int factionId = 0, bool? isConfirmed = null)
    {
        _id = id ?? string.Empty;
        _name = name ?? string.Empty;
        _factionId = factionId;
        _isConfirmed = isConfirmed;
        //_x = x;
        //_y = y;
        _summary = $"{_name} ({_id})";
    }

    // Factory légère : construit un UnitViewModel à partir du modèle d'unité du document
    public static UnitViewModel FromDocument(CRDocument doc, int unitId, bool includeSummary = false/*, int x, int y*/)
    {
        // create with id as fallback; fields will be populated if the model is found
        var vm = new UnitViewModel(unitId.ToString(), string.Empty/*, x, y*/);
        if (doc.FindUnit(unitId, out UnitModel? unit))
        {
            vm.Id = unit!.EncodedId;
            vm.Name = unit.Name;
            vm.FactionId = unit.FactionId;
            vm.IsConfirmed = unit.OrdersConfirmed;
            vm.PeopleNumber = unit.PeopleNumber;
            vm.Model = unit;
            FactionStatus factionStatus = doc.GetFactionStatus(unit.EffectiveFactionId);
            vm.CanEditOrders = unit.HasEditableOrders;
            vm.AreSkillsVisible = factionStatus is FactionStatus.ACTIVE or FactionStatus.ALLIED;
            vm.FactionStatusBrush = FactionStatusPalette.GetBorderBrush(factionStatus);
            vm.FactionStatusBackground = FactionStatusPalette.GetBackgroundBrush(factionStatus);
            vm.FactionStatusSelectedBackground = FactionStatusPalette.GetSelectedBackgroundBrush(factionStatus);
            vm.FactionStatusConfirmedBackground = FactionStatusPalette.Darken(vm.FactionStatusBackground);
            vm.FactionStatusSelectedConfirmedBackground = FactionStatusPalette.Darken(vm.FactionStatusSelectedBackground);
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
            SortSkillItems();
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

        IReadOnlyList<string> commands = unit.GetCommandLines();
        if (commands.Count > 0)
        {
            string summary = string.Join(Environment.NewLine, commands.Where(command => !string.IsNullOrWhiteSpace(command)));
            if (!string.IsNullOrWhiteSpace(summary))
            {
                OrdersSummary = summary;
            }
        }
    }

    public void SortSkillItems() => UnitSkillSorter.Sort(SkillItems);

    public override string ToString() => $"{Name} ({Id})";
}