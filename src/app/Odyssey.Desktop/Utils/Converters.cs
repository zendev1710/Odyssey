using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Odyssey.Utils;

public static partial class Converters
{
    /// <summary>
    /// Convert an integer to string.
    /// </summary>
    /// <param name="val">integer to convert</param>
    /// <returns></returns>
    public static string ToStringVal(int val)
    {
        return val.ToString("X");
    }

    // Conversion of integer to string (FXStringVal)
    public static string IntToString(int num, int @base = 10)
    {
        if (@base < 2 || @base > 16)
        {
            // LATER: throws an error
            //fxerror("FXStringVal: base out of range.\n");
        }

        return Convert.ToString(num, @base);
    }

    public static int StringToInt(string s, int @base = 10)
    {
        return Convert.ToInt32(s, @base);
    }

    public static int StringIdToInt(string s)
    {
        return DecodeBase36(s);
    }

    public static string ToStringValEx(ulong num, uint @base)
    {
        if (@base <= 16)
        {
            return Convert.ToString((long)num, (int)@base);
        }
        if (@base == 36)
        {
            return EncodeBase36(num);
        }
        throw new ArgumentException($"Invalid base: {@base}");
    }

    public static string IdToString(int id)
    {
        return ToStringValEx((ulong)id, 36);
    }

    public static string EncodeBase36(ulong num)
    {
        char[] buf = new char[40];
        char[] b36 = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();
        int p = buf.Length - 1;
        buf[p] = '\0';
        do
        {
            buf[--p] = b36[num % 36];
            num /= 36;
        } while (num > 0);

        return new string(buf, p, buf.Length - p - 1);
    }

    /// <summary>
    /// Decode the Base36 Encoded string into a number.
    /// Reused from https://www.stum.de/2008/10/20/base36-encoderdecoder-in-c/.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int DecodeBase36(string input)
    {
        const string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";
        var reversed = input.ToLower().Reverse();
        long result = 0;
        int pos = 0;
        foreach (char c in reversed)
        {
            result += CharList.IndexOf(c) * (long)Math.Pow(36, pos);
            pos++;
        }
        return (int)result;
    }

    /// <summary>
    /// Extract the coordinates (2 or 3 integers, that can be negative) from the input string.
    /// The third coordinate is optional and will be set to 0 if not found in the input string.
    /// Each integer is separated by one or more whitespaces.
    /// </summary>
    /// <param name="input">input to extract from the coordinates</param>
    /// <param name="x">first coordinate returned as an integer</param>
    /// <param name="y">second coordinate returned as an integer</param>
    /// <param name="z">third coordinate returned as an integer</param>
    /// <returns>true if extraction was successful; otherwise false </returns>
    public static bool ExtractCoordinates(string input, out int x, out int y, out int z)
    {
        x = 0; y = 0; z = 0;
        var match = CoordinatesRegex().Match(input);
        if (match.Success)
        {
            // Extract the coordinate values
            x = int.Parse(match.Groups[1].Value);
            y = int.Parse(match.Groups[2].Value);
            string thirdValue = match.Groups.Count >= 5 ? match.Groups[4].Value : string.Empty;
            z = !string.IsNullOrEmpty(thirdValue) ? int.Parse(match.Groups[4].Value) : 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Extract the coordinates (2 integers that can be negative, separated by comma) from the input string.
    /// </summary>
    /// <param name="input">input to extract from the coordinates</param>
    /// <param name="x">first coordinate returned as an integer</param>
    /// <param name="y">second coordinate returned as an integer</param>
    /// <returns>true if extraction was successful; otherwise false </returns>
    public static bool ExtractCoordinatesWithComma(string input, out int x, out int y)
    {
        return ExtractCoordinates(input.Replace(",", " "), out x, out y, out _);
    }

    public static string ToStringWithDecimals(int value)
    {
        return value % 100 == 0 ? $"{value / 100}" : $"{(value / 100.0F):F2}";
    }

    [GeneratedRegex(@"(-?\d+)\s+(-?\d+)(\s+(-?\d+))?")]
    private static partial Regex CoordinatesRegex();
}


