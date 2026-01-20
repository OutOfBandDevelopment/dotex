using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Caching;
using OoBDev.Redis.Caching.Providers;

namespace OoBDev.Redis.Caching;

/// <summary>
/// Service registration for Redis caching provider.
/// </summary>
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class RedisCachingRegistrar
{
    /// <summary>
    /// Adds Redis caching services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.TryAddSingleton<ICachingProvider, RedisCachingProvider>();
        services.TryAddKeyedSingleton<ICachingProvider, RedisCachingProvider>("Redis");
        services.TryAddTransient<IConnectionMultiplexerFactory, ConnectionMultiplexerFactory>();
        return services;
    }
}
