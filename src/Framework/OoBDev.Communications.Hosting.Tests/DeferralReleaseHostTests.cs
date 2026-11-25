using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.Hosting.Tests;

[TestClass]
public class DeferralReleaseHostTests
{
    public required TestContext TestContext { get; set; }

    private static DeferralReleaseHost Host(Mock<ICommunicationOrchestrator> orchestrator, FakeTimeProvider time)
        => new(orchestrator.Object, Options.Create(new SendReceiveOptions { DeferralReleaseInterval = TimeSpan.FromMinutes(1) }), time, NullLogger<DeferralReleaseHost>.Instance);

    private static async Task WaitForAsync(Func<bool> condition)
    {
        for (var i = 0; i < 200 && !condition(); i++)
        {
            await Task.Delay(10);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Host_EveryInterval_ReleasesDueRequests()
    {
        // Mock
        var orchestrator = new Mock<ICommunicationOrchestrator>();
        var calls = 0;
        orchestrator.Setup(o => o.ReleaseDueAsync(It.IsAny<CancellationToken>()))
            .Callback(() => Interlocked.Increment(ref calls))
            .ReturnsAsync(2);
        var time = new FakeTimeProvider();
        using var host = Host(orchestrator, time);

        // Test
        await host.StartAsync(CancellationToken.None);
        for (var i = 1; i <= 2; i++)
        {
            await Task.Delay(50);
            time.Advance(TimeSpan.FromMinutes(1));
            await WaitForAsync(() => Volatile.Read(ref calls) >= i);
        }

        await host.StopAsync(CancellationToken.None);
        TestContext.WriteLine($"release calls: {calls}");

        // Assert
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Host_FailedRelease_KeepsRunning()
    {
        // Mock
        var orchestrator = new Mock<ICommunicationOrchestrator>();
        var calls = 0;
        orchestrator.Setup(o => o.ReleaseDueAsync(It.IsAny<CancellationToken>()))
            .Returns(() => Interlocked.Increment(ref calls) == 1 ? Task.FromException<int>(new InvalidOperationException("down")) : Task.FromResult(0));
        var time = new FakeTimeProvider();
        using var host = Host(orchestrator, time);

        // Test
        await host.StartAsync(CancellationToken.None);
        for (var i = 1; i <= 2; i++)
        {
            await Task.Delay(50);
            time.Advance(TimeSpan.FromMinutes(1));
            await WaitForAsync(() => Volatile.Read(ref calls) >= i);
        }

        await host.StopAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(2, calls);
    }
}
