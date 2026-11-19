namespace OoBDev.Query.Metadata;

/// <summary>One link of shape <c>graph</c>: the two end ids as text and the weight.</summary>
public sealed class GraphEdge
{
    /// <summary>The first end.</summary>
    public string Source { get; set; } = "";

    /// <summary>The second end.</summary>
    public string Target { get; set; } = "";

    /// <summary>The weight the kind declares.</summary>
    public double Weight { get; set; }
}
