using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace OoBDev.Caching.Factories;

public class CacheableFactory : ICacheableFactory
{
    public const string DisabledConfigurationKey = "OoBDev:Caching:Disabled";

    private readonly IServiceProvider _serviceProvider;
    private readonly ICachingManager _cachingManager;
    private readonly bool _disabled;

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
