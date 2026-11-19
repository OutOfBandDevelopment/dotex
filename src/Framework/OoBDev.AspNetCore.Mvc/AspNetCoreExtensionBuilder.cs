using OoBDev.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using OoBDev.System.Security.Claims;
using System;

namespace OoBDev.AspNetCore.Mvc;

/// <summary>
/// Represents a builder for configuring ASP.NET Core extensions.
/// </summary>
public record AspNetCoreExtensionBuilder
{
    /// <summary>
    /// Gets or sets a value indicating whether authentication is required by default.
    /// </summary>
    /// <remarks>
    /// Set to <c>true</c> to require authentication by default; otherwise, set to <c>false</c>.
    /// The default value is <see cref="UserAuthorizationRequirement.RequireAuthenticatedByDefault"/>.
    /// </remarks>
    public bool RequireAuthenticatedByDefault { get; init; } = UserAuthorizationRequirement.RequireAuthenticatedByDefault;

    /// <summary>
    /// Gets or sets the claim that carries the application user ID.
    /// </summary>
    /// <remarks>
    /// A token without this claim is not authorized. Set to <c>null</c> when the application has no user ID of its own;
    /// the requirement is then off. The default is <see cref="CommonClaims.UserId"/>.
    /// </remarks>
    public string? ApplicationUserIdClaim { get; init; } = CommonClaims.UserId;

    /// <summary>
    /// Gets or sets the delegate for configuring an <see cref="AuthorizationPolicyBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Set to a delegate that configures an <see cref="AuthorizationPolicyBuilder"/>.
    /// The default value is <c>null</c>.
    /// </remarks>
    public Action<AuthorizationPolicyBuilder>? AuthorizationPolicyBuilder { get; init; }
}
