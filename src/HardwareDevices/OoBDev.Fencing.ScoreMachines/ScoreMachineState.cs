using System;

namespace OoBDev.Fencing.ScoreMachines;

/// <summary>An immutable score machine state; equal when both fencers, the clock and the match are equal.</summary>
/// <param name="Left">The left fencer.</param>
/// <param name="Right">The right fencer.</param>
/// <param name="Clock">The bout clock.</param>
/// <param name="Match">The match number.</param>
public sealed record ScoreMachineState(Fencer Left, Fencer Right, TimeSpan Clock, byte Match) : IScoreMachineState
{
    /// <summary>The state before any frame arrives.</summary>
    public static readonly IScoreMachineState Empty = new ScoreMachineState(default, default, default, default);

    /// <inheritdoc/>
    public override string ToString() => $"R:{Left} G:{Right} T:{Clock} M:{Match}";
}
