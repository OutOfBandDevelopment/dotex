using System.Collections.Generic;

namespace OoBDev.System.Net.SecurityManagement;

/// <summary>
/// Provides the base class for LDAP filter sets that combine multiple filters using logical operations (AND/OR).
/// </summary>
public abstract class LdapFilterSetBase : ILdapFilter
{
    /// <summary>
    /// Initializes a new instance of the LdapFilterSetBase class with the specified operation and filter set.
    /// </summary>
    /// <param name="operation">The logical operation (AND or OR) to apply to the filter set.</param>
    /// <param name="filterSet">The collection of LDAP filters to combine.</param>
    public LdapFilterSetBase(LdapFilterSetOperations operation, IEnumerable<ILdapFilter> filterSet)
    {
        Operation = operation;
        FilterSet = filterSet;
    }

    /// <summary>
    /// Gets the logical operation (AND or OR) used to combine the filters in this set.
    /// </summary>
    public LdapFilterSetOperations Operation { get; private set; }

    /// <summary>
    /// Gets the collection of LDAP filters that are combined by this filter set.
    /// </summary>
    public IEnumerable<ILdapFilter> FilterSet { get; private set; }
}
