using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Odyssey.Icons.Generator;

public sealed record SpellEntry(int Id, string Sname, string En, string De);

public sealed record SpellCatalog(int Version, IReadOnlyList<SpellEntry> Spells);

public sealed record CatalogStats(
    int CodeSpells,
    int PoSpells,
    int TotalSpells,
    IReadOnlyList<string> PoOnly,
    IReadOnlyList<string> MissingEnglish,
    IReadOnlyList<string> MissingGerman);

/// <summary>
/// Builds the spell catalog from the Eressea server sources.
/// </summary>
/// <remarks>
/// The catalog is the union of the spells registered in <c>spell_functions[]</c> and the
/// spell names translated in the PO files. The union keeps old reports readable and also
/// covers data-only spells created from the server configuration, which never appear in
/// <c>spell_functions[]</c>. Entries without a translation fall back to the server
/// identifier (<c>sname</c>).
/// </remarks>
public static class SpellCatalogBuilder
{
    public const int CurrentVersion = 1;

    public static string DefaultOutputPath(string repoRoot) =>
        Path.Combine(repoRoot, "src", "app", "Odyssey.Desktop", "Assets", "i18n", "spells.json");

    public static SpellCatalog Build(string serverRoot, bool includePoOnly, out CatalogStats stats)
    {
        var codeNames = SpellTable.Parse(Path.Combine(serverRoot, "src", "spells.c"));
        var german = PoFile.ParseSpellTranslations(Path.Combine(serverRoot, "res", "translations", "strings.de.po"));
        var english = PoFile.ParseSpellTranslations(Path.Combine(serverRoot, "res", "translations", "strings.en.po"));

        var poNames = new HashSet<string>(german.Keys, StringComparer.Ordinal);
        poNames.UnionWith(english.Keys);

        var codeSet = new HashSet<string>(codeNames, StringComparer.Ordinal);
        var names = new HashSet<string>(codeSet, StringComparer.Ordinal);
        if (includePoOnly)
        {
            names.UnionWith(poNames);
        }

        var entries = new List<SpellEntry>(names.Count);
        var ids = new Dictionary<int, string>();
        var missingEnglish = new List<string>();
        var missingGerman = new List<string>();

        foreach (var sname in names.OrderBy(name => name, StringComparer.Ordinal))
        {
            var en = english.GetValueOrDefault(sname);
            if (string.IsNullOrEmpty(en))
            {
                en = sname;
                missingEnglish.Add(sname);
            }

            var de = german.GetValueOrDefault(sname);
            if (string.IsNullOrEmpty(de))
            {
                de = sname;
                missingGerman.Add(sname);
            }

            var id = StrHash.Compute(sname);
            if (ids.TryGetValue(id, out var existing))
            {
                throw new InvalidOperationException(
                    $"Hash collision: '{existing}' and '{sname}' both map to {id}.");
            }

            ids[id] = sname;
            entries.Add(new SpellEntry(id, sname, en, de));
        }

        stats = new CatalogStats(
            codeNames.Count,
            poNames.Count,
            entries.Count,
            names.Where(name => !codeSet.Contains(name)).OrderBy(name => name, StringComparer.Ordinal).ToList(),
            missingEnglish,
            missingGerman);

        return new SpellCatalog(CurrentVersion, entries);
    }

    public static string Serialize(SpellCatalog catalog)
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

    public static void Write(SpellCatalog catalog, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Serialize(catalog), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
