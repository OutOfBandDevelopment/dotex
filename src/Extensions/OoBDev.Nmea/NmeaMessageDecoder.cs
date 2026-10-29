using OoBDev.System.IO.Messages;
using System;
using System.Buffers;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OoBDev.Nmea;

/// <summary>
/// Decodes one line, <c>$&lt;talker&gt;&lt;type&gt;,field,...*hh</c> and its line ending, into an <see cref="NmeaSentence"/>. NUL bytes
/// are dropped first: NMEA is printable ASCII, so a NUL can only be HID report padding.
/// </summary>
public sealed class NmeaMessageDecoder : IMessageDecoder<NmeaSentence>
{
    /// <inheritdoc/>
    public NmeaSentence Decode(ReadOnlySequence<byte> response)
    {
        var text = Encoding.ASCII.GetString(response.ToArray().Where(b => b != 0x00).ToArray()).TrimEnd('\r', '\n');
        if (text.Length < 4 || text[0] != '$')
            throw new ArgumentException("Not an NMEA sentence: it must start with '$' and have a talker and a type.", nameof(response));

        var star = text.IndexOf('*', StringComparison.Ordinal);
        var body = star >= 0 ? text[1..star] : text[1..];

        var checksum = NmeaChecksum.Absent;
        if (star >= 0 && star + 2 < text.Length)
        {
            var expected = body.Aggregate(0, (sum, c) => sum ^ c).ToString("X2", CultureInfo.InvariantCulture);
            checksum = string.Equals(expected, text.Substring(star + 1, 2), StringComparison.OrdinalIgnoreCase) ? NmeaChecksum.Valid : NmeaChecksum.Mismatch;
        }

        var parts = body.Split(',');
        if (parts[0].Length < 3)
            throw new ArgumentException("Not an NMEA sentence: the address field needs a two character talker and a type.", nameof(response));

        return new NmeaSentence(parts[0][..2], parts[0][2..], parts[1..], checksum);
    }
}
