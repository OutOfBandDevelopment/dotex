using System;

namespace OoBDev.System.Time;

/// <summary>
/// A hybrid logical clock value: wall-clock ticks plus a logical counter, so events from sites
/// with skewed clocks still order consistently with causality. Ordering is physical ticks first,
/// then the counter.
/// </summary>
/// <param name="PhysicalTicks">UTC ticks of the highest wall-clock time seen.</param>
/// <param name="LogicalCounter">Tie-breaker for events at the same physical tick.</param>
public readonly record struct HybridLogicalClock(long PhysicalTicks, int LogicalCounter)
    : IComparable<HybridLogicalClock>
{
    /// <inheritdoc />
    public int CompareTo(HybridLogicalClock other)
    {
        var physical = PhysicalTicks.CompareTo(other.PhysicalTicks);
        return physical != 0 ? physical : LogicalCounter.CompareTo(other.LogicalCounter);
    }

    /// <summary>Less than.</summary>
    public static bool operator <(HybridLogicalClock left, HybridLogicalClock right) => left.CompareTo(right) < 0;

    /// <summary>Greater than.</summary>
    public static bool operator >(HybridLogicalClock left, HybridLogicalClock right) => left.CompareTo(right) > 0;

    /// <summary>Less than or equal.</summary>
    public static bool operator <=(HybridLogicalClock left, HybridLogicalClock right) => left.CompareTo(right) <= 0;

    /// <summary>Greater than or equal.</summary>
    public static bool operator >=(HybridLogicalClock left, HybridLogicalClock right) => left.CompareTo(right) >= 0;
}
