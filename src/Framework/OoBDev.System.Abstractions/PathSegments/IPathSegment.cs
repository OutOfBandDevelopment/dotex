namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a strongly-typed segment of a path.
/// </summary>
/// <typeparam name="T">The type of the segment value.</typeparam>
public interface IPathSegment<out T> : IPathSegment
{
    /// <summary>
    /// Gets the value of the path segment.
    /// </summary>
    T Value { get; }
}

/// <summary>
/// Marker interface for path segment definitions.
/// </summary>
public interface IPathSegment
{
}
