namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Defines the comparison operators that can be used in LDAP filter expressions.
/// </summary>
public enum LdapFilterTypes
{
    /// <summary>
    /// Represents the equality (=) comparison operator.
    /// </summary>
    /// <remarks>=</remarks>
    Equals,

    /// <summary>
    /// Represents the approximate match (~=) comparison operator.
    /// </summary>
    /// <remarks>~=</remarks>
    Approximate,

    /// <summary>
    /// Represents the greater than or equal to (&gt;=) comparison operator.
    /// </summary>
    /// <remarks>&gt;=</remarks>
    GreaterThanOrEqualTo,

    /// <summary>
    /// Represents the less than or equal to (&lt;=) comparison operator.
    /// </summary>
    /// <remarks>&lt;=</remarks>
    LessThanOrEqualTo,
}
