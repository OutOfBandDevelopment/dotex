namespace OoBDev.System.Linq.Search;

/// <summary>
/// Expression builder operator options
/// </summary>
public enum Operators
{
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown,

    /// <summary>
    /// Values are equal
    /// </summary>
    EqualTo,

    /// <summary>
    /// Value is within set
    /// </summary>
    InSet,

    /// <summary>
    /// value is less than provided
    /// </summary>
    LessThan,
    /// <summary>
    /// value is less than or equal to provided
    /// </summary>
    LessThanOrEqualTo,
    /// <summary>
    /// value is greater than provided
    /// </summary>
    GreaterThan,
    /// <summary>
    /// value is less greater or equal to provided
    /// </summary>
    GreaterThanOrEqualTo,

    /// <summary>
    /// value not equal to
    /// </summary>
    NotEqualTo,
    //Ors,
    //Ands,
    //OrNull,
}
