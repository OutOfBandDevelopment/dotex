namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents an indexer/bracket notation in JSON Path expressions (e.g., [0], [key], [@.length-1]).
/// Used for array indexing and property access with computed names.
/// </summary>
/// <param name="child">The path segment representing the index or key expression inside the brackets.</param>
public class IndexerPathSegment(
    IPathSegment child
        ) : IPathSegment
{
    /// <summary>
    /// Gets the path segment representing the expression inside the brackets.
    /// </summary>
    public IPathSegment Child { get; } = child;

    /// <summary>
    /// Returns the string representation of the indexer in the format "[child]".
    /// </summary>
    /// <returns>A string representing the indexer notation.</returns>
    public override string ToString() => $"[{Child}]";
}