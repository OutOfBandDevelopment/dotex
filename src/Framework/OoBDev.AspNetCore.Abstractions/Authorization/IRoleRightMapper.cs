using System.Collections.Generic;

namespace OoBDev.AspNetCore.Authorization;

/// <summary>
/// Translates identity provider roles into application rights.
/// </summary>
public interface IRoleRightMapper
{
    /// <summary>
    /// Returns the union of the rights granted to the roles. Unmapped roles grant nothing.
    /// </summary>
    /// <param name="roles">Role names.</param>
    /// <returns>The rights.</returns>
    IReadOnlySet<string> MapRights(IEnumerable<string> roles);
}
