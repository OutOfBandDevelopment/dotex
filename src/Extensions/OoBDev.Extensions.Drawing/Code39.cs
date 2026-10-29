using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;

namespace OoBDev.Extensions.Drawing;

/// <summary>
/// Encodes text as a Code 39 barcode (<see href="https://en.wikipedia.org/wiki/Code_39"/>), either as the
/// 43-character standard set or as full ASCII through the two-character escapes.
/// </summary>
public static partial class Code39
{
    /// <summary>The height in pixels of the bitmaps produced.</summary>
    public const int BarHeight = 16;

    // Each symbol is 16 bits wide: nine elements (three wide), bars and spaces alternating, then the inter-character gap.
    private static readonly Dictionary<char, byte[]> _standard = new()
    {
        ['A'] = [0x8A, 0xE8], ['B'] = [0xA2, 0xE8], ['C'] = [0x88, 0xBA], ['D'] = [0xA8, 0xE8],
        ['E'] = [0x8A, 0x3A], ['F'] = [0xA2, 0x3A], ['G'] = [0xAB, 0x88], ['H'] = [0x8A, 0xE2],
        ['I'] = [0xA2, 0xE2], ['J'] = [0xA8, 0xE2], ['K'] = [0x8A, 0xB8], ['L'] = [0xA2, 0xB8],
        ['M'] = [0x88, 0xAE], ['N'] = [0xA8, 0xB8], ['O'] = [0x8A, 0x2E], ['P'] = [0xA2, 0x2E],
        ['Q'] = [0xAA, 0x38], ['R'] = [0x8A, 0x8E], ['S'] = [0xA2, 0x8E], ['T'] = [0xA8, 0x8E],
        ['U'] = [0x8E, 0xA8], ['V'] = [0xB8, 0xA8], ['W'] = [0x8E, 0x2A], ['X'] = [0xBA, 0x28],
        ['Y'] = [0x8E, 0x8A], ['Z'] = [0xB8, 0x8A],
        ['0'] = [0xAE, 0x22], ['1'] = [0x8B, 0xA8], ['2'] = [0xA3, 0xA8], ['3'] = [0x88, 0xEA],
        ['4'] = [0xAE, 0x28], ['5'] = [0x8B, 0x8A], ['6'] = [0xA3, 0x8A], ['7'] = [0xAE, 0x88],
        ['8'] = [0x8B, 0xA2], ['9'] = [0xA3, 0xA2],
        [' '] = [0xB8, 0xA2], ['-'] = [0xBA, 0x88], ['$'] = [0xBB, 0xBA], ['%'] = [0xAE, 0xEE],
        ['.'] = [0x8E, 0xA2], ['/'] = [0xBB, 0xAE], ['+'] = [0xBA, 0xEE], ['*'] = [0xBA, 0x22],
    };

    private static readonly Dictionary<char, string> _fullAscii = BuildFullAscii();

    [GeneratedRegex(@"^(\*[-0-9A-Z.$/+% ]+\*|[-0-9A-Z.$/+% ]+)$")]
    private static partial Regex ValidStandard();

    /// <summary>Converts text to the standard set using the full-ASCII escapes (<c>+A</c> for <c>a</c>, <c>/A</c> for <c>!</c>, and so on).</summary>
    /// <param name="message">Text of ASCII characters 0 to 127; surrounding <c>*</c> start and stop marks are removed.</param>
    /// <exception cref="InvalidOperationException">The text holds a character above 127.</exception>
    public static string ToStandard(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Any(c => !_fullAscii.ContainsKey(c)))
            throw new InvalidOperationException($"{nameof(message)} contains invalid characters");

        if (message.Length > 1 && message.StartsWith('*') && message.EndsWith('*'))
            message = message.Trim('*');

        return message.Aggregate(new StringBuilder(), (sb, c) => sb.Append(_fullAscii[c]), sb => sb.ToString());
    }

    /// <summary>Gets the bit pattern (one bit per pixel column, most significant bit first) of a standard-set message, with the <c>*</c> marks added when missing.</summary>
    /// <param name="message">Characters from <c>A-Z 0-9 - . $ / + %</c> and space, optionally wrapped in <c>*</c>.</param>
    /// <exception cref="InvalidOperationException">The text holds characters outside the standard set.</exception>
    public static byte[] ToPattern(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!ValidStandard().IsMatch(message))
            throw new InvalidOperationException($"{nameof(message)} contains invalid characters");

        if (!message.StartsWith('*'))
            message = '*' + message + '*';

        return message.SelectMany(c => _standard[c]).ToArray();
    }

    /// <summary>Draws a full-ASCII message as a 1 bit per pixel bitmap, 16 pixels per symbol and <see cref="BarHeight"/> high.</summary>
    [SupportedOSPlatform("windows")]
    public static Image EncodeFullAscii(string message) => EncodeStandard(ToStandard(message));

    /// <summary>Draws a standard-set message as a 1 bit per pixel bitmap, 16 pixels per symbol and <see cref="BarHeight"/> high.</summary>
    [SupportedOSPlatform("windows")]
    public static Image EncodeStandard(string message)
    {
        var pattern = ToPattern(message);

        var bitmap = new Bitmap(pattern.Length * 8, BarHeight, PixelFormat.Format1bppIndexed);
        var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
        try
        {
            for (var line = 0; line < BarHeight; line++)
                Marshal.Copy(pattern, 0, data.Scan0 + (data.Stride * line), pattern.Length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        return bitmap;
    }

    private static Dictionary<char, string> BuildFullAscii()
    {
        var map = new Dictionary<char, string> { [(char)0] = "%U", [' '] = " ", ['-'] = "-", ['.'] = "." };

        for (var i = 1; i <= 26; i++) map[(char)i] = "$" + (char)('A' + i - 1);
        for (var i = 27; i <= 31; i++) map[(char)i] = "%" + (char)('A' + i - 27);
        for (var i = 33; i <= 44; i++) map[(char)i] = "/" + (char)('A' + i - 33);
        map['/'] = "/O";
        for (var c = '0'; c <= '9'; c++) map[c] = c.ToString();
        map[':'] = "/Z";
        for (var i = 59; i <= 63; i++) map[(char)i] = "%" + (char)('F' + i - 59);
        map['@'] = "%V";
        for (var c = 'A'; c <= 'Z'; c++) map[c] = c.ToString();
        for (var i = 91; i <= 95; i++) map[(char)i] = "%" + (char)('K' + i - 91);
        map['`'] = "%W";
        for (var c = 'a'; c <= 'z'; c++) map[c] = "+" + char.ToUpperInvariant(c);
        for (var i = 123; i <= 127; i++) map[(char)i] = "%" + (char)('P' + i - 123);

        return map;
    }
}
