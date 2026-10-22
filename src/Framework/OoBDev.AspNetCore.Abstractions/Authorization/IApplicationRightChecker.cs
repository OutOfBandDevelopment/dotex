using System.Collections.Generic;
using System.Security.Claims;

namespace OoBDev.AspNetCore.Authorization;

/// <summary>
/// The single access decision shared by every API surface (REST, OData, GraphQL, CGI).
/// </summary>
public interface IApplicationRightChecker
{
    /// <summary>
    /// Checks that the principal is authenticated and holds at least one of the required rights.
    /// An empty list requires authentication only.
    /// </summary>
    /// <param name="principal">The caller.</param>
    /// <param name="requiredRights">The rights, any one of which is enough.</param>
    /// <returns>The decision.</returns>
    RightCheckResult Check(ClaimsPrincipal principal, IReadOnlyCollection<string> requiredRights);
}
