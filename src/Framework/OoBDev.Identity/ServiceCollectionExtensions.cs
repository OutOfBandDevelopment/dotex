using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Identity;

/// <summary>
/// Provides extension methods for adding identity-related services to the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add identity-related services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which identity services should be added.</param>
    /// <param name="configuration">The configuration used to bind options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection TryAddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddTransient<IUserManagementProvider, UserManagementProvider>();
        return services;
    }
}
