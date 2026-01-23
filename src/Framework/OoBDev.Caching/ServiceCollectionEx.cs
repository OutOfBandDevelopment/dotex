using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Caching.Factories;
using OoBDev.Caching.Managers;
using OoBDev.System;

namespace OoBDev.Caching;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register caching services.
/// </summary>
public static class ServiceCollectionEx
{
    /// <summary>
    /// Registers core caching services including the caching manager and cacheable factory.
    /// Also ensures required provider infrastructure is registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddCachingServices(this IServiceCollection services)
    {
        services.TryAddProviders(); // Register IStringFormatter and ISelectedService<T>
        services.TryAddTransient<ICachingManager, CachingManager>();
        services.TryAddTransient<ICacheableFactory, CacheableFactory>();
        return services;
    }
}
