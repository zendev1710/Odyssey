using System.Text.Json;

namespace Odyssey.Icons;

/// <summary>
/// Resolves German report vocabulary and English aliases to English icon files.
/// </summary>
public sealed class IconCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly Dictionary<string, IconEntry> byName = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IconEntry> byId = new(StringComparer.Ordinal);

    private IconCatalog(IReadOnlyList<IconEntry> entries)
    {
        Entries = entries;

        foreach (var entry in entries)
        {
            byId[entry.Id] = entry;
            if (string.Equals(entry.Role, IconRoles.Variant, StringComparison.Ordinal))
            {
                continue;
            }

            AddAlias(entry, entry.De);
            AddAlias(entry, entry.En);
            foreach (var alias in entry.Aliases)
            {
                AddAlias(entry, alias);
            }
        }
    }

    public IReadOnlyList<IconEntry> Entries { get; }

    public static IconCatalog Empty { get; } = new([]);

    public static IconCatalog Parse(string json)
    {
        var data = JsonSerializer.Deserialize<IconCatalogData>(json, JsonOptions)
            ?? throw new InvalidDataException("The icon catalog is empty.");
        return new IconCatalog(data.Icons ?? []);
    }

    public static IconCatalog Load(Stream stream)
    {
        var data = JsonSerializer.Deserialize<IconCatalogData>(stream, JsonOptions)
            ?? throw new InvalidDataException("The icon catalog is empty.");
        return new IconCatalog(data.Icons ?? []);
    }

    public IconEntry? Find(string category, string? name)
    {
        var normalized = IconName.Normalize(name);
        if (normalized.Length == 0)
        {
            return null;
        }

        if (byName.TryGetValue(Key(category, normalized), out var entry))
        {
            return entry;
        }

        foreach (var variant in IconName.PluralVariants(normalized))
        {
            if (byName.TryGetValue(Key(category, variant), out entry))
            {
                return entry;
            }
        }

        return null;
    }

    public IconEntry? FindById(string category, string? id)
    {
        var normalized = id?.Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(normalized)
            ? null
            : byId.GetValueOrDefault($"{category}/{normalized}");
    }

    /// <summary>
    /// Returns an avares path such as <c>/Assets/items/sword.gif</c>. The fallback is another
    /// manifest id in the same category, for example <c>race</c> or <c>undefined</c>.
    /// </summary>
    public string? ResolvePath(string category, string? name, string? fallbackId = null)
    {
        var entry = Find(category, name) ?? FindById(category, fallbackId);
        return entry is null ? null : "/Assets/" + entry.File;
    }

    private void AddAlias(IconEntry entry, string? alias)
    {
        var normalized = IconName.Normalize(alias);
        if (normalized.Length != 0)
        {
            byName.TryAdd(Key(entry.Category, normalized), entry);
        }
    }

    private static string Key(string category, string normalized) =>
        string.Concat(category, ":", normalized);
}
