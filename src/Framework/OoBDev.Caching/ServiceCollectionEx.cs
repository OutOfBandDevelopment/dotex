using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Caching.Factories;
using OoBDev.Caching.Managers;
using OoBDev.System;

namespace OoBDev.Caching;

public static class ServiceCollectionEx
{
    public static IServiceCollection AddCachingServices(this IServiceCollection services)
    {
        services.TryAddProviders(); // Register IStringFormatter and ISelectedService<T>
        services.TryAddTransient<ICachingManager, CachingManager>();
        services.TryAddTransient<ICacheableFactory, CacheableFactory>();
        return services;
    }
}
