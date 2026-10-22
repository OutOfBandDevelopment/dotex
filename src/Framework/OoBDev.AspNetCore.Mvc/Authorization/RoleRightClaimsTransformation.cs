using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Authorization;
using OoBDev.System.Security.Claims;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Authorization;

/// <summary>
/// Adds <c>app__application_right</c> claims for the principal's roles. Safe to run more than once.
/// </summary>
public class RoleRightClaimsTransformation(
    IRoleRightMapper mapper,
    IOptionsMonitor<RoleRightOptions> options
    ) : IClaimsTransformation
{
    /// <inheritdoc />
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (principal.Identity?.IsAuthenticated != true)
            return Task.FromResult(principal);

        var types = options.CurrentValue.RoleClaimTypes;
        var roles = principal.Claims.Where(c => types.Contains(c.Type, StringComparer.Ordinal)).Select(c => c.Value).Distinct();
        var held = principal.FindAll(CommonClaims.ApplicationRight).Select(c => c.Value).ToHashSet(StringComparer.Ordinal);
        var missing = mapper.MapRights(roles).Where(r => !held.Contains(r)).ToArray();
        if (missing.Length == 0)
            return Task.FromResult(principal);

        var clone = principal.Clone();
        var identity = (ClaimsIdentity?)clone.Identity ?? new ClaimsIdentity();
        foreach (var right in missing)
            identity.AddClaim(new Claim(CommonClaims.ApplicationRight, right));
        return Task.FromResult(clone);
    }
}
