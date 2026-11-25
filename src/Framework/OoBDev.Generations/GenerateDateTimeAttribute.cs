using System;

namespace OoBDev.Generations;

/// <summary>Generates a date inside a window ending at the supplied time (the clock is never read directly).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class GenerateDateTimeAttribute : Attribute
{
    /// <summary>How many days back from now the window reaches.</summary>
    public int DaysBack { get; set; } = 365;
}
