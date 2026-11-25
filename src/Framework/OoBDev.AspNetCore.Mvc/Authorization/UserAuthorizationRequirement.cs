using Microsoft.AspNetCore.Authorization;

namespace OoBDev.AspNetCore.Mvc.Authorization;

/// <summary>
/// Represents an authorization requirement for user-specific authorization.
/// </summary>
public record UserAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The default value indicating whether authentication is required for authorization.
    /// </summary>
    public const bool RequireAuthenticatedByDefault = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAuthorizationRequirement"/> class.
    /// </summary>
    /// <param name="applicationUserIdClaim">The claim that carries the application user ID; <c>null</c> when the application has none, which turns the requirement off.</param>
    public UserAuthorizationRequirement(string? applicationUserIdClaim) => ApplicationUserIdClaim = applicationUserIdClaim;

    /// <summary>
    /// Gets the claim that carries the application user ID, or <c>null</c> when none is configured.
    /// </summary>
    public string? ApplicationUserIdClaim { get; init; }

    /// <summary>
    /// Gets a value indicating whether the application user ID is required for authorization (true when a claim is configured).
    /// </summary>
    public bool RequireApplicationUserId => !string.IsNullOrWhiteSpace(ApplicationUserIdClaim);
}
