using System;
using System.Collections.Generic;
using System.Diagnostics;
using Prism.Events;
using Avalonia.Controls;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Represents a view model for report information, extending the <see cref="MessagesListViewModel"/>.
/// Report information are related to the message-type blocks within a CRDocument.
/// </summary>
/// <remarks>This class is designed to handle report-related data and operations within a view model context. It
/// provides functionality to manage and rebuild a tree structure based on document changes.</remarks>
public partial class ReportInfoViewModel : MessagesListViewModel
{
    public ReportInfoViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }
    public ReportInfoViewModel(IEventAggregator? eventAggregator) : base(eventAggregator)
    {
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // This view does not handle any selection change.
    }

    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        if (SetMapFile(cr))
        {
            CollectDataFromReport();
        }
    }

    protected override void OnActiveDocumentClosed(CRDocument cr)
    {
        base.Clear();
        // reset to an empty report document
        SetMapFile(new CRDocument());
    }

    /// <summary>
    /// Collect the messages from the current document.
    /// Battle messages are excluded from the collected data.
    /// </summary>
    protected void CollectDataFromReport()
    {
        Debug.WriteLine("[VM-REPORT--] Rebuild tree...");
        Clear();
        int factionsMessagesNumber = 0;
        CRDocument cr = GetDocument();
        DataBlock? startBlock = cr.FirstBlock?.Value;
        for (DataBlock? block = startBlock; block != null; block = block.GetNextBlock())
        {
            BlockType type = block.GetBlockType();
            if (type == BlockType.REGION)
            {
                break;
            }
            else if (type == BlockType.FACTION)
            {
                // LATER: handle faction messages
                factionsMessagesNumber++;
            }
            else if (type == BlockType.MESSAGE)
            {
                // Exclude battle messages 'having Depth to 4) from the collected data.
                // Battle messages are handled in a specific dedicated view model.
                if (block.GetDepth() == 3)
                {
                    List<DataBlock> targets = Report.GetMessageTargets(block);
                    Items.Add(new MessageEntry(block.Value(Strings.EN_MESSAGE_RENDERED), targets));
                }
            }
        }
        Debug.WriteLine($"[VM-REPORT--] RebuildTree nbmsg[{Items.Count}] nbfactionsmsg[{factionsMessagesNumber}]");
    }
}
