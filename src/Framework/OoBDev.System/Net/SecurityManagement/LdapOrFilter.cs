using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Represents an LDAP OR filter that combines multiple LDAP filters using the OR (|) logical operator.
/// At least one filter in the set must match for the overall filter to match.
/// </summary>
public class LdapOrFilter : LdapFilterSetBase
{
    /// <summary>
    /// Initializes a new instance of the LdapOrFilter class with a primary filter and additional filters.
    /// </summary>
    /// <param name="filter">The first LDAP filter.</param>
    /// <param name="filterSet">Additional LDAP filters to combine with OR logic.</param>
    public LdapOrFilter(ILdapFilter filter, params ILdapFilter[] filterSet)
        : this(new[] { filter }.Concat(filterSet))
    {
    }

    /// <summary>
    /// Initializes a new instance of the LdapOrFilter class with a collection of filters.
    /// </summary>
    /// <param name="filterSet">The collection of LDAP filters to combine with OR logic.</param>
    public LdapOrFilter(IEnumerable<ILdapFilter> filterSet)
        : base(LdapFilterSetOperations.And, filterSet)
    {
    }
}
