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
public sealed class ScheduleHostTests
{
    public TestContext TestContext { get; set; } = null!;

    private static async Task TicksAsync(FakeTimeProvider time, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await Task.Delay(50);
            time.Advance(TimeSpan.FromSeconds(10));
            await Task.Delay(50);
        }
    }

    [TestMethod]
    public async Task Host_DueItem_RunsOnTheNextPollAndNotAgain()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
        var seen = new List<string>();
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<TimeProvider>(time);
        services.TryAddScheduleHosting((item, _) => { lock (seen) seen.Add(item.Key); return Task.CompletedTask; }, o => o.PollInterval = TimeSpan.FromSeconds(10));
        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IScheduleStore>().RegisterAsync("k", time.GetUtcNow().AddSeconds(5), "ping", new byte[] { 1 }, CancellationToken.None);
        var host = provider.GetServices<IHostedService>().Single();

        await host.StartAsync(CancellationToken.None);
        await TicksAsync(time, 3);
        await host.StopAsync(CancellationToken.None);

        TestContext.WriteLine($"ran: {string.Join(",", seen)}");
        CollectionAssert.AreEqual(new[] { "k" }, seen);
    }

    [TestMethod]
    public async Task Host_FailingRun_KeepsPolling()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
        var calls = 0;
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<TimeProvider>(time);
        services.TryAddScheduleHosting((_, _) => { Interlocked.Increment(ref calls); throw new InvalidOperationException("boom"); }, o => { o.PollInterval = TimeSpan.FromSeconds(10); o.Lease = TimeSpan.FromSeconds(1); });
        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IScheduleStore>().RegisterAsync("k", time.GetUtcNow(), "ping", new byte[] { 1 }, CancellationToken.None);
        var host = provider.GetServices<IHostedService>().Single();

        await host.StartAsync(CancellationToken.None);
        await TicksAsync(time, 2);
        await host.StopAsync(CancellationToken.None);

        Assert.IsTrue(calls >= 2, $"calls {calls}");
    }

    [TestMethod]
    public void TryAddScheduleHosting_CalledTwice_RegistersOneHost()
    {
        var services = new ServiceCollection().AddLogging();
        services.TryAddScheduleHosting((_, _) => Task.CompletedTask);
        services.TryAddScheduleHosting((_, _) => Task.CompletedTask);
        using var provider = services.BuildServiceProvider();

        Assert.AreEqual(1, provider.GetServices<IHostedService>().Count());
    }
}
