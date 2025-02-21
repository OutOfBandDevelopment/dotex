using OoBDev.Keycloak.Identity;
using OoBDev.Microsoft.B2C.Identity;

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
    /// Specifies the identity provider for authentication. The default value is <see cref="IdentityProviders.AzureB2C"/>.
    /// </remarks>
    public IdentityProviders IdentityProvider { get; init; } = IdentityProviders.AzureB2C;

    /// <summary>
    /// Gets or sets the configuration section name for Microsoft Identity options.
    /// </summary>
    /// <value>
    /// The configuration section name for Microsoft Identity options. Default is "MicrosoftIdentityOptions".
    /// </value>
    public string MicrosoftIdentityConfigurationSection { get; init; } = nameof(MicrosoftIdentityOptions);

    /// <summary>
    /// Gets or sets the configuration section name for Keycloak identity options.
    /// </summary>
    /// <value>
    /// The configuration section name for Keycloak identity options. Default is "KeycloakIdentityOptions".
    /// </value>
    public string KeycloakIdentityConfigurationSection { get; init; } = nameof(KeycloakIdentityOptions);
}
