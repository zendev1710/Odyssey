using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Odyssey.Assets;
using Odyssey.Extensions;
using Odyssey.Icons;
using Odyssey.Models;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using Odyssey.Services;

namespace Odyssey.Search;

public enum GlobalSearchError
{
    None,
    InvalidPattern,
    TimedOut,
    Cancelled
}

public sealed record GlobalSearchResponse(IReadOnlyList<GlobalSearchResult> Results, GlobalSearchError Error);

public sealed class GlobalSearchEngine
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly CultureInfo[] SearchCultures = Enum.GetValues<SupportedUILanguage>()
        .Where(language => language != SupportedUILanguage.Unknown)
        .Select(language => CultureInfo.GetCultureInfo(language.ToIsoCode()))
        .ToArray();

    private readonly object _indexLock = new();
    private CRDocument? _indexedReport;
    private SearchEntry[] _index = [];

    public GlobalSearchResponse Search(
        CRDocument report,
        string query,
        bool matchCase,
        bool wholeWords,
        bool useRegex,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (string.IsNullOrWhiteSpace(query))
        {
            return new GlobalSearchResponse(Array.Empty<GlobalSearchResult>(), GlobalSearchError.None);
        }

        Regex matcher;
        try
        {
            string pattern = useRegex ? query : Regex.Escape(query);
            if (wholeWords)
            {
                pattern = $"(?<!\\w)(?:{pattern})(?!\\w)";
            }

            RegexOptions options = RegexOptions.CultureInvariant;
            if (!matchCase)
            {
                options |= RegexOptions.IgnoreCase;
            }

            matcher = new Regex(pattern, options, RegexTimeout);
        }
        catch (ArgumentException)
        {
            return new GlobalSearchResponse(Array.Empty<GlobalSearchResult>(), GlobalSearchError.InvalidPattern);
        }

        SearchEntry[]? entries = GetIndex(report, cancellationToken);
        if (entries is null)
        {
            return new GlobalSearchResponse(Array.Empty<GlobalSearchResult>(), GlobalSearchError.Cancelled);
        }

        List<GlobalSearchResult> results = [];
        try
        {
            for (int i = 0; i < entries.Length; i++)
            {
                // Poll every 64 entries to limit cancellation-check overhead while keeping searches responsive.
                if ((i & 63) == 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new GlobalSearchResponse(Array.Empty<GlobalSearchResult>(), GlobalSearchError.Cancelled);
                    }
                }

                SearchEntry entry = entries[i];
                if (entry.SearchableValues.Any(value => matcher.IsMatch(value)))
                {
                    results.Add(entry.Result);
                }
            }
        }
        catch (RegexMatchTimeoutException)
        {
            return new GlobalSearchResponse(Array.Empty<GlobalSearchResult>(), GlobalSearchError.TimedOut);
        }

        return new GlobalSearchResponse(results, GlobalSearchError.None);
    }

    public void ClearIndex()
    {
        lock (_indexLock)
        {
            _indexedReport = null;
            _index = [];
        }
    }

    private SearchEntry[]? GetIndex(CRDocument report, CancellationToken cancellationToken)
    {
        lock (_indexLock)
        {
            if (ReferenceEquals(_indexedReport, report))
            {
                return _index;
            }

            SearchEntry[]? entries = BuildIndex(report, cancellationToken);
            if (entries is null)
            {
                return null;
            }

            _index = entries;
            _indexedReport = report;
            return entries;
        }
    }

    private static SearchEntry[]? BuildIndex(CRDocument report, CancellationToken cancellationToken)
    {
        List<SearchEntry> entries = [];
        Dictionary<(string Category, string Name, string? ResourceKey), string[]> vocabularyCache = [];

        foreach (WorldPlane plane in report.Planes.Values)
        {
            foreach (RegionModel region in plane.OccupiedRegions.Values.Concat(plane.UnoccupiedRegions.Values))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                string name = FirstNonEmpty(region.Name, region.ShortName, region.Label, region.EncodedId);
                string regionName = FirstNonEmpty(region.Name, region.Label);
                string regionType = region.Region.Value(KeyType.TYPE);
                List<string?> searchableValues = GetSearchableValues(
                    [region.EncodedId, region.Name, region.ShortName, region.Label, regionType],
                    IconCategories.Terrain,
                    regionType,
                    "find_result_kind_region").ToList();
                AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Item, region.Herb, GetResourceKey("rules_item", region.Herb));

                int regionDepth = region.Region.GetDepth();
                for (DataBlock? block = region.Region.GetNextBlock(); block is not null && block.GetDepth() > regionDepth; block = block.GetNextBlock())
                {
                    if (block.GetBlockType() != BlockType.RESOURCE)
                    {
                        continue;
                    }

                    string resourceType = block.Value(KeyType.RESOURCE_TYPE);
                    AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Resource, resourceType, GetResourceKey("rules_item", resourceType));
                    AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Item, resourceType, GetResourceKey("rules_item", resourceType));
                }

                AddEntry(
                    entries,
                    new GlobalSearchResult(name, GlobalSearchCategory.Region, Lang.find_result_kind_region, region.EncodedId, regionName, string.Empty, region, region, null),
                    searchableValues.ToArray());
            }
        }

        Dictionary<int, RegionModel> factionRegions = [];
        foreach (UnitModel unit in report.Units.Values)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            if (!factionRegions.ContainsKey(unit.EffectiveFactionId) &&
                report.FindRegionById(unit.LocationId) is RegionModel unitRegion)
            {
                factionRegions[unit.EffectiveFactionId] = unitRegion;
            }

            RegionModel? region = report.FindRegionById(unit.LocationId);
            string name = FirstNonEmpty(unit.Name, unit.DisplayName, unit.EncodedId);
            List<string?> searchableValues = GetSearchableValues(
                [unit.EncodedId, unit.Name, unit.DisplayName, unit.Race, unit.TrueRaceType],
                IconCategories.Race,
                unit.Race,
                "find_result_kind_unit",
                GetResourceKey("rules_race", unit.Race),
                GetResourceKey("rules_race", unit.TrueRaceType)).ToList();
            AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Monster, unit.Race, GetResourceKey("rules_race", unit.Race));
            AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Monster, unit.TrueRaceType, GetResourceKey("rules_race", unit.TrueRaceType));

            if (!string.IsNullOrWhiteSpace(unit.Hero))
            {
                AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Status, "Hero", "heroes");
            }

            if (unit.EffectiveFactionId.IsMonster())
            {
                searchableValues.AddRange(["monster", "monsters", "monstre", "monstres"]);
            }

            AddUnitItemsAndRoles(unit, searchableValues, vocabularyCache, cancellationToken);
            AddEntry(
                entries,
                new GlobalSearchResult(
                    name,
                    GlobalSearchCategory.Unit,
                    Lang.find_result_kind_unit,
                    unit.EncodedId,
                    GetRegionName(region),
                    report.GetFactionName(unit.EffectiveFactionId),
                    unit,
                    region,
                    null),
                searchableValues.ToArray());
        }

        foreach (BuildingModel building in report.Buildings.Values)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            RegionModel? region = report.FindRegionById(building.LocationId);
            string name = FirstNonEmpty(building.Name, building.DisplayName, building.EncodedId);
            AddEntry(
                entries,
                new GlobalSearchResult(
                    name,
                    GlobalSearchCategory.Building,
                    Lang.find_result_kind_building,
                    building.EncodedId,
                    GetRegionName(region),
                    GetFactionName(report, building.FactionId),
                    building,
                    region,
                    null),
                GetSearchableValues(
                    [building.EncodedId, building.Name, building.DisplayName, building.Type],
                    IconCategories.Building,
                    building.Type,
                    "find_result_kind_building"));
        }

        foreach (ShipModel ship in report.Ships.Values)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            RegionModel? region = ship.CurrentRegion ?? report.FindRegionById(ship.LocationId);
            int factionId = ship.OwnerFactionId >= 0 ? ship.OwnerFactionId : ship.FactionId;
            string name = FirstNonEmpty(ship.Name, ship.DisplayName, ship.EncodedId);
            AddEntry(
                entries,
                new GlobalSearchResult(
                    name,
                    GlobalSearchCategory.Ship,
                    Lang.find_result_kind_ship,
                    ship.EncodedId,
                    GetRegionName(region),
                    GetFactionName(report, factionId),
                    ship,
                    region,
                    null),
                GetSearchableValues(
                    [ship.EncodedId, ship.Name, ship.DisplayName, ship.Type],
                    IconCategories.Ship,
                    ship.Type,
                    "find_result_kind_ship"));
        }

        foreach (FactionModel faction in report.FactionModels)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }
            factionRegions.TryGetValue(faction.Id, out RegionModel? region);
            string name = FirstNonEmpty(faction.FactionName, faction.Name, faction.Data.IdToString());
            string factionRace = faction.Data.Value(KeyType.TYPE);
            LocatedFactionModel? locatedFaction = region is null
                ? null
                : new LocatedFactionModel(faction, region, faction.Id, faction.Name);
            AddEntry(
                entries,
                new GlobalSearchResult(
                    name,
                    GlobalSearchCategory.Faction,
                    Lang.find_result_kind_faction,
                    faction.Data.IdToString(),
                    GetRegionName(region),
                    faction.Name,
                    null,
                    region,
                    locatedFaction),
                GetSearchableValues(
                    [faction.Data.IdToString(), faction.FactionName, faction.Name, faction.Race.ToString(), factionRace],
                    IconCategories.Race,
                    factionRace,
                    "find_result_kind_faction"));
        }

        return entries.ToArray();
    }

    private static void AddEntry(List<SearchEntry> entries, GlobalSearchResult result, params string?[] searchableValues)
    {
        string[] values = searchableValues
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();
        entries.Add(new SearchEntry(result, values));
    }

    private static string?[] GetSearchableValues(
        IEnumerable<string?> values,
        string iconCategory,
        string? typeValue,
        params string?[] localizationResourceKeys)
    {
        List<string?> searchableValues = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        if (IconCatalogService.Catalog.Find(iconCategory, typeValue) is IconEntry entry)
        {
            searchableValues.Add(entry.De);
            searchableValues.Add(entry.En);
            searchableValues.AddRange(entry.Aliases);
        }

        foreach (string? resourceKey in localizationResourceKeys)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                continue;
            }

            foreach (CultureInfo culture in SearchCultures)
            {
                string? localizedValue = Lang.ResourceManager.GetString(resourceKey, culture);
                if (!string.IsNullOrWhiteSpace(localizedValue))
                {
                    searchableValues.Add(localizedValue);
                }
            }
        }

        return searchableValues.ToArray();
    }

    private static void AddUnitItemsAndRoles(
        UnitModel unit,
        List<string?> searchableValues,
        Dictionary<(string Category, string Name, string? ResourceKey), string[]> vocabularyCache,
        CancellationToken cancellationToken)
    {
        DataBlock unitData = unit.SourceDataBlock;
        int unitDepth = unitData.GetDepth();
        for (DataBlock? block = unitData.GetNextBlock(); block is not null && block.GetDepth() > unitDepth; block = block.GetNextBlock())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (block.GetBlockType() == BlockType.ITEMS)
            {
                foreach (DataKey item in block.GetData())
                {
                    string itemName = item.GetKeyFromType();
                    AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Item, itemName, GetResourceKey("rules_item", itemName));
                    AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Effect, itemName, GetResourceKey("rules_effect", itemName));
                }
            }
            else if (block.GetBlockType() == BlockType.TALENTS)
            {
                foreach (DataKey skill in block.GetData())
                {
                    string skillName = skill.GetKeyFromType();
                    IconEntry? skillEntry = IconCatalogService.Catalog.Find(IconCategories.Skill, skillName);
                    if (skillEntry?.Id is "skill/alchemy" or "skill/alchemy_alchimie" or "skill/magic" or "skill/tactics")
                    {
                        AddVocabularyTerms(searchableValues, vocabularyCache, IconCategories.Skill, skillName, GetResourceKey("rules_skill", skillName));
                    }
                }
            }

        foreach (string commandLine in unit.GetCommandLines())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            searchableValues.Add(commandLine);
        }
        }
    }

    private static void AddVocabularyTerms(
        List<string?> searchableValues,
        Dictionary<(string Category, string Name, string? ResourceKey), string[]> vocabularyCache,
        string category,
        string? name,
        string? resourceKey)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var cacheKey = (category, name, resourceKey);
        if (!vocabularyCache.TryGetValue(cacheKey, out string[]? terms))
        {
            List<string?> values = [name];
            if (IconCatalogService.Catalog.Find(category, name) is IconEntry entry)
            {
                values.Add(entry.De);
                values.Add(entry.En);
                values.AddRange(entry.Aliases);
            }

            if (resourceKey is not null)
            {
                foreach (CultureInfo culture in SearchCultures)
                {
                    values.Add(Lang.ResourceManager.GetString(resourceKey, culture));
                }
            }

            terms = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            vocabularyCache[cacheKey] = terms;
        }

        searchableValues.AddRange(terms);
    }

    private static string? GetResourceKey(string prefix, string? name) =>
        string.IsNullOrWhiteSpace(name) ? null : $"{prefix}_{name}";

    private static string GetRegionName(RegionModel? region)
    {
        return region is null ? string.Empty : FirstNonEmpty(region.Name, region.Label);
    }

    private static string GetFactionName(CRDocument report, int factionId)
    {
        return factionId < 0 ? string.Empty : report.GetFactionName(factionId);
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private sealed record SearchEntry(GlobalSearchResult Result, string[] SearchableValues);
}
