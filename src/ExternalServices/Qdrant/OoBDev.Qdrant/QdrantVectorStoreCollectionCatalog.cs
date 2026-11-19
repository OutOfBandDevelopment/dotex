using OoBDev.Search.Semantic;
using Qdrant.Client.Grpc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Qdrant;

/// <summary>Lists the collections of a Qdrant instance.</summary>
/// <param name="client">The Qdrant client.</param>
public sealed class QdrantVectorStoreCollectionCatalog(QdrantGrpcClient client) : IVectorStoreCollectionCatalog
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.Collections.ListAsync(new ListCollectionsRequest(), cancellationToken: cancellationToken).ConfigureAwait(false);
        return [.. response.Collections.Select(c => c.Name).Order(global::System.StringComparer.Ordinal)];
    }
}
