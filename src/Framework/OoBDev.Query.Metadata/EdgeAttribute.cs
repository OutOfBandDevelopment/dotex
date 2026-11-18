using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Marks a kind whose rows are links between two things (a folder pair, an archive pair). Shape <c>graph</c> returns them as edges plus the nodes they touch.
/// </summary>
/// <param name="source">SQL expression for the first end.</param>
/// <param name="target">SQL expression for the second end.</param>
/// <param name="weight">SQL expression for the link strength; the strongest links come first.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EdgeAttribute(string source, string target, string weight) : Attribute
{
    /// <summary>The first end.</summary>
    public string Source { get; } = source;

    /// <summary>The second end.</summary>
    public string Target { get; } = target;

    /// <summary>The weight.</summary>
    public string Weight { get; } = weight;
}
