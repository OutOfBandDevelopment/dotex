using OoBDev.Search.Semantic;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace OoBDev.Qdrant;

/// <summary>
/// Represents a factory for creating instances of <see cref="QdrantVectorStoreProvider"/>.
/// </summary>
public class QdrantVectorStoreProviderFactory : IVectorStoreProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="QdrantVectorStoreProvider"/> class with the specified dependencies.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public QdrantVectorStoreProviderFactory(
            IServiceProvider serviceProvider
            ) => _serviceProvider = serviceProvider;

    /// <summary>
    /// Creates a new instance of <see cref="QdrantVectorStoreProvider"/> based on the specified collection name.
    /// </summary>
    /// <param name="containerName">The name of the collection.</param>
    /// <returns>A new instance of <see cref="QdrantVectorStoreProvider"/>.</returns>
    public IVectorStoreProvider Create(string containerName) =>
        ActivatorUtilities.CreateInstance<QdrantVectorStoreProvider>(_serviceProvider, containerName);
}
