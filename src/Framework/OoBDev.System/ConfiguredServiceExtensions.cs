using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OoBDev.System;

/// <summary>
/// Registers a service whose implementation is chosen by a configuration value.
/// </summary>
public static class ConfiguredServiceExtensions
{
    /// <summary>
    /// Registers a keyed factory that resolves <typeparamref name="TService"/> by the key found at a configuration path.
    /// Replaces the earlier <c>ISelectedService&lt;T&gt;</c> wrapper.
    /// </summary>
    /// <remarks>
    /// Consumers inject <c>[FromKeyedServices(selectedKey)] TService</c>. When the path has no value the default
    /// (un-keyed) registration is used, so registration order does not matter. A value naming a key that is not
    /// registered throws instead of silently falling back.
    /// </remarks>
    /// <typeparam name="TService">The service type to select.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationPath">The configuration path holding the implementation key, for example <c>OoBDev:CachingProvider:Type</c>.</param>
    /// <param name="selectedKey">The key the selecting factory is registered under.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddConfiguredKeyedService<TService>(
        this IServiceCollection services,
        string configurationPath,
        object selectedKey
        )
        where TService : class
    {
        services.TryAddKeyedTransient(selectedKey, (serviceProvider, _) =>
        {
            var key = serviceProvider.GetRequiredService<IConfiguration>()[configurationPath];
            if (string.IsNullOrWhiteSpace(key))
                return serviceProvider.GetRequiredService<TService>();

            return serviceProvider.GetKeyedService<TService>(key)
                ?? throw new InvalidOperationException(
                    $"Configuration '{configurationPath}' selects '{key}' but no {typeof(TService).Name} is registered with that key.");
        });
        return services;
    }
}
