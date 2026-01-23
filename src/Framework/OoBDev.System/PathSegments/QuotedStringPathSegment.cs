namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a quoted string literal in JSON Path expressions.
/// Used for string values enclosed in double quotes.
/// </summary>
/// <param name="value">The string value without the surrounding quotes.</param>
public sealed class QuotedStringPathSegment(string value) : BaseValuePathSegment<string>(value)
{
    /// <summary>
    /// Returns the string representation of the quoted string in the format "value".
    /// </summary>
    /// <returns>A string representing the quoted string literal.</returns>
    public override string ToString() => $@"""{Value}""";
}