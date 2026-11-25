using System;

namespace OoBDev.Fencing.ScoreMachines;

/// <summary>The lamps lit for one fencer.</summary>
[Flags]
public enum Lights
{
    /// <summary>No lamp.</summary>
    None = 0x0,
    /// <summary>White lamp (off target).</summary>
    White = 0x1,
    /// <summary>Yellow lamp.</summary>
    Yellow = 0x2,
    /// <summary>Colored touch lamp (red for the left fencer, green for the right).</summary>
    Touch = 0x4,
}
