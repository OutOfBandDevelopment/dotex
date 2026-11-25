using System.Collections.Generic;

namespace OoBDev.Keycloak.Identity;

/// <summary>A realm user as the read-only directory shows it.</summary>
/// <param name="Id">The Keycloak user id.</param>
/// <param name="UserName">The sign-in name.</param>
/// <param name="Email">The email address, when set.</param>
/// <param name="FirstName">The first name, when set.</param>
/// <param name="LastName">The last name, when set.</param>
/// <param name="Enabled">False when the account is disabled.</param>
/// <param name="Roles">The realm roles assigned directly to the user.</param>
public sealed record KeycloakUserSummary(
    string Id,
    string UserName,
    string? Email,
    string? FirstName,
    string? LastName,
    bool Enabled,
    IReadOnlyList<string> Roles);
