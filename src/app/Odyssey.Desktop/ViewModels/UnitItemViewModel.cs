using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Icons;
using Odyssey.Services;

namespace Odyssey.ViewModels.Tools;

public partial class UnitItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty] private int _initialQuantity; // Quantité avant ordres

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FinalQuantity))]
    [NotifyPropertyChangedFor(nameof(DisplayQuantity))]
    [NotifyPropertyChangedFor(nameof(QuantityText))]
    private int _deltaQuantity;   // Ce qui est donné/utilisé

    [ObservableProperty]
    private int _weight;

    public int FinalQuantity => InitialQuantity + DeltaQuantity;
    public string QuantityText => FinalQuantity.ToString();
    public string DisplayQuantity => DeltaQuantity != 0
        ? $"{InitialQuantity} → {FinalQuantity}"
        : $"{InitialQuantity}";
    public string TooltipLabel => $"{InitialQuantity} {Name}{(InitialQuantity == 1 ? string.Empty : "s")}";

    public string IconPath => string.IsNullOrEmpty(Key)
        ? string.Empty
        : IconCatalogService.ResolvePath(IconCategories.Item, Key, "weapons") ?? string.Empty;

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _displayString = string.Empty;

    public UnitItemViewModel(string name, int quantity, string type)
        : this(name, name, quantity, type)
    {
    }

    public UnitItemViewModel(string itemKey, string displayName, int quantity, string type)
    {
        Key = itemKey ?? string.Empty;
        Name = string.IsNullOrWhiteSpace(displayName) ? (itemKey ?? string.Empty) : displayName;
        InitialQuantity = quantity;
        Type = type;

        // TODO: définir le poids en fonction du type d'item et de sa quantité
        Weight = 0;
        DisplayString = $"{quantity} {Name}";
    }

    partial void OnDeltaQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(FinalQuantity));
        OnPropertyChanged(nameof(DisplayQuantity));
        OnPropertyChanged(nameof(QuantityText));
    }
}