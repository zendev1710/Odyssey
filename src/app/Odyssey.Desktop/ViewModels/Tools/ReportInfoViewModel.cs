using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using Odyssey.Services;
using Prism.Events;
using System.Collections.Generic;
using System.Diagnostics;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Represents a view model for report information, extending the <see cref="MessagesListViewModel"/>.
/// Report information are related to the message-type blocks within a CRDocument.
/// </summary>
/// <remarks>This class is designed to handle report-related data and operations within a view model context. It
/// provides functionality to manage and rebuild a tree structure based on document changes.</remarks>
public partial class ReportInfoViewModel : MessagesListViewModel
{
    public ReportInfoViewModel(IEventAggregator? eventAggregator = null, IActiveDocumentService? documentService = null) : base(eventAggregator, documentService)
    {
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // This view does not handle any selection change.
    }

    protected override void OnActiveDocumentChanged(CRDocument cr)
    {
        base.OnActiveDocumentChanged(cr);
        CollectDataFromReport();
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
        DataBlock? startBlock = Report.FirstBlock?.Value;
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
