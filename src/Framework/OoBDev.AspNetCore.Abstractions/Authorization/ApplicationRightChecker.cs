using OoBDev.System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.AspNetCore.Authorization;

/// <summary>
/// Default <see cref="IApplicationRightChecker"/>: deny by default, any one listed right is enough.
/// </summary>
public class ApplicationRightChecker : IApplicationRightChecker
{
    /// <inheritdoc />
    public RightCheckResult Check(ClaimsPrincipal principal, IReadOnlyCollection<string> requiredRights)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(requiredRights);

        if (principal.Identity?.IsAuthenticated != true)
            return RightCheckResult.Unauthenticated;
        if (requiredRights.Count == 0)
            return RightCheckResult.Allowed;

        var held = principal.FindAll(CommonClaims.ApplicationRight).Select(c => c.Value);
        return held.Any(h => requiredRights.Contains(h, StringComparer.Ordinal))
            ? RightCheckResult.Allowed
            : RightCheckResult.Forbidden;
    }
}
