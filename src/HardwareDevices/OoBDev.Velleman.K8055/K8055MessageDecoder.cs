using OoBDev.System.IO.Messages;
using System;
using System.Buffers;

namespace OoBDev.Velleman.K8055;

/// <summary>
/// Decodes the unprompted 9-byte input report: <c>[00, 00, 03, AnalogIn1, AnalogIn2, CounterLo1, CounterHi1, CounterLo2, CounterHi2]</c>.
/// Byte 0 is the HID report id, so the digital input byte is byte 1.
/// </summary>
public sealed class K8055MessageDecoder : IMessageDecoder<K8055Reading>
{
    /// <inheritdoc/>
    public K8055Reading Decode(ReadOnlySequence<byte> response)
    {
        if (response.Length != K8055Reading.Length)
            throw new ArgumentException($"A K8055 report is {K8055Reading.Length} bytes, not {response.Length}.", nameof(response));

        Span<byte> frame = stackalloc byte[K8055Reading.Length];
        response.CopyTo(frame);
        return new K8055Reading(
            frame[1],
            frame[3],
            frame[4],
            (ushort)(frame[5] | (frame[6] << 8)),
            (ushort)(frame[7] | (frame[8] << 8)));
    }
}
