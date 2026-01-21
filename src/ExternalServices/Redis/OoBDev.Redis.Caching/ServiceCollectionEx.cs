using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Redis.Caching;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register Redis caching services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceCollectionEx
{
    /// <summary>
    /// Registers Redis distributed caching provider services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddRedisCachingServices(this IServiceCollection services) =>
        new RedisCachingRegistrar().AddServices(services);
}
