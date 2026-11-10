using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Models;
using OoBDev.Search.Semantic;
using OoBDev.TestUtilities;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Search.Tests.Semantic;

[TestClass]
public class VectorStoreFactoryTests
{
    [VectorStore(CollectionName = "from-attribute")]
    private sealed class AttributedItem { }

    [Table("from-table")]
    private sealed class TableItem { }

    private sealed class PlainItem { }

    private static VectorStoreFactory Build(
        FakeVectorStoreProvider? keyed = null,
        string keyedName = "docs",
        FakeVectorStoreProviderFactory[]? factories = null,
        FakeVectorStoreProvider? fallback = null)
    {
        var services = new ServiceCollection();
        if (keyed != null) services.AddKeyedSingleton<IVectorStoreProvider>(keyedName, keyed);
        if (fallback != null) services.AddSingleton<IVectorStoreProvider>(fallback);
        var provider = services.BuildServiceProvider();
        return new VectorStoreFactory(provider, factories ?? []);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Create_KeyedProviderRegistered_UsesKeyedProvider()
    {
        // Stage
        var keyed = new FakeVectorStoreProvider();
        var factory = Build(keyed, factories: [new FakeVectorStoreProviderFactory(new FakeVectorStoreProvider())], fallback: new FakeVectorStoreProvider());

        // Test
        var result = factory.Create("docs");

        // Assert
        Assert.AreSame(keyed, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Create_NoKeyedProvider_UsesFirstFactoryResult()
    {
        // Stage
        var fromFactory = new FakeVectorStoreProvider();
        var factory = Build(factories: [new FakeVectorStoreProviderFactory(null), new FakeVectorStoreProviderFactory(fromFactory)], fallback: new FakeVectorStoreProvider());

        // Test
        var result = factory.Create("docs");

        // Assert
        Assert.AreSame(fromFactory, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Create_NoFactoryResult_UsesDefaultProvider()
    {
        // Stage
        var fallback = new FakeVectorStoreProvider();
        var factory = Build(factories: [new FakeVectorStoreProviderFactory(null)], fallback: fallback);

        // Test
        var result = factory.Create("docs");

        // Assert
        Assert.AreSame(fallback, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Create_NoProviderAtAll_Throws()
    {
        // Stage
        var factory = Build();

        // Test / Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => factory.Create("docs"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Create_AnyProvider_SetsCollectionName()
    {
        // Stage
        var fallback = new FakeVectorStoreProvider();
        var factory = Build(fallback: fallback);

        // Test
        factory.Create("orders");

        // Assert
        Assert.AreEqual("orders", fallback.CollectionName);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CreateOfT_VectorStoreAttribute_UsesAttributeName()
    {
        // Stage
        var fallback = new FakeVectorStoreProvider();

        // Test
        Build(fallback: fallback).Create<AttributedItem>();

        // Assert
        Assert.AreEqual("from-attribute", fallback.CollectionName);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CreateOfT_TableAttribute_UsesTableName()
    {
        // Stage
        var fallback = new FakeVectorStoreProvider();

        // Test
        Build(fallback: fallback).Create<TableItem>();

        // Assert
        Assert.AreEqual("from-table", fallback.CollectionName);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CreateOfT_NoAttribute_UsesTypeName()
    {
        // Stage
        var fallback = new FakeVectorStoreProvider();

        // Test
        Build(fallback: fallback).Create<PlainItem>();

        // Assert
        Assert.AreEqual(nameof(PlainItem), fallback.CollectionName);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task WrappedVectorStore_Calls_ForwardToWrapped()
    {
        // Stage
        var inner = new FakeVectorStoreProvider();
        var store = new WrappedVectorStore<PlainItem>(inner);
        var vector = new ReadOnlyMemory<float>([1f]);
        var metadata = new Dictionary<string, object>();

        // Test
        var ids1 = await store.StoreVectorsAsync([vector], metadata);
        var ids2 = await store.StoreVectorsAsync([(vector, (IDictionary<string, object>)metadata)], metadata);
        var listed = await ListAll(store.ListAsync());
        var near = await ListAll(store.FindNeighborsAsync(vector));
        var grouped = await ListAll(store.FindNeighborsAsync(vector, "file"));

        // Assert
        CollectionAssert.AreEqual(new[] { "a" }, ids1);
        CollectionAssert.AreEqual(new[] { "b" }, ids2);
        Assert.AreEqual("listed", listed);
        Assert.AreEqual("near", near);
        Assert.AreEqual("grouped", grouped);
        Assert.AreEqual(5, inner.Calls.Count);
        Assert.AreEqual("FindNeighborsAsync:file", inner.Calls[4]);
    }

    private static async Task<string> ListAll(IAsyncEnumerable<SearchResultModel> items)
    {
        var ids = new List<string>();
        await foreach (var item in items) ids.Add(item.ItemId);
        return ids.Single();
    }
}
