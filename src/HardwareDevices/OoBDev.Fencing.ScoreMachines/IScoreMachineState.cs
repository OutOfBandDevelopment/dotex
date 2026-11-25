using System;

namespace OoBDev.Fencing.ScoreMachines;

/// <summary>The decoded state of a fencing score machine.</summary>
public interface IScoreMachineState
{
    /// <summary>The left fencer (red).</summary>
    Fencer Left { get; }

    /// <summary>The right fencer (green).</summary>
    Fencer Right { get; }

    /// <summary>The bout clock.</summary>
    TimeSpan Clock { get; }

    /// <summary>The match (period) number.</summary>
    byte Match { get; }
}
