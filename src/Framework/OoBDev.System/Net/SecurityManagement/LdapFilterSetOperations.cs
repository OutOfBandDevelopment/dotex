namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Defines the logical operations that can be used to combine multiple LDAP filters in a filter set.
/// </summary>
public enum LdapFilterSetOperations
{
    /// <summary>
    /// Represents the AND (&amp;) logical operation. All filters must match.
    /// </summary>
    /// <remarks>&amp;</remarks>
    And,

    /// <summary>
    /// Represents the OR (|) logical operation. At least one filter must match.
    /// </summary>
    /// <remarks>|</remarks>
    Or,
}
