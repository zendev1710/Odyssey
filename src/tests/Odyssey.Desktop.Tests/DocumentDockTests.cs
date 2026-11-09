using Dock.Model.Controls;
using Dock.Model.Core;
using Odyssey.Core.Services;
using Odyssey.Models.Documents;
using Odyssey.ViewModels;
using Odyssey.ViewModels.Documents;
using Xunit;

namespace Odyssey.Desktop.Tests;

[Collection("Shared document services")]
public class DocumentDockTests
{
    [Fact]
    public void CreatedLayoutRegistersDocumentDockAndKeepsEditedFile()
    {
        ServiceLocator.Initialize();
        DockFactory factory = new();
        IRootDock layout = factory.CreateLayout();
        factory.InitLayout(layout);

        IDocumentDock? dock = factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId);
        Assert.NotNull(dock);
        var report = DocumentModifiedTests.CreateReport();
        var file = new FileViewModel { Id = "report", Document = report };
        factory.AddDockable(dock, file);
        factory.SetActiveDockable(file);
        report.SetEditedCommands(1, ["MOVE 1"]);

        Assert.Contains(file, dock.VisibleDockables!);
        Assert.Same(file, dock.ActiveDockable);
        Assert.True(file.IsDocumentModified());
    }

    [Fact]
    public void LegacyLayoutWithoutDocumentDockIdIsRestored()
    {
        ServiceLocator.Initialize();
        DockFactory factory = new();
        IRootDock layout = factory.CreateLayout();
        factory.InitLayout(layout);
        IDocumentDock dock = Assert.IsAssignableFrom<IDocumentDock>(factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId));
        dock.Id = string.Empty;

        factory.InitLayout(layout);

        Assert.Same(dock, factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId));
        Assert.Equal(DockFactory.DocumentDockId, dock.Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClosingModifiedFileRespectsConfirmation(bool approve)
    {
        ServiceLocator.Initialize();
        DockFactory factory = new();
        IRootDock layout = factory.CreateLayout();
        factory.InitLayout(layout);
        IDocumentDock dock = Assert.IsAssignableFrom<IDocumentDock>(factory.GetDockable<IDocumentDock>(DockFactory.DocumentDockId));
        CRDocument report = DocumentModifiedTests.CreateReport();
        TaskCompletionSource<bool> response = new();
        TaskCompletionSource<bool> finished = new();
        FileViewModel file = new() { Id = "report", Document = report };
        file.ConfirmCloseAsync = _ => response.Task;
        file.CloseApproved = closedFile =>
        {
            factory.CloseDockable(closedFile);
            finished.SetResult(true);
        };
        factory.AddDockable(dock, file);
        factory.SetActiveDockable(file);
        report.SetEditedCommands(1, ["MOVE 1"]);

        factory.CloseDockable(file);
        Assert.Contains(file, dock.VisibleDockables!);
        response.SetResult(approve);
        if (approve)
        {
            await finished.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.DoesNotContain(file, dock.VisibleDockables!);
        }
        else
        {
            Assert.Contains(file, dock.VisibleDockables!);
        }
    }
}
