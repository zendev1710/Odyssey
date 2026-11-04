namespace Odyssey.Icons;

/// <summary>
/// One icon in <c>Assets/i18n/icons.json</c>. Primary entries (<see cref="IconRoles.Entry"/> and
/// <see cref="IconRoles.Category"/>) participate in name lookup. Variants are preserved files that
/// must not be selected by the resolver.
/// </summary>
public sealed record IconEntry(
    string Category,
    string Id,
    string Role,
    string De,
    string En,
    IReadOnlyList<string> Aliases,
    string File);

public static class IconRoles
{
    public const string Entry = "entry";
    public const string Category = "category";
    public const string Variant = "variant";
}

public static class IconCategories
{
    public const string Animal = "animal";
    public const string Building = "building";
    public const string Effect = "effect";
    public const string Faction = "faction";
    public const string Item = "item";
    public const string Monster = "monster";
    public const string Race = "race";
    public const string Resource = "resource";
    public const string Skill = "skill";
    public const string Spell = "spell";
    public const string Status = "status";
    public const string Technical = "technical";
    public const string Terrain = "terrain";
    public const string Trade = "trade";
    public const string Ship = "ship";
}

public sealed record IconCatalogData(int Version, IReadOnlyList<IconEntry> Icons);
