using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.ComplexEvents.Hosting.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class RecurringHostTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Host_RecurringSchedule_RunsEachOccurrenceAndRecordsTheLastRun()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 23, 10, 0, 0, TimeSpan.Zero));
        var seen = new List<DateTimeOffset>();
        var lastRuns = new InMemoryLastRunStore();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<TimeProvider>(time);
        services.AddSingleton<ILastRunStore>(lastRuns);
        services.TryAddScheduleHosting((item, _) => { lock (seen) seen.Add(item.DueAt); return Task.CompletedTask; }, o => o.PollInterval = TimeSpan.FromSeconds(10));
        services.AddSingleton(sp => new RecurringScheduler(
            sp.GetRequiredService<ILastRunStore>(),
            new Dictionary<string, RecurringSchedule> { ["hb"] = new(sp.GetRequiredService<IScheduleStore>(), time, "hb", new CronRecurrence("* * * * *"), "Heartbeat", new byte[] { 1 }) }));
        await using var provider = services.BuildServiceProvider();
        var host = provider.GetServices<IHostedService>().Single();

        await host.StartAsync(CancellationToken.None);
        for (var i = 0; i < 14; i++)
        {
            await Task.Delay(30);
            time.Advance(TimeSpan.FromSeconds(10));
            await Task.Delay(30);
        }

        await host.StopAsync(CancellationToken.None);

        TestContext.WriteLine(string.Join(",", seen.Select(s => s.ToString("HH:mm", global::System.Globalization.CultureInfo.InvariantCulture))));
        Assert.IsTrue(seen.Count >= 2, $"ran {seen.Count}");
        Assert.AreEqual(seen.Max(), await lastRuns.GetAsync("hb", CancellationToken.None));
    }
}
