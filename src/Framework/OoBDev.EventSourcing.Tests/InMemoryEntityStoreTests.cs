using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class InMemoryEntityStoreTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SaveAsync_NewThenUpdate_IncrementsVersion()
    {
        var store = new InMemoryEntityStore();

        var first = await store.SaveAsync("t", "k", "1", 0);
        var second = await store.SaveAsync("t", "k", "2", 1);

        Assert.AreEqual(1, first!.Version);
        Assert.AreEqual(2, second!.Version);
        Assert.AreEqual("2", (await store.GetAsync("t", "k"))!.Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SaveAsync_StaleVersion_ReturnsNull()
    {
        var store = new InMemoryEntityStore();
        await store.SaveAsync("t", "k", "1", 0);

        Assert.IsNull(await store.SaveAsync("t", "k", "x", 0));
        Assert.IsNull(await store.SaveAsync("t", "other", "x", 5));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task DeleteAsync_ChecksVersionWhenGiven()
    {
        var store = new InMemoryEntityStore();
        await store.SaveAsync("t", "k", "1", 0);

        Assert.IsFalse(await store.DeleteAsync("t", "k", 9));
        Assert.IsTrue(await store.DeleteAsync("t", "k", 1));
        Assert.IsFalse(await store.DeleteAsync("t", "k"));
        Assert.IsNull(await store.GetAsync("t", "k"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task QueryAsync_FiltersByTypeAndPrefix_OrderedByKey()
    {
        var store = new InMemoryEntityStore();
        await store.SaveAsync("t", "b2", "v", 0);
        await store.SaveAsync("t", "b1", "v", 0);
        await store.SaveAsync("t", "c1", "v", 0);
        await store.SaveAsync("u", "b3", "v", 0);

        var result = await store.QueryAsync("t", "b");

        CollectionAssert.AreEqual(new[] { "b1", "b2" }, result.Select(e => e.Key).ToArray());
        Assert.AreEqual(3, (await store.QueryAsync("t")).Count);
    }
}
