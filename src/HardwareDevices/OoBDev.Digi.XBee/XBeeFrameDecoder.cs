using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.Digi.XBee;

/// <summary>Decodes the frame data of a received API frame.</summary>
public static class XBeeFrameDecoder
{
    /// <summary>Reads one whole frame (envelope included) and decodes it.</summary>
    /// <param name="raw">The raw bytes of one frame, as <see cref="XBeeSegmenter"/> delivers them.</param>
    /// <param name="escaped">True for API mode 2.</param>
    /// <param name="frame">The decoded frame.</param>
    /// <returns>True when the bytes are exactly one valid frame.</returns>
    public static bool TryDecode(ReadOnlySpan<byte> raw, bool escaped, out XBeeFrame? frame)
    {
        frame = null;
        if (XBeeFraming.TryRead(raw, escaped, out var data, out var consumed) != XBeeReadStatus.Complete || consumed != raw.Length)
            return false;
        frame = DecodeData(data);
        return true;
    }

    /// <summary>Decodes frame data (frame type byte first). A short or malformed body of a known type comes back as <see cref="XBeeUnknownFrame"/>.</summary>
    /// <param name="data">The frame data.</param>
    /// <returns>The frame.</returns>
    public static XBeeFrame DecodeData(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            throw new ArgumentException("Frame data needs at least the frame type byte.", nameof(data));
        var type = data[0];
        var body = data[1..];
        return type switch
        {
            XBeeFrameTypes.AtCommandResponse when body.Length >= 4 =>
                new XBeeAtCommandResponse(body[0], Encoding.ASCII.GetString(body.Slice(1, 2)), body[3], body[4..].ToArray()),
            XBeeFrameTypes.ReceivePacket when body.Length >= 11 =>
                new XBeeReceivePacket(BinaryPrimitives.ReadUInt64BigEndian(body), BinaryPrimitives.ReadUInt16BigEndian(body[8..]), body[10], body[11..].ToArray()),
            XBeeFrameTypes.IoDataSample when body.Length >= 15 && Sample(body) is { } sample => sample,
            _ => new XBeeUnknownFrame(type, body.ToArray()),
        };
    }

    private static XBeeIoSample? Sample(ReadOnlySpan<byte> body)
    {
        var digitalMask = BinaryPrimitives.ReadUInt16BigEndian(body[12..]);
        var analogMask = body[14];
        var position = 15;
        ushort digital = 0;
        if (digitalMask != 0)
        {
            if (body.Length < position + 2)
                return null;
            digital = BinaryPrimitives.ReadUInt16BigEndian(body[position..]);
            position += 2;
        }

        var analog = new Dictionary<int, ushort>();
        for (var line = 0; line < 8; line++)
        {
            if ((analogMask & (1 << line)) == 0)
                continue;
            if (body.Length < position + 2)
                return null;
            analog[line] = BinaryPrimitives.ReadUInt16BigEndian(body[position..]);
            position += 2;
        }

        return new XBeeIoSample(BinaryPrimitives.ReadUInt64BigEndian(body), BinaryPrimitives.ReadUInt16BigEndian(body[8..]), body[10], digitalMask, digital, analog);
    }
}
