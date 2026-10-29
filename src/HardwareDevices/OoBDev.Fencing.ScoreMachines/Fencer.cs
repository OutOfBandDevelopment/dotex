using System.Diagnostics;
using System.Globalization;

namespace OoBDev.Fencing.ScoreMachines;

/// <summary>One fencer's side of the score box.</summary>
/// <param name="Score">The score.</param>
/// <param name="Cards">The penalty cards.</param>
/// <param name="Lights">The lamps lit.</param>
/// <param name="Priority">Whether the priority signal is on.</param>
[DebuggerDisplay("S{Score} L{Lights} C{Cards} P{Priority}")]
public readonly record struct Fencer(byte Score, Cards Cards, Lights Lights, bool Priority)
{
    /// <inheritdoc/>
    public override string ToString() =>
        $"S>{Score.ToString("000", CultureInfo.InvariantCulture)} L>{Lights} C>{Cards} P>{Priority}";
}
