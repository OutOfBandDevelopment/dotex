// Ignore Spelling: Ldap

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Represents an LDAP NOT filter that negates the result of another LDAP filter.
/// The filter matches when the wrapped filter does NOT match.
/// </summary>
public class LdapNotFilter : ILdapFilter
{
    /// <summary>
    /// Initializes a new instance of the LdapNotFilter class with the filter to negate.
    /// </summary>
    /// <param name="wrapped">The LDAP filter whose result will be negated.</param>
    public LdapNotFilter(ILdapFilter wrapped) => Wrapped = wrapped;

    /// <summary>
    /// Gets the LDAP filter that is being negated by this NOT filter.
    /// </summary>
    public ILdapFilter Wrapped { get; init; }

    /// <summary>
    /// Determines whether the specified object is equal to this LDAP NOT filter by comparing the wrapped filters.
    /// </summary>
    /// <param name="obj">The object to compare with this filter.</param>
    /// <returns>true if the specified object is a LdapNotFilter with an equal wrapped filter; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj switch { LdapNotFilter inner => Wrapped.Equals(inner.Wrapped), _ => false };

    /// <summary>
    /// Returns the hash code for this LDAP NOT filter based on the wrapped filter.
    /// </summary>
    /// <returns>A hash code for this filter.</returns>
    public override int GetHashCode() => new { Wrapped, }.GetHashCode();
}
