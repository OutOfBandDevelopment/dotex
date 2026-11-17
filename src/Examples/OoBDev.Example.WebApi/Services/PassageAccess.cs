using OoBDev.AspNetCore.Authorization;
using OoBDev.WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.WebApi.Services;

/// <summary>Filters passages by the application right they require, before anything is scored, so a hit never reveals hidden text.</summary>
public static class PassageAccess
{
    /// <summary>Returns the passages the caller may see: those with no required right, and those whose right the caller holds.</summary>
    /// <param name="passages">The candidate passages.</param>
    /// <param name="user">The caller.</param>
    /// <param name="checker">The right checker (the same one the endpoints use).</param>
    /// <returns>The visible passages, in their original order.</returns>
    public static IList<RagPassage> VisibleTo(IEnumerable<RagPassage> passages, ClaimsPrincipal user, IApplicationRightChecker checker) =>
        [.. passages.Where(p => string.IsNullOrWhiteSpace(p.RequiredRight) || checker.Check(user, [p.RequiredRight]) == RightCheckResult.Allowed)];
}
