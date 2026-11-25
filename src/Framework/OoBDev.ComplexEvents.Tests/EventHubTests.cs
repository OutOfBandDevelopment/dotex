using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.ComplexEvents.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class EventHubTests
{
    public TestContext TestContext { get; set; } = null!;

    public sealed record Ping(int N);

    public sealed record Unregistered(int N);

    private sealed class Recorder : IComplexEventDispatcher
    {
        public List<object> Seen { get; } = [];

        public Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
            lock (Seen)
            {
                Seen.Add(@event!);
            }

            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task PublishThenConsume_DispatchesKnownTypesAndSkipsTheRest()
    {
        var client = new InMemoryEventHubClient();
        var recorder = new Recorder();
        var consumer = new EventHubConsumer(client, recorder, [typeof(Ping)]);
        var publisher = new EventHubPublisher(client);
        await publisher.PublishAsync(new Ping(1), CancellationToken.None);
        await publisher.PublishAsync(new Unregistered(2), CancellationToken.None);
        await client.PublishAsync(new EventHubMessage(typeof(Ping).FullName!, new byte[] { 0x7b }), CancellationToken.None);
        await publisher.PublishAsync(new Ping(3), CancellationToken.None);
        using var cts = new CancellationTokenSource();

        var run = consumer.RunAsync(cts.Token);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (consumer.Skipped < 2 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        await Task.Delay(50);
        await cts.CancelAsync();
        await run;
        TestContext.WriteLine($"seen={recorder.Seen.Count} skipped={consumer.Skipped}");

        CollectionAssert.AreEqual(new object[] { new Ping(1), new Ping(3) }, recorder.Seen);
        Assert.AreEqual(2, consumer.Skipped);
    }
}
