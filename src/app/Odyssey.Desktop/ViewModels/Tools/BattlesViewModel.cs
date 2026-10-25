using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Odyssey.ViewModels.Tools;

public partial class BattlesViewModel : DocumentToolViewModelBase
{
    public int BattlesNumber { get { return Battles.Count; } }

    public ObservableCollection<BattleModel> Battles { get; } = [];

    public BattlesViewModel() : base()
    {
        Clear();
    }

    /// <summary>
    /// Handles changes to the selected battle index.
    /// </summary>
    /// <remarks>Updates the <see cref="SelectedBattleText"/> based on the new index. If <paramref
    /// name="newValue"/> is negative, <see cref="SelectedBattleText"/> is set to an empty string.</remarks>
    /// <param name="oldValue">The previous index of the selected battle.</param>
    /// <param name="newValue">The new index of the selected battle. Must be non-negative to select a valid battle.</param>
    partial void OnSelectedBattleIndexChanged(int oldValue, int newValue)
    {
        SelectedBattleText = newValue >= 0 ? Battles[newValue].Content : string.Empty;
    }

    /// <summary>
    /// Handles the event when the active document changes.
    /// </summary>
    /// <remarks>This method updates the internal state based on the new active document. If a new document is linked to this viewmodel,
    /// it triggers data collection.</remarks>
    /// <param name="cr">The new active <see cref="CRDocument"/> instance.</param>
    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

        Clear();
        CollectData();
    }

    protected override void OnSelectionChanged(IEntitySelection selection, SelectionChangeTypes selType, object? sender)
    {
        if (ReferenceEquals(sender, this)) return;

        bool regionSelected = selection.IsRegionSelected();
        bool regionChanged = selType.HasFlag(SelectionChangeTypes.Region);

        Debug.WriteLine($"[VM-BATTLES] OnSelectionChanged: region changed={regionChanged}, region selected={regionSelected}");

        if (regionChanged)
        {
            string label = selection.Region!.DataBlock.GetUILabel();
            if (Battles.FirstOrDefault(b => b.Label!.StartsWith(label)) is BattleModel battle)
            {
                SelectedBattleIndex = Battles.IndexOf(battle);
            }
        }
    }

    /// <summary>
    /// Collects and processes battle data from the report blocks.
    /// </summary>
    /// <remarks>This method iterates through the data blocks in the report to identify and process
    /// battle-related information. It updates the list of battles and sets the selected battle index if any battles are
    /// found.</remarks>
    protected void CollectData()
    {
        Clear();
        if (Report.Battles.Count > 0)
        {
            foreach (var item in Report.Battles.OrderBy(b => b.Label))
            {
                Battles.Add(item);
            }
            SelectedBattleIndex = 0;
        }

        HasBattle = BattlesNumber > 0;
        Debug.WriteLine($"Number of battles = {BattlesNumber}");
    }

    private void Clear()
    {
        // LATER: check about Selection that should be cleared also
        Battles.Clear();
        SelectedBattleIndex = -1;
        SelectedBattleText = string.Empty;
        HasBattle = false;
    }

    /// <summary>
    /// Select the region in the explorer for the currently selected battle.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRevealBattleInExplorer))]
    private void RevealBattleInExplorer()
    {
        if (SelectedBattleIndex >= 0)
        {
            BattleModel battle = Battles[SelectedBattleIndex];
            
            if (Report.ResolveLocation(battle.X, battle.Y, (PlaneType)0, out ILocation? location))
            {
                // new SimpleSelection(Report, battle.X, battle.Y, 0/*battle.GetPlane()*/)
                SendSelectionChangedEvent(new EntitySelection(location!));
            }
        }
    }

    /// <summary>
    /// Checks if the selected battle can be revealed in the explorer.
    /// Returns true if a battle is selected and it has an associated region.
    private bool CanRevealBattleInExplorer()
    {
        return SelectedBattleIndex >= 0;
    }

    /// <summary>
    /// Index of the selected battle.
    /// </summary>
    [ObservableProperty]
    private int _selectedBattleIndex;

    /// <summary>
    /// Text of the selected battle.
    /// </summary>
    [ObservableProperty]
    private string? _selectedBattleText;

    /// <summary>
    /// Whether there are battles available in the report document.
    /// </summary>
    [ObservableProperty]
    private bool _hasBattle;
}
