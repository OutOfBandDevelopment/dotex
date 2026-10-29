using OoBDev.System.IO.Messages;
using System;
using System.Buffers;

namespace OoBDev.ZWave;

/// <summary>
/// Classifies one unit cut by <see cref="ZWaveSegmenter"/> as an ACK, NAK, CAN, a checksum-valid data frame or invalid bytes.
/// </summary>
public sealed class ZWaveMessageDecoder : IMessageDecoder<ZWaveFrame>
{
    /// <inheritdoc/>
    public ZWaveFrame Decode(ReadOnlySequence<byte> response)
    {
        var raw = response.ToArray();
        if (raw.Length == 1)
        {
            return raw[0] switch
            {
                ZWaveFrame.AckByte => new ZWaveFrame(ZWaveFrameKind.Ack, raw),
                ZWaveFrame.NakByte => new ZWaveFrame(ZWaveFrameKind.Nak, raw),
                ZWaveFrame.CanByte => new ZWaveFrame(ZWaveFrameKind.Can, raw),
                _ => new ZWaveFrame(ZWaveFrameKind.Invalid, raw),
            };
        }

        var valid = raw.Length >= 5
            && raw[0] == ZWaveFrame.StartOfFrame
            && raw[1] + 2 == raw.Length
            && ZWaveFrame.Checksum(raw.AsSpan(1, raw.Length - 2)) == raw[^1];
        return new ZWaveFrame(valid ? ZWaveFrameKind.Data : ZWaveFrameKind.Invalid, raw);
    }
}
