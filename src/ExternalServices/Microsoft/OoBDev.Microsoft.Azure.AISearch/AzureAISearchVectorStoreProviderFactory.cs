using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using OoBDev.Search.Semantic;
using System;

namespace OoBDev.Microsoft.Azure.AISearch;

/// <summary>
/// Creates a vector store provider for a named index.
/// </summary>
public class AzureAISearchVectorStoreProviderFactory(IOptions<AzureAISearchOptions> options) : IVectorStoreProviderFactory
{
    /// <inheritdoc />
    public IVectorStoreProvider Create(string containerName)
    {
        var settings = options.Value;
        var endpoint = new Uri(settings.Endpoint);
        if (string.IsNullOrEmpty(settings.ApiKey))
        {
            var credential = new DefaultAzureCredential();
            return new AzureAISearchVectorStoreProvider(
                new SearchIndexClient(endpoint, credential), new SearchClient(endpoint, containerName, credential), options, containerName);
        }

        var key = new AzureKeyCredential(settings.ApiKey);
        return new AzureAISearchVectorStoreProvider(
            new SearchIndexClient(endpoint, key), new SearchClient(endpoint, containerName, key), options, containerName);
    }
}
