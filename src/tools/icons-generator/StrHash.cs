using System.Text;

namespace Odyssey.Icons.Generator;

/// <summary>
/// Reproduction of the server-side <c>str_hash</c> function (eressea/server/clibs/strings.c).
/// </summary>
/// <remarks>
/// The server accumulates into a signed 32-bit integer with wrapping overflow, then
/// clears the sign bit:
/// <code>
/// int key = 0;
/// while (*s) key = key * 37 + *s++;
/// return key &amp; 0x7FFFFFFF;
/// </code>
/// The result is the identifier emitted in <c>ZAUBER</c> report blocks. Spell identifiers
/// are ASCII, so the signed/unsigned interpretation of each byte does not matter in
/// practice; <see cref="sbyte"/> is used to mirror the server's <c>char</c> arithmetic.
/// </remarks>
public static class StrHash
{
    public static int Compute(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var key = 0;
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            unchecked
            {
                key = key * 37 + (sbyte)b;
            }
        }

        return key & 0x7FFFFFFF;
    }
}
