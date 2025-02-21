using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Communications;

/// <summary>
/// Extension methods for configuring communication services in <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add communication-related services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddCommunicationsServices(this IServiceCollection services) => services;
}
