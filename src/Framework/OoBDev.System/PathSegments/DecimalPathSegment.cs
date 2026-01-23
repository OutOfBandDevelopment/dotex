namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a path segment containing a decimal value.
/// </summary>
/// <param name="value">The decimal value for this path segment.</param>
public class DecimalPathSegment(decimal value) : BaseValuePathSegment<decimal>(value)
{
}