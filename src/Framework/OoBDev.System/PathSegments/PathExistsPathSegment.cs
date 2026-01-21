namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a path existence check in JSON Path filter expressions.
/// Used to test whether a specific path exists in the JSON structure.
/// </summary>
/// <param name="path">The binary path segment to check for existence.</param>
public class PathExistsPathSegment(BinaryPathSegment path) : BaseValuePathSegment<BinaryPathSegment>(path)
{
    /// <summary>
    /// Returns the string representation of the path existence check in the format "[path]".
    /// </summary>
    /// <returns>A string representing the path existence check.</returns>
    public override string ToString() => $"[{Value}]";
}