using OoBDev.System.Net;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BinaryDataDecoders.Net;

/// <summary>
/// Provides utility methods for converting between hexadecimal strings and byte arrays.
/// </summary>
public partial class ConvertEx
{
    /// <summary>
    /// Determines whether the specified string is a valid hexadecimal string (contains only hexadecimal digit pairs).
    /// </summary>
    /// <param name="hexString">The string to validate.</param>
    /// <returns>true if the string is a valid hexadecimal string; otherwise, false.</returns>
    public static bool IsHexString(string hexString) =>
        HexStringRegex().IsMatch(hexString);

    /// <summary>
    /// Converts a hexadecimal string to a byte array.
    /// </summary>
    /// <param name="hexString">The hexadecimal string to convert. Must contain an even number of hexadecimal digits.</param>
    /// <returns>A byte array containing the decoded hexadecimal values.</returns>
    /// <exception cref="InvalidHexadecimalStringException">Thrown when the input string is not a valid hexadecimal string.</exception>
    public static byte[] FromHexString(string hexString)
    {
        InvalidHexadecimalStringException.Check(hexString);

        var len = hexString.Length;

        var buffer = new byte[len / 2];

        for (var i = 0; i < len; i += 2)
        {
            var part = hexString.Substring(i, 2);
            var parsed = byte.Parse(part, NumberStyles.HexNumber);
            buffer[i / 2] = parsed;
        }

        return buffer;
    }

    /// <summary>
    /// Converts a byte array to its hexadecimal string representation.
    /// </summary>
    /// <param name="buffer">The byte array to convert.</param>
    /// <returns>A string containing the hexadecimal representation of the byte array.</returns>
    public static string ToHexString(byte[] buffer) => buffer.Aggregate(new StringBuilder(), (sb, v) => sb.Append(v), sb => sb.ToString());

    [GeneratedRegex("([0-9a-fA-F]{2}){1,}", RegexOptions.Compiled)]
    private static partial Regex HexStringRegex();
}
