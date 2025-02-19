using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace OoBDev.Search.Semantic;

/// <summary>
/// Represents a factory for creating vector stores.
/// </summary>
public class VectorStoreFactory : IVectorStoreFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IVectorStoreProviderFactory> _factories;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorStoreFactory"/> class.
    /// </summary>
    /// <param name="factories">The factory used to create blob container providers.</param>
    /// <param name="serviceProvider">The IOC Service Provider.</param>
    public VectorStoreFactory(
         IServiceProvider serviceProvider,
        IEnumerable<IVectorStoreProviderFactory> factories
        )
    {
        _serviceProvider = serviceProvider;
        _factories = factories;
    }

    /// <summary>
    /// Creates a vector store based on the specified collection name.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The created vector store.</returns>
    public virtual IVectorStore Create(string collectionName)
    {
        var provider = _serviceProvider.GetKeyedService<IVectorStoreProvider>(collectionName);

        provider ??= _factories.Select(i => i.Create(collectionName))
                               .FirstOrDefault(i => i != null);

        provider ??= _serviceProvider.GetRequiredService<IVectorStoreProvider>();

        provider.CollectionName = collectionName;
        return provider;
    }

    /// <summary>
    /// Creates a vector store based on the specified type.
    /// </summary>
    /// <typeparam name="T">The type for which the vector store is created.</typeparam>
    /// <returns>The created vector store.</returns>
    public IVectorStore Create<T>() =>
            new WrappedVectorStore<T>(
            Create(
                typeof(T).GetCustomAttribute<VectorStoreAttribute>()?.CollectionName ??
                typeof(T).GetCustomAttribute<TableAttribute>()?.Name ??
                typeof(T).Name
                ));
}
