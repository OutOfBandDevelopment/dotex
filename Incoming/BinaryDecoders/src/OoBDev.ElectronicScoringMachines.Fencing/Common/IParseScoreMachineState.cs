using System;

namespace OoBDev.ElectronicScoringMachines.Fencing.Common;

public interface IParseScoreMachineState
{
    IScoreMachineState Parse(ReadOnlySpan<byte> frame);
}
