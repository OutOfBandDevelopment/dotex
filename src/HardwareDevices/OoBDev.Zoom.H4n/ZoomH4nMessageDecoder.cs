using OoBDev.System.IO.Messages;
using System;
using System.Buffers;

namespace OoBDev.Zoom.H4n;

/// <summary>
/// Decodes one status byte into <see cref="ZoomH4nStatus"/>. Undocumented bits are dropped.
/// </summary>
public sealed class ZoomH4nMessageDecoder : IMessageDecoder<ZoomH4nStatus>
{
    private const byte _documentedBits = 0x01 | 0x02 | 0x10 | 0x20 | 0x40;

    /// <inheritdoc/>
    public ZoomH4nStatus Decode(ReadOnlySequence<byte> response)
    {
        if (response.Length != 1)
            throw new ArgumentException($"A Zoom H4n status is one byte, not {response.Length}.", nameof(response));

        return (ZoomH4nStatus)(response.FirstSpan[0] & _documentedBits);
    }
}
