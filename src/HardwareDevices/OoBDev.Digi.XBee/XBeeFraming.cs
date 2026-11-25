using System;
using System.Collections.Generic;

namespace OoBDev.Digi.XBee;

/// <summary>
/// The XBee API frame envelope: start delimiter <c>0x7E</c>, a 16-bit length, the frame data, and a checksum that makes the frame data plus the
/// checksum add up to <c>0xFF</c>. In API mode 2 the bytes <c>7E 7D 11 13</c> after the delimiter are escaped as <c>7D</c> then the byte XOR <c>20</c>;
/// the length and checksum describe the unescaped bytes.
/// </summary>
public static class XBeeFraming
{
    /// <summary>The start delimiter.</summary>
    public const byte StartDelimiter = 0x7E;

    /// <summary>The escape byte of API mode 2.</summary>
    public const byte Escape = 0x7D;

    /// <summary>The largest frame data length accepted. Radio payloads are untrusted, so a larger declared length is rejected instead of waited for.</summary>
    public const int MaxFrameData = 1600;

    /// <summary>Wraps frame data in an envelope.</summary>
    /// <param name="frameData">The frame data: frame type byte first.</param>
    /// <param name="escaped">True for API mode 2.</param>
    /// <returns>The bytes to write to the radio.</returns>
    /// <exception cref="ArgumentException">The frame data is empty or longer than <see cref="MaxFrameData"/>.</exception>
    public static byte[] Encode(ReadOnlySpan<byte> frameData, bool escaped = false)
    {
        if (frameData.Length is 0 or > MaxFrameData)
            throw new ArgumentException($"Frame data must be 1 to {MaxFrameData} bytes.", nameof(frameData));

        var plain = new byte[frameData.Length + 4];
        plain[0] = StartDelimiter;
        plain[1] = (byte)(frameData.Length >> 8);
        plain[2] = (byte)frameData.Length;
        frameData.CopyTo(plain.AsSpan(3));
        plain[^1] = Checksum(frameData);
        if (!escaped)
            return plain;

        var result = new List<byte>(plain.Length + 4) { StartDelimiter };
        foreach (var b in plain.AsSpan(1))
        {
            if (b is StartDelimiter or Escape or 0x11 or 0x13)
            {
                result.Add(Escape);
                result.Add((byte)(b ^ 0x20));
            }
            else
            {
                result.Add(b);
            }
        }

        return [.. result];
    }

    /// <summary>The checksum byte for <paramref name="frameData"/>.</summary>
    /// <param name="frameData">The frame data.</param>
    /// <returns><c>0xFF</c> minus the low byte of the sum.</returns>
    public static byte Checksum(ReadOnlySpan<byte> frameData)
    {
        var sum = 0;
        foreach (var b in frameData)
            sum += b;
        return (byte)(0xFF - (sum & 0xFF));
    }

    /// <summary>Reads one frame from the front of <paramref name="raw"/>.</summary>
    /// <param name="raw">The received bytes, starting at the start delimiter.</param>
    /// <param name="escaped">True for API mode 2.</param>
    /// <param name="frameData">The unescaped frame data (frame type first) when the status is <see cref="XBeeReadStatus.Complete"/>.</param>
    /// <param name="consumed">The number of raw bytes the frame took when complete.</param>
    /// <returns>The status.</returns>
    public static XBeeReadStatus TryRead(ReadOnlySpan<byte> raw, bool escaped, out byte[] frameData, out int consumed)
    {
        frameData = [];
        consumed = 0;
        if (raw.Length == 0)
            return XBeeReadStatus.Incomplete;
        if (raw[0] != StartDelimiter)
            return XBeeReadStatus.Invalid;

        var position = 1;
        var head = new byte[2];
        for (var i = 0; i < head.Length; i++)
        {
            var status = Next(raw, escaped, ref position, out head[i]);
            if (status != XBeeReadStatus.Complete)
                return status;
        }

        var length = (head[0] << 8) | head[1];
        if (length is 0 or > MaxFrameData)
            return XBeeReadStatus.Invalid;

        var data = new byte[length];
        for (var i = 0; i < length; i++)
        {
            var status = Next(raw, escaped, ref position, out data[i]);
            if (status != XBeeReadStatus.Complete)
                return status;
        }

        var check = Next(raw, escaped, ref position, out var checksum);
        if (check != XBeeReadStatus.Complete)
            return check;
        if (checksum != Checksum(data))
            return XBeeReadStatus.Invalid;

        frameData = data;
        consumed = position;
        return XBeeReadStatus.Complete;
    }

    private static XBeeReadStatus Next(ReadOnlySpan<byte> raw, bool escaped, ref int position, out byte value)
    {
        value = 0;
        if (position >= raw.Length)
            return XBeeReadStatus.Incomplete;
        var b = raw[position];
        if (b == StartDelimiter && escaped)
            return XBeeReadStatus.Invalid;
        if (!escaped || b != Escape)
        {
            value = b;
            position++;
            return XBeeReadStatus.Complete;
        }

        if (position + 1 >= raw.Length)
            return XBeeReadStatus.Incomplete;
        value = (byte)(raw[position + 1] ^ 0x20);
        position += 2;
        return XBeeReadStatus.Complete;
    }
}
