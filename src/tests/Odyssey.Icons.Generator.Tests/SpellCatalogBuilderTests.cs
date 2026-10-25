namespace Odyssey.Icons.Generator.Tests;

public class SpellCatalogBuilderTests
{
    private static string FixtureRoot =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "Server");

    [Fact]
    public void Builds_the_union_of_code_and_po_spells()
    {
        var catalog = SpellCatalogBuilder.Build(FixtureRoot, includePoOnly: true, out var stats);

        Assert.Equal(SpellCatalogBuilder.CurrentVersion, catalog.Version);
        Assert.Equal(
            new[] { "irongolem", "sparkle", "treegrow", "wisps" },
            catalog.Spells.Select(spell => spell.Sname));

        var treegrow = catalog.Spells.Single(spell => spell.Sname == "treegrow");
        Assert.Equal(992591507, treegrow.Id);
        Assert.Equal("Grove of Oak Trees", treegrow.En);
        Assert.Equal("Hain des Lebens", treegrow.De);

        var wisps = catalog.Spells.Single(spell => spell.Sname == "wisps");
        Assert.Equal("wisps", wisps.En);
        Assert.Equal("wisps", wisps.De);

        Assert.Equal(new[] { "wisps" }, stats.MissingEnglish);
        Assert.Equal(new[] { "wisps" }, stats.MissingGerman);
        Assert.Equal(new[] { "sparkle" }, stats.PoOnly);
        Assert.Equal(3, stats.CodeSpells);
        Assert.Equal(3, stats.PoSpells);
        Assert.Equal(4, stats.TotalSpells);
    }

    [Fact]
    public void Code_only_excludes_po_only_spells()
    {
        var catalog = SpellCatalogBuilder.Build(FixtureRoot, includePoOnly: false, out var stats);

        Assert.Equal(
            new[] { "irongolem", "treegrow", "wisps" },
            catalog.Spells.Select(spell => spell.Sname));
        Assert.Empty(stats.PoOnly);
        Assert.Equal(3, stats.TotalSpells);
    }

    [Fact]
    public void Serializes_camel_case_json_with_utf8_names()
    {
        var catalog = SpellCatalogBuilder.Build(FixtureRoot, includePoOnly: true, out _);

        var json = SpellCatalogBuilder.Serialize(catalog);

        Assert.Contains("\"version\": 1", json, StringComparison.Ordinal);
        Assert.Contains("\"sname\": \"treegrow\"", json, StringComparison.Ordinal);
        Assert.Contains("\"de\": \"Hain des Lebens\"", json, StringComparison.Ordinal);
        Assert.EndsWith("\n", json, StringComparison.Ordinal);
    }
}
