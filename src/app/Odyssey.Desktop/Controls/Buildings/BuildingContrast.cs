using Avalonia.Media;
using Odyssey.Models.Data;
using System;

namespace Odyssey.Controls.Buildings;

/// <summary>Colors of the contrast plate drawn behind a building icon.</summary>
public readonly record struct BuildingPlate(Color Fill, Color Rim);

/// <summary>Values of the \"building contrast\" map setting.</summary>
public static class BuildingContrastModes
{
    /// <summary>Light plate with a colored rim (best readability).</summary>
    public const int Plate = 0;

    /// <summary>No contrast aid: the icon is drawn directly on the terrain.</summary>
    public const int None = 1;

    /// <summary>Outline only: a colored rim around the icon, without any filled background.</summary>
    public const int Rim = 2;

    /// <summary>Scale factor applied to the icon size to obtain the plate size.</summary>
    public const double PlateScale = 1.12;
}

/// <summary>
/// Builds the contrast "plate" drawn behind building icons so that the small, mostly dark
/// pixel-art icons stay readable on top of the textured terrain images.
/// The plate is a very light tint of the terrain's dominant color, framed by a darker rim of the
/// same hue. This keeps the icon legible on every terrain (forest, mountain, glacier, ...) while
/// still hinting at the terrain color underneath.
/// </summary>
public static class BuildingContrast
{
    // Approximate dominant colors of the terrain images (sampled from Assets/map/terrains),
    // indexed by the Terrains.* constants.
    private static readonly (byte R, byte G, byte B)[] TerrainBase =
    [
        (120, 120, 120), // UNKNOWN
        (104, 123, 219), // OCEAN
        (124, 156, 76),  // SWAMP
        (171, 230, 110), // PLAINS
        (224, 174, 76),  // DESERT
        (126, 193, 97),  // FOREST
        (218, 192, 144), // HIGHLAND
        (138, 132, 125), // MOUNTAIN
        (212, 208, 221), // GLACIER
        (140, 134, 127), // VOLCANO
        (182, 99, 43),   // VOLCANO_ACTIVE
        (159, 170, 221), // PACKICE
        (134, 140, 168), // ICEBERG
        (124, 137, 218), // ICEFLOE
        (109, 110, 105), // CORRIDOR
        (54, 42, 8),     // WALL
        (109, 110, 105), // HALL
        (91, 91, 91),    // FOG
        (91, 91, 91),    // THICKFOG
        (251, 120, 11),  // FIREWALL
        (98, 119, 220),  // MAHLSTROM
    ];

    /// <summary>
    /// Returns the fill and rim colors of the contrast plate for the given terrain type.
    /// </summary>
    public static BuildingPlate GetPlate(int terrain)
    {
        (byte r, byte g, byte b) = terrain >= 0 && terrain < TerrainBase.Length
            ? TerrainBase[terrain]
            : TerrainBase[Terrains.UNKNOWN];

        var baseColor = Color.FromRgb(r, g, b);

        // Very light tint of the terrain color for the plate...
        Color fill = Blend(baseColor, Colors.White, 0.86);

        // ...and a much darker, saturated rim of the same hue so the plate stands out from the
        // terrain even when the terrain itself is very light (glacier, plains, desert...).
        Color rim = Blend(baseColor, Colors.Black, 0.62);

        return new BuildingPlate(
            Color.FromArgb(214, fill.R, fill.G, fill.B),
            Color.FromArgb(240, rim.R, rim.G, rim.B));
    }

    private static Color Blend(Color from, Color to, double amount)
    {
        double t = Math.Clamp(amount, 0.0, 1.0);
        return Color.FromRgb(
            (byte)Math.Round(from.R + (to.R - from.R) * t),
            (byte)Math.Round(from.G + (to.G - from.G) * t),
            (byte)Math.Round(from.B + (to.B - from.B) * t));
    }
}
