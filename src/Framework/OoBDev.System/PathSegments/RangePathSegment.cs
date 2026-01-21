namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents an array slice or range operation in JSON Path expressions (e.g., [start:end:step]).
/// Used for selecting multiple array elements using Python-style slice notation.
/// </summary>
/// <param name="start">The starting index of the range (optional, defaults to 0 if null).</param>
/// <param name="end">The ending index of the range (optional, defaults to array length if null).</param>
/// <param name="step">The step/increment value (optional, defaults to 1 if null).</param>
public class RangePathSegment(IPathSegment<int>? start, IPathSegment<int>? end, IPathSegment<int>? step) : IPathSegment
{
    /// <summary>
    /// Gets the starting index of the range.
    /// </summary>
    public IPathSegment<int>? Start { get; } = start;

    /// <summary>
    /// Gets the ending index of the range.
    /// </summary>
    public IPathSegment<int>? End { get; } = end;

    /// <summary>
    /// Gets the step/increment value for the range.
    /// </summary>
    public IPathSegment<int>? Step { get; } = step;

    /// <summary>
    /// Returns the string representation of the range in the format "start:end:step".
    /// </summary>
    /// <returns>A string representing the range operation.</returns>
    public override string ToString() => $"{Start}:{End}:{Step}";
}
