using System;

namespace OoBDev.Generations;

/// <summary>Generates a string property.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class GenerateStringAttribute : Attribute
{
    /// <summary>Shortest length (used by <see cref="StringPreset.Word"/>).</summary>
    public int MinLength { get; set; } = 3;

    /// <summary>Longest length (used by <see cref="StringPreset.Word"/>).</summary>
    public int MaxLength { get; set; } = 12;

    /// <summary>What kind of text to make.</summary>
    public StringPreset Preset { get; set; } = StringPreset.Word;
}
