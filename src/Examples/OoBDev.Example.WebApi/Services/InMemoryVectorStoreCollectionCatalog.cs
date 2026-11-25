using OoBDev.Search.Semantic;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Services;

/// <summary>Lists the collections of <see cref="InMemoryVectorStoreProviderFactory"/>.</summary>
public sealed class InMemoryVectorStoreCollectionCatalog : IVectorStoreCollectionCatalog
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> ListCollectionsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(InMemoryVectorStoreProviderFactory.CollectionNames());
}
