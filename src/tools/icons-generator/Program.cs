namespace Odyssey.Icons.Generator;

public static class Program
{
    public static int Main(string[] args)
    {
        string? serverRoot = null;
        string? outputPath = null;
        string? iconsOutputPath = null;
        string? manifestPath = null;
        string? assetsRoot = null;
        var codeOnly = false;
        var check = false;
        var icons = false;
        var iconsCheck = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--server":
                    if (++i >= args.Length)
                    {
                        return Usage("--server requires a path.");
                    }

                    serverRoot = args[i];
                    break;
                case "--out":
                    if (++i >= args.Length)
                    {
                        return Usage("--out requires a path.");
                    }

                    outputPath = args[i];
                    break;
                case "--icons":
                    icons = true;
                    break;
                case "--icons-out":
                    if (++i >= args.Length)
                    {
                        return Usage("--icons-out requires a path.");
                    }

                    icons = true;
                    iconsOutputPath = args[i];
                    break;
                case "--icons-check":
                    icons = true;
                    iconsCheck = true;
                    break;
                case "--manifest":
                    if (++i >= args.Length)
                    {
                        return Usage("--manifest requires a path.");
                    }

                    manifestPath = args[i];
                    break;
                case "--assets":
                    if (++i >= args.Length)
                    {
                        return Usage("--assets requires a path.");
                    }

                    assetsRoot = args[i];
                    break;
                case "--code-only":
                    codeOnly = true;
                    break;
                case "--check":
                    check = true;
                    break;
                case "-h":
                case "--help":
                    PrintUsage();
                    return 0;
                default:
                    return Usage($"Unknown argument: {args[i]}");
            }
        }

        return icons
            ? RunIcons(manifestPath, assetsRoot, iconsOutputPath, iconsCheck || check)
            : RunSpells(serverRoot, outputPath, codeOnly, check);
    }

    private static int RunIcons(
        string? manifestPath,
        string? assetsRoot,
        string? outputPath,
        bool check)
    {
        var repoRoot = FindRepoRoot();
        manifestPath ??= IconCatalogBuilder.DefaultManifestPath(repoRoot);
        assetsRoot ??= IconCatalogBuilder.DefaultAssetsRoot(repoRoot);
        outputPath ??= IconCatalogBuilder.DefaultOutputPath(repoRoot);

        try
        {
            var result = IconCatalogBuilder.Build(manifestPath, assetsRoot);
            var stats = result.Stats;

            Console.WriteLine($"manifest       : {manifestPath}");
            Console.WriteLine($"assets         : {assetsRoot}");
            Console.WriteLine($"entries        : {stats.Entries}");
            Console.WriteLine($"variants       : {stats.Variants}");
            Console.WriteLine($"categories     : {stats.Categories}");
            Console.WriteLine($"aliases        : {stats.Aliases}");
            Console.WriteLine($"output         : {outputPath}");

            var json = IconCatalogBuilder.Serialize(result.Catalog);

            if (check)
            {
                if (!File.Exists(outputPath))
                {
                    Console.Error.WriteLine($"error: catalog not found: {outputPath}");
                    return 1;
                }

                var current = File.ReadAllText(outputPath);
                if (!string.Equals(current, json, StringComparison.Ordinal))
                {
                    Console.Error.WriteLine("error: icon catalog is out of date.");
                    return 1;
                }

                Console.WriteLine("icon catalog is up to date.");
                return 0;
            }

            IconCatalogBuilder.Write(result.Catalog, outputPath);
            Console.WriteLine("icon catalog written.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static int RunSpells(
        string? serverRoot,
        string? outputPath,
        bool codeOnly,
        bool check)
    {
        serverRoot ??= Environment.GetEnvironmentVariable("ERESSEA_SERVER");
        if (string.IsNullOrWhiteSpace(serverRoot))
        {
            return Usage("Missing server path. Use --server <path> or set ERESSEA_SERVER.");
        }

        serverRoot = Path.GetFullPath(serverRoot);
        if (!Directory.Exists(serverRoot))
        {
            return Usage($"Server directory not found: {serverRoot}");
        }

        outputPath ??= SpellCatalogBuilder.DefaultOutputPath(FindRepoRoot());

        try
        {
            var catalog = SpellCatalogBuilder.Build(serverRoot, includePoOnly: !codeOnly, out var stats);

            Console.WriteLine($"server         : {serverRoot}");
            Console.WriteLine($"code spells    : {stats.CodeSpells}");
            Console.WriteLine($"po spells      : {stats.PoSpells}");
            Console.WriteLine($"po-only spells : {stats.PoOnly.Count}");
            Console.WriteLine($"catalog        : {stats.TotalSpells}");
            Console.WriteLine($"missing en     : {stats.MissingEnglish.Count}{Format(stats.MissingEnglish)}");
            Console.WriteLine($"missing de     : {stats.MissingGerman.Count}{Format(stats.MissingGerman)}");
            Console.WriteLine($"output         : {outputPath}");

            var json = SpellCatalogBuilder.Serialize(catalog);

            if (check)
            {
                if (!File.Exists(outputPath))
                {
                    Console.Error.WriteLine($"error: catalog not found: {outputPath}");
                    return 1;
                }

                var current = File.ReadAllText(outputPath);
                if (!string.Equals(current, json, StringComparison.Ordinal))
                {
                    Console.Error.WriteLine("error: catalog is out of date.");
                    return 1;
                }

                Console.WriteLine("catalog is up to date.");
                return 0;
            }

            SpellCatalogBuilder.Write(catalog, outputPath);
            Console.WriteLine("catalog written.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 1;
        }
    }

    private static string Format(IReadOnlyList<string> names) =>
        names.Count == 0 ? string.Empty : $" ({string.Join(", ", names)})";

    private static string FindRepoRoot()
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

        return Environment.CurrentDirectory;
    }

    private static int Usage(string message)
    {
        Console.Error.WriteLine($"error: {message}");
        PrintUsage();
        return 2;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
            Odyssey.Icons.Generator - generate Assets/i18n/spells.json and Assets/i18n/icons.json.

            Usage:
              Odyssey.Icons.Generator --server <eressea/server> [--out <file>] [--code-only] [--check]
              Odyssey.Icons.Generator --icons [--icons-out <file>] [--manifest <file>] [--assets <dir>]
              Odyssey.Icons.Generator --icons-check [--manifest <file>] [--assets <dir>]

            Spell options:
              --server <path>  Path to the Eressea server directory (or set ERESSEA_SERVER).
              --out <file>     Spell output file (default: <repo>/src/app/Odyssey.Desktop/Assets/i18n/spells.json).
              --code-only      Only include spells registered in spells.c (exclude .po-only entries).
              --check          Verify that the existing spell catalog is up to date instead of writing it.

            Icon options:
              --icons          Generate the icon catalog from the checked-in icon table.
              --icons-out      Icon output file (default: <repo>/src/app/Odyssey.Desktop/Assets/i18n/icons.json).
              --icons-check    Verify that the existing icon catalog is up to date instead of writing it.
              --manifest       Icon table (default: <repo>/src/tools/icons-generator/data/icons.tsv).
              --assets         Asset root (default: <repo>/src/app/Odyssey.Desktop/Assets).

            General:
              -h, --help       Show this help.
            """);
    }
}
