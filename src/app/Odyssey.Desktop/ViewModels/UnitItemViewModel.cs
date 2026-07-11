using CommunityToolkit.Mvvm.ComponentModel;

namespace Odyssey.ViewModels.Tools;

public partial class UnitItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty] private int _initialQuantity; // Quantité avant ordres

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FinalQuantity))]
    [NotifyPropertyChangedFor(nameof(DisplayQuantity))]
    private int _deltaQuantity;   // Ce qui est donné/utilisé

    [ObservableProperty]
    private int _weight;

 // Propriété calculée pour la vue
    public int FinalQuantity => InitialQuantity + DeltaQuantity;
    public string DisplayQuantity => DeltaQuantity != 0
        ? $"{InitialQuantity} → {FinalQuantity}"
        : $"{InitialQuantity}";

    [ObservableProperty]
    private string _type = string.Empty;

    [ObservableProperty]
    private string _displayString = string.Empty;

    public UnitItemViewModel(string name, int quantity, string type)
    {
        Name = name;
        InitialQuantity = quantity;
        Type = type;

        // TODO: définir le poids en fonction du type d'item et de sa quantité
        Weight = 0;

        // Exemple de formatage pour l'affichage : "10 Sword"
        DisplayString = $"{quantity} {name}";
    }

    // Utilisation de l'attribut NotifyPropertyChangedFor pour lier FinalQuantity
    partial void OnDeltaQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(FinalQuantity));
        OnPropertyChanged(nameof(DisplayQuantity));
    }
}