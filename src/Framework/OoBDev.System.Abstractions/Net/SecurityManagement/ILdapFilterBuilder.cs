// Ignore Spelling: Ldap

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Provides functionality to build LDAP filter strings from filter definitions.
/// </summary>
public interface ILdapFilterBuilder
{
    /// <summary>
    /// Builds an LDAP filter string from the specified filter definition.
    /// </summary>
    /// <param name="filter">The LDAP filter definition.</param>
    /// <returns>The LDAP filter string, or null if the filter cannot be built.</returns>
    string? Build(ILdapFilter filter);
}