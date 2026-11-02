using System;
using Odyssey.Models.Documents;
using Odyssey.Models;
using Odyssey.Models.Tools;
using System.Collections.Generic;
using System.Linq;

namespace Odyssey.Core.Services;

public enum UnitOrderMode
{
    ReportOrder,
    BestSkillLevel,
    Name
}

public sealed record RegionUnitOrder(RegionModel Region, IReadOnlyList<UnitModel> Units);

public interface IUnitNavigationOrderService
{
    event EventHandler? OrderChanged;
    CRDocument? Report { get; }
    IReadOnlyList<RegionUnitOrder> OrderedRegions { get; }
    IReadOnlyList<UnitModel> OrderedUnits { get; }
    void SetOrder(CRDocument report, IEnumerable<RegionUnitOrder> regions);
}

public sealed class UnitNavigationOrderService : IUnitNavigationOrderService
{
    private IReadOnlyList<UnitModel> _orderedUnits = [];

    public event EventHandler? OrderChanged;
    public CRDocument? Report { get; private set; }
    public IReadOnlyList<RegionUnitOrder> OrderedRegions { get; private set; } = [];
    public IReadOnlyList<UnitModel> OrderedUnits => _orderedUnits;

    public void SetOrder(CRDocument report, IEnumerable<RegionUnitOrder> regions)
    {
        Report = report;
        OrderedRegions = regions
            .Select(region => new RegionUnitOrder(region.Region, region.Units.ToArray()))
            .ToArray();
        _orderedUnits = OrderedRegions.SelectMany(region => region.Units).ToArray();
        OrderChanged?.Invoke(this, EventArgs.Empty);
    }
}