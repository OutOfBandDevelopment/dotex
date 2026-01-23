namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents the descendants operator (//) in JSON Path expressions.
/// This operator recursively searches for matching elements at any depth in the document structure.
/// </summary>
public class DescendantsPathSegment : IPathSegment
{
    /// <summary>
    /// Returns the string representation of the descendants operator.
    /// </summary>
    /// <returns>The string "//".</returns>
    public override string ToString() => $"//";
}