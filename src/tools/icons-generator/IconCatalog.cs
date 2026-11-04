using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Odyssey.Icons;

namespace Odyssey.Icons.Generator;

public sealed record IconCatalogStats(
    int Entries,
    int Categories,
    int Variants,
    int Aliases);

public sealed record IconCatalogResult(
    IconCatalogData Catalog,
    IconCatalogStats Stats);

/// <summary>
/// Builds <c>Assets/i18n/icons.json</c> from the checked-in icon table.
/// </summary>
/// <remarks>
/// The TSV is the source of truth. It lists every asset, including preserved variants. Only
/// entries with role <c>entry</c> or <c>category</c> participate in name lookup, so German report
/// words and English aliases always resolve to the canonical Eressea artwork.
/// </remarks>
public static partial class IconCatalogBuilder
{
    private static readonly string[] Categories =
    [
        "animal",
        "building",
        "effect",
        "faction",
        "item",
        "monster",
        "race",
        "resource",
        "skill",
        "spell",
        "status",
        "technical",
        "terrain",
        "trade",
        "ship",
    ];

    private static readonly string[] Columns =
    [
        "category",
        "id",
        "role",
        "de",
        "en",
        "aliases",
        "file",
    ];

    [GeneratedRegex("^[a-z0-9_./-]+$")]
    private static partial Regex SafePath();

    public static string DefaultManifestPath(string repoRoot) =>
        Path.Combine(repoRoot, "src", "tools", "icons-generator", "data", "icons.tsv");

    public static string DefaultAssetsRoot(string repoRoot) =>
        Path.Combine(repoRoot, "src", "app", "Odyssey.Desktop", "Assets");

    public static string DefaultOutputPath(string repoRoot) =>
        Path.Combine(DefaultAssetsRoot(repoRoot), "i18n", "icons.json");

    public static IconCatalogResult Build(string manifestPath, string assetsRoot)
    {
        var drafts = ReadManifest(manifestPath, assetsRoot);
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var files = new HashSet<string>(StringComparer.Ordinal);

        foreach (var draft in drafts)
        {
            if (!ids.Add(draft.Id))
            {
                throw new InvalidOperationException($"duplicate icon id '{draft.Id}'.");
            }

            if (!files.Add(draft.File))
            {
                throw new InvalidOperationException($"duplicate icon file '{draft.File}'.");
            }

            if (draft.Role == IconRoles.Variant)
            {
                if (draft.De.Length != 0 || draft.En.Length != 0 || draft.Aliases.Count != 0)
                {
                    throw new InvalidOperationException(
                        $"variant '{draft.Id}' must not define names or aliases.");
                }

                continue;
            }

            if (draft.De.Length == 0 && draft.En.Length == 0)
            {
                throw new InvalidOperationException($"'{draft.Id}' needs a German or English name.");
            }

            RegisterAlias(draft, draft.De, aliases);
            RegisterAlias(draft, draft.En, aliases);
            foreach (var alias in draft.Aliases)
            {
                RegisterAlias(draft, alias, aliases);
            }
        }

        AddEnglishPluralAliases(drafts, aliases);

        var entries = drafts
            .OrderBy(draft => draft.Category, StringComparer.Ordinal)
            .ThenBy(draft => draft.Id, StringComparer.Ordinal)
            .Select(draft => new IconEntry(
                draft.Category,
                draft.Id,
                draft.Role,
                draft.De,
                draft.En,
                draft.Aliases,
                draft.File))
            .ToArray();

        var stats = new IconCatalogStats(
            Entries: entries.Count(entry => entry.Role != IconRoles.Variant),
            Categories: entries.Select(entry => entry.Category).Distinct(StringComparer.Ordinal).Count(),
            Variants: entries.Count(entry => entry.Role == IconRoles.Variant),
            Aliases: entries.Where(entry => entry.Role != IconRoles.Variant).Sum(entry => entry.Aliases.Count));

        return new IconCatalogResult(new IconCatalogData(1, entries), stats);
    }

    public static string Serialize(IconCatalogData catalog)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NewLine = "\n",
        };

        return JsonSerializer.Serialize(catalog, options) + "\n";
    }

    public static void Write(IconCatalogData catalog, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Serialize(catalog), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static List<IconDraft> ReadManifest(string manifestPath, string assetsRoot)
    {
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException($"icon manifest not found: {manifestPath}", manifestPath);
        }

        var lines = File.ReadAllLines(manifestPath);
        if (lines.Length == 0)
        {
            throw new InvalidOperationException($"icon manifest is empty: {manifestPath}");
        }

        var header = lines[0].Split('\t');
        if (!header.SequenceEqual(Columns, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"unexpected icon manifest header; expected: {string.Join('\t', Columns)}");
        }

        var root = Path.GetFullPath(assetsRoot);
        var seenLine = 2;
        var drafts = new List<IconDraft>();
        foreach (var line in lines.Skip(1))
        {
            if (line.Length == 0)
            {
                continue;
            }

            var parts = line.Split('\t');
            if (parts.Length != Columns.Length)
            {
                throw new InvalidOperationException(
                    $"line {seenLine}: expected {Columns.Length} columns, found {parts.Length}.");
            }

            var category = parts[0];
            var id = parts[1];
            var role = parts[2];
            var de = parts[3];
            var en = parts[4];
            var aliases = parts[5]
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            var file = parts[6];

            if (!Categories.Contains(category, StringComparer.Ordinal))
            {
                throw new InvalidOperationException($"line {seenLine}: unknown category '{category}'.");
            }

            if (!id.StartsWith(category + "/", StringComparison.Ordinal) || id.Length == category.Length + 1)
            {
                throw new InvalidOperationException(
                    $"line {seenLine}: id '{id}' must start with '{category}/'.");
            }

            if (role is not (IconRoles.Entry or IconRoles.Category or IconRoles.Variant))
            {
                throw new InvalidOperationException($"line {seenLine}: unknown role '{role}'.");
            }

            if (!IsSafePath(id) || !IsSafePath(file))
            {
                throw new InvalidOperationException(
                    $"line {seenLine}: id and file must be lowercase ASCII paths.");
            }

            var fullPath = Path.GetFullPath(Path.Combine(root, file.Replace('/', Path.DirectorySeparatorChar)));
            if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"line {seenLine}: file escapes the assets directory.");
            }

            if (!File.Exists(fullPath))
            {
                throw new InvalidOperationException($"line {seenLine}: asset not found: {file}");
            }

            drafts.Add(new IconDraft(category, id, role, de, en, aliases, file, seenLine));
            seenLine++;
        }

        return drafts;
    }

    private static void RegisterAlias(
        IconDraft draft,
        string? value,
        Dictionary<string, string> aliases)
    {
        var normalized = IconName.Normalize(value);
        if (normalized.Length == 0)
        {
            return;
        }

        var key = string.Concat(draft.Category, ":", normalized);
        if (aliases.TryGetValue(key, out var owner) && owner != draft.Id)
        {
            throw new InvalidOperationException(
                $"alias '{value}' is used by both '{owner}' and '{draft.Id}' (line {draft.Line}).");
        }

        aliases[key] = draft.Id;
    }

    private static void AddEnglishPluralAliases(
        List<IconDraft> drafts,
        Dictionary<string, string> aliases)
    {
        foreach (var draft in drafts.Where(draft => draft.Role != IconRoles.Variant))
        {
            var normalized = IconName.Normalize(draft.En);
            if (normalized.Length == 0)
            {
                continue;
            }

            var existing = new HashSet<string>(
                draft.Aliases.Select(IconName.Normalize),
                StringComparer.Ordinal);
            foreach (var variant in IconName.PluralVariants(normalized))
            {
                if (variant.Length == 0 || variant == normalized || !existing.Add(variant))
                {
                    continue;
                }

                var key = string.Concat(draft.Category, ":", variant);
                if (aliases.ContainsKey(key))
                {
                    continue;
                }

                aliases[key] = draft.Id;
                draft.Aliases.Add(variant);
            }
        }
    }

    private static bool IsSafePath(string value) => SafePath().IsMatch(value);

    private sealed record IconDraft(
        string Category,
        string Id,
        string Role,
        string De,
        string En,
        List<string> Aliases,
        string File,
        int Line);
}
