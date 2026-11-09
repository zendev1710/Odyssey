using System.Text;
using Odyssey.Core.Services;
using Odyssey.Models.Dal;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.ViewModels.Tools;
using Xunit;

namespace Odyssey.Desktop.Tests;

public class PlaneTests
{
    [Theory]
    [InlineData("4 -2", 0)]
    [InlineData("4 -2 1", 1)]
    [InlineData("4 -2 1137", 1137)]
    public void RegionBlockUsesItsParsedPlane(string coordinates, int expectedPlane)
    {
        var block = new RegionDataBlock(coordinates);
        Assert.Equal(expectedPlane, block.GetPlane());
        Assert.Equal(expectedPlane, (int)((ILocation)block).Plane);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReportAndMapKeepRegionsOnTheirOwnPlanes(bool astralRegionIsLast)
    {
        const string world = "REGION 4 -2\n\"Ebene\";Terrain\n";
        const string astral = "REGION 4 -2 1\n\"Astralraum\";Terrain\n";
        string regions = astralRegionIsLast ? world + astral : astral + world;
        string text = "VERSION 1\nPARTEI 1\n1;Optionen\n1;Anzahl Personen\n" + regions;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        Assert.True(ReportFileService.LoadStream("planes", stream, out EresseaDocument loaded, out string error), error);
        var report = Assert.IsType<CRDocument>(loaded);
        Assert.True(report.FindRegion(4, -2, PlaneType.WORLD, out var worldRegion));
        Assert.True(report.FindRegion(4, -2, PlaneType.ASTRAL, out var astralRegion));
        Assert.NotNull(worldRegion);
        Assert.NotNull(astralRegion);
        Assert.Equal(0, worldRegion.GetPlane());
        Assert.Equal(1, astralRegion.GetPlane());
        Assert.NotEqual(worldRegion.Id, astralRegion.Id);
        Assert.Single(report.Planes[0].UnoccupiedRegions);
        Assert.Single(report.Planes[1].UnoccupiedRegions);

        ServiceLocator.Initialize();
        var map = new MapViewModel();
        ServiceLocator.DocumentService.SetActiveDocument(report);
        Assert.Same(report.Planes[0], map.SelectedPlane);
        Assert.Contains(map.UnoccupiedRegions, entry => ReferenceEquals(entry.Value, worldRegion));
        Assert.DoesNotContain(map.UnoccupiedRegions, entry => ReferenceEquals(entry.Value, astralRegion));
        map.SelectedPlane = report.Planes[1];
        Assert.Contains(map.UnoccupiedRegions, entry => ReferenceEquals(entry.Value, astralRegion));
        Assert.DoesNotContain(map.UnoccupiedRegions, entry => ReferenceEquals(entry.Value, worldRegion));
        ServiceLocator.EntitySelectionService.SelectItem(worldRegion, worldRegion, this);
        Assert.Same(report.Planes[0], map.SelectedPlane);
        Assert.Same(worldRegion, map.SelectedRegion);
        ServiceLocator.EntitySelectionService.SelectItem(astralRegion, astralRegion, this);
        Assert.Same(report.Planes[1], map.SelectedPlane);
        Assert.Same(astralRegion, map.SelectedRegion);
    }

    [Fact]
    public void ReportCanContainSpecialPlaneWithoutAstralPlane()
    {
        const string text = "VERSION 1\nPARTEI 1\n1;Optionen\n1;Anzahl Personen\nREGION 4 -2 1137\n\"Ebene\";Terrain\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        Assert.True(ReportFileService.LoadStream("special", stream, out EresseaDocument loaded, out string error), error);
        var report = Assert.IsType<CRDocument>(loaded);
        Assert.True(report.FindRegion(4, -2, PlaneType.ARENA, out var region));
        Assert.NotNull(region);
        Assert.Single(report.Planes[1137].UnoccupiedRegions);
        Assert.Empty(report.Planes[0].UnoccupiedRegions);
    }
}
