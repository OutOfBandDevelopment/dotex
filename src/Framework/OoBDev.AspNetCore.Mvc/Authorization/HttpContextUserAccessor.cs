using Microsoft.AspNetCore.Http;
using OoBDev.System.Security;
using System.Security.Claims;

namespace OoBDev.AspNetCore.Mvc.Authorization;

/// <summary>
/// Provides access to the current user from the HTTP context.
/// </summary>
/// <param name="httpContextAccessor">The accessor for the current HTTP context.</param>
public class HttpContextUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    /// <summary>
    /// Gets the principal of the current request, or <see langword="null"/> outside a request.
    /// </summary>
    public ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}
