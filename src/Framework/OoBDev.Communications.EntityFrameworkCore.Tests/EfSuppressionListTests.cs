using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.EntityFrameworkCore.Tests;

[TestClass]
public class EfSuppressionListTests
{
    private SqliteSuppressionsDatabase database = null!;
    private EfSuppressionList list = null!;

    [TestInitialize]
    public void Initialize()
    {
        database = new SqliteSuppressionsDatabase();
        list = new EfSuppressionList(database.Factory, TimeProvider.System);
    }

    [TestCleanup]
    public async Task Cleanup() => await database.DisposeAsync();

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Add_ThenIsSuppressed_IgnoringCase()
    {
        await list.AddAsync("Ann@Example.test", "bounce");

        Assert.IsTrue(await list.IsSuppressedAsync("ann@example.TEST"));
        Assert.IsFalse(await list.IsSuppressedAsync("bob@example.test"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Add_Twice_KeepsFirstReasonAndDoesNotThrow()
    {
        await list.AddAsync("ann@example.test", "bounce");
        await list.AddAsync("ann@example.test", "complaint");

        using var db = database.Factory.CreateDbContext();
        var row = db.Suppressions.Single();
        Assert.AreEqual("bounce", row.Reason);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Remove_LiftsSuppression_AndUnknownIsFine()
    {
        await list.AddAsync("ann@example.test", "unsubscribe");

        await list.RemoveAsync("ann@example.test");
        await list.RemoveAsync("nobody@example.test");

        Assert.IsFalse(await list.IsSuppressedAsync("ann@example.test"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Address_Blank_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => list.IsSuppressedAsync(" "));
    }
}
