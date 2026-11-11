using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.AISearch.Tests;

/// <summary>
/// Calls a real Azure AI Search service. Manual execution only: needs <c>AZURE_SEARCH_ENDPOINT</c> and <c>AZURE_SEARCH_API_KEY</c>
/// and may incur cost. Each run creates and deletes its own index.
/// </summary>
[TestClass]
public class AzureAISearchLiveTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.LiveIntegration)]
    public async Task StoreAndFindNeighbors_RoundTripsThroughARealIndex()
    {
        // Stage
        var endpoint = TestContext.GetPropertyOrDefault("AZURE_SEARCH_ENDPOINT", Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT") ?? string.Empty);
        var key = TestContext.GetPropertyOrDefault("AZURE_SEARCH_API_KEY", Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY") ?? string.Empty);
        if (endpoint.Length == 0 || key.Length == 0)
            Assert.Inconclusive("Set AZURE_SEARCH_ENDPOINT and AZURE_SEARCH_API_KEY to run this live test.");

        var name = $"test-{Guid.NewGuid():N}";
        var options = Options.Create(new AzureAISearchOptions { Endpoint = endpoint, IndexName = name, ApiKey = key, EnsureIndexExists = true, Top = 3 });
        var credential = new AzureKeyCredential(key);
        var indexClient = new SearchIndexClient(new Uri(endpoint), credential);
        var provider = new AzureAISearchVectorStoreProvider(indexClient, new SearchClient(new Uri(endpoint), name, credential), options, name);
        try
        {
            // Test
            await provider.StoreVectorsAsync(
                [(new float[] { 1, 0, 0 }, (IDictionary<string, object>)new Dictionary<string, object> { ["file"] = "x" }),
                 (new float[] { 0, 1, 0 }, (IDictionary<string, object>)new Dictionary<string, object> { ["file"] = "y" })],
                new Dictionary<string, object>());
            var found = new List<OoBDev.Search.Models.SearchResultModel>();
            for (var attempt = 0; attempt < 10 && found.Count == 0; attempt++)
            {
                await Task.Delay(1000);
                found = await provider.FindNeighborsAsync(new float[] { 1, 0, 0 }).ToListAsync();
            }

            // Assert
            Assert.IsTrue(found.Count > 0);
            Assert.AreEqual("x", found.First().MetaData!["file"]);
        }
        finally
        {
            await indexClient.DeleteIndexAsync(name);
        }
    }
}
