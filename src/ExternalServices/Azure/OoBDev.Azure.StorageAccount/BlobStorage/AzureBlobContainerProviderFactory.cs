using OoBDev.Documents.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace OoBDev.Azure.StorageAccount.BlobStorage;

/// <summary>
/// Represents a factory for creating instances of <see cref="AzureBlobContainerProvider"/>.
/// </summary>
public class AzureBlobContainerProviderFactory : IBlobContainerProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<AzureBlobProviderOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobContainerProvider"/> class with the specified dependencies.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="options">The service configurations.</param>
    public AzureBlobContainerProviderFactory(
            IServiceProvider serviceProvider,
            IOptions<AzureBlobProviderOptions> options
            )
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    /// <summary>
    /// Creates a new instance of <see cref="AzureBlobContainerProvider"/> based on the specified collection name.
    /// </summary>
    /// <param name="containerName">The name of the collection.</param>
    /// <returns>A new instance of <see cref="AzureBlobContainerProvider"/>.</returns>
    public IBlobContainerProvider? Create(string containerName) =>
        _options.Value.ConnectionString == null ? null :
        ActivatorUtilities.CreateInstance<AzureBlobContainerProvider>(_serviceProvider, containerName);
}
