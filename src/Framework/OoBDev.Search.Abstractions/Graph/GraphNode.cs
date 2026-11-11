using System;
using System.Collections.Generic;

namespace OoBDev.Search.Graph;

/// <summary>
/// A node of a graph: a stable identifier, a label (type) and its properties.
/// </summary>
/// <param name="Id">The node identifier, unique within the label.</param>
/// <param name="Label">The node label, for example <c>Person</c>.</param>
/// <param name="Properties">The node properties.</param>
public sealed record GraphNode(string Id, string Label, IReadOnlyDictionary<string, object?> Properties)
{
    /// <summary>
    /// Initializes a node without properties.
    /// </summary>
    /// <param name="id">The node identifier.</param>
    /// <param name="label">The node label.</param>
    public GraphNode(string id, string label)
        : this(id, label, new Dictionary<string, object?>(StringComparer.Ordinal))
    {
    }
}
