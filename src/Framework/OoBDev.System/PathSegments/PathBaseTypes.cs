namespace OoBDev.System.PathSegments;

/// <summary>
/// Defines the base type of a JSON Path expression, indicating whether it starts from the root or is relative.
/// </summary>
public enum PathBaseTypes
{
    /// <summary>
    /// Indicates a root path starting from the document root (represented by ":" or "$").
    /// </summary>
    Root,

    /// <summary>
    /// Indicates a relative path starting from the current context (represented by ".").
    /// </summary>
    Relative,
}
