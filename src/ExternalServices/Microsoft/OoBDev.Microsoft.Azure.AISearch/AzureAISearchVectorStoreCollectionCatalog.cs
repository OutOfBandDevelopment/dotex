using Azure;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using OoBDev.Search.Semantic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.AISearch;

/// <summary>Lists the indexes of an Azure AI Search service as vector store collections.</summary>
public sealed class AzureAISearchVectorStoreCollectionCatalog : IVectorStoreCollectionCatalog
{
    private readonly SearchIndexClient _indexClient;

    /// <summary>Creates the catalog over an index client.</summary>
    /// <param name="indexClient">The index management client.</param>
    public AzureAISearchVectorStoreCollectionCatalog(SearchIndexClient indexClient) => _indexClient = indexClient;

    /// <summary>Creates the catalog from the adapter options.</summary>
    /// <param name="options">The adapter options.</param>
    public AzureAISearchVectorStoreCollectionCatalog(IOptions<AzureAISearchOptions> options)
        : this(CreateClient(options.Value))
    {
    }

    private static SearchIndexClient CreateClient(AzureAISearchOptions settings)
    {
        var endpoint = new Uri(settings.Endpoint);
        return string.IsNullOrEmpty(settings.ApiKey)
            ? new SearchIndexClient(endpoint, new DefaultAzureCredential())
            : new SearchIndexClient(endpoint, new AzureKeyCredential(settings.ApiKey));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ListCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var names = new List<string>();
        await foreach (var name in _indexClient.GetIndexNamesAsync(cancellationToken).ConfigureAwait(false))
        {
            names.Add(name);
        }

        return [.. names.Order(StringComparer.Ordinal)];
    }
}
