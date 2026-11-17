using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.System.Retro.Apple2.ApplesoftBASIC;

/// <summary>
/// Turns parsed Applesoft lines back into the tokenized binary form that <see cref="Detokenizer"/> reads: a two-byte length, then per line
/// the address of the next line, the line number and the content ending in zero, and a final zero address.
/// </summary>
public static class ApplesoftTokenizer
{
    /// <summary>The address the program starts at (<c>TXTTAB</c>), which the next-line addresses are relative to.</summary>
    public const int DefaultStartAddress = 0x0801;

    /// <summary>
    /// Tokenizes a listing.
    /// </summary>
    /// <param name="listing">The program text, one numbered line per row.</param>
    /// <returns>The tokenized program bytes.</returns>
    public static byte[] Tokenize(string listing) => Tokenize(new ApplesoftParser().ParseListing(listing));

    /// <summary>
    /// Tokenizes parsed lines.
    /// </summary>
    /// <param name="lines">The lines in program order.</param>
    /// <param name="startAddress">The address of the first line.</param>
    /// <returns>The tokenized program bytes.</returns>
    /// <exception cref="InvalidDataException">The program does not fit in 16-bit addresses.</exception>
    public static byte[] Tokenize(IReadOnlyList<ApplesoftLine> lines, int startAddress = DefaultStartAddress)
    {
        ArgumentNullException.ThrowIfNull(lines);
        var body = new List<byte>();
        foreach (var line in lines)
        {
            var content = Content(line);
            var next = startAddress + body.Count + 4 + content.Count + 1;
            if (next > ushort.MaxValue)
            {
                throw new InvalidDataException("The program is too large for 16-bit addresses.");
            }

            body.Add((byte)next);
            body.Add((byte)(next >> 8));
            body.Add((byte)line.Number);
            body.Add((byte)(line.Number >> 8));
            body.AddRange(content);
            body.Add(0);
        }

        body.AddRange([0, 0]);
        var result = new List<byte> { (byte)body.Count, (byte)(body.Count >> 8) };
        result.AddRange(body);
        return [.. result];
    }

    private static List<byte> Content(ApplesoftLine line)
    {
        var content = new List<byte>();
        for (var s = 0; s < line.Statements.Count; s++)
        {
            if (s > 0)
            {
                content.Add((byte)':');
            }

            foreach (var element in line.Statements[s].Elements)
            {
                if (element.TokenCode is { } code)
                {
                    content.Add(code);
                }
                else
                {
                    foreach (var c in element.Text)
                    {
                        content.Add((byte)(c & 0x7F));
                    }
                }
            }
        }

        return content;
    }
}
