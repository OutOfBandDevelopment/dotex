using OoBDev.System.IO.Messages;
using System.Buffers;
using System.Text;

namespace OoBDev.Scpi;

/// <summary>
/// Decodes a reply to a <see cref="ScpiReply"/>: Latin-1 text with the CR and LF at the ends removed, or for a definite-length block the data bytes in <see cref="ScpiReply.Block"/>.
/// </summary>
public sealed class ScpiMessageDecoder : IMessageDecoder<ScpiReply>
{
    /// <inheritdoc/>
    public ScpiReply Decode(ReadOnlySequence<byte> response)
    {
        if (ScpiBlockHeader.TryRead(response, out var headerLength, out var dataLength) == ScpiBlockHeader.Result.Block
            && response.Length >= (long)headerLength + dataLength)
            return new(string.Empty, response.Slice(headerLength, dataLength).ToArray());

        return new(Encoding.Latin1.GetString(response.ToArray()).Trim('\r', '\n'));
    }
}
