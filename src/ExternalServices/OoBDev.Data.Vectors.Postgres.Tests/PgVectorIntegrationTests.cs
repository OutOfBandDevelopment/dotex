using System;
using OoBDev.TestUtilities.Containers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OoBDev.Search.Models;
using OoBDev.TestUtilities;

namespace OoBDev.Data.Vectors.Postgres.Tests;

[TestClass]
[TestCategory(TestCategories.Integration)]
public class PgVectorIntegrationTests
{
    private string _schema = "";
    private string _connectionString = "";

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void Init()
    {
        _connectionString = TestContext.GetRequiredProperty<string>("PGVECTOR_CONNECTION_STRING");
        if (!_connectionString.Contains("oobdev_test", StringComparison.Ordinal))
            Assert.Inconclusive("The connection string is not a test database.");
        _schema = $"v_{Guid.NewGuid():N}";
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        if (_schema.Length == 0)
            return;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS \"{_schema}\" CASCADE", connection);
        await command.ExecuteNonQueryAsync();
    }

    private PgVectorStore Store(int dimensions = 3, VectorMetric metric = VectorMetric.Cosine, bool half = false, int limit = 10) =>
        new(Options.Create(new PgVectorOptions { ConnectionString = _connectionString, Schema = _schema, Table = "vectors", Dimensions = dimensions, Metric = metric, UseHalfVector = half, DefaultLimit = limit }));

    private static IDictionary<string, object> Meta(string key, string value) => new Dictionary<string, object> { [key] = value };

    private static async Task<List<T>> All<T>(IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
            list.Add(item);
        return list;
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task FindNeighbors_Cosine_OrdersNearestFirstWithScore()
    {
        await using var store = Store();
        await store.EnsureCreatedAsync();
        var a = (await store.StoreVectorsAsync([new float[] { 1, 0, 0 }], Meta("name", "a")))[0];
        await store.StoreVectorsAsync([new float[] { 0, 1, 0 }], Meta("name", "b"));

        var found = await All(store.FindNeighborsAsync(new float[] { 1, 0.1f, 0 }));

        Assert.AreEqual(2, found.Count);
        Assert.AreEqual(a, found[0].ItemId);
        Assert.IsTrue(found[0].Score > found[1].Score);
        Assert.AreEqual("a", found[0].MetaData!["name"].ToString());
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task FindNeighbors_MultiVectorItem_ReturnsItemOnceWithMatchingVectorIndex()
    {
        await using var store = Store();
        await store.EnsureCreatedAsync();
        var pages = (await store.StoreVectorsAsync([new float[] { 0, 1, 0 }, new float[] { 0, 0, 1 }, new float[] { 1, 0, 0 }], Meta("name", "doc")))[0];
        await store.StoreVectorsAsync([new float[] { 0.5f, 0.5f, 0 }], Meta("name", "other"));

        var matches = await All(store.FindNeighborsWithExplanationAsync(new float[] { 0, 0.1f, 1 }));

        Assert.AreEqual(2, matches.Count);
        Assert.AreEqual(1, matches.Count(m => m.Result.ItemId == pages));
        var best = matches.Single(m => m.Result.ItemId == pages);
        Assert.AreEqual(1, best.VectorIndex);
        Assert.IsTrue(best.Distance < 0.1);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task FindNeighbors_GroupBy_ReturnsBestPerGroup()
    {
        await using var store = Store();
        await store.EnsureCreatedAsync();
        await store.StoreVectorsAsync([(new ReadOnlyMemory<float>(new float[] { 1, 0, 0 }), Meta("album", "x")), (new ReadOnlyMemory<float>(new float[] { 0.9f, 0.1f, 0 }), Meta("album", "x")), (new ReadOnlyMemory<float>(new float[] { 0, 1, 0 }), Meta("album", "y"))], new Dictionary<string, object>());

        var found = await All(store.FindNeighborsAsync(new float[] { 1, 0, 0 }, "album"));

        Assert.AreEqual(2, found.Count);
        CollectionAssert.AreEquivalent(new[] { "x", "y" }, found.Select(f => f.MetaData!["album"].ToString()).ToArray());
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task FindNeighbors_L2AndHalfVector_Work()
    {
        await using var store = Store(metric: VectorMetric.L2, half: true);
        await store.EnsureCreatedAsync();
        var near = (await store.StoreVectorsAsync([new float[] { 1, 1, 1 }], Meta("n", "near")))[0];
        await store.StoreVectorsAsync([new float[] { 9, 9, 9 }], Meta("n", "far"));

        var found = await All(store.FindNeighborsAsync(new float[] { 1, 1, 1.1f }));

        Assert.AreEqual(near, found[0].ItemId);
        Assert.IsTrue(found[0].Score > found[1].Score);
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Store_WrongDimension_Throws()
    {
        await using var store = Store();
        await store.EnsureCreatedAsync();

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => store.StoreVectorsAsync([new float[] { 1, 2 }], Meta("a", "b")));
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task List_ReturnsEveryStoredVectorWithMetadata()
    {
        await using var store = Store();
        await store.EnsureCreatedAsync();
        await store.StoreVectorsAsync([new float[] { 1, 0, 0 }, new float[] { 0, 1, 0 }], Meta("k", "v"));

        var listed = await All(store.ListAsync());

        Assert.AreEqual(2, listed.Count);
        Assert.IsTrue(listed.All(l => l.MetaData!["k"].ToString() == "v"));
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task Store_ItemsOverload_MergesSharedMetadata()
    {
        await using var store = Store();
        await store.EnsureCreatedAsync();

        var ids = await store.StoreVectorsAsync([(new ReadOnlyMemory<float>(new float[] { 1, 0, 0 }), Meta("own", "1"))], Meta("shared", "s"));

        var found = (await All(store.ListAsync())).Single();
        Assert.AreEqual(ids[0], found.ItemId);
        Assert.AreEqual("s", found.MetaData!["shared"].ToString());
        Assert.AreEqual("1", found.MetaData["own"].ToString());
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task CreateHnswIndex_IsExplicitAndRepeatable()
    {
        await using var store = Store();
        await store.EnsureCreatedAsync();
        Assert.AreEqual(0, await IndexCount());

        await store.CreateHnswIndexAsync();
        await store.CreateHnswIndexAsync();

        Assert.AreEqual(1, await IndexCount());
    }

    [IntegrationTestMethod("oobd-test-pgvector")]
    public async Task FindNeighbors_TenThousandVectorsWithHnsw_ReturnsInBoundedTime()
    {
        await using var store = Store(dimensions: 8, limit: 5);
        await store.EnsureCreatedAsync();
        var random = new Random(42);
        var items = Enumerable.Range(0, 10_000).Select(i => (new ReadOnlyMemory<float>(Enumerable.Range(0, 8).Select(_ => (float)random.NextDouble()).ToArray()), Meta("i", i.ToString()))).ToArray();
        await store.StoreVectorsAsync(items, new Dictionary<string, object>());
        await store.CreateHnswIndexAsync();

        var watch = Stopwatch.StartNew();
        var found = await All(store.FindNeighborsAsync(items[1234].Item1));
        watch.Stop();

        TestContext.WriteLine($"10,000 vectors, HNSW, top 5: {watch.ElapsedMilliseconds} ms on {Environment.MachineName} {DateTime.UtcNow:yyyy-MM-dd}");
        Assert.AreEqual("1234", found[0].MetaData!["i"].ToString());
        Assert.IsTrue(watch.ElapsedMilliseconds < 5000);
    }

    private async Task<long> IndexCount()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT count(*) FROM pg_indexes WHERE schemaname = @s AND indexdef ILIKE '%hnsw%'", connection);
        command.Parameters.AddWithValue("s", _schema);
        return (long)(await command.ExecuteScalarAsync())!;
    }
}
