using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

[TestClass]
public class EfEntityStoreTests
{
    private SqliteDatabase _database = null!;
    private EfEntityStore _store = null!;

    [TestInitialize]
    public void Initialize()
    {
        _database = new SqliteDatabase();
        _store = new EfEntityStore(_database.Factory);
    }

    [TestCleanup]
    public async Task Cleanup() => await _database.DisposeAsync();

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task SaveAsync_NewThenUpdate_IncrementsVersion()
    {
        var first = await _store.SaveAsync("t", "k", "{\"v\":1}", 0);
        var second = await _store.SaveAsync("t", "k", "{\"v\":2}", 1);

        Assert.AreEqual(1, first!.Version);
        Assert.AreEqual(2, second!.Version);
        Assert.AreEqual("{\"v\":2}", (await _store.GetAsync("t", "k"))!.Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task SaveAsync_StaleVersionOrDuplicateInsert_ReturnsNull()
    {
        await _store.SaveAsync("t", "k", "{}", 0);

        Assert.IsNull(await _store.SaveAsync("t", "k", "{}", 0));
        Assert.IsNull(await _store.SaveAsync("t", "k", "{}", 7));
        Assert.IsNull(await _store.SaveAsync("t", "missing", "{}", 3));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task DeleteAsync_ChecksVersionWhenGiven()
    {
        await _store.SaveAsync("t", "k", "{}", 0);

        Assert.IsFalse(await _store.DeleteAsync("t", "k", 9));
        Assert.IsTrue(await _store.DeleteAsync("t", "k", 1));
        Assert.IsFalse(await _store.DeleteAsync("t", "k"));
        Assert.IsNull(await _store.GetAsync("t", "k"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task QueryAsync_FiltersByTypeAndPrefix_OrderedByKey()
    {
        await _store.SaveAsync("t", "b2", "{}", 0);
        await _store.SaveAsync("t", "b1", "{}", 0);
        await _store.SaveAsync("t", "c1", "{}", 0);
        await _store.SaveAsync("u", "b3", "{}", 0);

        var result = await _store.QueryAsync("t", "b");

        CollectionAssert.AreEqual(new[] { "b1", "b2" }, result.Select(e => e.Key).ToArray());
        Assert.AreEqual(3, (await _store.QueryAsync("t")).Count);
    }
}
