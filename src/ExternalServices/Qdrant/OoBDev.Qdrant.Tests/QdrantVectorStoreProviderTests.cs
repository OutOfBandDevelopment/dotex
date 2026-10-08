using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Qdrant.Tests;

[TestClass]
public class QdrantVectorStoreProviderTests
{
    public required TestContext TestContext { get; set; }

    private QdrantGrpcClient? _client;
    private string _collection = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        var host = TestContext.GetPropertyOrDefault("QDRANT_HOST", "127.0.0.1");
        if (host == "localhost") host = "127.0.0.1"; // gRPC resolves localhost to an unusable IPv6 address on some hosts
        var grpcPort = TestContext.GetPropertyOrDefault("QDRANT_GRPC_PORT", 6334);
        _collection = $"IntegrationTest_{Guid.NewGuid():N}";
        _client = new QdrantGrpcClientFactory(Options.Create(new QdrantOptions
        {
            Url = $"http://{host}:{grpcPort}",
            CollectionName = _collection,
        })).Create();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_client != null)
            await _client.Collections.DeleteAsync(new() { CollectionName = _collection });
    }

    private QdrantVectorStoreProvider CreateProvider() =>
        new(_client!, Options.Create(new QdrantOptions
        {
            Url = "unused",
            CollectionName = _collection,
            EnsureCollectionExists = true,
        }), _collection);

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task FindNeighborsAsync_NearestVectorFirst()
    {
        var provider = CreateProvider();
        await provider.StoreVectorsAsync(
            [
                (new float[] { 1, 0, 0 }, new Dictionary<string, object> { ["name"] = "x" }),
                (new float[] { 0, 1, 0 }, new Dictionary<string, object> { ["name"] = "y" }),
            ],
            new Dictionary<string, object> { ["group"] = "g" });

        var results = await FirstAsync(provider.FindNeighborsAsync(new float[] { 0.9f, 0.1f, 0 }));

        Assert.IsNotEmpty(results);
        Assert.AreEqual("x", results[0].MetaData!["name"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task FindNeighborsAsync_GroupBy_ReturnsOneHitPerGroup()
    {
        var provider = CreateProvider();
        await provider.StoreVectorsAsync(
            [
                (new float[] { 1, 0, 0 }, new Dictionary<string, object> { ["name"] = "x", ["group"] = "a" }),
                (new float[] { 0.9f, 0.1f, 0 }, new Dictionary<string, object> { ["name"] = "x2", ["group"] = "a" }),
                (new float[] { 0, 1, 0 }, new Dictionary<string, object> { ["name"] = "y", ["group"] = "b" }),
            ],
            new Dictionary<string, object>());

        var results = await FirstAsync(provider.FindNeighborsAsync(new float[] { 1, 0, 0 }, "group"));

        Assert.HasCount(2, results);
    }

    private static async Task<List<OoBDev.Search.Models.SearchResultModel>> FirstAsync(
        IAsyncEnumerable<OoBDev.Search.Models.SearchResultModel> source)
    {
        var list = new List<OoBDev.Search.Models.SearchResultModel>();
        await foreach (var item in source)
            list.Add(item);
        return list;
    }
}
