using Avalonia.Media;
using Odyssey.Icons;
using Odyssey.Models.Data;
using Odyssey.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Odyssey.Controls.Buildings;

/// <summary>Values of the "building style" map setting.</summary>
public static class BuildingStyles
{
    public const int Auto = 0;     // icon when available, otherwise a vector symbol
    public const int Icons = 1;    // icons only
    public const int Symbols = 2;  // vector symbols only
}

/// <summary>
/// Vector shape used as a fallback when no icon bitmap is available for a building type.
/// </summary>
public enum BuildingShape
{
    Square,
    Triangle,
    Circle,
    Rhombus,
}

/// <summary>
/// Describes how a building type is drawn on the map (icon bitmap and/or vector fallback)
/// and how it should be placed inside its region hexagon.
/// </summary>
public sealed record BuildingVisual
{
    /// <summary>Normalized (canonical) type key, e.g. "saegewerk".</summary>
    public string TypeKey { get; init; } = string.Empty;

    /// <summary>avares-relative path of the icon bitmap, e.g. "/Assets/buildings/sawmill.gif".</summary>
    public string IconPath { get; init; } = string.Empty;

    /// <summary>Shape drawn when the icon bitmap is missing.</summary>
    public BuildingShape FallbackShape { get; init; } = BuildingShape.Square;

    /// <summary>Fill color of the fallback shape.</summary>
    public Color FallbackColor { get; init; } = Color.FromRgb(0x6E, 0x7C, 0x8C);

    /// <summary>Draw/placement priority: lower values are placed first (castles get the center).</summary>
    public int LayerPriority { get; init; } = 10;

    /// <summary>
    /// Preferred neighbour terrain (Terrains.*) this building "belongs" to, or -1 when it has no
    /// particular affinity. Used to push the building toward the hex edge facing that terrain.
    /// </summary>
    public int AffinityTerrain { get; init; } = -1;

    /// <summary>True for castles/fortifications, which are placed at the center of the hexagon.</summary>
    public bool IsFortification { get; init; }

    /// <summary>Size bucket for fortifications (0 = small, 1 = medium, 2 = large).</summary>
    public int SizeBucket { get; init; }
}

/// <summary>
/// Resolves a building type name (as stored in the report, usually a German name such as
/// "Leuchtturm") to a <see cref="BuildingVisual"/>. Icon bitmaps are resolved through the
/// English icon catalog in Assets/i18n/icons.json; unknown types fall back to a colored vector shape.
/// </summary>
public static class BuildingCatalog
{
    private const string ExplorerFallbackIconPath = "/Assets/buildings/buildings.gif";

    // English (or alternate) type names -> canonical German type key.
    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["academy"] = "akademie",
        ["mine"] = "bergwerk",
        ["castle"] = "burg",
        ["dam"] = "damm",
        ["fortress"] = "festung",
        ["frame"] = "geruest",
        ["scaffold"] = "geruest",
        ["foundation"] = "grundmauern",
        ["harbour"] = "hafen",
        ["harbor"] = "hafen",
        ["tradingpost"] = "handelsposten",
        ["caravanserai"] = "karawanserei",
        ["lighthouse"] = "leuchtturm",
        ["magetower"] = "magierturm",
        ["marketplace"] = "marktplatz",
        ["monument"] = "monument",
        ["stable"] = "pferdezucht",
        ["portal"] = "portal",
        ["sawmill"] = "saegewerk",
        ["smithy"] = "schmiede",
        ["quarry"] = "steinbruch",
        ["stonecircle"] = "steinkreis",
        ["inn"] = "taverne",
        ["tavern"] = "taverne",
        ["tower"] = "turm",
        ["guardhouse"] = "wachstube",
        ["guardtower"] = "wachturm",
        ["wormhole"] = "wurmloch",
        ["citadel"] = "zitadelle",
        ["fortification"] = "befestigung",
    };

    // Colors
    private static readonly Color FortColor = Color.FromRgb(0x8A, 0x5A, 0x44);
    private static readonly Color WaterColor = Color.FromRgb(0x3F, 0x6F, 0xA8);
    private static readonly Color ForestColor = Color.FromRgb(0x3F, 0x7A, 0x46);
    private static readonly Color StoneColor = Color.FromRgb(0x7A, 0x7A, 0x7A);
    private static readonly Color SandColor = Color.FromRgb(0xB0, 0x8A, 0x3E);
    private static readonly Color IceColor = Color.FromRgb(0x6F, 0xA8, 0xC8);
    private static readonly Color MagicColor = Color.FromRgb(0x6A, 0x4F, 0xA8);
    private static readonly Color MonumentColor = Color.FromRgb(0x9A, 0x9A, 0x9A);
    private static readonly Color DefaultColor = Color.FromRgb(0x6E, 0x7C, 0x8C);

    private static readonly Dictionary<string, BuildingVisual> Known = new()
    {
        // Fortifications (center placement, size buckets)
        ["turm"] = Fort("turm", 0),
        ["wachturm"] = Fort("wachturm", 0),
        ["wachstube"] = Fort("wachstube", 0),
        ["geruest"] = Fort("geruest", 0),
        ["grundmauern"] = Fort("grundmauern", 0),
        ["befestigung"] = Fort("befestigung", 1),
        ["burg"] = Fort("burg", 1),
        ["festung"] = Fort("festung", 2),
        ["zitadelle"] = Fort("zitadelle", 2),

        // Water-bound
        ["hafen"] = V("hafen", BuildingShape.Circle, WaterColor, affinity: Terrains.OCEAN),
        ["leuchtturm"] = V("leuchtturm", BuildingShape.Triangle, WaterColor, affinity: Terrains.OCEAN),

        // Resource buildings with a terrain affinity
        ["saegewerk"] = V("saegewerk", BuildingShape.Square, ForestColor, affinity: Terrains.FOREST),
        ["pferdezucht"] = V("pferdezucht", BuildingShape.Square, ForestColor, affinity: Terrains.PLAINS),
        ["steinbruch"] = V("steinbruch", BuildingShape.Square, StoneColor, affinity: Terrains.MOUNTAIN),
        ["bergwerk"] = V("bergwerk", BuildingShape.Square, StoneColor, affinity: Terrains.MOUNTAIN),
        ["damm"] = V("damm", BuildingShape.Rhombus, Color.FromRgb(0x3E, 0x7C, 0x7C), affinity: Terrains.SWAMP),
        ["karawanserei"] = V("karawanserei", BuildingShape.Square, SandColor, affinity: Terrains.DESERT),
        ["tunnel"] = V("tunnel", BuildingShape.Rhombus, IceColor, affinity: Terrains.GLACIER),

        // Other buildings
        ["magierturm"] = V("magierturm", BuildingShape.Triangle, MagicColor),
        ["akademie"] = V("akademie", BuildingShape.Square, DefaultColor),
        ["schmiede"] = V("schmiede", BuildingShape.Square, Color.FromRgb(0x8A, 0x4A, 0x3A)),
        ["taverne"] = V("taverne", BuildingShape.Circle, DefaultColor),
        ["handelsposten"] = V("handelsposten", BuildingShape.Rhombus, SandColor),
        ["marktplatz"] = V("marktplatz", BuildingShape.Circle, SandColor),
        ["portal"] = V("portal", BuildingShape.Circle, MagicColor),
        ["wurmloch"] = V("wurmloch", BuildingShape.Circle, MagicColor),
        ["monument"] = V("monument", BuildingShape.Triangle, MonumentColor, priority: 20),
        ["steinkreis"] = V("steinkreis", BuildingShape.Circle, MonumentColor, priority: 20),
    };

    private static BuildingVisual V(
        string key,
        BuildingShape shape,
        Color color,
        int affinity = -1,
        int priority = 10)
    {
        return new BuildingVisual
        {
            TypeKey = key,
            FallbackShape = shape,
            FallbackColor = color,
            AffinityTerrain = affinity,
            LayerPriority = priority,
        };
    }

    private static BuildingVisual Fort(string key, int bucket)
    {
        return new BuildingVisual
        {
            TypeKey = key,
            FallbackShape = BuildingShape.Square,
            FallbackColor = FortColor,
            IsFortification = true,
            SizeBucket = bucket,
            LayerPriority = 0,
        };
    }

    private static BuildingVisual UnknownVisual(string key)
    {
        return new BuildingVisual
        {
            TypeKey = key,
            FallbackShape = BuildingShape.Square,
            FallbackColor = DefaultColor,
        };
    }

    /// <summary>Resolves a report building type into a <see cref="BuildingVisual"/>.</summary>
    public static BuildingVisual Resolve(string? type)
    {
        string key = Normalize(type);
        if (key.Length == 0)
        {
            return UnknownVisual("unknown");
        }

        if (Aliases.TryGetValue(key, out string? canonical))
        {
            key = canonical;
        }

        string iconPath = IconCatalogService.ResolvePath(IconCategories.Building, type, "buildings")
            ?? IconCatalogService.ResolvePath(IconCategories.Building, key, "buildings")
            ?? ExplorerFallbackIconPath;

        BuildingVisual visual = Known.TryGetValue(key, out BuildingVisual? known) && known is not null
            ? known
            : UnknownVisual(key);

        return visual with { IconPath = iconPath };
    }

    /// <summary>
    /// Returns the small Explorer icon for a building type from the English icon catalog.
    /// Unknown types fall back to the generic building icon.
    /// </summary>
    public static string GetExplorerIconPath(string? type) =>
        IconCatalogService.ResolvePath(IconCategories.Building, type, "buildings")
        ?? ExplorerFallbackIconPath;

    /// <summary>
    /// Normalizes a type name: lowercase, folds German umlauts and drops separators so that
    /// "Sägewerk" or "Akademie der Künste" can be matched against icon catalog aliases.
    /// </summary>
    public static string Normalize(string? type)
    {
        if (string.IsNullOrEmpty(type))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(type.Length);
        foreach (char c in type.Trim().ToLowerInvariant())
        {
            switch (c)
            {
                case 'ä':
                    sb.Append("ae");
                    break;
                case 'ö':
                    sb.Append("oe");
                    break;
                case 'ü':
                    sb.Append("ue");
                    break;
                case 'ß':
                    sb.Append("ss");
                    break;
                case ' ':
                case '-':
                case '_':
                case '.':
                    break;
                default:
                    if (char.IsLetterOrDigit(c))
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }
}
