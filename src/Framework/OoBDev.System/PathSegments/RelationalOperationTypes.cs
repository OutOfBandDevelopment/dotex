namespace OoBDev.System.PathSegments;

/// <summary>
/// Defines the types of relational comparison operations supported in JSON Path filter expressions.
/// </summary>
public enum RelationalOperationTypes
{
    /// <summary>
    /// Equal comparison (==).
    /// </summary>
    Equal,

    /// <summary>
    /// Greater than or equal comparison (&gt;=).
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Greater than comparison (&gt;).
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Less than or equal comparison (&lt;=).
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Less than comparison (&lt;).
    /// </summary>
    LessThan,

    /// <summary>
    /// Not equal comparison (!=).
    /// </summary>
    NotEqual,
}
