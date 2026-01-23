namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a path segment containing an integer numeric value.
/// </summary>
/// <param name="value">The integer value for this path segment.</param>
public class NumericPathSegment(int value) : BaseValuePathSegment<int>(value)
{
}