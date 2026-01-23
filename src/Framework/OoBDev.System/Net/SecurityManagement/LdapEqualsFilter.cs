using System;

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Represents an LDAP equality filter that matches when an attribute equals a specific value.
/// Supports GUID, byte array, and string value comparisons.
/// </summary>
public record LdapEqualsFilter : LdapSimpleFilter
{
    /// <summary>
    /// Initializes a new instance of the LdapEqualsFilter class for comparing a GUID attribute value.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to filter on.</param>
    /// <param name="value">The GUID value to match.</param>
    public LdapEqualsFilter(string attributeName, Guid value)
        : base(attributeName, LdapFilterTypes.Equals, value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LdapEqualsFilter class for comparing a byte array attribute value.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to filter on.</param>
    /// <param name="value">The byte array value to match.</param>
    public LdapEqualsFilter(string attributeName, byte[] value)
        : base(attributeName, LdapFilterTypes.Equals, value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the LdapEqualsFilter class for comparing a string attribute value.
    /// </summary>
    /// <param name="attributeName">The name of the LDAP attribute to filter on.</param>
    /// <param name="value">The string value to match.</param>
    public LdapEqualsFilter(string attributeName, string value)
        : base(attributeName, LdapFilterTypes.Equals, value)
    {
    }
}
