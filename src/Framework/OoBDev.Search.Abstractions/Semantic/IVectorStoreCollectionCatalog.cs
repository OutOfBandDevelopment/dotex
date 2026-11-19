using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Search.Semantic;

/// <summary>
/// Lists the collections a vector store backend holds. Backends that can enumerate their collections register an implementation (the
/// browse screens use it); <see cref="IVectorStoreFactory"/> is unchanged because a collection is created on first use by name.
/// </summary>
public interface IVectorStoreCollectionCatalog
{
    /// <summary>
    /// Lists the collection names.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection names in the backend's order.</returns>
    Task<IReadOnlyList<string>> ListCollectionsAsync(CancellationToken cancellationToken = default);
}
