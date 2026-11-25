using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using OoBDev.Search.Models;
using OoBDev.Search.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.AISearch;

/// <summary>
/// Stores vectors and finds neighbors in an Azure AI Search index.
/// </summary>
public class AzureAISearchVectorStoreProvider : IVectorStoreProvider
{
    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly IOptions<AzureAISearchOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureAISearchVectorStoreProvider"/> class.
    /// </summary>
    /// <param name="indexClient">The index management client.</param>
    /// <param name="searchClient">The client of the index named <paramref name="collectionName"/>.</param>
    /// <param name="options">The options.</param>
    /// <param name="collectionName">The index name.</param>
    public AzureAISearchVectorStoreProvider(
        SearchIndexClient indexClient,
        SearchClient searchClient,
        IOptions<AzureAISearchOptions> options,
        string collectionName)
    {
        _indexClient = indexClient;
        _searchClient = searchClient;
        _options = options;
        CollectionName = collectionName;
    }

    /// <inheritdoc />
    public string CollectionName { get; set; }

    /// <inheritdoc />
    public Task<string[]> StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, IDictionary<string, object> metadata) =>
        StoreVectorsAsync(embeddings.Select(e => (e, (IDictionary<string, object>)new Dictionary<string, object>(StringComparer.Ordinal))), metadata);

    /// <inheritdoc />
    public async Task<string[]> StoreVectorsAsync(
        IEnumerable<(ReadOnlyMemory<float> embedding, IDictionary<string, object> metadata)> items,
        IDictionary<string, object> metadata)
    {
        var list = items.ToList();
        if (list.Count == 0)
            return [];
        if (_options.Value.EnsureIndexExists)
            await _indexClient.CreateOrUpdateIndexAsync(AzureAISearchMapper.CreateIndex(CollectionName, list[0].embedding.Length)).ConfigureAwait(false);

        var documents = list
            .Select(i => AzureAISearchMapper.ToDocument(i.embedding, metadata.Concat(i.metadata)))
            .ToList();
        await _searchClient.UploadDocumentsAsync(documents).ConfigureAwait(false);
        return documents.Select(d => (string)d[AzureAISearchGlobals.IdField]).ToArray();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(IEnumerable<string> ids)
    {
        var keys = ids.ToArray();
        if (keys.Length > 0)
            await _searchClient.DeleteDocumentsAsync(AzureAISearchGlobals.IdField, keys).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SearchResultModel> ListAsync()
    {
        var options = new SearchOptions { Size = 1000, Select = { AzureAISearchGlobals.IdField, AzureAISearchGlobals.PayloadField } };
        var response = await _searchClient.SearchAsync<SearchDocument>("*", options).ConfigureAwait(false);
        await foreach (var result in response.Value.GetResultsAsync().ConfigureAwait(false))
            yield return AzureAISearchMapper.ToResult(result.Document, 0);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find)
    {
        var response = await _searchClient.SearchAsync<SearchDocument>(null, AzureAISearchMapper.CreateNeighborOptions(find, _options.Value.Top)).ConfigureAwait(false);
        await foreach (var result in response.Value.GetResultsAsync().ConfigureAwait(false))
            yield return AzureAISearchMapper.ToResult(result.Document, result.Score);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        await foreach (var item in FindNeighborsAsync(find).ConfigureAwait(false))
        {
            var key = item.MetaData is { } m && m.TryGetValue(groupBy, out var v) ? v?.ToString() ?? "" : item.ItemId;
            if (seen.Add(key))
                yield return item;
        }
    }
}
