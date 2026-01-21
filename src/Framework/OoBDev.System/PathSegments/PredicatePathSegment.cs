namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a predicate expression in JSON Path (enclosed in curly braces {}).
/// Used for complex filtering conditions in path expressions.
/// </summary>
/// <param name="child">The path segment representing the predicate expression content.</param>
public class PredicatePathSegment(
    IPathSegment child
        ) : IPathSegment
{
    /// <summary>
    /// Gets the path segment representing the predicate expression content.
    /// </summary>
    public IPathSegment Child { get; } = child;

    /// <summary>
    /// Returns the string representation of the predicate in the format "{child}".
    /// </summary>
    /// <returns>A string representing the predicate expression.</returns>
    public override string ToString() => $"{{{Child}}}";
}