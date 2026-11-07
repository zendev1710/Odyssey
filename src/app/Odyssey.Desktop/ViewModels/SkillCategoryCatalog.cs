using System;
using System.Collections.Generic;
using System.Linq;
using Odyssey.Models.Localization;

namespace Odyssey.ViewModels;

internal sealed record SkillCategoryDefinition(string Key, IReadOnlyList<string> SkillKeys);

internal static class SkillCategoryCatalog
{
    public const string Combat = "combat";
    public const string Movement = "movement";
    public const string Production = "production";
    public const string Economy = "economy";
    public const string Magic = "magic";
    public const string Reconnaissance = "reconnaissance";

    public static IReadOnlyList<SkillCategoryDefinition> Categories { get; } =
    [
        new(Combat,
        [
            "Bogenschießen", "Katapultbedienung", "Armbrustschießen", "Hiebwaffen",
            "Stangenwaffen", "WaffenloserKampf", "Ausdauer", "Taktik"
        ]),
        new(Movement, ["Segeln", "Reiten"]),
        new(Production,
        [
            "Alchemie", "Pferdedressur", "Schiffbau", "Straßenbau", "Steinbau", "Bergbau",
            "Waffenbau", "Rüstungsbau", "Wagenbau", "Kräuterkunde", "Holzfällen", "Burgenbau"
        ]),
        new(Economy, ["Handeln", "Steuereintreiben", "Unterhaltung"]),
        new(Magic, ["Magie"]),
        new(Reconnaissance, ["Spionage", "Tarnung", "Wahrnehmung"])
    ];

    public static IReadOnlyList<string> DefaultCategoryOrder { get; } = Categories.Select(category => category.Key).ToArray();
    public static IReadOnlyList<string> SkillKeys { get; } = Categories.SelectMany(category => category.SkillKeys).ToArray();

    private static readonly IReadOnlyDictionary<string, string> SkillCategories = Categories
        .SelectMany(category => category.SkillKeys.Select(skillKey => new { skillKey, category.Key }))
        .ToDictionary(item => item.skillKey, item => item.Key, StringComparer.Ordinal);

    public static string? GetCategoryKey(string skillKey)
    {
        return SkillCategories.TryGetValue(skillKey, out string? categoryKey) ? categoryKey : null;
    }

    public static int GetCategoryOrderIndex(string skillKey, IReadOnlyDictionary<string, int> orderIndexes)
    {
        return GetCategoryKey(skillKey) is string categoryKey && orderIndexes.TryGetValue(categoryKey, out int index)
            ? index
            : int.MaxValue;
    }

    public static string[] NormalizeCategoryOrder(IEnumerable<string> savedKeys)
    {
        HashSet<string> knownKeys = new(DefaultCategoryOrder, StringComparer.Ordinal);
        List<string> orderedKeys = savedKeys
            .Where(knownKeys.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        orderedKeys.AddRange(DefaultCategoryOrder.Where(key => !orderedKeys.Contains(key, StringComparer.Ordinal)));
        return orderedKeys.ToArray();
    }
}

public sealed class SkillCategoryOrderItemViewModel
{
    public string Key { get; }
    public string Name { get; }

    public SkillCategoryOrderItemViewModel(string key)
    {
        Key = key;
        Name = Labels.Localize($"skill_category_{key}");
    }
}
