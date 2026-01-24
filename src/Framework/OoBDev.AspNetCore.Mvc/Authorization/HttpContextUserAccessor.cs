using OoBDev.System.Security;
using System.Security.Principal;

namespace OoBDev.AspNetCore.Mvc.Authorization;

/// <summary>
/// Provides access to the current user's identity from the HTTP context.
/// </summary>
public class HttpContextUserAccessor : ICurrentUserAccessor
{
    private IIdentity _identity;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpContextUserAccessor"/> class.
    /// </summary>
    /// <param name="identity">The current user's identity.</param>
    public HttpContextUserAccessor(
        IIdentity identity
        ) => _identity = identity;

    /// <summary>
    /// Gets the current user's username.
    /// </summary>
    public string? UserName => _identity?.Name;
}
