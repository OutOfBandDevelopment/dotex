namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a function call in JSON Path expressions (e.g., length(), min(), max()).
/// Contains the function name and its parameters as path segments.
/// </summary>
/// <param name="name">The path segment representing the function name.</param>
/// <param name="parameters">The path segment representing the function parameters.</param>
public record FunctionPathSegment(
     IPathSegment name,
     IPathSegment parameters
        ) : IPathSegment
{
    /// <summary>
    /// Gets the path segment representing the function name.
    /// </summary>
    public IPathSegment Name { get; } = name;

    /// <summary>
    /// Gets the path segment representing the function parameters.
    /// </summary>
    public IPathSegment Parameters { get; } = parameters;

    /// <summary>
    /// Returns the string representation of the function call in the format "name(parameters)".
    /// </summary>
    /// <returns>A string representing the function call.</returns>
    public override string ToString() => $"{Name}({Parameters})";
}