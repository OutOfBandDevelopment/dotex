using OoBDev.Identity;
using OoBDev.Keycloak.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Keycloak;

/// <summary>
/// Provides extension methods for configuring services related to Keycloak in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add Keycloak-related services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configuration">The configuration containing Keycloak-related settings.</param>
    /// <param name="keycloakIdentityConfigurationSection">
    /// The configuration section name for Keycloak identity options. Default is "KeycloakIdentityOptions".
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection TryAddKeycloakServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string keycloakIdentityConfigurationSection
#else
        string keycloakIdentityConfigurationSection = nameof(KeycloakIdentityOptions)
#endif
    )
    {
        services.TryAddTransient<IIdentityManager, ManageKeycloakUser>();

        services.Configure<KeycloakIdentityOptions>(options => configuration.Bind(keycloakIdentityConfigurationSection, options));

        return services;
    }
}
