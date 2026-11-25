using Microsoft.Extensions.DependencyInjection;
using OoBDev.Search.Semantic;
using System;

namespace OoBDev.Data.Vectors.SqlStore;

/// <summary>Creates <see cref="SqlVectorStoreProvider"/> instances per collection.</summary>
public class SqlVectorStoreProviderFactory : IVectorStoreProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Creates the factory.</summary>
    public SqlVectorStoreProviderFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <inheritdoc/>
    public IVectorStoreProvider Create(string containerName) =>
        ActivatorUtilities.CreateInstance<SqlVectorStoreProvider>(_serviceProvider, containerName);
}
