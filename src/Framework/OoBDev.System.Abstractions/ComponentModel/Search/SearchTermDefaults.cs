namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// Specifies default search term options for comparison.
/// </summary>
public enum SearchTermDefaults
{
    /// <summary>
    /// Represents an equal comparison for search terms.
    /// </summary>
    EqualTo,

    /// <summary>
    /// Represents a contains comparison for search terms.
    /// </summary>
    Contains,

    /// <summary>
    /// Represents a starts-with comparison for search terms.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Represents an ends-with comparison for search terms.
    /// </summary>
    EndsWith,
}
