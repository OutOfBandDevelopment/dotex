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
    private readonly string hostName = "192.168.1.170";

    public required TestContext TestContext { get; set; }

    private OpenSearchLowLevelClient GetClient()
    {
        var connection = new ConnectionConfiguration(
                new Uri($"http://{hostName}:9200")
            )
            .BasicAuthentication("admin", "UY8rB3tC7ygzsFNWdRpxZb")
            .EnableHttpCompression(true)
            .ThrowExceptions(true)
            .PrettyJson()
            ;
        var client = new OpenSearchLowLevelClient(connection);
        return client;
    }

    [DataTestMethod]
    [TestCategory(TestCategories.DevLocal)]
    [DataRow(storeName)]
    [DataRow("summary")]
    public async Task CreateIndexTest(string indexName)
    {
        var id = Guid.NewGuid().ToString();
        var client = GetClient();
        var result = await client.IndexAsync<StringResponse>(indexName, id, PostData.Serializable(new
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
    [TestCategory(TestCategories.DevLocal)]
    public async Task SearchIndexTest()
    {
        //var id = Guid.NewGuid().ToString();
        var client = GetClient();
        var result = await client.SearchAsync<StringResponse>(storeName,
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
