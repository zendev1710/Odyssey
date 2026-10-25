using CommunityToolkit.HighPerformance;
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

    public int TotalSkill { get; private set; }

    public RegionDataBlock? FromRegion { get; private set; }

    public DataBlock? ToRegion { get; private set; }

    /// <summary>Region where the ship is currently located (its destination for the current turn).</summary>
    public RegionDataBlock? CurrentRegion { get; private set; }

    /// <summary>Faction owning the ship, resolved from its captain/owner unit when the report is parsed.</summary>
    public int OwnerFactionId { get; private set; } = -1;

    public List<DataBlock> TravelledByRegions { get; private set; } = [];

    public Dictionary<int, RegionDataBlock> CrossedRegions { get; private set; } = [];

    // neighbour offsets for pointy-topped axial coords (q,r)
    private static readonly (int Dq, int Dr)[] NeighborOffsets =
    {
        (0, 1), (0, -1), (1, 0), (-1, 0), (1, -1), (-1, 1),
    };

    public ShipModel(DataBlock shipDataBlock, int locationId/*, DataBlock? region = null*/) : base(shipDataBlock, Categories.Ship, locationId/*, region*/)
    {
    }

    public void AddCrossedRegions(List<DataBlock> regions)
    {
        if (regions.Count > 0)
        {
            foreach (DataBlock block in regions)
            {
                if (block is not RegionDataBlock region)
                {
                    continue;
                }

                int key = GetNodeKey(region);
                if (!CrossedRegions.ContainsKey(key))
                {
                    CrossedRegions[key] = region;
                }
            }
        }
        else
        {
            Debug.WriteLine($"AddCrossedRegions without any region for {this} !");
        }
    }

    public void SetCurrentRegion(RegionDataBlock? region)
    {
        CurrentRegion = region;
    }

    public void SetOwnerFactionId(int factionId)
    {
        OwnerFactionId = factionId;
    }

    /// <summary>
    /// Resolves the faction owning the ship from its captain/owner unit, with the ship faction key as fallback.
    /// </summary>
    public void ResolveOwnerFaction(CRDocument report)
    {
        int ownerUnitId = Structure.ValueInt(KeyType.OWNER, -1);
        if (ownerUnitId <= 0)
        {
            ownerUnitId = Structure.ValueInt(KeyType.CAPTAIN, -1);
        }

        if (ownerUnitId > 0 && report.FindUnit(ownerUnitId, out UnitModel? ownerUnit) && ownerUnit is not null)
        {
            OwnerFactionId = CRDocument.GetFactionIdForUnit(ownerUnit.DataBlock);
            return;
        }

        OwnerFactionId = Structure.ValueInt(KeyType.FACTION, -1);
    }

    /// <summary>
    /// Resolves the route from a SAIL message. The crossed regions recorded in the report are used as
    /// the allowed intermediate regions, and a shortest path is searched between the two endpoints.
    /// </summary>
    public void SetFromTo(RegionDataBlock? fromRegion, RegionDataBlock? toRegion)
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
        RegionDataBlock start = candidates
            .OrderByDescending(r => HexDistance(r, CurrentRegion))
            .First();

        var allowed = new Dictionary<int, RegionDataBlock>();
        foreach (RegionDataBlock candidate in candidates)
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

    private static List<DataBlock> BuildRoute(
        RegionDataBlock from,
        RegionDataBlock to,
        IEnumerable<RegionDataBlock> crossedRegions)
    {
        var nodes = new Dictionary<int, RegionDataBlock>();
        AddNode(nodes, from);
        AddNode(nodes, to);

        foreach (RegionDataBlock region in crossedRegions)
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

    private static void AddNode(Dictionary<int, RegionDataBlock> nodes, RegionDataBlock region)
    {
        nodes.TryAdd(GetNodeKey(region), region);
    }

    /// <summary>
    /// Breadth-first search restricted to the regions that were actually crossed by the ship.
    /// </summary>
    private static List<DataBlock>? FindPath(
        RegionDataBlock from,
        RegionDataBlock to,
        IReadOnlyDictionary<int, RegionDataBlock> nodes)
    {
        int startKey = GetNodeKey(from);
        int targetKey = GetNodeKey(to);

        if (startKey == targetKey)
        {
            return [from];
        }

        if (!nodes.TryGetValue(startKey, out RegionDataBlock? startNode) ||
            !nodes.TryGetValue(targetKey, out RegionDataBlock? targetNode))
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
            RegionDataBlock node = nodes[key];
            int plane = node.GetPlane();
            int x = node.GetX();
            int y = node.GetY();

            foreach ((int dq, int dr) in NeighborOffsets)
            {
                int nextKey = Coordinates.GetId(x + dq, y + dr, (PlaneType)plane);
                if (!nodes.TryGetValue(nextKey, out RegionDataBlock? next) || !visited.Add(nextKey))
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

    private static List<DataBlock> ReconstructPath(
        IReadOnlyDictionary<int, RegionDataBlock> nodes,
        IReadOnlyDictionary<int, int> previous,
        int startKey,
        int targetKey)
    {
        var path = new List<DataBlock>();
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
    private static List<DataBlock> BuildFallbackRoute(
        RegionDataBlock from,
        RegionDataBlock to,
        IEnumerable<RegionDataBlock> crossedRegions)
    {
        const int MaxDetour = 3;

        int directDistance = HexDistance(from, to);
        var remaining = crossedRegions
            .Where(r => GetNodeKey(r) != GetNodeKey(from) && GetNodeKey(r) != GetNodeKey(to))
            .Where(r => r.GetPlane() == from.GetPlane() && r.GetPlane() == to.GetPlane())
            .Where(r => HexDistance(from, r) + HexDistance(r, to) <= directDistance + MaxDetour)
            .GroupBy(GetNodeKey)
            .Select(g => g.First())
            .ToList();

        var result = new List<DataBlock> { from };
        RegionDataBlock current = from;
        int progress = 0;

        while (remaining.Count > 0)
        {
            int currentRemaining = HexDistance(current, to);
            RegionDataBlock? next = remaining
                .Where(r => HexDistance(from, r) >= progress)
                .Where(r => IsNeighbour(current, r) || HexDistance(r, to) <= currentRemaining)
                .OrderBy(r => IsNeighbour(current, r) ? 0 : 1)
                .ThenBy(r => HexDistance(current, r) + HexDistance(r, to))
                .ThenBy(r => HexDistance(current, r))
                .ThenBy(r => r.GetX())
                .ThenBy(r => r.GetY())
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
    private static void CompleteRoute(List<DataBlock> route)
    {
        if (route.Count < 2)
        {
            return;
        }

        var completed = new List<DataBlock>(route.Count * 2) { route[0] };
        for (int i = 1; i < route.Count; i++)
        {
            if (route[i - 1] is RegionDataBlock previous &&
                route[i] is RegionDataBlock next &&
                previous.GetPlane() == next.GetPlane())
            {
                foreach ((int x, int y) in EnumerateIntermediateHexes(previous, next))
                {
                    completed.Add(CreateRouteRegion(x, y, previous.GetPlane()));
                }
            }

            completed.Add(route[i]);
        }

        route.Clear();
        route.AddRange(completed);
    }

    private static IEnumerable<(int X, int Y)> EnumerateIntermediateHexes(RegionDataBlock from, RegionDataBlock to)
    {
        int ax = from.GetX();
        int az = from.GetY();
        int ay = -ax - az;

        int bx = to.GetX();
        int bz = to.GetY();
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

    private static RegionDataBlock CreateRouteRegion(int x, int y, int plane)
    {
        return new RegionDataBlock($"{x} {y} {plane}");
    }

    private static bool IsNeighbour(RegionDataBlock a, RegionDataBlock b)
    {
        if (a.GetPlane() != b.GetPlane())
        {
            return false;
        }

        int dx = b.GetX() - a.GetX();
        int dy = b.GetY() - a.GetY();
        foreach ((int dq, int dr) in NeighborOffsets)
        {
            if (dx == dq && dy == dr)
            {
                return true;
            }
        }

        return false;
    }

    private static int HexDistance(RegionDataBlock a, RegionDataBlock b)
    {
        int ax = a.GetX();
        int az = a.GetY();
        int ay = -ax - az;

        int bx = b.GetX();
        int bz = b.GetY();
        int by = -bx - bz;

        return Math.Max(Math.Abs(ax - bx), Math.Max(Math.Abs(ay - by), Math.Abs(az - bz)));
    }

    private static int GetNodeKey(RegionDataBlock region)
    {
        return Coordinates.GetId(region.GetX(), region.GetY(), (PlaneType)region.GetPlane());
    }
   

    /// <summary>
    /// Collects ship data from its DataKey properties.
    /// Collects also owner info, units aboard and effects, using region DataBlock and its children DataBlock objects. 
    /// </summary>
    /// <param name="report">CR report</param>
    /// <param name="region">Region where is located the ship</param>
    /// <param name="con    tainerProperty"></param>
    /// <returns></returns>
    protected override bool CollectDataFromBlock(CRDocument report/*, DataBlock? region*/, ref DataProperty? structureProperty)
    {
        if (Structure == null) { return false; }
        structureProperty = null;
        DataProperties.Clear();

        foreach (DataKey kb in Structure.GetData())
        {
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
                int number = unitModel.DataBlock.ValueInt(KeyType.NUMBER);
                DataBlock? child = null;
                if (number > 0 && CRDocument.GetSeenChild(ref child, unitModel.DataBlock, BlockType.TALENTS))
                {
                    int skill = child!.ValueSkill(DataKey.SKILL_SAILING_NAME);
                    total += skill * number;
                }
            }
        }
        return total;
    }
}
