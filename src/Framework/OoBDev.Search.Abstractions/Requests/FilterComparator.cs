namespace OoBDev.Search.Requests;

/// <summary>
/// How a <see cref="FieldFilter"/> compares a field with its value.
/// </summary>
public enum FilterComparator
{
    /// <summary>The field equals the value.</summary>
    Equals,

    /// <summary>The field contains the value.</summary>
    Contains,

    /// <summary>The field is greater than the value.</summary>
    Greater,

    /// <summary>The field is greater than or equal to the value.</summary>
    GreaterOrEqual,

    /// <summary>The field is less than the value.</summary>
    Less,

    /// <summary>The field is less than or equal to the value.</summary>
    LessOrEqual,
}
