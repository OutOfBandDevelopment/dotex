using System;

namespace OoBDev.Fencing.ScoreMachines;

/// <summary>The penalty cards a fencer holds.</summary>
[Flags]
public enum Cards
{
    /// <summary>No card.</summary>
    None = 0x0,
    /// <summary>Yellow card (warning).</summary>
    Yellow = 0x1,
    /// <summary>Red card (penalty).</summary>
    Red = 0x2,
    /// <summary>Black card (exclusion).</summary>
    Black = 0x4,
}
