// Ignore Spelling: Ldap

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Represents an LDAP substring filter that matches when an attribute value starts with a specific prefix.
/// Equivalent to the LDAP filter syntax (attributeName=value*).
/// </summary>
public record LdapStartsWith : LdapSimpleFilter
{
    /// <summary>
    /// Initializes a new instance of the LdapStartsWith class for matching attribute values that start with the specified prefix.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to filter on.</param>
    /// <param name="value">The prefix value that the attribute must start with.</param>
    public LdapStartsWith(string attributeName, string value)
        : base(attributeName, LdapFilterTypes.Equals, value, "*")
    {
    }
}
