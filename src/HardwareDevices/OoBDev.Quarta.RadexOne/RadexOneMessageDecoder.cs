using OoBDev.System.IO.Messages;
using System;
using System.Buffers;

namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// Decodes one reply packet. A packet whose header or checksum is invalid raises <see cref="ArgumentException"/>.
/// </summary>
public sealed class RadexOneMessageDecoder : IMessageDecoder<RadexOneReply>
{
    /// <inheritdoc/>
    public RadexOneReply Decode(ReadOnlySequence<byte> response)
    {
        var packet = response.ToArray();
        if (!RadexOneFramer.TryParseReply(packet, out var packetNumber, out var extension))
            throw new ArgumentException($"{packet.Length} bytes are not a valid Radex One reply (prefix, type marker or checksum).", nameof(response));

        return new RadexOneReply(packetNumber, extension);
    }
}
