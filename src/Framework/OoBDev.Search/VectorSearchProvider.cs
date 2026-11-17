using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.AI;
using OoBDev.Search.Requests;

namespace OoBDev.Search;

/// <summary>
/// Searches by meaning: the query vector is <see cref="SearchAnchor.NearVector"/>, or the embedding of <see cref="SearchAnchor.NearText"/> or the request text, and the hits are the store's nearest neighbours.
/// A vector store holds no filterable fields, so every filter is refused; a cursor and "like this item" anchors are not supported.
/// </summary>
/// <param name="store">The vector store to query.</param>
/// <param name="embeddings">Turns query text into a vector.</param>
public sealed class VectorSearchProvider(IVectorStore store, IEmbeddingProvider embeddings) : ISearchProvider
{
    /// <summary>The explanation field name of every hit.</summary>
    public const string Field = "vector";

    /// <inheritdoc />
    public IReadOnlySet<string> SupportedFilters { get; } = new HashSet<string>(StringComparer.Ordinal);

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        foreach (var filter in request.Filters)
        {
            throw new UnsupportedFilterException(filter.Name);
        }

        if (request.Cursor is not null)
        {
            throw new NotSupportedException("A vector search returns one page and has no cursor.");
        }

        if (request.Anchor?.LikeItemId is not null)
        {
            throw new NotSupportedException("Searching by an existing item is not supported by the vector provider.");
        }

        var vector = await QueryVectorAsync(request, cancellationToken).ConfigureAwait(false);
        if (vector is null)
        {
            return new SearchResult([]);
        }

        var items = new List<SearchItem>();
        await foreach (var hit in store.FindNeighborsAsync(vector.Value).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            items.Add(new SearchItem(hit.ItemId, hit.Score, new SearchExplanation(Field: Field), hit.MetaData));
            if (items.Count >= request.PageSize)
            {
                break;
            }
        }

        return new SearchResult(items);
    }

    private async Task<ReadOnlyMemory<float>?> QueryVectorAsync(SearchRequest request, CancellationToken cancellationToken)
    {
        if (request.Anchor?.NearVector is { } near)
        {
            return near;
        }

        var text = request.Anchor?.NearText ?? request.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return await embeddings.GenerateEmbeddingAsync(text, null, cancellationToken).ConfigureAwait(false);
    }
}
