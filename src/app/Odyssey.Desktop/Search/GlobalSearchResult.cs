using System.Linq;
using Odyssey.Models;

namespace Odyssey.Search;

public enum GlobalSearchCategory
{
    Region,
    Unit,
    Building,
    Ship,
    Faction
}

public sealed class GlobalSearchResult
{
    public string Name { get; }
    public GlobalSearchCategory Category { get; }
    public string Kind { get; }
    public string Identifier { get; }
    public string RegionName { get; }
    public string FactionName { get; }
    public string Details { get; }
    public EntityModel? Entity { get; }
    public RegionModel? Region { get; }
    public LocatedFactionModel? LocatedFaction { get; }

    public GlobalSearchResult(
        string name,
        GlobalSearchCategory category,
        string kind,
        string identifier,
        string regionName,
        string factionName,
        EntityModel? entity,
        RegionModel? region,
        LocatedFactionModel? locatedFaction)
    {
        Name = name;
        Category = category;
        Kind = kind;
        Identifier = identifier;
        RegionName = regionName;
        FactionName = factionName;
        Entity = entity;
        Region = region;
        LocatedFaction = locatedFaction;
        Details = string.Join(" | ", new[] { kind, identifier, regionName, factionName }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }
}
