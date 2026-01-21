// Ignore Spelling: Ldap

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Represents an LDAP presence filter that matches when an attribute exists, regardless of its value.
/// Equivalent to the LDAP filter syntax (attributeName=*).
/// </summary>
public record LdapPresentFilter : LdapSimpleFilter
{
    /// <summary>
    /// Initializes a new instance of the LdapPresentFilter class for checking attribute presence.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to check for presence.</param>
    public LdapPresentFilter(string attributeName)
        : base(attributeName, LdapFilterTypes.Equals, "", "*")
    {
    }
}
