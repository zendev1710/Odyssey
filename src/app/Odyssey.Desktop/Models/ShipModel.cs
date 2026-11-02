using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.Models;

public class ShipModel: StructureModel
{
    public int Coast { get; private set; } = -1;

    public int DamagePercent { get; private set; }

    public int Cargo { get; private set; }

    public int Capacity { get; private set; }

    /// <summary>Number of ships in the fleet ("Anzahl"), at least one for older reports.</summary>
    public int Number { get; private set; } = 1;

    public int TotalSkill { get; private set; }

    public RegionModel? FromRegion { get; private set; }

    public RegionModel? ToRegion { get; private set; }

    /// <summary>Region where the ship is currently located (its destination for the current turn).</summary>
    public RegionModel? CurrentRegion { get; private set; }

    /// <summary>Faction owning the ship, resolved from its captain/owner unit when the report is parsed.</summary>
    public int OwnerFactionId { get; private set; } = -1;

    public List<ILocation> TravelledByRegions { get; private set; } = [];

    public Dictionary<int, RegionModel> CrossedRegions { get; private set; } = [];

    // neighbour offsets for pointy-topped axial coords (q,r)
    private static readonly (int Dq, int Dr)[] NeighborOffsets =
    {
        (0, 1), (0, -1), (1, 0), (-1, 0), (1, -1), (-1, 1),
    };

    public ShipModel(DataBlock shipDataBlock, int locationId) : base(shipDataBlock, Categories.Ship, locationId)
    {
    }

    public void AddCrossedRegions(IEnumerable<RegionModel> regions)
    {
        bool hasRegions = false;
        foreach (RegionModel region in regions)
        {
            hasRegions = true;
            int key = GetNodeKey(region);
            if (!CrossedRegions.ContainsKey(key))
            {
                CrossedRegions[key] = region;
            }
        }

        if (!hasRegions)
        {
            Debug.WriteLine($"AddCrossedRegions without any region for {this} !");
        }
    }

    public void SetCurrentRegion(RegionModel? region)
    {
        CurrentRegion = region;
    }

    public void SetOwnerFactionId(int factionId)
    {
        OwnerFactionId = factionId;
    }

    public int GetOwnerUnitId()
    {
        int ownerUnitId = Structure.ValueInt(KeyType.OWNER, -1);
        if (ownerUnitId <= 0)
        {
            ownerUnitId = Structure.ValueInt(KeyType.CAPTAIN, -1);
        }
        return ownerUnitId;
    }

    public bool TryResolveOwnerUnit(CRDocument report, out UnitModel? ownerUnit)
    {
        int ownerUnitId = GetOwnerUnitId();
        if (ownerUnitId > 0 && report.FindUnit(ownerUnitId, out ownerUnit) && ownerUnit is not null)
        {
            SetOwnerUnit(ownerUnit);
            ownerUnit.SetCommandedShip(this);
            return true;
        }

        ownerUnit = null;
        SetOwnerUnit(null);
        return false;
    }

    /// <summary>
    /// Resolves the faction owning the ship from its captain/owner unit, with the ship faction key as fallback.
    /// </summary>
    public void ResolveOwnerFaction(CRDocument report)
    {
        if (TryResolveOwnerUnit(report, out UnitModel? ownerUnit) && ownerUnit is not null)
        {
            OwnerFactionId = ownerUnit.EffectiveFactionId;
            return;
        }

        OwnerFactionId = Structure.ValueInt(KeyType.FACTION, -1);
    }

    /// <summary>
    /// Resolves the route from a SAIL message. The crossed regions recorded in the report are used as
    /// the allowed intermediate regions, and a shortest path is searched between the two endpoints.
    /// </summary>
    public void SetFromTo(RegionModel? fromRegion, RegionModel? toRegion)
    {
        FromRegion = fromRegion;
        ToRegion = toRegion;
        TravelledByRegions.Clear();

        if (fromRegion is null || toRegion is null)
        {
            Debug.WriteLine($"FromRegion or ToRegion is null for {this} !");
            return;
        }

        TravelledByRegions.AddRange(BuildRoute(fromRegion, toRegion, CrossedRegions.Values));
        CompleteRoute(TravelledByRegions);
    }

    /// <summary>
    /// Builds a route for ships that have no SAIL message (allied/enemy ships in particular).
    /// The crossed regions form the route, and the ship's current region is used as the destination.
    /// </summary>
    public void ResolveInferredRoute()
    {
        if (TravelledByRegions.Count >= 2)
        {
            return;
        }

        if (CurrentRegion is null)
        {
            return;
        }

        int currentKey = GetNodeKey(CurrentRegion);
        var candidates = CrossedRegions.Values
            .Where(r => GetNodeKey(r) != currentKey)
            .GroupBy(GetNodeKey)
            .Select(g => g.First())
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        // The departure is the crossed region farthest from the current position. This ignores
        // small branches and gives the most plausible start of the travelled chain.
        RegionModel start = candidates
            .OrderByDescending(r => HexDistance(r, CurrentRegion))
            .First();

        var allowed = new Dictionary<int, ILocation>();
        foreach (ILocation candidate in candidates)
        {
            allowed[GetNodeKey(candidate)] = candidate;
        }
        allowed[currentKey] = CurrentRegion;

        var path = FindPath(start, CurrentRegion, allowed);
        if (path is null || path.Count < 2)
        {
            path = BuildFallbackRoute(start, CurrentRegion, candidates);
        }

        if (path.Count < 2)
        {
            return;
        }

        FromRegion = start;
        ToRegion = CurrentRegion;
        TravelledByRegions.AddRange(path);
        CompleteRoute(TravelledByRegions);
    }

    private static List<ILocation> BuildRoute(
        ILocation from,
        ILocation to,
        IEnumerable<ILocation> crossedRegions)
    {
        var nodes = new Dictionary<int, ILocation>();
        AddNode(nodes, from);
        AddNode(nodes, to);

        foreach (ILocation region in crossedRegions)
        {
            AddNode(nodes, region);
        }

        var path = FindPath(from, to, nodes);
        if (path is not null && path.Count > 0)
        {
            return path;
        }

        return BuildFallbackRoute(from, to, nodes.Values);
    }

    private static void AddNode(Dictionary<int, ILocation> nodes, ILocation region)
    {
        nodes.TryAdd(GetNodeKey(region), region);
    }

    /// <summary>
    /// Breadth-first search restricted to the regions that were actually crossed by the ship.
    /// </summary>
    private static List<ILocation>? FindPath(
        ILocation from,
        ILocation to,
        IReadOnlyDictionary<int, ILocation> nodes)
    {
        int startKey = GetNodeKey(from);
        int targetKey = GetNodeKey(to);

        if (startKey == targetKey)
        {
            return [from];
        }

        if (!nodes.TryGetValue(startKey, out ILocation? startNode) ||
            !nodes.TryGetValue(targetKey, out ILocation? targetNode))
        {
            return null;
        }

        var previous = new Dictionary<int, int>();
        var visited = new HashSet<int> { startKey };
        var queue = new Queue<int>();
        queue.Enqueue(startKey);

        while (queue.Count > 0)
        {
            int key = queue.Dequeue();
            ILocation node = nodes[key];
            int plane = (int)node.Plane;
            int x = node.X;
            int y = node.Y;

            foreach ((int dq, int dr) in NeighborOffsets)
            {
                int nextKey = Coordinates.GetId(x + dq, y + dr, (PlaneType)plane);
                if (!nodes.TryGetValue(nextKey, out ILocation? next) || !visited.Add(nextKey))
                {
                    continue;
                }

                previous[nextKey] = key;

                if (nextKey == targetKey)
                {
                    return ReconstructPath(nodes, previous, startKey, targetKey);
                }

                queue.Enqueue(nextKey);
            }
        }

        // Keep the target node referenced so its existence is validated even when no path is found.
        _ = targetNode;
        return null;
    }

    private static List<ILocation> ReconstructPath(
        IReadOnlyDictionary<int, ILocation> nodes,
        IReadOnlyDictionary<int, int> previous,
        int startKey,
        int targetKey)
    {
        var path = new List<ILocation>();
        int currentKey = targetKey;

        while (true)
        {
            path.Add(nodes[currentKey]);

            if (currentKey == startKey)
            {
                break;
            }

            if (!previous.TryGetValue(currentKey, out int previousKey))
            {
                return [];
            }

            currentKey = previousKey;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Greedy fallback used when the crossed regions do not form a complete path. Only regions
    /// compatible with the direct route are kept, and the chain is forced to make progress towards
    /// the destination. Missing intermediate hexes are filled later by <see cref="CompleteRoute"/>.
    /// </summary>
    private static List<ILocation> BuildFallbackRoute(
        ILocation from,
        ILocation to,
        IEnumerable<ILocation> crossedRegions)
    {
        const int MaxDetour = 3;

        int directDistance = HexDistance(from, to);
        var remaining = crossedRegions
            .Where(r => GetNodeKey(r) != GetNodeKey(from) && GetNodeKey(r) != GetNodeKey(to))
            .Where(r => r.Plane == from.Plane && r.Plane == to.Plane)
            .Where(r => HexDistance(from, r) + HexDistance(r, to) <= directDistance + MaxDetour)
            .GroupBy(GetNodeKey)
            .Select(g => g.First())
            .ToList();

        var result = new List<ILocation> { from };
        ILocation current = from;
        int progress = 0;

        while (remaining.Count > 0)
        {
            int currentRemaining = HexDistance(current, to);
            ILocation? next = remaining
                .Where(r => HexDistance(from, r) >= progress)
                .Where(r => IsNeighbour(current, r) || HexDistance(r, to) <= currentRemaining)
                .OrderBy(r => IsNeighbour(current, r) ? 0 : 1)
                .ThenBy(r => HexDistance(current, r) + HexDistance(r, to))
                .ThenBy(r => HexDistance(current, r))
                .ThenBy(r => r.X)
                .ThenBy(r => r.Y)
                .FirstOrDefault();

            if (next is null)
            {
                break;
            }

            result.Add(next);
            remaining.Remove(next);
            current = next;
            progress = Math.Max(progress, HexDistance(from, next));
        }

        if (GetNodeKey(current) != GetNodeKey(to))
        {
            result.Add(to);
        }

        return result;
    }

    /// <summary>
    /// Inserts synthetic regions between non-adjacent route points. Crossed-region reports are not
    /// always complete, so this keeps the rendered curve on the hex grid instead of jumping across
    /// several regions in a straight line.
    /// </summary>
    private static void CompleteRoute(List<ILocation> route)
    {
        if (route.Count < 2)
        {
            return;
        }

        var completed = new List<ILocation>(route.Count * 2) { route[0] };
        for (int i = 1; i < route.Count; i++)
        {
            ILocation previous = route[i - 1];
            ILocation next = route[i];
            if (previous.Plane == next.Plane)
            {
                foreach ((int x, int y) in EnumerateIntermediateHexes(previous, next))
                {
                    completed.Add(CreateRouteRegion(x, y, (int)previous.Plane));
                }
            }

            completed.Add(route[i]);
        }

        route.Clear();
        route.AddRange(completed);
    }

    private static IEnumerable<(int X, int Y)> EnumerateIntermediateHexes(ILocation from, ILocation to)
    {
        int ax = from.X;
        int az = from.Y;
        int ay = -ax - az;

        int bx = to.X;
        int bz = to.Y;
        int by = -bx - bz;

        int distance = Math.Max(Math.Abs(ax - bx), Math.Max(Math.Abs(ay - by), Math.Abs(az - bz)));
        for (int step = 1; step < distance; step++)
        {
            double t = (double)step / distance;
            double x = ax + (bx - ax) * t;
            double y = ay + (by - ay) * t;
            double z = az + (bz - az) * t;

            int rx = (int)Math.Round(x);
            int ry = (int)Math.Round(y);
            int rz = (int)Math.Round(z);

            double dx = Math.Abs(rx - x);
            double dy = Math.Abs(ry - y);
            double dz = Math.Abs(rz - z);

            if (dx > dy && dx > dz)
            {
                rx = -ry - rz;
            }
            else if (dy > dz)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            yield return (rx, rz);
        }
    }

    private static ILocation CreateRouteRegion(int x, int y, int plane)
    {
        return new RouteLocation(x, y, (PlaneType)plane);
    }

    private static bool IsNeighbour(ILocation a, ILocation b)
    {
        if (a.Plane != b.Plane)
        {
            return false;
        }

        int dx = b.X - a.X;
        int dy = b.Y - a.Y;
        foreach ((int dq, int dr) in NeighborOffsets)
        {
            if (dx == dq && dy == dr)
            {
                return true;
            }
        }

        return false;
    }

    private static int HexDistance(ILocation a, ILocation b)
    {
        int ax = a.X;
        int az = a.Y;
        int ay = -ax - az;

        int bx = b.X;
        int bz = b.Y;
        int by = -bx - bz;

        return Math.Max(Math.Abs(ax - bx), Math.Max(Math.Abs(ay - by), Math.Abs(az - bz)));
    }

    private static int GetNodeKey(ILocation region)
    {
        return Coordinates.GetId(region.X, region.Y, region.Plane);
    }

    private sealed class RouteLocation(int x, int y, PlaneType plane) : ILocation
    {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public PlaneType Plane { get; } = plane;
    }
   

    /// <summary>
    /// Collects ship data from its DataKey properties.
    /// Collects also owner info, units aboard and effects, using region DataBlock and its children DataBlock objects. 
    /// </summary>
    /// <param name="report">CR report</param>
    /// <param name="region">Region where is located the ship</param>
    /// <param name="con    tainerProperty"></param>
    /// <returns></returns>
    protected override bool CollectDataFromBlock(CRDocument report, ref DataProperty? structureProperty)
    {
        if (Structure == null) { return false; }
        structureProperty = null;
        DataProperties.Clear();

        foreach (DataKey kb in Structure.GetData())
        {
            // StructureModel ignores this key, but a ship needs its fleet size to compare its
            // current size with the maximum size of the whole fleet.
            if (kb.GetKeyType() == KeyType.NUMBER)
            {
                Number = Math.Max(1, kb.GetInt());
                continue;
            }

            if (!HandleData(kb))
            {
                // Specific detailed ship data
                switch (kb.GetKeyType())
                {
                    case KeyType.DAMAGE:
                        DamagePercent = kb.GetInt();
                        break;
                    case KeyType.COAST:
                        Coast = kb.GetInt();
                        break;
                    case KeyType.CARGO:
                        Cargo = kb.GetInt();
                        break;
                    case KeyType.CAPACITY:
                        Capacity = kb.GetInt();
                        break;
                    case KeyType.LOAD:
                    case KeyType.MAXLOAD:
                        // Ignore
                        break;
                    case KeyType.FACTION:
                        break;
                    default:
                        AddDataProperty(kb);
                        break;
                }
            }
        }

        string structureLabel = GetLabel();
        structureProperty = new DataProperty(Categories.Node, structureLabel, string.Empty, structureLabel, string.Empty, this/*Structure*/);

        CollectOwnerInfo(report);
        if (CollectUnits(report, KeyType.SHIP, Structure.GetId()))
        {
            TotalSkill = ComputeTotalSkill();
        }

        CollectEffects();

        return true;
    }

    private int ComputeTotalSkill()
    {
        int total = 0;
        if (Units.Count > 0)
        {
            foreach (UnitModel unitModel in Units)
            {
                int number = unitModel.SourceDataBlock.ValueInt(KeyType.NUMBER);
                DataBlock? child = null;
                if (number > 0 && CRDocument.GetSeenChild(ref child, unitModel.SourceDataBlock, BlockType.TALENTS))
                {
                    int skill = child!.ValueSkill(DataKey.SKILL_SAILING_NAME);
                    total += skill * number;
                }
            }
        }
        return total;
    }
}
