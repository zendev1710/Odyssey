using Odyssey.Icons;

namespace Odyssey.Icons.Generator.Tests;

public class IconCatalogBuilderTests
{
    private static string RepoRoot
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "src", "Odyssey.slnx")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Could not locate the repository root.");
        }
    }

    private static string ManifestPath => IconCatalogBuilder.DefaultManifestPath(RepoRoot);
    private static string AssetsRoot => IconCatalogBuilder.DefaultAssetsRoot(RepoRoot);

    [Fact]
    public void Builds_a_complete_catalog_with_unique_ids_and_files()
    {
        var result = IconCatalogBuilder.Build(ManifestPath, AssetsRoot);

        Assert.True(result.Stats.Entries > 600);
        Assert.True(result.Stats.Variants > 100);
        Assert.Equal(15, result.Stats.Categories);
        Assert.Equal(result.Catalog.Icons.Count, result.Catalog.Icons.Select(icon => icon.Id).Distinct().Count());
        Assert.Equal(result.Catalog.Icons.Count, result.Catalog.Icons.Select(icon => icon.File).Distinct().Count());
    }

    [Fact]
    public void Keeps_category_and_item_icons_separate()
    {
        var result = IconCatalogBuilder.Build(ManifestPath, AssetsRoot);

        var category = result.Catalog.Icons.Single(icon => icon.Id == "resource/herbs");
        var item = result.Catalog.Icons.Single(icon => icon.Id == "item/herbs");

        Assert.Equal(IconRoles.Category, category.Role);
        Assert.Equal("resources/herbs.gif", category.File);
        Assert.Equal(IconRoles.Category, item.Role);
        Assert.Equal("items/herbs.gif", item.File);
    }

    [Fact]
    public void Applies_the_icon_naming_decisions()
    {
        var result = IconCatalogBuilder.Build(ManifestPath, AssetsRoot);

        var architecture = result.Catalog.Icons.Single(icon => icon.Id == "skill/architecture");
        Assert.Equal("Baukunst", architecture.De);
        Assert.Equal("Architecture", architecture.En);
        Assert.Equal("skills/architecture.gif", architecture.File);
        Assert.Contains("baukunst", architecture.Aliases);

        var masonry = result.Catalog.Icons.Single(icon => icon.Id == "skill/masonry");
        Assert.Equal("Burgenbau", masonry.De);
        Assert.Equal("Masonry", masonry.En);
        Assert.Equal("skills/masonry.gif", masonry.File);

        var mine = result.Catalog.Icons.Single(icon => icon.Id == "building/mine");
        Assert.Equal("Bergwerk", mine.De);
        Assert.Equal("buildings/mine.gif", mine.File);

        var roadwork = result.Catalog.Icons.Single(icon => icon.Id == "skill/roadwork");
        Assert.Equal("skills/roadwork.gif", roadwork.File);

        var race = result.Catalog.Icons.Single(icon => icon.Id == "race/race");
        Assert.Equal(IconRoles.Category, race.Role);
        Assert.Equal("races/race.gif", race.File);
    }

    [Fact]
    public void Preserves_alternate_artwork_as_ignored_variants()
    {
        var result = IconCatalogBuilder.Build(ManifestPath, AssetsRoot);

        var armour = result.Catalog.Icons.Single(icon => icon.Id == "item/armour");
        var armourAlt = result.Catalog.Icons.Single(icon => icon.Id == "item/armour_alt");

        Assert.Equal(IconRoles.Category, armour.Role);
        Assert.Equal(IconRoles.Variant, armourAlt.Role);
        Assert.Empty(armourAlt.De);
        Assert.Empty(armourAlt.En);
        Assert.Empty(armourAlt.Aliases);
    }

    [Fact]
    public void Adds_plural_aliases_when_they_do_not_conflict()
    {
        var result = IconCatalogBuilder.Build(ManifestPath, AssetsRoot);

        var herbs = result.Catalog.Icons.Single(icon => icon.Id == "resource/herbs");
        var potions = result.Catalog.Icons.Single(icon => icon.Id == "item/potions");

        Assert.Contains("herb", herbs.Aliases);
        Assert.Contains("potion", potions.Aliases);
    }

    [Fact]
    public void Resolves_german_and_english_names_to_english_assets()
    {
        var result = IconCatalogBuilder.Build(ManifestPath, AssetsRoot);
        var catalog = IconCatalog.Parse(IconCatalogBuilder.Serialize(result.Catalog));

        Assert.Equal("/Assets/skills/architecture.gif", catalog.ResolvePath("skill", "Baukunst"));
        Assert.Equal("/Assets/buildings/mine.gif", catalog.ResolvePath("building", "Bergwerk"));
        Assert.Equal("/Assets/races/merfolk.gif", catalog.ResolvePath("race", "Aquarians"));
        Assert.Equal("/Assets/terrains/dense_fog.gif", catalog.ResolvePath("terrain", "thickfog"));
        Assert.Equal("/Assets/terrains/mountains.gif", catalog.ResolvePath("terrain", "Mountain"));
        Assert.Equal("/Assets/items/sword.gif", catalog.ResolvePath("item", "Schwert"));
        Assert.Equal("/Assets/ships/trireme.gif", catalog.ResolvePath("ship", "Tireme"));
    }

    [Fact]
    public void Resolver_ignores_variants()
    {
        var result = IconCatalogBuilder.Build(ManifestPath, AssetsRoot);
        var catalog = IconCatalog.Parse(IconCatalogBuilder.Serialize(result.Catalog));

        Assert.Null(catalog.Find("skill", "masonry_alt"));
        Assert.Equal(IconRoles.Variant, catalog.FindById("skill", "masonry_alt")?.Role);
    }

    [Fact]
    public void Rejects_duplicate_aliases_in_the_manifest()
    {
        using var temp = new TempDirectory();
        var assets = Path.Combine(temp.Root, "assets");
        temp.Write("assets/items/apple.gif", string.Empty);
        temp.Write("assets/items/pear.gif", string.Empty);

        var manifest = temp.Write(
            "icons.tsv",
            "category\tid\trole\tde\ten\taliases\tfile\n" +
            "item\titem/apple\tentry\tApfel\tApple\t\titems/apple.gif\n" +
            "item\titem/pear\tentry\tApfel\tPear\t\titems/pear.gif\n");

        var error = Assert.Throws<InvalidOperationException>(
            () => IconCatalogBuilder.Build(manifest, assets));
        Assert.Contains("Apfel", error.Message, StringComparison.Ordinal);
    }
}
