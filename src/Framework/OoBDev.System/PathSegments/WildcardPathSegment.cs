namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a wildcard selector in JSON Path expressions.
/// The wildcard (*) matches all elements in an object or array.
/// </summary>
public class WildcardPathSegment : IPathSegment
{
    /// <summary>
    /// Returns the string representation of the wildcard selector.
    /// </summary>
    /// <returns>The wildcard character "*".</returns>
    public override string ToString() => "*";
}