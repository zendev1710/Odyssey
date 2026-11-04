using System;

namespace Odyssey.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Capitalizes the first letter of the string.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        // Using AsSpan has the best performance
        Span<char> destination = stackalloc char[1];
        value.AsSpan(0, 1).ToUpperInvariant(destination);
        return $"{destination}{value.AsSpan(1)}";
    }
}