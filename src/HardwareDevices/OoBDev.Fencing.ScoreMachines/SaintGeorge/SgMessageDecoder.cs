using OoBDev.System.IO.Messages;
using System.Buffers;

namespace OoBDev.Fencing.ScoreMachines.SaintGeorge;

/// <summary>Decodes frames cut for an <see cref="SgDevice"/>. It keeps the state between frames because each frame carries one fact.</summary>
public sealed class SgMessageDecoder : IMessageDecoder<IScoreMachineState>
{
    private readonly SgStateParser _parser = new();

    /// <inheritdoc/>
    public IScoreMachineState Decode(ReadOnlySequence<byte> response) => _parser.Parse(response.ToArray())!;
}
