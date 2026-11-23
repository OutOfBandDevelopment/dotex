using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.ComplexEvents.EntityFrameworkCore.Tests;

[TestCategory("Simulate")]
[TestClass]
public sealed class EfLastRunStoreTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task SetAndGet_KeepsTheNewestAndSurvivesANewStoreInstance()
    {
        await using var database = new SqliteScheduleDatabase();
        var first = new EfLastRunStore(database.Factory);
        var at = new DateTimeOffset(2026, 11, 23, 10, 15, 0, TimeSpan.Zero);

        Assert.IsNull(await first.GetAsync("hb", CancellationToken.None));
        await first.SetAsync("hb", at, CancellationToken.None);
        await first.SetAsync("hb", at.AddHours(-1), CancellationToken.None);
        await first.SetAsync("hb", at.AddMinutes(15), CancellationToken.None);
        var second = new EfLastRunStore(database.Factory);
        var read = await second.GetAsync("hb", CancellationToken.None);
        TestContext.WriteLine($"read {read:O}");

        Assert.AreEqual(at.AddMinutes(15), read);
        Assert.IsNull(await second.GetAsync("other", CancellationToken.None));
    }
}
