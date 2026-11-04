using System.Text;

namespace Odyssey.Icons;

/// <summary>
/// Normalizes report names for icon lookup. The server keeps German report vocabulary,
/// while the asset tree and its manifest use English names. Normalization is deliberately
/// lossy: it folds umlauts, ignores separators and case, and only keeps letters and digits.
/// </summary>
public static class IconName
{
    public static string Normalize(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(name.Length);
        foreach (var character in name.Trim().ToLowerInvariant())
        {
            switch (character)
            {
                case 'ä':
                    builder.Append("ae");
                    break;
                case 'ö':
                    builder.Append("oe");
                    break;
                case 'ü':
                    builder.Append("ue");
                    break;
                case 'ß':
                    builder.Append("ss");
                    break;
                case ' ':
                case '-':
                case '_':
                case '.':
                    break;
                default:
                    if (char.IsLetterOrDigit(character))
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns common singular/plural spellings for an English name. This is a fallback for
    /// names that are not explicitly listed in the manifest; explicit aliases always win.
    /// </summary>
    public static IEnumerable<string> PluralVariants(string normalized)
    {
        if (normalized.Length < 3)
        {
            yield break;
        }

        if (normalized.EndsWith("ies", StringComparison.Ordinal))
        {
            yield return normalized[..^3] + "y";
        }
        else if (normalized.EndsWith("ves", StringComparison.Ordinal))
        {
            yield return normalized[..^3] + "f";
            yield return normalized[..^3] + "fe";
        }
        else if (normalized.EndsWith("es", StringComparison.Ordinal))
        {
            yield return normalized[..^2];
            yield return normalized[..^1];
        }
        else if (normalized.EndsWith('s'))
        {
            yield return normalized[..^1];
        }
        else if (normalized.EndsWith("fe", StringComparison.Ordinal))
        {
            yield return normalized[..^2] + "ves";
        }
        else if (normalized.EndsWith('f'))
        {
            yield return normalized[..^1] + "ves";
        }
        else if (normalized.EndsWith('y'))
        {
            yield return normalized[..^1] + "ies";
        }
        else
        {
            yield return normalized + "s";
        }
    }
}
