using System;

namespace OoBDev.Generations;

/// <summary>Generates a list or array property with a count in a range.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class GenerateCollectionAttribute : Attribute
{
    /// <summary>Fewest items.</summary>
    public int MinCount { get; set; } = 1;

    /// <summary>Most items.</summary>
    public int MaxCount { get; set; } = 5;
}
