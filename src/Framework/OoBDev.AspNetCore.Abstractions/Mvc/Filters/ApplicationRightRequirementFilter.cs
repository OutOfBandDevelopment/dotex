using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>
/// Authorization filter to compared application rights for user to rights required by endpoint
/// </summary>
public class ApplicationRightRequirementFilter(string[] rights) : IAuthorizationFilter
{
    private readonly IReadOnlyList<string> _rights = rights;
    private static readonly ApplicationRightChecker DefaultChecker = new();

    /// <summary>
    /// Ensure that current authenticated user matches as least one requested right
    /// </summary>
    /// <param name="context"></param>
    public virtual void OnAuthorization(AuthorizationFilterContext context)
    {
        var checker = context.HttpContext.RequestServices?.GetService<IApplicationRightChecker>() ?? DefaultChecker;
        context.Result = checker.Check(context.HttpContext.User, _rights.ToArray()) switch
        {
            RightCheckResult.Unauthenticated => new ChallengeResult(),
            RightCheckResult.Forbidden => new ForbidResult(),
            _ => context.Result,
        };
    }

    internal bool Any(IEnumerable<string> items) => items.Any(_rights.Contains);
}
