using System.Collections.Generic;
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
public class ReceivePollingHostTests
{
    private static ReceivePollingHost Host(Mock<IReceiveService> receive, FakeTimeProvider time, params ChannelDefinition[] channels)
        => new(receive.Object, Options.Create(new SendReceiveOptions { Channels = channels }), time, NullLogger<ReceivePollingHost>.Instance);

    private static ChannelDefinition Channel(string name, TimeSpan? interval)
        => new() { Name = name, Protocol = "email", Provider = "p", PollInterval = interval, PollBatchSize = 5 };

    private static async Task WaitForAsync(Func<bool> condition)
    {
        for (var i = 0; i < 200 && !condition(); i++)
        {
            await Task.Delay(10);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Host_PolledChannel_PollsEveryInterval_AndSkipsOthers()
    {
        var receive = new Mock<IReceiveService>();
        var polls = 0;
        receive.Setup(r => r.PollAsync("mail", 5, It.IsAny<CancellationToken>()))
            .Callback(() => Interlocked.Increment(ref polls))
            .ReturnsAsync([]);
        var time = new FakeTimeProvider();
        using var host = Host(receive, time, Channel("mail", TimeSpan.FromMinutes(1)), Channel("sms", null));

        await host.StartAsync(CancellationToken.None);
        for (var i = 1; i <= 2; i++)
        {
            await Task.Delay(50);
            time.Advance(TimeSpan.FromMinutes(1));
            await WaitForAsync(() => Volatile.Read(ref polls) >= i);
        }

        await host.StopAsync(CancellationToken.None);
        Assert.AreEqual(2, polls);
        receive.Verify(r => r.PollAsync("sms", It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Host_FailedPoll_KeepsPolling()
    {
        var receive = new Mock<IReceiveService>();
        var polls = 0;
        receive.Setup(r => r.PollAsync("mail", 5, It.IsAny<CancellationToken>()))
            .Returns(() => Interlocked.Increment(ref polls) == 1
                ? Task.FromException<IReadOnlyList<IMessage>>(new InvalidOperationException("down"))
                : Task.FromResult<IReadOnlyList<IMessage>>([]));
        var time = new FakeTimeProvider();
        using var host = Host(receive, time, Channel("mail", TimeSpan.FromSeconds(30)));

        await host.StartAsync(CancellationToken.None);
        for (var i = 1; i <= 2; i++)
        {
            await Task.Delay(50);
            time.Advance(TimeSpan.FromSeconds(30));
            await WaitForAsync(() => Volatile.Read(ref polls) >= i);
        }

        await host.StopAsync(CancellationToken.None);
        Assert.AreEqual(2, polls);
    }
}
