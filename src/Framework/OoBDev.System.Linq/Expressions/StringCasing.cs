namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// Specifies the string casing options for use in sorting and expression transformations.
/// </summary>
public enum StringCasing
{
    /// <summary>
    /// Use the default string casing (no transformation).
    /// </summary>
    Default = 0,

    /// <summary>
    /// Transform strings to uppercase.
    /// </summary>
    Upper = 1,

    /// <summary>
    /// Transform strings to lowercase.
    /// </summary>
    Lower = 2,
}
