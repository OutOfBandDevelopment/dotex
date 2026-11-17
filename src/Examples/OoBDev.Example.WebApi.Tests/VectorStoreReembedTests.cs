using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AI;
using OoBDev.SBert.AllMiniLmL6V2;
using OoBDev.Search.Semantic;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class VectorStoreReembedTests
{
    private sealed class FakeEmbedding : IEmbeddingProvider
    {
        public int Length => 2;

        public Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string content, string? model, CancellationToken cancellationToken) =>
            Task.FromResult(new ReadOnlyMemory<float>([content.Length, 1f]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Reembed_ReplacesUntaggedRowsWithText_AndLeavesCurrentOnesAlone()
    {
        var stores = new InMemoryVectorStoreProviderFactory();
        var collection = $"Test_{Guid.NewGuid():N}";
        var store = stores.Create(collection);
        var tag = EmbeddingVersion.Tag(SBertGlobals.AllMiniLmL6V2Key, 384);
        await store.StoreVectorsAsync([new ReadOnlyMemory<float>([9f, 9f])], new Dictionary<string, object> { ["text"] = "old row" });
        await store.StoreVectorsAsync([new ReadOnlyMemory<float>([8f, 8f])], new Dictionary<string, object> { ["note"] = "no text" });
        await store.StoreVectorsAsync([new ReadOnlyMemory<float>([7f, 7f])], EmbeddingVersion.Stamp(new Dictionary<string, object> { ["text"] = "current" }, tag));
        var controller = new VectorStoreController(new FakeEmbedding(), stores);

        var result = (await controller.Reembed(collection)).Value!;

        var rows = await stores.Create(collection).ListAsync().ToListAsync();
        Assert.AreEqual(1, result.Replaced);
        Assert.AreEqual(tag, result.Tag);
        Assert.HasCount(3, rows);
        Assert.AreEqual(2, rows.Count(r => EmbeddingVersion.Read(r) == tag));
        Assert.AreEqual(1, rows.Count(r => EmbeddingVersion.Read(r) is null));
    }
}
