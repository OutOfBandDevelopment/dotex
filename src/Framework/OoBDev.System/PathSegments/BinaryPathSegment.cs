namespace OoBDev.System.PathSegments;

/// <summary>
/// Path segment representation for binary operators
/// </summary>
/// <param name="left">left operation</param>
/// <param name="right">right operation</param>
public class BinaryPathSegment(
    IPathSegment left,
    IPathSegment right
        ) : IPathSegment
{
    /// <summary>
    /// Gets the left segment of the binary path.
    /// </summary>
    public IPathSegment Left { get; } = left;

    /// <summary>
    /// Gets the right segment of the binary path.
    /// </summary>
    public IPathSegment Right { get; } = right;

    /// <summary>
    /// Returns a string representation of the binary path segment in the form "Left/Right".
    /// </summary>
    /// <returns>A string representing the binary path segment.</returns>
    public override string ToString() => $"{Left}/{Right}";
}
