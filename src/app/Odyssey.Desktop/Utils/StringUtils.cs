using System;

namespace Odyssey.Utils
{
    /// <summary>
    /// Provides utility methods for string manipulation.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Determines whether the first word in the given string is numeric.
        /// A word is delimited by a space or a semicolon.
        /// This method is written to avoid unnecessary allocations.
        /// </summary>
        /// <param name="s">The input string to check.</param>
        /// <returns>
        /// True if the first word is a valid integer; otherwise, false.
        /// </returns>
        public static bool IsFirstWordNumeric(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            // Find the index of the first separator (' ' or ';')
            int sepIdx = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ' || s[i] == ';')
                {
                    sepIdx = i;
                    break;
                }
            }

            // Extract the first word without allocating a new array
            ReadOnlySpan<char> firstWord = sepIdx == -1 ? s.AsSpan() : s.AsSpan(0, sepIdx);

            // Try to parse the first word as an integer
            return int.TryParse(firstWord, out _);
        }
    }
}
