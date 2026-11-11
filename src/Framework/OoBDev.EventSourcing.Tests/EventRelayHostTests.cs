using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.EventSourcing.Hosting;
using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class EventRelayHostTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StartAsync_SendsStoredEventsThenStops()
    {
        // Stage
        var sent = new List<EventRecord>();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new Mock<IMessageQueueSender<EventsChannel>>();
        sender.Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
            .Callback<object, string?, string?, string?, int>((m, _, _, _, _) =>
            {
                sent.Add((EventRecord)m);
                if (sent.Count == 2)
                    done.TrySetResult();
            })
            .ReturnsAsync("id");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OoBDev:EventSourcing:Relay:SafetyWindow"] = "00:00:00",
            ["OoBDev:EventSourcing:Relay:PollInterval"] = "00:00:00.020",
        }).Build());
        services.AddSingleton(sender.Object);
        services.TryAddEventRelayHost();
        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey)
            .AppendAsync("s1", 0, [new NewEvent("A", "{}"), new NewEvent("B", "{}")]);
        var host = provider.GetServices<IHostedService>().OfType<EventRelayHost>().Single();

        // Test
        await host.StartAsync(CancellationToken.None);
        await done.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await host.StopAsync(CancellationToken.None);

        // Assert
        CollectionAssert.AreEqual(new[] { "A", "B" }, sent.Select(e => e.Type).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StartAsync_FailingSender_KeepsRunning()
    {
        // Stage
        var calls = 0;
        var retried = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new Mock<IMessageQueueSender<EventsChannel>>();
        sender.Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
            .Returns(() =>
            {
                if (Interlocked.Increment(ref calls) == 1)
                    throw new InvalidOperationException("broker down");
                retried.TrySetResult();
                return Task.FromResult("id");
            });
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OoBDev:EventSourcing:Relay:SafetyWindow"] = "00:00:00",
            ["OoBDev:EventSourcing:Relay:PollInterval"] = "00:00:00.020",
        }).Build());
        services.AddSingleton(sender.Object);
        services.TryAddEventRelayHost();
        await using var provider = services.BuildServiceProvider();
        await provider.GetRequiredKeyedService<IEventStore>(EventSourcingGlobals.EventStoreSelectedKey)
            .AppendAsync("s1", 0, [new NewEvent("A", "{}")]);
        var host = provider.GetServices<IHostedService>().OfType<EventRelayHost>().Single();

        // Test
        await host.StartAsync(CancellationToken.None);
        await retried.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await host.StopAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(calls >= 2);
    }
}
