using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace OoBDev.Caching.Factories;

/// <summary>
/// Factory for creating cacheable proxy instances that intercept method calls and apply caching logic.
/// </summary>
public class CacheableFactory : ICacheableFactory
{
    /// <summary>
    /// Configuration key to disable caching globally.
    /// </summary>
    public const string DisabledConfigurationKey = "OoBDev:Caching:Disabled";

    private readonly IServiceProvider _serviceProvider;
    private readonly ICachingManager _cachingManager;
    private readonly bool _disabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheableFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <param name="cachingManager">The caching manager for cache operations.</param>
    /// <param name="config">The configuration to determine if caching is disabled.</param>
    public CacheableFactory(
        IServiceProvider serviceProvider,
        ICachingManager cachingManager,
        IConfiguration config
        )
    {
        _serviceProvider = serviceProvider;
        _cachingManager = cachingManager;

        bool.TryParse(config?[DisabledConfigurationKey], out _disabled);
    }

    /// <summary>
    /// Creates a cacheable proxy instance for the specified interface and implementation.
    /// If caching is disabled, returns the raw implementation instance.
    /// </summary>
    /// <typeparam name="TInterface">The interface type to proxy.</typeparam>
    /// <typeparam name="TImplemention">The implementation type to wrap with caching behavior.</typeparam>
    /// <returns>A proxy instance that intercepts method calls and applies caching logic.</returns>
    /// <exception cref="ApplicationException">Thrown when unable to create an instance.</exception>
    public TInterface Create<TInterface, TImplemention>()
        where TImplemention : class, TInterface =>
        ActivatorUtilities.CreateInstance<TImplemention>(_serviceProvider) switch
        {
            TImplemention instance when _disabled => instance,
            TImplemention instance => CachedProxy<TInterface, TImplemention>.Create(
                instance,
                _cachingManager,
                _serviceProvider.GetRequiredService<ILogger<TImplemention>>() //TODO: what if this took ILoggerFactory instead
                ),
            _ => throw new ApplicationException($"Unable to created instance of {typeof(TImplemention)}")
        };
}
