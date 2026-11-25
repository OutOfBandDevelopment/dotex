using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.EntityFrameworkCore.Tests;

[TestClass]
public class EfOrchestrationStoreTests
{
    private SqliteOrchestrationDatabase database = null!;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void Initialize() => database = new SqliteOrchestrationDatabase();

    [TestCleanup]
    public async Task Cleanup() => await database.DisposeAsync();

    private static DeferredRequest Deferred(string target, DateTimeOffset due)
        => new(new CommunicationRequest(target, "welcome", new Dictionary<string, object?> { ["EmailAddress"] = "a@example.test", ["Count"] = 3, ["Vip"] = true, ["Gone"] = null }, RequestPriority.High, "c-1"), due);

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task TakeDue_ReturnsOnlyDueRequests_WithDataIntact_AndRemovesThem()
    {
        // Stage
        var store = new EfDeferralStore(database.Factory);
        var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
        await store.AddAsync(Deferred("late", now.AddHours(1)));
        await store.AddAsync(Deferred("early", now.AddMinutes(-5)));

        // Test
        var due = await store.TakeDueAsync(now);
        var again = await store.TakeDueAsync(now);
        TestContext.WriteLine($"due: {string.Join(",", due.Select(d => d.Request.TargetId))}; second call: {again.Count}");

        // Assert
        var taken = due.Single();
        Assert.AreEqual("early", taken.Request.TargetId);
        Assert.AreEqual(now.AddMinutes(-5), taken.DueAt);
        Assert.AreEqual(RequestPriority.High, taken.Request.Priority);
        Assert.AreEqual("c-1", taken.Request.CorrelationId);
        Assert.AreEqual("a@example.test", taken.Request.Data["EmailAddress"]);
        Assert.AreEqual(3L, taken.Request.Data["Count"]);
        Assert.AreEqual(true, taken.Request.Data["Vip"]);
        Assert.IsNull(taken.Request.Data["Gone"]);
        Assert.AreEqual(0, again.Count);
        Assert.AreEqual(1, await database.Factory.CreateDbContext().DeferredRequests.CountAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task TakeDue_TwoInstances_EachRequestIsHandedOutOnce()
    {
        // Stage
        var first = new EfDeferralStore(database.Factory);
        var second = new EfDeferralStore(database.Factory);
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 5; i++)
        {
            await first.AddAsync(Deferred($"t{i}", now.AddMinutes(-1)));
        }

        // Test
        var results = await Task.WhenAll(first.TakeDueAsync(now), second.TakeDueAsync(now));
        TestContext.WriteLine($"first {results[0].Count}, second {results[1].Count}");

        // Assert
        Assert.AreEqual(5, results[0].Count + results[1].Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Preference_SetThenGet_RoundTripsAndReplaces()
    {
        // Stage
        var provider = new EfTargetPreferenceProvider(database.Factory);
        var zone = TimeZoneInfo.Utc;
        var preference = new TargetPreference("u1", ["email", "sms"], CultureInfo.GetCultureInfo("fr-FR"), zone, RequestPriority.High, new TimeOnly(22, 0), new TimeOnly(7, 30));

        // Test
        await provider.SetAsync(preference);
        var read = await provider.GetAsync("u1");
        await provider.SetAsync(preference with { Channels = ["sms"], QuietHoursStart = null, QuietHoursEnd = null });
        var replaced = await provider.GetAsync("u1");
        var unknown = await provider.GetAsync("nobody");

        // Assert
        Assert.IsNotNull(read);
        CollectionAssert.AreEqual(new[] { "email", "sms" }, read.Channels.ToArray());
        Assert.AreEqual("fr-FR", read.Culture.Name);
        Assert.AreEqual(zone.Id, read.TimeZone.Id);
        Assert.AreEqual(RequestPriority.High, read.MinimumPriority);
        Assert.AreEqual(new TimeOnly(22, 0), read.QuietHoursStart);
        Assert.AreEqual(new TimeOnly(7, 30), read.QuietHoursEnd);
        Assert.IsNotNull(replaced);
        CollectionAssert.AreEqual(new[] { "sms" }, replaced.Channels.ToArray());
        Assert.IsNull(replaced.QuietHoursStart);
        Assert.IsNull(unknown);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddEntityFrameworkOrchestration_RegistersStores()
    {
        // Stage
        var services = new ServiceCollection();

        // Test
        services.TryAddEntityFrameworkOrchestration((_, o) => o.UseSqlite("Data Source=:memory:"));
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<EfDeferralStore>(provider.GetRequiredService<IDeferralStore>());
        Assert.IsInstanceOfType<EfTargetPreferenceProvider>(provider.GetRequiredService<ITargetPreferenceProvider>());
    }
}
