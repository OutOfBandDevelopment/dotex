using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Represents an LDAP AND filter that combines multiple LDAP filters using the AND (&amp;) logical operator.
/// All filters in the set must match for the overall filter to match.
/// </summary>
public class LdapAndFilter : LdapFilterSetBase
{
    /// <summary>
    /// Initializes a new instance of the LdapAndFilter class with a primary filter and additional filters.
    /// </summary>
    /// <param name="filter">The first LDAP filter.</param>
    /// <param name="filterSet">Additional LDAP filters to combine with AND logic.</param>
    public LdapAndFilter(ILdapFilter filter, params ILdapFilter[] filterSet)
        : this(new[] { filter }.Concat(filterSet))
    {
    }

    /// <summary>
    /// Initializes a new instance of the LdapAndFilter class with a collection of filters.
    /// </summary>
    /// <param name="filterSet">The collection of LDAP filters to combine with AND logic.</param>
    public LdapAndFilter(IEnumerable<ILdapFilter> filterSet)
        : base(LdapFilterSetOperations.And, filterSet)
    {
    }
}
