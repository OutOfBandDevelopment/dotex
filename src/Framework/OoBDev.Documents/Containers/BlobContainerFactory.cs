using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace OoBDev.Documents.Containers;

/// <summary>
/// Represents a factory for creating blob containers.
/// </summary>
public class BlobContainerFactory : IBlobContainerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IBlobContainerProviderFactory> _factories;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobContainerFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The IOC Service provider.</param>
    /// <param name="factories">The factory used to create blob container providers.</param>
    /// <param name="logger">The logger for this service.</param>
    public BlobContainerFactory(
        IServiceProvider serviceProvider,
        IEnumerable<IBlobContainerProviderFactory> factories,
        ILogger<BlobContainerFactory> logger
        )
    {
        _serviceProvider = serviceProvider;
        _factories = factories;
        _logger = logger;
    }

    /// <summary>
    /// Creates a blob container with the specified container name.
    /// </summary>
    /// <param name="containerName">The name of the container.</param>
    /// <returns>An instance of <see cref="IBlobContainer"/>.</returns>
    public virtual IBlobContainer Create(string containerName)
    {
        var provider = _serviceProvider.GetKeyedService<IBlobContainerProvider>(containerName);

        provider ??= _factories.Select(i => i.Create(containerName))
                               .FirstOrDefault(i => i != null);

        provider ??= _serviceProvider.GetRequiredService<IBlobContainerProvider>();

        provider.ContainerName = containerName;

        _logger.LogInformation("Container: {containerName} -> {type}", containerName, provider.GetType());

        return provider;
    }

    /// <summary>
    /// Creates a blob container with a name derived from the specified type.
    /// </summary>
    /// <typeparam name="T">The type used to derive the container name.</typeparam>
    /// <returns>An instance of <see cref="IBlobContainer"/>.</returns>
    public virtual IBlobContainer<T> Create<T>() =>
        new WrappedBlobContainer<T>(
        Create(
            typeof(T).GetCustomAttribute<BlobContainerAttribute>()?.ContainerName ??
            typeof(T).GetCustomAttribute<TableAttribute>()?.Name ??
            typeof(T).Name
            ));
}
