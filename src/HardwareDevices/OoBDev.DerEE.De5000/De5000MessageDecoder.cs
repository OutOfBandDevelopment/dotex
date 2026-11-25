using OoBDev.System.IO.Messages;
using System;
using System.Buffers;

namespace OoBDev.DerEE.De5000;

/// <summary>
/// Decodes one packet cut by <see cref="De5000Segmenter"/>.
/// </summary>
public sealed class De5000MessageDecoder : IMessageDecoder<De5000Frame>
{
    /// <inheritdoc/>
    public De5000Frame Decode(ReadOnlySequence<byte> response) =>
        De5000Frame.TryParse(response.ToArray(), out var frame)
            ? frame
            : throw new FormatException("Not a DE-5000 packet.");
}
