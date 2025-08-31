using Avalonia.Controls;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using static Odyssey.Models.Documents.CRDocument;
using static Odyssey.Utils.Converters;

namespace Odyssey.ViewModels.Tools;

public partial class UnitOrdersViewModel : DocumentToolViewModelBase
{
    private static int FreeTempUnitNumber { get; set; } = 1;

    private bool IsWrapModeEnabled { get; set; }

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private bool _isConfirmed;

    [ObservableProperty]
    private string? _makeTempUnitSnippet;

    [ObservableProperty]
    private bool _canEditOrders;

    public TextDocument Document { get; set; }

    public UnitOrdersViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }
    public UnitOrdersViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
        // when enabled, next/previous unit commands traverse the explorer view in circular mode, i.e.
        // the previous of the first editable unit is the last matching one, the next of the last editable unit is the first matching one
        IsWrapModeEnabled = true;
        Document = new TextDocument { Text = "" };
        CanEditOrders = false;
        Reset();
    }

    /// <summary>
    /// Automatically sent by the ObservableObject when the property IsConfirmed has changed.
    /// </summary>
    /// <param name="oldValue">old value.</param>
    /// <param name="newValue">new value</param>
    partial void OnIsConfirmedChanged(bool oldValue, bool newValue)
    {
        ISelection sel = Selection;
        if (!HasDocument || sel == null || !sel.IsUnitSelected())
        {
            return;
        }

        // TODO: manage ItemTypes.CONFIRMATION
        // Selection.Selected |= ItemTypes.CONFIRMATION;
        DataBlock unit = Selection!.Item!;
        Report.SetConfirmed(ref unit!, newValue);
        NotifyUnitConfirmedStatusHasChanged();
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Handle event only if it comes from Explorer view
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        HandleSelection(selectionChange.Selection);
    }

    protected override int OnReportChange(ISelection selection)
    {
        HandleSelection(selection);
        return 1;
    }

    /// <summary>
    /// Links the report document to the view model.
    /// </summary>
    /// <param name="cr">the report document to link</param>
    /// <returns>true if a new report document has been linked to the view model; otherwise false</returns>
    protected override bool SetMapFile(CRDocument cr)
    {
        if (!IsSameDocument(cr) && IsModified)
        {
            // LATER: what about if orders were modified (save file) ?
        }

        return base.SetMapFile(cr);
    }

    /// <summary>
    /// Make TEMP UNIT requested.
    /// </summary>
    public void OnMakeTempUnitRequested()
    {
        MakeTempUnitSnippet = CreateMakeTempUnitOrder();
        // TODO
        // PublishMessageEvent(MessageType type, MessageId id, ref readonly object? data)
        // Send a command (notify event ?) to append the "snippet" in text editor area
    }

    private void HandleSelection(ISelection sel)
    {
        if (sel.IsUnitSelected() && IsSelected(sel))
        {
            // if currently selected unit is the specified selection unit, nothing is done
            return;
        }

        // Handle all kind of selection :
        // - unit selection => update orders view with unit orders and update confirmed status
        // - other selection => empty orders view content and disable commands related to orders edition
        SetSelection(sel);
        OnEditableUnitSelected();
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
        DataBlock? start = null;
        if (Selection.IsUnitSelected())
        {
            start = Selection.Item;
        }
        else if (Selection.IsRegionSelected())
        {
            start = Selection.Region;
        }
        if (start == null)
        {
            return;
        }

        bool wrap = IsWrapModeEnabled;
        DataBlock? unit = null;
        DataBlock? firstBlock = Report.FirstBlock!.Value;
        DataBlock? lastBlock = Report.LastBlock!.Value;
        DataBlock? startBlock = start.GetPreviousBlock();
        DataBlock? block = startBlock;
        while (wrap || block != firstBlock)
        {
            if (block == firstBlock)
            {
                // first block reacht
                if (!wrap)
                {
                    break;
                }
                wrap = false;
                block = lastBlock;
            }
            if (block == start)
            {
                // There is only one unit to edit
                break;
            }
            if (block.GetBlockType() != BlockType.UNIT)
            {
                block = block.GetPreviousBlock();
                continue;
            }
            // search command block
            if (!Report.IsConfirmed(block))
            {
                unit = block;
                break;
            }
            block = block.GetPreviousBlock();
        }

        if (unit != null)
        {
            OnEditableUnitSelected();
            ISelection? sel = new SimpleItemSelection(unit, null, null);
            PublishSelectionChangedEvent(new SelectionChange(sel, this, null));
        }
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
        DataBlock? start = null;
        if (Selection.IsUnitSelected())
        {
            start = Selection.Item;
        }
        else if (Selection.IsRegionSelected())
        {
            start = Selection.Item;
        }
        if (start == null)
        {
            return;
        }
        bool wrap = IsWrapModeEnabled;
        DataBlock? unit = null;
        DataBlock firstBlock = Report.FirstBlock?.Value!;
        DataBlock? startBlock = start.GetNextBlock();
        for (DataBlock? block = startBlock; wrap || block != null; block = block.GetNextBlock())
        {
            if (block == null)
            {
                // last block reacht
                if (!wrap)
                {
                    break;
                }
                wrap = false;
                block = firstBlock;
            }

            if (block == start)
            {
                // There is only one unit to edit
                break;
            }

            if (block.GetBlockType() != BlockType.UNIT)
            {
                continue;
            }
            // search command block
            if (!Report.IsConfirmed(block))
            {
                unit = block;
                break;
            }
        }

        if (unit != null)
        {
            OnEditableUnitSelected();
            ISelection? sel = new SimpleItemSelection(unit, null, null);
            PublishSelectionChangedEvent(new SelectionChange(sel, this, null));
        }
    }

    private bool CanToggleConfirmedStatus()
    {
        // TODO: manage other cases
        return Report.HasData();
    }
    private bool CanSelectPreviousUnit()
    {
        return Report.HasData();
    }
    private bool CanSelectNextUnit()
    {
        return Report.HasData();
    }
    private void NotifyUnitConfirmedStatusHasChanged()
    {
        SetSelection(Selection);
    }

    /// <summary>
    /// Load unit orders lines, and returns them as a concatenated string.
    /// </summary>
    /// <param name="commandBlock">DataBlock embedding the orders</param>
    /// <returns>All the orders lines as a concatenated string.</returns>
    private static string LoadCommands(in DataBlock commandBlock)
    {
        Debug.WriteLine("[VM-ORDERS--] Load Commands -> BEGIN");
        StringBuilder textBuilder = new();
        OrdersAttachment? cmds = commandBlock.GetAttachment() as OrdersAttachment;
        if (cmds != null)
        {
            // TODO: hide original commands
            //hideKeys = true;
            foreach (var cmd in cmds.Commands)
            {
                textBuilder.Append(cmd);
                textBuilder.Append(Environment.NewLine);
            }
        }
        else
        {
            foreach (var data in commandBlock.GetData())
            {
                textBuilder.Append(data.GetValue());
                textBuilder.Append(Environment.NewLine);
            }
        }
        Debug.WriteLine("[VM-ORDERS--] Load Commands -> ENDED");
        return textBuilder.ToString();
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

        if (Selection.IsUnitSelected())
        {
            DataBlock block = Selection.Item!;
            if (block.ValueInt(KeyType.FACTION) == Report.GetActiveFactionId())
            {
                return block.ValueInt(KeyType.ORDERS_CONFIRMED);
            }
            return -1;
        }
        else if (Selection.IsRegionSelected())
        {
            DataBlock block = Selection.Item!;
            return block.GetAttachment() is RegionAttachment stats ? stats.Unconfirmed : -1;
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
        Document.Text = content;
    }

    private void OnEditableUnitSelected()
    {
        string orders = "";
        string unitName = "";
        bool editable = false;
        bool isConfirmed = false;
        if (HasDocument && Selection.IsUnitSelected())
        {
            // Only a selected unit is handled for orders
            DataBlock unit = Selection.Item!;
            unitName = unit.GetUILabel();
            DataBlock cmd = new();
            if (GetKnownCommands(ref cmd!, unit!))
            {
                orders = LoadCommands(cmd!);
                editable = true;
                isConfirmed  = Report.IsConfirmed(unit);
            }
        }
        CanEditOrders = editable;
        IsConfirmed = isConfirmed;
        Title = string.IsNullOrEmpty(unitName) ? "Unit orders" : $"Unit orders for {unitName}";

        SetOrders(orders);

        // To enable the commands if necessary
        SelectPreviousUnitCommand.NotifyCanExecuteChanged();
        SelectNextUnitCommand.NotifyCanExecuteChanged();
        ToggleConfirmedStatusCommand.NotifyCanExecuteChanged();
    }
}
