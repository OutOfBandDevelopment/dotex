using OoBDev.System.IO.Messages;
using System;
using System.Buffers;

namespace OoBDev.Fencing.ScoreMachines.Favero;

/// <summary>Decodes one frame cut for a <see cref="FaveroDevice"/>.</summary>
public sealed class FaveroMessageDecoder : IMessageDecoder<IScoreMachineState>
{
    private readonly FaveroStateParser _parser = new();

    /// <inheritdoc/>
    public IScoreMachineState Decode(ReadOnlySequence<byte> response) =>
        _parser.Parse(response.ToArray()) ?? throw new FormatException("Empty Favero frame.");
}
