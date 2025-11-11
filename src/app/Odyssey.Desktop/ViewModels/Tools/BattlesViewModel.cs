using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System.Collections.Generic;
using System.Diagnostics;
using Prism.Events;
using Avalonia.Controls;
using System;
using System.Collections.ObjectModel;
using Odyssey.Models.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Models.Documents.SimpleItemSelection;
using Odyssey.Models;

namespace Odyssey.ViewModels.Tools;

public partial class BattlesViewModel : DocumentToolViewModelBase
{
    public int BattlesNumber { get { return Battles.Count; } }

    public ObservableCollection<BattleModel> Battles { get; } = [];

    public BattlesViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }

    public BattlesViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
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
    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            CollectData();
        }
    }

    protected override void OnActiveDocumentClosed(CRDocument cr)
    {
        Clear();
        // reset to an empty report document
        SetMapFile(new CRDocument());
    }

    /// <summary>
    /// Select battle item if a region with a battle has been selected.
    /// </summary>
    /// <param name="selectionChange">the viewmodel which has publish the SelectionStateChanged event</param>
    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Exclude this selector from selection change events to avoid reentrancy issues
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.MainWindowVmId];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        ISelection? sel = selectionChange.Selection;
        if (sel != null && sel.HasRegion() && !IsSelected(sel))
        {
            SetSelection(sel);
            string label = sel.Region!.GetUILabel();
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
        // LATER: maybe should use CRDocument.Battles property instead of collecting data here by iterating through all the blocks.
        List<BattleModel> battles = [];
        DataBlock? startBlock = Report.FirstBlock?.Value;
        string currentFactionName = string.Empty;

        // Iterate through the blocks to find battles
        for (var block = startBlock; block != null; block = block.GetNextBlock())
        {
            BlockType type = block.GetBlockType();
            if (type == BlockType.REGION)
            {
                break;
            }
            else if (type == BlockType.FACTION)
            {
                // TODO: is there a better way to get the faction name?
                FactionModel? factionModel = Report.GetFaction(block.GetId());
                currentFactionName = factionModel is not null ? factionModel.Name : string.Empty;
                //currentFactionName = GetFactionName(block);
            }
            else if (type == BlockType.BATTLE)
            {
                BattleModel battleModel = new(Report, currentFactionName);
                if (battleModel.CollectData(block))
                {
                    battles.Add(battleModel);
                }
            }
        }
        if (battles.Count > 0)
        {
            Battles.AddRange(battles.OrderBy(b => b.Label));
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
            DataBlock? region = Battles[SelectedBattleIndex].Region;
            ISelection sel = new SimpleItemSelection(region, region, null);
            SetSelection(sel);
            SendSelectionChangedEvent(sel);
        }
    }

    /// <summary>
    /// Checks if the selected battle can be revealed in the explorer.
    /// Returns true if a battle is selected and it has an associated region.
    private bool CanRevealBattleInExplorer()
    {
        return (SelectedBattleIndex >= 0 && Battles[SelectedBattleIndex].Region != null);
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
