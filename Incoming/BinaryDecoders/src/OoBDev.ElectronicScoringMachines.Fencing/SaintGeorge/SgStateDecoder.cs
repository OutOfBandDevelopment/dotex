using OoBDev.ElectronicScoringMachines.Fencing.Common;
using OoBDev.IO.Messages;
using System;
using System.Buffers;

namespace OoBDev.ElectronicScoringMachines.Fencing.SaintGeorge;

public class SgStateDecoder : IMessageDecoder<IScoreMachineState>
{
    private readonly IParseScoreMachineState _parser = new SgStateParser();

    public IScoreMachineState Decode(ReadOnlySequence<byte> response)
    {
        Span<byte> buffer = new byte[response.Length];
        response.CopyTo(buffer);
        return _parser.Parse(buffer);
    }
}