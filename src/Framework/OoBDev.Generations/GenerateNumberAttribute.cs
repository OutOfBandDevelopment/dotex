using System;

namespace OoBDev.Generations;

/// <summary>Generates an integer or decimal property inside a range.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class GenerateNumberAttribute : Attribute
{
    /// <summary>Smallest value, inclusive.</summary>
    public long Min { get; set; }

    /// <summary>Largest value, inclusive.</summary>
    public long Max { get; set; } = 1000;
}
