using OoBDev.Identity;
using OoBDev.Microsoft.B2C.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Microsoft.B2C;

/// <summary>
/// Extension methods for adding Microsoft B2C services to the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Microsoft B2C services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to which Microsoft B2C services should be added.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to add services to.</param>
    /// <param name="microsoftIdentityConfigurationSection">The name for the ConfigurationSectionName.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddMicrosoftB2CServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string microsoftIdentityConfigurationSection
#else
        string microsoftIdentityConfigurationSection = nameof(MicrosoftIdentityOptions)
#endif
        )
    {
        services.TryAddTransient<IIdentityManager, ManageGraphUser>();

        services.Configure<MicrosoftIdentityOptions>(options => configuration.Bind(microsoftIdentityConfigurationSection, options));

        return services;
    }
}
