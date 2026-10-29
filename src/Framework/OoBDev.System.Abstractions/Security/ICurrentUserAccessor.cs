using System.Security.Claims;

namespace OoBDev.System.Security;

/// <summary>
/// Provides access to information about the current user.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Gets the principal of the current user, or <see langword="null"/> when there is none.
    /// </summary>
    ClaimsPrincipal? User { get; }

    /// <summary>
    /// Gets the username of the current user.
    /// </summary>
    string? UserName => User?.Identity?.Name;
}
