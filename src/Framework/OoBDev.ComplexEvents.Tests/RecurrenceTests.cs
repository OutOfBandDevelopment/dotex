using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.ComplexEvents.Tests;

[ScheduleAt("*/15 * * * *", CatchUp = CatchUpPolicy.RunLatest)]
public sealed record Heartbeat;

[TestCategory("Unit")]
[TestClass]
public sealed class RecurrenceTests
{
    public TestContext TestContext { get; set; } = null!;

    private static DateTimeOffset At(int month, int day, int hour, int minute) => new(2026, month, day, hour, minute, 0, TimeSpan.Zero);

    [TestMethod]
    [DataRow("*/15 * * * *", 11, 23, 10, 7, 11, 23, 10, 15)]
    [DataRow("*/15 * * * *", 11, 23, 10, 45, 11, 23, 11, 0)]
    [DataRow("0 9 * * 1-5", 11, 21, 12, 0, 11, 23, 9, 0)]
    [DataRow("30 2 1 * *", 11, 23, 0, 0, 12, 1, 2, 30)]
    [DataRow("0 0 29 2 *", 11, 23, 0, 0, 2, 29, 0, 0)]
    [DataRow("5,10 8-9 * * *", 11, 23, 8, 10, 11, 23, 9, 5)]
    [DataRow("0 12 * * 7", 11, 23, 0, 0, 11, 29, 12, 0)]
    public void Cron_Next_FindsTheFollowingOccurrence(string cron, int m, int d, int h, int min, int em, int ed, int eh, int emin)
    {
        var after = At(m, d, h, min);
        var next = new CronRecurrence(cron).Next(after);
        TestContext.WriteLine($"{cron}: {after:u} -> {next:u}");

        if (em == 2 && ed == 29)
        {
            Assert.IsNotNull(next);
            Assert.AreEqual(2, next.Value.Month);
            Assert.AreEqual(29, next.Value.Day);
            return;
        }

        Assert.AreEqual(At(em, ed, eh, emin), next);
    }

    [TestMethod]
    public void Cron_DayOfMonthAndWeekday_MatchEither()
    {
        // 2026-11-23 is a Monday; "the 25th or any Friday" next matches Friday the 27th? No: the 25th (Wednesday) comes first.
        var next = new CronRecurrence("0 0 25 * 5").Next(At(11, 23, 0, 0));
        Assert.AreEqual(At(11, 25, 0, 0), next);
        Assert.AreEqual(At(11, 27, 0, 0), new CronRecurrence("0 0 25 * 5").Next(At(11, 25, 0, 0)));
    }

    [TestMethod]
    [DataRow("* * * *")]
    [DataRow("60 * * * *")]
    [DataRow("* 24 * * *")]
    [DataRow("*/0 * * * *")]
    [DataRow("5-1 * * * *")]
    [DataRow("a * * * *")]
    public void Cron_InvalidExpression_Throws(string cron) =>
        Assert.ThrowsExactly<FormatException>(() => new CronRecurrence(cron));

    [TestMethod]
    public void Cron_ImpossibleDate_ReturnsNull() =>
        Assert.IsNull(new CronRecurrence("0 0 31 2 *").Next(At(11, 23, 0, 0)));

    [TestMethod]
    public void ScheduleAtAttribute_CarriesCronAndPolicy()
    {
        var attribute = (ScheduleAtAttribute)Attribute.GetCustomAttribute(typeof(Heartbeat), typeof(ScheduleAtAttribute))!;
        Assert.AreEqual("*/15 * * * *", attribute.Cron);
        Assert.AreEqual(CatchUpPolicy.RunLatest, attribute.CatchUp);
    }

    [TestMethod]
    [DataRow(CatchUpPolicy.Skip, 1)]
    [DataRow(CatchUpPolicy.RunLatest, 2)]
    [DataRow(CatchUpPolicy.RunAll, 4)]
    public async Task EnsureAsync_AfterDowntime_AppliesTheCatchUpPolicy(CatchUpPolicy policy, int expected)
    {
        var clock = new FakeTimeProvider(At(11, 23, 10, 50));
        var store = new InMemoryScheduleStore(clock);
        var schedule = new RecurringSchedule(store, clock, "hb", new CronRecurrence("*/15 * * * *"), "Heartbeat", new byte[] { 1 }, policy);

        // Last ran at 10:00; 10:15, 10:30 and 10:45 were missed; 11:00 is next.
        var registered = await schedule.EnsureAsync(At(11, 23, 10, 0), CancellationToken.None);
        TestContext.WriteLine(string.Join(", ", registered));

        Assert.AreEqual(expected, registered.Count);
        Assert.AreEqual(At(11, 23, 11, 0), registered[^1]);
    }

    [TestMethod]
    public async Task EnsureAsync_CalledTwice_DoesNotDuplicateOccurrences()
    {
        var clock = new FakeTimeProvider(At(11, 23, 10, 50));
        var store = new InMemoryScheduleStore(clock);
        var schedule = new RecurringSchedule(store, clock, "hb", new CronRecurrence("*/15 * * * *"), "Heartbeat", new byte[] { 1 });

        await schedule.EnsureAsync(null, CancellationToken.None);
        await schedule.EnsureAsync(null, CancellationToken.None);
        clock.SetUtcNow(At(11, 23, 11, 1));
        var leased = await store.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None);

        Assert.AreEqual(1, leased.Count);
        Assert.AreEqual(At(11, 23, 11, 0), leased[0].DueAt);
    }
}
