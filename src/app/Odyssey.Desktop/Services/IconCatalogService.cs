using System;
using Avalonia.Platform;
using Odyssey.Icons;

namespace Odyssey.Services;

/// <summary>
/// Loads the generated icon catalog embedded in the application assets.
/// </summary>
public static class IconCatalogService
{
    private const string CatalogUri = "avares://Odyssey/Assets/i18n/icons.json";

    private static readonly Lazy<IconCatalog> LazyCatalog = new(LoadCatalog);

    public static IconCatalog Catalog => LazyCatalog.Value;

    public static string? ResolvePath(string category, string? name, string? fallbackId = null) =>
        IconAssetResolver.ResolvePreferredPath(Catalog.ResolvePath(category, name, fallbackId));

    private static IconCatalog LoadCatalog()
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri(CatalogUri));
            return IconCatalog.Load(stream);
        }
        catch
        {
            return IconCatalog.Empty;
        }
    }
}
