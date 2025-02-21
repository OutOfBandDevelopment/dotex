using OoBDev.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
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
    /// Gets or sets a value indicating whether an application user ID is required.
    /// </summary>
    /// <remarks>
    /// Set to <c>true</c> to require an application user ID; otherwise, set to <c>false</c>.
    /// The default value is <see cref="UserAuthorizationRequirement.RequireApplicationUserId"/>.
    /// </remarks>
    public bool RequireApplicationUserId { get; init; } = true; //TODO: should be able to detect if mapping claim is configured and use that instead

    /// <summary>
    /// Gets or sets the delegate for configuring an <see cref="AuthorizationPolicyBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Set to a delegate that configures an <see cref="AuthorizationPolicyBuilder"/>.
    /// The default value is <c>null</c>.
    /// </remarks>
    public Action<AuthorizationPolicyBuilder>? AuthorizationPolicyBuilder { get; init; } = default;
}
