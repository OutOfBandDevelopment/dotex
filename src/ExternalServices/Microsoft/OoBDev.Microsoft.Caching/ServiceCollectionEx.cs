using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Microsoft.Caching;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register Microsoft in-memory caching services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceCollectionEx
{
    /// <summary>
    /// Registers Microsoft in-memory caching provider services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddMicrosoftCachingServices(this IServiceCollection services) =>
        new MicrosoftCachingRegistrar().AddServices(services);
}
