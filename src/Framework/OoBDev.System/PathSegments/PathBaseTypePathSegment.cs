namespace OoBDev.System.PathSegments;

/// <summary>
/// Represents the base type indicator of a JSON Path expression (root ":" or relative ".").
/// This segment appears at the beginning of a path to indicate whether it's absolute or relative.
/// </summary>
/// <param name="type">The path base type (Root or Relative).</param>
public sealed class PathBaseTypePathSegment(PathBaseTypes type) : BaseValuePathSegment<PathBaseTypes>(type)
{
    /// <summary>
    /// Returns the string representation of the path base type.
    /// Root returns ":", Relative returns ".".
    /// </summary>
    /// <returns>A string representing the path base type.</returns>
    public override string ToString() => Value switch
    {
        PathBaseTypes.Root => ":",
        PathBaseTypes.Relative => ".",
        _ => $"{Value}",
    };
}
