namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a string value in JSON Path expressions.
/// Used for string literals without quotes in path contexts.
/// </summary>
/// <param name="value">The string value.</param>
public sealed class StringPathSegment(string value) : BaseValuePathSegment<string>(value)
{
}