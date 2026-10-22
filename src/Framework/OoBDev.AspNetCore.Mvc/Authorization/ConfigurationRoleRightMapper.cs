using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.Authorization;

/// <summary>
/// Maps roles to rights from <see cref="RoleRightOptions.Roles"/>; role names compare case-insensitively.
/// </summary>
public class ConfigurationRoleRightMapper(IOptionsMonitor<RoleRightOptions> options) : IRoleRightMapper
{
    /// <inheritdoc />
    public IReadOnlySet<string> MapRights(IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);
        var map = new Dictionary<string, string[]>(options.CurrentValue.Roles, StringComparer.OrdinalIgnoreCase);
        return roles
            .Where(r => map.ContainsKey(r))
            .SelectMany(r => map[r])
            .ToHashSet(StringComparer.Ordinal);
    }
}
