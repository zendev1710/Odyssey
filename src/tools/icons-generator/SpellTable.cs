using System.Text.RegularExpressions;

namespace Odyssey.Icons.Generator;

/// <summary>
/// Extracts the registered spell identifiers from the server's <c>spell_functions[]</c> table.
/// </summary>
public static partial class SpellTable
{
    [GeneratedRegex("^\\{\\s*\"([^\"]+)\"")]
    private static partial Regex EntryPattern();

    public static IReadOnlyList<string> Parse(string path)
    {
        var names = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var inTable = false;

        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();

            if (!inTable)
            {
                if (line.Contains("spell_functions[]", StringComparison.Ordinal) && line.Contains('{'))
                {
                    inTable = true;
                }

                continue;
            }

            if (line.StartsWith("};", StringComparison.Ordinal))
            {
                break;
            }

            var match = EntryPattern().Match(line);
            if (match.Success && seen.Add(match.Groups[1].Value))
            {
                names.Add(match.Groups[1].Value);
            }
        }

        if (!inTable)
        {
            throw new InvalidOperationException($"spell_functions[] not found in '{path}'.");
        }

        return names;
    }
}
