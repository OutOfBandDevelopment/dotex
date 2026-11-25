using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.System.Retro.Apple2.ApplesoftBASIC;

/// <summary>
/// ApplesSoft Basic detokenizer
/// </summary>
public class Detokenizer
{
    private readonly TokenMap _map = new();

    /// <summary>
    /// Transforms tokenized program bytes into readable AppleSoft BASIC lines. Reading past the end yields zero bytes, so a
    /// truncated program ends its last line instead of throwing.
    /// </summary>
    /// <param name="input">The tokenized program: a two-byte size header, then lines of next-address, line number, content and a zero terminator.</param>
    /// <returns>The first element describes the file size; each following element is one program line.</returns>
    public IReadOnlyList<string> GetLines(ReadOnlyMemory<byte> input)
    {
        var span = input.Span;
        var pos = 0;
        var lines = new List<string> { $"$$$ FILE SIZE :{ReadUInt16(span, ref pos)}" };

        bool notDone;
        do
        {
            var addressOfNextLine = ReadUInt16(span, ref pos);
            notDone = pos < span.Length;
            var lineNumber = ReadUInt16(span, ref pos);

            if (addressOfNextLine == 0 || lineNumber == 0)
            {
                continue;
            }

            lines.Add(ReadLine(span, ref pos, lineNumber));
        } while (notDone);

        return lines;
    }

    private string ReadLine(ReadOnlySpan<byte> span, ref int pos, ushort lineNumber)
    {
        var sb = new StringBuilder($"{lineNumber} ");
        while (true)
        {
            var v = pos < span.Length ? span[pos] : (byte)0;
            pos++;
            if (v == 0)
            {
                return sb.ToString();
            }

            if ((v & 0x80) == 0)
            {
                sb.Append((char)v);
            }
            else
            {
                sb.Append(_map.GetToken(v)).Append(' ');
            }
        }
    }

    private static ushort ReadUInt16(ReadOnlySpan<byte> span, ref int pos)
    {
        var low = pos < span.Length ? span[pos] : 0;
        var high = pos + 1 < span.Length ? span[pos + 1] : 0;
        pos += 2;
        return (ushort)(low | (high << 8));
    }
}
