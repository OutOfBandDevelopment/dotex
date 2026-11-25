using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.ComplexEvents;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;

namespace OoBDev.Microsoft.Azure.EventHub.Tests;

[TestClass]
public sealed class AzureEventHubIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    [IntegrationTestMethod("oobd-test-azurite,oobd-test-eventhubs")]
    [TestCategory("Integration")]
    public async Task PublishAndRead_AgainstTheEmulator_RoundTripsTheEventType()
    {
        // Stage
        var connection = TestContext.GetRequiredProperty<string>("EVENTHUB_CONNECTION_STRING");
        var hub = TestContext.GetRequiredProperty<string>("EVENTHUB_NAME");
        var group = TestContext.GetRequiredProperty<string>("EVENTHUB_CONSUMER_GROUP");
        var type = $"Test_{Guid.NewGuid():N}";
        TestContext.WriteLine($"hub {hub}, group {group}, event type {type}");
        var options = global::Microsoft.Extensions.Options.Options.Create(new AzureEventHubOptions { ConnectionString = connection, EventHubName = hub, ConsumerGroup = group, ReadFromEarliest = true });
        await using var client = new AzureEventHubClient(options);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        // Test
        await client.PublishAsync(new EventHubMessage(type, "{\"value\":7}"u8.ToArray()), timeout.Token);
        EventHubMessage? found = null;
        await foreach (var message in client.ReadAsync(timeout.Token))
        {
            if (message.EventType == type)
            {
                found = message;
                break;
            }
        }

        // Assert
        Assert.IsNotNull(found);
        Assert.AreEqual("{\"value\":7}", global::System.Text.Encoding.UTF8.GetString(found.Payload.Span));
    }
}
