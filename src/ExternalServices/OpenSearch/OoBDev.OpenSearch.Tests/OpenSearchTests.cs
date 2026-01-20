using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSearch.Net;
using System;
using System.Threading.Tasks;

namespace OoBDev.OpenSearch.Tests;

[TestClass]
public class OpenSearchTests
{
    private const string storeName = "docs";

    public required TestContext TestContext { get; set; }

    private OpenSearchLowLevelClient? _client;
    private string? _testIndexName;

    [TestInitialize]
    public void TestInitialize()
    {
        // Create unique index name for this test run
        _testIndexName = $"integrationtest_{Guid.NewGuid():N}";
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        // Cleanup: Delete the test index
        if (_client != null && _testIndexName != null)
        {
            try
            {
                await _client.Indices.DeleteAsync<StringResponse>(_testIndexName);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private OpenSearchLowLevelClient GetClient()
    {
        var url = TestContext.GetRequiredProperty<string>("OPENSEARCH_URL");
        var username = TestContext.GetRequiredProperty<string>("OPENSEARCH_USERNAME");
        var password = TestContext.GetRequiredProperty<string>("OPENSEARCH_PASSWORD");

        var connection = new ConnectionConfiguration(new Uri(url))
            .ServerCertificateValidationCallback((_, _, _, _) => true)//HACK: never do this in prod! this is for integration testing only
            .BasicAuthentication(username, password)
            .EnableHttpCompression(true)
            .ThrowExceptions(true)
            .PrettyJson()
            ;
        _client = new OpenSearchLowLevelClient(connection);
        return _client;
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateIndexTest()
    {
        var id = Guid.NewGuid().ToString();
        var client = GetClient();
        var result = await client.IndexAsync<StringResponse>(_testIndexName, id, PostData.Serializable(new
        {
            Id = id,
            Name = "Hello",
            Body = "World!"
        }));

        TestContext.WriteLine($"HttpStatusCode: {result.HttpStatusCode}");
        TestContext.WriteLine($"DebugInformation: {result.DebugInformation}");
        TestContext.WriteLine($"Body: {result.Body}");

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SearchIndexTest()
    {
        // First create a document to search for
        var id = Guid.NewGuid().ToString();
        var client = GetClient();

        await client.IndexAsync<StringResponse>(_testIndexName, id, PostData.Serializable(new
        {
            Id = id,
            Name = "Hello",
            Body = "World!"
        }));

        // Wait a moment for indexing to complete
        await Task.Delay(1000);

        var result = await client.SearchAsync<StringResponse>(_testIndexName,
            PostData.Serializable(new
            {
                query = new
                {
                    match = new
                    {
                        Name = new
                        {
                            query = "Helo"
                        }
                    }
                }
            }));

        TestContext.WriteLine($"HttpStatusCode: {result.HttpStatusCode}");
        TestContext.WriteLine($"DebugInformation: {result.DebugInformation}");
        TestContext.WriteLine($"Body: {result.Body}");

        Assert.IsTrue(result.Success);
    }

}
