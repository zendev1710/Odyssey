using System.Text;

namespace Odyssey.Icons.Generator;

/// <summary>
/// Minimal reader for gettext PO files.
/// </summary>
/// <remarks>
/// Only spell entries are relevant: <c>msgctxt "spell"</c>, <c>msgid</c> (the server
/// identifier, <c>sname</c>) and <c>msgstr</c> (the display name). Multi-line strings
/// and the usual backslash escapes are supported.
/// </remarks>
public static class PoFile
{
    public static IReadOnlyDictionary<string, string> ParseSpellTranslations(string path)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        string? context = null;
        string? id = null;
        string? translation = null;
        var field = Field.None;

        foreach (var raw in File.ReadLines(path))
        {
            var line = raw.Trim();

            if (line.Length == 0)
            {
                Flush();
                continue;
            }

            if (line[0] == '#')
            {
                continue;
            }

            if (line.StartsWith("msgctxt ", StringComparison.Ordinal))
            {
                context = Unquote(line, "msgctxt ".Length);
                field = Field.Context;
            }
            else if (line.StartsWith("msgid ", StringComparison.Ordinal))
            {
                id = Unquote(line, "msgid ".Length);
                field = Field.Id;
            }
            else if (line.StartsWith("msgstr ", StringComparison.Ordinal))
            {
                translation = Unquote(line, "msgstr ".Length);
                field = Field.Translation;
            }
            else if (line[0] == '"')
            {
                var continuation = Unescape(line.Length >= 2 && line[^1] == '"' ? line[1..^1] : line[1..]);
                switch (field)
                {
                    case Field.Context:
                        context = (context ?? string.Empty) + continuation;
                        break;
                    case Field.Id:
                        id = (id ?? string.Empty) + continuation;
                        break;
                    case Field.Translation:
                        translation = (translation ?? string.Empty) + continuation;
                        break;
                }
            }
        }

        Flush();
        return result;

        void Flush()
        {
            if (string.Equals(context, "spell", StringComparison.Ordinal)
                && !string.IsNullOrEmpty(id)
                && !result.ContainsKey(id))
            {
                result[id] = translation ?? string.Empty;
            }

            context = null;
            id = null;
            translation = null;
            field = Field.None;
        }
    }

    private static string Unquote(string line, int prefixLength)
    {
        var value = line[prefixLength..].Trim();
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        {
            value = value[1..^1];
        }

        return Unescape(value);
    }

    private static string Unescape(string value)
    {
        if (!value.Contains('\\', StringComparison.Ordinal))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length);
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != '\\' || i + 1 >= value.Length)
            {
                builder.Append(value[i]);
                continue;
            }

            i++;
            builder.Append(value[i] switch
            {
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                '"' => '"',
                '\\' => '\\',
                _ => value[i],
            });
        }

        return builder.ToString();
    }

    private enum Field
    {
        None,
        Context,
        Id,
        Translation,
    }
}
