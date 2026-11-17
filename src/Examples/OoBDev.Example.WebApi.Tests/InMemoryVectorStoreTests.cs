using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.WebApi.Services;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class InMemoryVectorStoreTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task DeleteAsync_RemovesOnlyTheNamedRows_AndIdsStayUnique()
    {
        var store = new InMemoryVectorStoreProviderFactory().Create($"Test_{Guid.NewGuid():N}");
        var metadata = new Dictionary<string, object>();
        var first = await store.StoreVectorsAsync([new ReadOnlyMemory<float>([1f, 0f])], metadata);
        var second = await store.StoreVectorsAsync([new ReadOnlyMemory<float>([0f, 1f])], metadata);

        await store.DeleteAsync([.. first, "no-such-id"]);
        var third = await store.StoreVectorsAsync([new ReadOnlyMemory<float>([1f, 1f])], metadata);

        var left = await store.ListAsync().Select(r => r.ItemId).ToListAsync();
        CollectionAssert.AreEquivalent(new[] { second[0], third[0] }, left);
        Assert.AreNotEqual(second[0], third[0]);
    }
}
