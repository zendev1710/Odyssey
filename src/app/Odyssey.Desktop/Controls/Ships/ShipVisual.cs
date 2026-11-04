using Avalonia.Media;
using Odyssey.Controls.Buildings;
using Odyssey.Icons;
using Odyssey.Services;
using System.Collections.Generic;

namespace Odyssey.Controls.Ships;

/// <summary>Values of the "ship style" map setting.</summary>
public static class ShipStyles
{
    public const int Image = 0;   // pre-rendered ship images, with transparency
    public const int Symbol = 1;  // compact vector marker
    public const int Vector = 2;  // detailed vector silhouette
}

/// <summary>Ship silhouette used by the symbol/vector drawing modes.</summary>
public enum ShipShape
{
    Boat,
    Longboat,
    Dragonship,
    Caravel,
    Trireme,
    Galleon,
    Unknown,
}

/// <summary>Main sail shape used by the vector silhouettes.</summary>
public enum SailStyle
{
    Square,
    Lateen,
}

/// <summary>
/// Describes how a ship type is drawn on the map: which image set to use and how its
/// vector silhouette should look.
/// </summary>
public sealed record ShipVisual
{
    /// <summary>Canonical English type key, e.g. "longboat". Matches the asset file prefix.</summary>
    public string TypeKey { get; init; } = "ship";

    public ShipShape Shape { get; init; } = ShipShape.Unknown;

    public Color HullColor { get; init; } = Color.FromRgb(0x6E, 0x7C, 0x8C);

    public Color SailColor { get; init; } = Color.FromRgb(0xE6, 0xE9, 0xEC);

    public Color TrimColor { get; init; } = Color.FromRgb(0x3A, 0x46, 0x52);

    /// <summary>Number of masts drawn in vector mode (1..3).</summary>
    public int Masts { get; init; } = 1;

    public SailStyle SailStyle { get; init; } = SailStyle.Square;

    public bool HasOars { get; init; }

    public bool HasDragonHead { get; init; }

    public bool HasShields { get; init; }

    public bool HasSternCastle { get; init; }

    /// <summary>Relative size of the marker, so that a galleon looks bigger than a boat.</summary>
    public double SizeFactor { get; init; } = 1.0;

    /// <summary>
    /// Maximum size of one ship of this type, as defined by the Eressea rules. Zero when the
    /// type is unknown, which disables construction detection for that ship.
    /// </summary>
    public int MaxSize { get; init; }
}

/// <summary>
/// Resolves a ship type name (as stored in the report, usually German: "Langboot",
/// "Drachenschiff"...) to a <see cref="ShipVisual"/>. English aliases are accepted too.
/// </summary>
public static class ShipCatalog
{
    private const string IconsFolder = "/Assets/ships/visual";

    // Every spelling (German, English, French) -> canonical English asset key.
    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["boot"] = "boat",
        ["boat"] = "boat",
        ["bateau"] = "boat",

        ["langboot"] = "longboat",
        ["longboat"] = "longboat",

        ["drachenschiff"] = "dragonship",
        ["dragonship"] = "dragonship",
        ["drakkar"] = "dragonship",

        ["karavelle"] = "caravel",
        ["caravel"] = "caravel",
        ["caravelle"] = "caravel",

        ["trireme"] = "trireme",
        ["triere"] = "trireme",

        ["galeone"] = "galleon",
        ["galeon"] = "galleon",
        ["galleon"] = "galleon",
        ["galion"] = "galleon",

        ["schiff"] = "ship",
        ["ship"] = "ship",
        ["navire"] = "ship",
    };

    private static readonly Dictionary<string, ShipVisual> Known = new()
    {
        ["boat"] = new ShipVisual
        {
            TypeKey = "boat",
            Shape = ShipShape.Boat,
            HullColor = Color.FromRgb(0xA9, 0x77, 0x3F),
            SailColor = Color.FromRgb(0xF2, 0xE7, 0xCC),
            TrimColor = Color.FromRgb(0x5E, 0x3B, 0x1F),
            Masts = 1,
            SizeFactor = 0.82,
            MaxSize = 5,
        },
        ["longboat"] = new ShipVisual
        {
            TypeKey = "longboat",
            Shape = ShipShape.Longboat,
            HullColor = Color.FromRgb(0x8B, 0x5A, 0x2B),
            SailColor = Color.FromRgb(0xE8, 0xD3, 0xA8),
            TrimColor = Color.FromRgb(0x4A, 0x2E, 0x14),
            Masts = 1,
            HasShields = true,
            SizeFactor = 0.94,
            MaxSize = 50,
        },
        ["dragonship"] = new ShipVisual
        {
            TypeKey = "dragonship",
            Shape = ShipShape.Dragonship,
            HullColor = Color.FromRgb(0x7A, 0x2E, 0x24),
            SailColor = Color.FromRgb(0xE7, 0xC2, 0x7A),
            TrimColor = Color.FromRgb(0xD9, 0xA5, 0x21),
            Masts = 1,
            HasDragonHead = true,
            HasShields = true,
            SizeFactor = 1.0,
            MaxSize = 100,
        },
        ["caravel"] = new ShipVisual
        {
            TypeKey = "caravel",
            Shape = ShipShape.Caravel,
            HullColor = Color.FromRgb(0x6D, 0x4A, 0x2A),
            SailColor = Color.FromRgb(0xF5, 0xEF, 0xD8),
            TrimColor = Color.FromRgb(0x9A, 0x74, 0x40),
            Masts = 2,
            SailStyle = SailStyle.Lateen,
            SizeFactor = 1.02,
            MaxSize = 250,
        },
        ["trireme"] = new ShipVisual
        {
            TypeKey = "trireme",
            Shape = ShipShape.Trireme,
            HullColor = Color.FromRgb(0x99, 0x66, 0x33),
            SailColor = Color.FromRgb(0xE4, 0xD4, 0xA8),
            TrimColor = Color.FromRgb(0x4E, 0x32, 0x18),
            Masts = 1,
            HasOars = true,
            SizeFactor = 1.05,
            MaxSize = 200,
        },
        ["galleon"] = new ShipVisual
        {
            TypeKey = "galleon",
            Shape = ShipShape.Galleon,
            HullColor = Color.FromRgb(0x46, 0x36, 0x2A),
            SailColor = Color.FromRgb(0xF8, 0xF2, 0xE0),
            TrimColor = Color.FromRgb(0xC8, 0xA2, 0x4A),
            Masts = 3,
            HasSternCastle = true,
            SizeFactor = 1.18,
            MaxSize = 2000,
        },
        ["ship"] = new ShipVisual
        {
            TypeKey = "ship",
            Shape = ShipShape.Unknown,
            HullColor = Color.FromRgb(0x6E, 0x7C, 0x8C),
            SailColor = Color.FromRgb(0xE6, 0xE9, 0xEC),
            TrimColor = Color.FromRgb(0x3A, 0x46, 0x52),
            Masts = 1,
            SizeFactor = 0.92,
        },
    };

    /// <summary>Resolves a report ship type into a <see cref="ShipVisual"/>.</summary>
    public static ShipVisual Resolve(string? type)
    {
        string key = BuildingCatalog.Normalize(type);
        if (key.Length == 0)
        {
            return Known["ship"];
        }

        if (Aliases.TryGetValue(key, out string? canonical))
        {
            key = canonical;
        }

        return Known.TryGetValue(key, out ShipVisual? visual) ? visual : Known["ship"];
    }

    /// <summary>
    /// Returns the image resource path for a ship on the given coast.
    /// Coast values are 0..5 (NW, NE, E, SE, SW, W); -1 means "not docked".
    /// The Magellan image sets use index 0 for the center and 1..6 for the six coasts.
    /// </summary>
    public static string GetIconPath(ShipVisual visual, int coast)
    {
        int index = coast is >= 0 and <= 5 ? coast + 1 : 0;
        return $"{IconsFolder}/{visual.TypeKey}{index}.png";
    }

    /// <summary>Fallback image path, used when a type-specific image is missing.</summary>
    public static string GetGenericIconPath(int coast)
    {
        int index = coast is >= 0 and <= 5 ? coast + 1 : 0;
        return $"{IconsFolder}/ship{index}.png";
    }

    /// <summary>
    /// Returns the small Explorer icon for a ship type, using the Magellan2 ship-type icons.
    /// Unknown types fall back to the generic "ships" icon.
    /// </summary>
    public static string GetExplorerIconPath(string? type) =>
        IconCatalogService.ResolvePath(IconCategories.Ship, type, "ships")
        ?? "/Assets/ships/ships.gif";
}
