using System;

namespace OoBDev.Caching;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/> to support cacheable service creation.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Creates a cacheable proxy for the specified interface and implementation.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to proxy.</typeparam>
    /// <typeparam name="TImplemention">The implementation type to wrap with caching behavior.</typeparam>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A cacheable proxy instance.</returns>
    /// <exception cref="NotSupportedException">Thrown when the cacheable factory cannot create the proxy.</exception>
    public static TInterface Cacheable<TInterface, TImplemention>(this IServiceProvider serviceProvider)
        where TImplemention : class, TInterface =>
        serviceProvider.GetService(typeof(ICacheableFactory)) switch
        {
            ICacheableFactory factory => factory.Create<TInterface, TImplemention>(),
            _ => throw new NotSupportedException()
        };
}
