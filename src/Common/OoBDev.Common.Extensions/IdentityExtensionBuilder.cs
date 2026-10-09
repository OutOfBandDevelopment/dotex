using OoBDev.Keycloak.Identity;

namespace OoBDev.Common.Extensions;

/// <summary>
/// Represents a builder for configuring identity extensions.
/// </summary>
public record IdentityExtensionBuilder
{
    /// <summary>
    /// Gets or sets the identity provider to use.
    /// </summary>
    /// <remarks>
    /// Specifies the identity provider for authentication. The default value is <see cref="IdentityProviders.Keycloak"/>.
    /// </remarks>
    public IdentityProviders IdentityProvider { get; init; } = IdentityProviders.Keycloak;

    /// <summary>
    /// Gets or sets the configuration section name for Keycloak identity options.
    /// </summary>
    /// <value>
    /// The configuration section name for Keycloak identity options. Default is "KeycloakIdentityOptions".
    /// </value>
    public string KeycloakIdentityConfigurationSection { get; init; } = nameof(KeycloakIdentityOptions);
}
