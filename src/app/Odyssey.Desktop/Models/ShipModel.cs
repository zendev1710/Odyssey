using CommunityToolkit.HighPerformance;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using System.Collections.Generic;
using System.Diagnostics;
using static Odyssey.Models.Tools.DataProperty;

namespace Odyssey.Models;

public class ShipModel: ContainerModel
{
    public int Coast { get; private set; } = -1;

    public int DamagePercent { get; private set; }

    public int Cargo { get; private set; }

    public int Capacity { get; private set; }

    public int TotalSkill { get; private set; }

    public DataBlock? FromRegion { get; private set; }

    public DataBlock? ToRegion { get; private set; }

    public List<DataBlock> TravelledByRegions { get; private set; } = [];

    public Dictionary<int, DataBlock> CrossedRegions { get; private set; } = [];

    public ShipModel(DataBlock shipDataBlock, DataBlock? region = null) : base(shipDataBlock, Categories.Ship, region)
    {
    }

    public void AddCrossedRegions(List<DataBlock> regions)
    {
        if (regions.Count > 0)
        {
            foreach (DataBlock region in regions)
            {
                int key = Coordinates.GetId(region.GetX(), region.GetY());
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

    public void SetFromTo(DataBlock? fromRegion, DataBlock? toRegion)
    {
        FromRegion = fromRegion;
        ToRegion = toRegion;


        // TODO: collect data ?
        // TODO: retrieve captain route command
        // TODO: compute ship movements from FromRegion to ToRegion with CrossedRegions sorting
        ResolveMovements();
    }

    private void ResolveMovements()
    {
        // Use FromRegion coords
        // Use CrossedRegions coords
        // Use ToRegion coords
        if (FromRegion is null || ToRegion is null)
        {
            Debug.WriteLine($"FromRegion or ToRegion is null for {this} !");
            return;
        }

        List<DataBlock> candidates = [];
        foreach (var r in CrossedRegions)
        {
            var b = r.Value;
            if (b != FromRegion && b != ToRegion)
            {
                candidates.Add(r.Value);
            }
        }

        if (candidates.Count == 0)
        {
            return; // new List<DataBlock>();
        }

        // copy candidates to mutable list
        var remaining = new List<DataBlock>(candidates);
        var ordered = new List<DataBlock>();

        // neighbor offsets for pointy-topped axial coords (q,r)
        int[,] offsets = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 }, { 1, -1 }, { -1, 1 } };
        bool severalRoadsPossible = false;

        while (remaining.Count > 0)
        {
            DataBlock? last = ordered.Count > 0 ? ordered[^1] : FromRegion;
            DataBlock? next = null;

            if (last != null)
            {
                int lx = last.GetX(), ly = last.GetY();
                // find a direct neighbour
                foreach (var candidate in remaining)
                {
                    int cx = candidate.GetX(), cy = candidate.GetY();
                    List<DataBlock> nextCandidates = [];
                    for (int i = 0; i < 6; i++)
                    {
                        if (cx == lx + offsets[i, 0] && cy == ly + offsets[i, 1])
                        {
                            nextCandidates.Add(candidate);
                            //next = candidate;
                            //break;
                        }
                    }
                    if (nextCandidates.Count == 1)
                    {
                        next = nextCandidates[0];
                    }
                    else if (nextCandidates.Count > 1)
                    {
                        next = nextCandidates[0];
                        severalRoadsPossible = true;
                    }

                    if (next != null) break;
                }
            }

            if (next == null)
            {
                // fallback: choose nearest by hex distance to last (or arbitrary if last null)
                next = remaining[0];
            }

            ordered.Add(next);
            remaining.Remove(next);
        }

        if (severalRoadsPossible)
        {
            // TODO: use ROUTE command to get the real path
        }

        TravelledByRegions.Add(FromRegion);
        foreach (var r in ordered)
        {
            TravelledByRegions.Add(r);
        }
        TravelledByRegions.Add(ToRegion);
    }
   

    /// <summary>
    /// Collects ship data from its DataKey properties.
    /// Collects also owner info, units aboard and effects, using region DataBlock and its children DataBlock objects. 
    /// </summary>
    /// <param name="report">CR report</param>
    /// <param name="region">Region where is located the ship</param>
    /// <param name="containerProperty"></param>
    /// <returns></returns>
    public override bool CollectData(CRDocument report/*, DataBlock? region*/, ref DataProperty? containerProperty)
    {
        if (Container == null) { return false; }
        containerProperty = null;
        DataProperties.Clear();

        foreach (DataKey kb in Container.GetData())
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
                        AddDataProperty(report, kb);
                        break;
                }
            }
        }

        string containerLabel = GetLabel();
        containerProperty = new DataProperty(Categories.Node, containerLabel, string.Empty, containerLabel, string.Empty, Container);

        CollectOwnerInfo(report);
        if (CollectUnits(/*region,*/ KeyType.SHIP, Container.GetId()))
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
            foreach (DataBlock unit in Units)
            {
                int number = unit.ValueInt(KeyType.NUMBER);
                DataBlock? child = null;
                if (number > 0 && CRDocument.GetSeenChild(ref child, unit, BlockType.TALENTS))
                {
                    int skill = child!.ValueSkill(DataKey.SKILL_SAILING_NAME);
                    total += skill * number;
                }
            }
        }
        return total;
    }
}
