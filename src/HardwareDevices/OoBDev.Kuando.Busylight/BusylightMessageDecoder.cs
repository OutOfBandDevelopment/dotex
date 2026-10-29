using OoBDev.System.IO.Messages;
using System.Buffers;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// Decodes the poll reply into <see cref="BusylightIdentification"/>.
/// </summary>
public sealed class BusylightMessageDecoder : IMessageDecoder<BusylightIdentification>
{
    /// <inheritdoc/>
    public BusylightIdentification Decode(ReadOnlySequence<byte> response)
    {
        var bytes = response.ToArray();
        var chars = new char[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
            chars[i] = bytes[i] is >= 0x20 and < 0x7F ? (char)bytes[i] : '.';
        return new BusylightIdentification(new string(chars));
    }
}
