using System;

namespace OoBDev.Fencing.ScoreMachines;

/// <summary>Turns one frame from a score machine into a state.</summary>
public interface IParseScoreMachineState
{
    /// <summary>Parses one frame.</summary>
    /// <param name="frame">The frame, as cut by the device's segmenter.</param>
    /// <returns>The state, or <see langword="null"/> when the frame is empty and the machine keeps no previous state.</returns>
    IScoreMachineState? Parse(ReadOnlySpan<byte> frame);
}
