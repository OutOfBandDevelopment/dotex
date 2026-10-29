using OoBDev.System.IO.Messages;
using System.Buffers;
using System.Text;

namespace OoBDev.Scpi;

/// <summary>
/// Decodes a reply to a <see cref="ScpiReply"/>: Latin-1 text with the CR and LF at the ends removed.
/// </summary>
public sealed class ScpiMessageDecoder : IMessageDecoder<ScpiReply>
{
    /// <inheritdoc/>
    public ScpiReply Decode(ReadOnlySequence<byte> response) =>
        new(Encoding.Latin1.GetString(response.ToArray()).Trim('\r', '\n'));
}
