using Odyssey.Models.Data;

namespace Odyssey.Models;

/// <summary>
/// Associates a faction with the region where it is represented in the explorer tree.
/// Special factions (anonymous or traitor) do not have a FactionModel and therefore have
/// a null <see cref="Faction"/> reference.
/// </summary>
public sealed class LocatedFactionModel
{
    public FactionModel? Faction { get; }
    public RegionModel Region { get; }
    public int FactionId { get; }
    public string Name { get; }
    public FactionRace Race => Faction?.Race ?? FactionRace.Unknown;

    public LocatedFactionModel(FactionModel? faction, RegionModel region, int factionId, string name)
    {
        Faction = faction;
        Region = region;
        FactionId = factionId;
        Name = name;
    }
}
