using System.Text;
using Odyssey.Models.Dal;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.ViewModels.Documents;
using Xunit;

namespace Odyssey.Desktop.Tests;

public class DocumentModifiedTests
{
    [Fact]
    public void EditedCommandsMarkFileViewModelModified()
    {
        CRDocument report = CreateReport();
        FileViewModel file = new() { Document = report };

        report.SetEditedCommands(1, ["MOVE 1"]);

        Assert.True(file.IsModified);
        Assert.True(file.IsDocumentModified());
        report.SetEditedCommands(1, ["WORK"]);
        Assert.False(file.IsDocumentModified());
        Assert.False(report.HasEditedOrderData);
    }

    [Fact]
    public void RevertingOneUnitPreservesOtherPendingChanges()
    {
        CRDocument report = CreateReport();
        FileViewModel file = new() { Document = report };
        report.SetEditedCommands(1, ["MOVE 1"]);
        report.SetEditedCommands(2, ["MOVE 2"]);
        report.SetEditedUnitConfirmationStatus(1, false);

        report.SetEditedCommands(1, ["WORK"]);
        Assert.True(file.IsDocumentModified());
        Assert.False(report.TryGetEditedCommands(1, out _));
        Assert.True(report.TryGetEditedCommands(2, out _));
        report.SetEditedCommands(2, ["WORK"]);
        Assert.True(file.IsDocumentModified());
        report.SetEditedUnitConfirmationStatus(1, true);
        Assert.False(file.IsDocumentModified());
    }

    [Fact]
    public void CommittedCommandsBecomeBaselineAndSurviveLaterSave()
    {
        CRDocument report = CreateReport();
        FileViewModel file = new() { Document = report };
        string path = Path.GetTempFileName();
        try
        {
            report.SetEditedCommands(1, ["MOVE 1"]);
            Assert.True(ReportFileService.SaveWithOrderEdits(path, report, MapType.FULL, []));
            report.CommitEditedOrderChanges();
            report.SetModified(false);

            report.SetEditedCommands(1, ["MOVE 12"]);
            report.SetEditedCommands(1, ["MOVE 1"]);
            Assert.False(file.IsDocumentModified());
            Assert.Equal(new[] { "MOVE 1" }, report.Units[1].GetCommandLines());
            report.SetEditedCommands(2, ["MOVE 2"]);
            Assert.True(ReportFileService.SaveWithOrderEdits(path, report, MapType.FULL, []));
            Assert.True(ReportFileService.LoadFile(path, out EresseaDocument loaded, out string error), error);
            Assert.Equal(new[] { "MOVE 1" }, Assert.IsType<CRDocument>(loaded).Units[1].GetCommandLines());

            report.SetEditedCommands(1, []);
            Assert.True(ReportFileService.SaveWithOrderEdits(path, report, MapType.FULL, []));
            report.CommitEditedOrderChanges();
            report.SetModified(false);
            Assert.Empty(report.Units[1].GetCommandLines());
            report.SetEditedCommands(1, ["WORK"]);
            report.DiscardEditedOrderChanges();
            Assert.True(ReportFileService.SaveWithOrderEdits(path, report, MapType.FULL, []));
            Assert.True(ReportFileService.LoadFile(path, out loaded, out error), error);
            Assert.Empty(Assert.IsType<CRDocument>(loaded).Units[1].GetCommandLines());
        }
        finally
        {
            File.Delete(path);
        }
    }

    internal static CRDocument CreateReport()
    {
        const string text = "VERSION 1\nPARTEI 1\n1;Optionen\n1;Anzahl Personen\n" +
            "REGION 4 -2\n\"Ebene\";Terrain\n" +
            "EINHEIT 1\n1;Partei\n1;ejcOrdersConfirmed\nCOMMANDS\n\"WORK\"\n" +
            "EINHEIT 2\n1;Partei\n1;ejcOrdersConfirmed\nCOMMANDS\n\"WORK\"\n";
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(text));
        Assert.True(ReportFileService.LoadStream("report", stream, out EresseaDocument loaded, out string error), error);
        CRDocument report = Assert.IsType<CRDocument>(loaded);
        Assert.True(report.Units[1].HasEditableOrders);
        Assert.True(report.Units[2].HasEditableOrders);
        return report;
    }
}