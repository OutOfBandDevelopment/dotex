namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents a base class for path segments that hold a value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value held by this path segment.</typeparam>
public abstract class BaseValuePathSegment<T> : IPathSegment<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseValuePathSegment{T}"/> class.
    /// </summary>
    /// <param name="value">The value to be stored in the path segment.</param>
    protected BaseValuePathSegment(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value stored in this path segment.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Returns a string representation of this path segment's value.
    /// </summary>
    /// <returns>A string representing the value stored in this path segment.</returns>
    public override string ToString() => $"{Value}";
}
