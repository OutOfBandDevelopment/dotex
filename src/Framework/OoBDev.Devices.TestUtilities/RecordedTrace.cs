using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.Devices.TestUtilities;

/// <summary>Reads byte-exact device traces written as hex text, so a recorded capture can sit in a test next to a comment naming its source.</summary>
public static class RecordedTrace
{
    /// <summary>Parses hex bytes separated by spaces, commas, dashes or line breaks; an optional <c>0x</c> prefix is allowed on each byte.</summary>
    /// <param name="hex">The trace text.</param>
    /// <returns>The bytes.</returns>
    /// <exception cref="FormatException">A token is not a one or two digit hex byte.</exception>
    public static byte[] Bytes(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);
        var tokens = hex.Split([' ', ',', '-', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries);
        return [.. tokens.Select(ParseByte)];
    }

    /// <summary>Parses a trace with one frame per non-empty line; a line starting with <c>#</c> is a comment.</summary>
    /// <param name="hexLines">The trace text.</param>
    /// <returns>One byte array per frame.</returns>
    public static IReadOnlyList<byte[]> Frames(string hexLines)
    {
        ArgumentNullException.ThrowIfNull(hexLines);
        return [.. hexLines
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && l[0] != '#')
            .Select(Bytes)];
    }

    private static byte ParseByte(string token)
    {
        var digits = token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? token[2..] : token;
        return digits.Length is 1 or 2 && byte.TryParse(digits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new FormatException($"'{token}' is not a hex byte.");
    }
}
