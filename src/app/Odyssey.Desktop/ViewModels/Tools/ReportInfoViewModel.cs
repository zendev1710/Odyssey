using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System.Diagnostics;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Represents a view model for report information, extending the <see cref="MessagesListViewModel"/>.
/// Report information are related to the messages within a CRDocument.
/// </summary>
/// <remarks>This class is designed to handle report-related data and operations within a view model context. It
/// provides functionality to manage and rebuild a tree structure based on document changes.</remarks>
public partial class ReportInfoViewModel : MessagesListViewModel
{
    public ReportInfoViewModel() : base()
    {
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);

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
        foreach (var message in Report.GetReportMessages())
        {
            Items.Add(new MessageEntry(message.Text, message.Targets));
        }
        Debug.WriteLine($"[VM-REPORT--] CollectDataFromReport nbmsg[{Items.Count}]");
    }
}
