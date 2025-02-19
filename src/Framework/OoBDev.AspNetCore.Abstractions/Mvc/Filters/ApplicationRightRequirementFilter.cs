using OoBDev.System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>
/// Authorization filter to compared application rights for user to rights required by endpoint
/// </summary>
public class ApplicationRightRequirementFilter(string[] rights) : IAuthorizationFilter
{
    private readonly IReadOnlyList<string> _rights = rights;

    /// <summary>
    /// Ensure that current authenticated user matches as least one requested right
    /// </summary>
    /// <param name="context"></param>
    public virtual void OnAuthorization(AuthorizationFilterContext context)
    {
        var userAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated;
        var userRights = context.HttpContext.User.GetClaimValues(CommonClaims.ApplicationRight);

        if (userAuthenticated is null or false)
            context.Result = new ForbidResult();
        else if (_rights.Any() && !Any(userRights.Select(c => c.value)))
            context.Result = new ForbidResult();
    }

    internal bool Any(IEnumerable<string> items) => items.Any(_rights.Contains);
}
