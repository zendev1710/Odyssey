using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Core.Services;
using Odyssey.Models;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Settings;
using Odyssey.TextMate;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using static Odyssey.Utils.Converters;

namespace Odyssey.ViewModels.Tools;

public partial class UnitOrdersViewModel : DocumentToolViewModelBase
{
    // Collections mises à jour par HandleSelection
    public ObservableCollection<UnitViewModel> UnitsInRegion { get; } = new();

    // Ajoutez cette propriété dans UnitOrdersViewModel
    public ObservableCollection<UnitItemViewModel> InventoryItems { get; } = new();
    public GlobalSettings Settings => GlobalSettings.Shared;

    private static int FreeTempUnitNumber { get; set; } = 1;

    private bool IsWrapModeEnabled { get; set; }

    /// <summary>
    /// Key of the region the context data (UnitsInRegion) is currently built from, or -1 when no region
    /// is loaded. Used to not recompute the context data when navigating inside the same region.
    /// </summary>
    private int _contextRegionKey = -1;
    private int? _activeEditableUnitId;
    private bool _isLoadingOrders;
    private bool _isSyncingConfirmation;
    private CRDocument? _editedReport;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private bool _isConfirmed;

    [ObservableProperty]
    private string? _makeTempUnitSnippet;

    [ObservableProperty]
    private string _currentUnitId;

    [ObservableProperty]
    private bool _canEditOrders;

    private readonly List<ColorThemeViewModel> _allThemes = [];
    public ObservableCollection<ColorThemeViewModel> AvailableThemes { get; } = [];

    [ObservableProperty]
    private ColorThemeViewModel _selectedTheme = new(ExtendedThemeName.DarkPlus);

    public TextDocument Document { get; set; }

    public UnitOrdersViewModel() : base()
    {
        // when enabled, next/previous unit commands traverse the explorer view in circular mode, i.e.
        // the previous of the first editable unit is the last matching one, the next of the last editable unit is the first matching one
        IsWrapModeEnabled = true;
        Settings.PropertyChanged += OnGlobalSettingsPropertyChanged;
        ServiceLocator.UnitNavigationOrderService.OrderChanged += OnUnitNavigationOrderChanged;
        Document = new TextDocument { Text = "" };
        CanEditOrders = false;
        Reset();

        foreach (ExtendedThemeName themeName in Enum.GetValues<ExtendedThemeName>())
        {
            _allThemes.Add(new ColorThemeViewModel(themeName));
        }

        SelectedTheme = _allThemes.Single(theme => theme.ThemeName == ExtendedThemeName.DarkPlus);
        UpdateAvailableThemes(isDarkMode: true);

        Document.TextChanged += (sender, args) =>
        {
            RecalculateInventoryProjections();
            if (!_isLoadingOrders && CanEditOrders && _activeEditableUnitId is int unitId)
            {
                Report.SetEditedCommands(unitId, GetCommandLines(Document.Text));
                IsModified = true;
            }
        };
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

        // The context data has to be collected again from the new document.
        _contextRegionKey = -1;
        UnitsInRegion.Clear();
        _activeEditableUnitId = null;
        if (_editedReport is not null)
        {
            _editedReport.ModifiedChanged -= OnReportModifiedChanged;
            _editedReport.OrderConfirmationChanged -= OnOrderConfirmationChanged;
        }
        _editedReport = report;
        _editedReport.ModifiedChanged += OnReportModifiedChanged;
        _editedReport.OrderConfirmationChanged += OnOrderConfirmationChanged;
        IsModified = report.HasEditedOrderData;
        NotifyNavigationCommandsCanExecuteChanged();
    }

    private void OnGlobalSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GlobalSettings.NavigateOnlyToEditableUnits) ||
            e.PropertyName == nameof(GlobalSettings.NavigateOnlyToUnconfirmedOrders))
        {
            NotifyNavigationCommandsCanExecuteChanged();
        }
    }

    private void OnUnitNavigationOrderChanged(object? sender, EventArgs e)
    {
        NotifyNavigationCommandsCanExecuteChanged();
    }

    private void OnOrderConfirmationChanged(object? sender, EventArgs e)
    {
        NotifyNavigationCommandsCanExecuteChanged();
    }

    private void OnReportModifiedChanged(object? sender, EventArgs args)
    {
        IsModified = _editedReport?.HasEditedOrderData ?? false;
    }

    private void SetDisplayedConfirmationStatus(bool isConfirmed)
    {
        _isSyncingConfirmation = true;
        try
        {
            IsConfirmed = isConfirmed;
        }
        finally
        {
            _isSyncingConfirmation = false;
        }
    }

    // <summary>
    /// Parses the editor text to calculate the impact of GIVE/USE commands on the inventory.
    /// </summary>
    private void RecalculateInventoryProjections()
    {
        // 1. Reset all deltas
        foreach (var item in InventoryItems)
        {
            item.DeltaQuantity = 0;
        }

        // 2. Parse the text line by line
        var lines = Document.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var upperLine = line.ToUpper().Trim();

            // Handle GIVE
            if (upperLine.StartsWith("GIVE"))
            {
                var parts = upperLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                // Syntax: GIVE <unit id> <number> <item>
                if (parts.Length >= 4 && int.TryParse(parts[2], out int quantity))
                {
                    string itemName = parts[3];
                    var itemVm = InventoryItems.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
                    if (itemVm != null)
                    {
                        itemVm.DeltaQuantity -= quantity;
                    }
                }
            }
            // Handle USE
            else if (upperLine.StartsWith("USE"))
            {
                var parts = upperLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                // Syntax: USE [<number>] potion
                if (parts.Length >= 2)
                {
                    int quantity = 1; // Default
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int q)) quantity = q;

                    string itemName = parts.Length >= 3 ? parts[2] : parts[1];
                    var itemVm = InventoryItems.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
                    if (itemVm != null)
                    {
                        itemVm.DeltaQuantity -= quantity;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Automatically sent by the ObservableObject when the property IsConfirmed has changed.
    /// </summary>
    /// <param name="oldValue">old value.</param>
    /// <param name="newValue">new value</param>
    partial void OnIsConfirmedChanged(bool oldValue, bool newValue)
    {
        if (_isSyncingConfirmation)
        {
            return;
        }

        IEntitySelection selection = CurrentSelection;
        if (!HasDocument || !CanEditOrders ||
            selection.Item is not UnitModel unitModel ||
            _activeEditableUnitId != unitModel.Id)
        {
            return;
        }

        Report.SetEditedUnitConfirmationStatus(unitModel.Id, newValue);
        NotifyUnitConfirmedStatusHasChanged();
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this)) return;
        HandleSelection(selection, selType);
    }

    /// <summary>
    /// Make TEMP UNIT requested.
    /// </summary>
    public void OnMakeTempUnitRequested()
    {
        if (!CanEditOrders || _activeEditableUnitId is null)
        {
            return;
        }

        string snippet = CreateMakeTempUnitOrder();
        MakeTempUnitSnippet = snippet;
        if (Document.TextLength > 0 && !Document.Text.EndsWith('\n'))
        {
            Document.Insert(Document.TextLength, Environment.NewLine);
        }
        Document.Insert(Document.TextLength, snippet);
    }

    private void HandleSelection(IEntitySelection selection, SelectionChangeTypes selType)
    {
        // Handle all kind of selection :
        // - unit selection => update orders view with unit orders and update confirmed status
        // - other selection => empty orders view content and disable commands related to orders edition
        OnEditableUnitSelected(selection, selType);
        // TODO: update confirmed status (check/unchecked) when navigating from one unit to another
    }

    [RelayCommand(CanExecute = nameof(CanSelectPreviousUnit))]
    private void SelectPreviousUnit()
    {
        if (!CanSelectPreviousUnit())
        {
            // should beep
            return;
        }
        Debug.WriteLine("[VM-ORDERS--] Selecting previous unit...");
        SelectUnit(FindAdjacentUnit(next: false));
    }

    [RelayCommand(CanExecute = nameof(CanToggleConfirmedStatus))]
    private void ToggleConfirmedStatus()
    {
        // TODO
    }

    [RelayCommand(CanExecute = nameof(CanSelectNextUnit))]
    private void SelectNextUnit()
    {
        if (!CanSelectNextUnit())
        {
            // should beep
            return;
        }
        Debug.WriteLine("[VM-ORDERS--] Selecting next unit...");
        SelectUnit(FindAdjacentUnit(next: true));
    }

    private UnitModel? FindAdjacentUnit(bool next)
    {
        IUnitNavigationOrderService order = ServiceLocator.UnitNavigationOrderService;
        if (!ReferenceEquals(order.Report, Report))
        {
            return null;
        }

        bool editableOnly = GlobalSettings.Get<bool>(GlobalSettings.UNIT_ORDERS_NAVIGATE_EDITABLE_ONLY);
        bool unconfirmedOnly = GlobalSettings.Get<bool>(GlobalSettings.UNIT_ORDERS_NAVIGATE_UNCONFIRMED_ONLY);
        IEntitySelection selection = CurrentSelection;
        IReadOnlyList<UnitModel> orderedUnits = order.OrderedUnits;
        if (selection.Item is UnitModel selectedUnit)
        {
            int currentIndex = -1;
            for (int i = 0; i < orderedUnits.Count; i++)
            {
                if (orderedUnits[i].Id == selectedUnit.Id)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex >= 0)
            {
                for (int distance = 1; distance < orderedUnits.Count; distance++)
                {
                    int candidateIndex = currentIndex + (next ? distance : -distance);
                    if (IsWrapModeEnabled)
                    {
                        candidateIndex = (candidateIndex % orderedUnits.Count + orderedUnits.Count) % orderedUnits.Count;
                    }
                    else if (candidateIndex < 0 || candidateIndex >= orderedUnits.Count)
                    {
                        break;
                    }

                    UnitModel candidate = orderedUnits[candidateIndex];
                    if (IsNavigableUnit(candidate, editableOnly, unconfirmedOnly))
                    {
                        return candidate;
                    }
                }
                return null;
            }
        }

        if (selection.Region is not RegionModel selectedRegion)
        {
            return null;
        }

        IReadOnlyList<RegionUnitOrder> orderedRegions = order.OrderedRegions;
        int selectedRegionIndex = -1;
        for (int i = 0; i < orderedRegions.Count; i++)
        {
            if (orderedRegions[i].Region.Id == selectedRegion.Id)
            {
                selectedRegionIndex = i;
                break;
            }
        }
        if (selectedRegionIndex < 0 || orderedRegions.Count == 0)
        {
            return null;
        }

        for (int distance = 0; distance < orderedRegions.Count; distance++)
        {
            int regionIndex = next
                ? selectedRegionIndex + distance
                : selectedRegionIndex - distance - 1;
            if (IsWrapModeEnabled)
            {
                regionIndex = (regionIndex % orderedRegions.Count + orderedRegions.Count) % orderedRegions.Count;
            }
            else if (regionIndex < 0 || regionIndex >= orderedRegions.Count)
            {
                break;
            }

            IReadOnlyList<UnitModel> regionUnits = orderedRegions[regionIndex].Units;
            for (int i = 0; i < regionUnits.Count; i++)
            {
                int unitIndex = next ? i : regionUnits.Count - i - 1;
                UnitModel candidate = regionUnits[unitIndex];
                if (IsNavigableUnit(candidate, editableOnly, unconfirmedOnly))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private bool IsNavigableUnit(UnitModel unit, bool editableOnly, bool unconfirmedOnly)
    {
        bool isActiveFaction = Report.IsActiveFaction(unit.FactionId);
        bool hasEditableOrders = unit.HasEditableOrders;
        return isActiveFaction &&
               (!editableOnly || hasEditableOrders) &&
               (!unconfirmedOnly || (hasEditableOrders && unit.OrdersConfirmed == false));
    }

    private void SelectUnit(UnitModel? unitModel)
    {
        if (unitModel is null)
        {
            return;
        }

        IEntitySelection newSelection = new EntitySelection(unitModel, null, null);
        OnEditableUnitSelected(newSelection, SelectionChangeTypes.Unit);
        SendSelectionChangedEvent(newSelection);
    }

    private bool CanToggleConfirmedStatus()
    {
        // TODO: manage other cases
        return Report.HasData();
    }
    private bool CanSelectPreviousUnit()
    {
        return Report.HasData() && FindAdjacentUnit(next: false) is not null;
    }
    private bool CanSelectNextUnit()
    {
        return Report.HasData() && FindAdjacentUnit(next: true) is not null;
    }

    private void NotifyNavigationCommandsCanExecuteChanged()
    {
        SelectPreviousUnitCommand.NotifyCanExecuteChanged();
        SelectNextUnitCommand.NotifyCanExecuteChanged();
    }
    private void NotifyUnitConfirmedStatusHasChanged()
    {
        SendSelectionChangedEvent(CurrentSelection);
    }

    /// <summary>
    /// Load unit orders lines, and returns them as a concatenated string.
    /// </summary>
    /// <param name="commands">The unit's command lines</param>
    /// <returns>All the orders lines as a concatenated string.</returns>
    private static string LoadCommands(IReadOnlyList<string> commands)
    {
        Debug.WriteLine("[VM-ORDERS--] Load Commands -> BEGIN");
        StringBuilder textBuilder = new();
        foreach (string command in commands)
        {
            textBuilder.Append(command);
            textBuilder.Append(Environment.NewLine);
        }
        Debug.WriteLine("[VM-ORDERS--] Load Commands -> ENDED");
        return textBuilder.ToString();
    }

    private static string[] GetCommandLines(string text)
    {
        string[] lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        if (lines.Length > 0 && lines[^1].Length == 0)
        {
            Array.Resize(ref lines, lines.Length - 1);
        }
        return lines;
    }

    private static string GetFreeTemp()
    {
        return ToStringVal(FreeTempUnitNumber++);
    }
    /// <summary>
    /// Creates a MAKE TEMP UNIT order and returns the result string.
    /// an internal counter is used to generate a unique number for the temporary unit.
    /// The result string wil be like:
    /// MAKE TEMP
    /// ...
    /// END
    /// </summary>
    private static string CreateMakeTempUnitOrder()
    {
        StringBuilder textBuilder = new();
        textBuilder.Append("MAKE TEMP "); // DE : "MACHE TEMP "
        textBuilder.Append(Environment.NewLine);
        textBuilder.Append(GetFreeTemp());
        textBuilder.Append('\t');
        textBuilder.Append(Environment.NewLine);
        textBuilder.Append("END"); // DE : ENDE
        textBuilder.Append(Environment.NewLine);
        return textBuilder.ToString();
    }

    /// <summary>
    /// Returns the confirmed status value of the selected unit.
    /// </summary>
    /// <returns>The confirmed status value if a unit is selected; otherwise -1</returns>
    public int GetConfirmed()
    {
        if (!HasDocument)
        {
            return -1;
        }

        IEntitySelection selection = CurrentSelection;
        if (selection.HasUnit())
        {
            if (selection.Item is UnitModel unitModel &&
                Report.IsActiveFaction(unitModel.FactionId) &&
                unitModel.OrdersConfirmed is bool ordersConfirmed)
            {
                return ordersConfirmed ? 1 : 0;
            }
            return -1;
        }
        else if (selection.IsRegionSelected() && selection.Region is RegionModel region)
        {
            return Report.GetUnitsInRegion(region)
                .Count(unit => Report.IsActiveFaction(unit.FactionId) &&
                               unit.HasEditableOrders &&
                               unit.OrdersConfirmed == false);
        }
        return -1; // no unit with order block
    }

    private void Reset()
    {
        Debug.WriteLine("[VM-ORDERS--] Reset");
        IsModified = false;
        Title = "Unit orders";
    }

    /// <summary>
    /// Copy unit orders text into document text.
    /// </summary>
    /// <param name="content">text to copy</param>
    private void SetOrders(string content)
    {
        Debug.WriteLine($"set orders to {content}...");
        _isLoadingOrders = true;
        try
        {
            Document.Text = content;
        }
        finally
        {
            _isLoadingOrders = false;
        }
    }

    private void OnEditableUnitSelected(IEntitySelection selection, SelectionChangeTypes selType)
    {
        bool unitSelected = selection.HasUnit();

        string orders = "";
        string unitName = "";
        bool editable = false;
        bool isConfirmed = false;
        InventoryItems.Clear();

        if (!HasDocument)
            return;

        if (!unitSelected)
        {
            _activeEditableUnitId = null;
            CanEditOrders = false;
            SetDisplayedConfirmationStatus(false);
            CurrentUnitId = string.Empty;
            Title = "Unit orders";
            SetOrders(string.Empty);
            UpdateContextData(selection);
            SelectPreviousUnitCommand.NotifyCanExecuteChanged();
            SelectNextUnitCommand.NotifyCanExecuteChanged();
            ToggleConfirmedStatusCommand.NotifyCanExecuteChanged();
            return;
        }

        UnitModel? unitModel = selection.Item as UnitModel;
        if (unitModel is null)
            return;


        // Only a selected unit is handled for orders
        unitName = unitModel.DisplayLabel;
        CurrentUnitId = unitModel.StringId;
        if (unitModel.TryGetCommands(out IReadOnlyList<string> unitCommands))
        {
            if (Report.TryGetEditedCommands(unitModel.Id, out IReadOnlyList<string> editedCommands))
            {
                orders = string.Join(Environment.NewLine, editedCommands);
                if (editedCommands.Count > 0)
                {
                    orders += Environment.NewLine;
                }
            }
            else
            {
                orders = LoadCommands(unitCommands);
            }
            editable = true;
            isConfirmed = unitModel.OrdersConfirmed ??
                throw new InvalidOperationException($"Unit {unitModel.Id} has no editable orders confirmation status.");
            _activeEditableUnitId = unitModel.Id;
        }
        else
        {
            _activeEditableUnitId = null;
        }

        // Collecter les items
        List<DataProperty> itemsProperties = [];
        if (unitModel.PrepareAndCollectItems(ref itemsProperties))
        {
            foreach (var prop in itemsProperties)
            {
                // 'prop.Label' contient le nom localisé, 'prop.Value' la quantité
                int quantity = int.TryParse(prop.Value, out int q) ? q : 0;
                InventoryItems.Add(new UnitItemViewModel(prop.Label, quantity, "Item"));
            }
        }


        // TODO: when set from false to true, IsEnabled and IsReadonly should be set
        CanEditOrders = editable;
        SetDisplayedConfirmationStatus(isConfirmed);
        Title = string.IsNullOrEmpty(unitName) ? "Unit orders" : $"Unit orders for {unitName}";
        SetOrders(orders);

        // To enable the commands if necessary
        SelectPreviousUnitCommand.NotifyCanExecuteChanged();
        SelectNextUnitCommand.NotifyCanExecuteChanged();
        ToggleConfirmedStatusCommand.NotifyCanExecuteChanged();

        // mettre à jour les données de contexte utilisées par les OrderDefinition.DynamicProvider (completion)
        UpdateContextData(selection);
        // Notify that the orders content has changed, to update completion proposals if necessary
    }

    private void UpdateContextData(IEntitySelection sel)
    {
        // Context data are the units of the region of the selected unit (or of the selected region).
        // They are only recomputed when the region changes: navigating from a unit to another unit of
        // the same region does not change the context.
        RegionModel? region = sel.Item switch
        {
            UnitModel unit => Report.FindRegionById(unit.LocationId),
            RegionModel selectedRegion when sel.IsRegionSelected() => selectedRegion,
            _ => sel.Region
        };

        // compute the same region key used for indexing in CRDocument
        int regionKey = region?.Id ?? -1;

        if (regionKey == _contextRegionKey)
        {
            return;
        }

        _contextRegionKey = regionKey;
        UnitsInRegion.Clear();

        if (region is null)
        {
            return;
        }

        // If the region contains many units, consider creating only a page/virtualized subset here.
        foreach (UnitModel unit in Report.GetUnitsInRegion(region))
        {
            UnitsInRegion.Add(UnitViewModel.FromDocument(Report, unit.Id/*, x, y*/));
        }
    }

    public void UpdateAvailableThemes(bool isDarkMode)
    {
        var availableThemes = _allThemes
            .Where(theme => IsDarkTheme(theme.ThemeName) == isDarkMode)
            .ToArray();
        var defaultThemeName = isDarkMode ? ExtendedThemeName.DarkPlus : ExtendedThemeName.LightPlus;
        var defaultTheme = availableThemes.Single(theme => theme.ThemeName == defaultThemeName);

        foreach (var theme in availableThemes)
        {
            if (!AvailableThemes.Contains(theme))
            {
                AvailableThemes.Add(theme);
            }
        }

        if (!availableThemes.Any(theme => theme.ThemeName == SelectedTheme.ThemeName))
        {
            SelectedTheme = defaultTheme;
        }

        for (int i = AvailableThemes.Count - 1; i >= 0; i--)
        {
            if (!availableThemes.Contains(AvailableThemes[i]))
            {
                AvailableThemes.RemoveAt(i);
            }
        }

        for (int i = 0; i < availableThemes.Length; i++)
        {
            int currentIndex = AvailableThemes.IndexOf(availableThemes[i]);
            if (currentIndex != i)
            {
                AvailableThemes.Move(currentIndex, i);
            }
        }
    }

    private static bool IsDarkTheme(ExtendedThemeName themeName) => themeName switch
    {
        ExtendedThemeName.Abbys or
        ExtendedThemeName.Dark or
        ExtendedThemeName.DarkPlus or
        ExtendedThemeName.DimmedMonokai or
        ExtendedThemeName.KimbieDark or
        ExtendedThemeName.OneDark or
        ExtendedThemeName.Monokai or
        ExtendedThemeName.Red or
        ExtendedThemeName.SolarizedDark or
        ExtendedThemeName.TomorrowNightBlue or
        ExtendedThemeName.HighContrastDark or
        ExtendedThemeName.Dracula or
        ExtendedThemeName.AtomOneDark or
        ExtendedThemeName.VisualStudioDark => true,

        ExtendedThemeName.Light or
        ExtendedThemeName.LightPlus or
        ExtendedThemeName.QuietLight or
        ExtendedThemeName.SolarizedLight or
        ExtendedThemeName.HighContrastLight or
        ExtendedThemeName.AtomOneLight or
        ExtendedThemeName.VisualStudioLight or
        ExtendedThemeName.MagellanLight => false,

        _ => throw new ArgumentOutOfRangeException(nameof(themeName), themeName, "No light/dark category is defined for this theme.")
    };
}


