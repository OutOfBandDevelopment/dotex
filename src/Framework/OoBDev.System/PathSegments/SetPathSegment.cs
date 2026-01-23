using System.Collections.Generic;

namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a set or collection of path segments in JSON Path expressions.
/// Used for multiple selections like [0,2,4] to select multiple array indices or properties.
/// </summary>
/// <param name="set">The collection of path segments in the set.</param>
public class SetPathSegment(
    IEnumerable<IPathSegment> set
        ) : IPathSegment
{
    /// <summary>
    /// Gets the collection of path segments in the set.
    /// </summary>
    public IEnumerable<IPathSegment> Set { get; } = set;

    /// <summary>
    /// Returns the string representation of the set as comma-separated values.
    /// </summary>
    /// <returns>A string representing the set with elements joined by commas.</returns>
    public override string ToString() => string.Join(",", Set);

    /// <summary>
    /// Represents an empty set with no elements.
    /// </summary>
    public static readonly IPathSegment Empty = new SetPathSegment([]);
}