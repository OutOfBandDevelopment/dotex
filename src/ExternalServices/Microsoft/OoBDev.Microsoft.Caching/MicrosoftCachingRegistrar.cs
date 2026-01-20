using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Caching;
using OoBDev.Microsoft.Caching.Providers;

namespace OoBDev.Microsoft.Caching;

/// <summary>
/// Service registration for Microsoft in-memory caching provider.
/// </summary>
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class MicrosoftCachingRegistrar
{
    /// <summary>
    /// Adds Microsoft in-memory caching services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public IServiceCollection AddServices(IServiceCollection services)
    {
        services.AddMemoryCache(); // Register IMemoryCache
        services.TryAddSingleton<ICachingProvider, MicrosoftMemoryCachingProvider>();
        services.TryAddKeyedSingleton<ICachingProvider, MicrosoftMemoryCachingProvider>("MemoryCache");
        return services;
    }
}
