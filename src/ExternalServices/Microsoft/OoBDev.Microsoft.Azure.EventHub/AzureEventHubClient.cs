using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;
using OoBDev.ComplexEvents;

namespace OoBDev.Microsoft.Azure.EventHub;

/// <summary>The Azure Event Hubs implementation of <see cref="IEventHubClient"/>. The event type travels in the <c>eventType</c> application property.</summary>
public sealed class AzureEventHubClient : IEventHubClient, IAsyncDisposable
{
    /// <summary>The application property that carries the event type name.</summary>
    public const string EventTypeProperty = "eventType";

    private readonly EventHubProducerClient producer;
    private readonly EventHubConsumerClient consumer;
    private readonly bool fromEarliest;

    /// <summary>Creates the client from options.</summary>
    /// <param name="options">The settings.</param>
    public AzureEventHubClient(IOptions<AzureEventHubOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var value = options.Value;
        var group = string.IsNullOrWhiteSpace(value.ConsumerGroup) ? EventHubConsumerClient.DefaultConsumerGroupName : value.ConsumerGroup;
        producer = string.IsNullOrWhiteSpace(value.EventHubName)
            ? new EventHubProducerClient(value.ConnectionString)
            : new EventHubProducerClient(value.ConnectionString, value.EventHubName);
        consumer = string.IsNullOrWhiteSpace(value.EventHubName)
            ? new EventHubConsumerClient(group, value.ConnectionString)
            : new EventHubConsumerClient(group, value.ConnectionString, value.EventHubName);
        fromEarliest = value.ReadFromEarliest;
    }

    /// <inheritdoc />
    public async Task PublishAsync(EventHubMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        var data = new EventData(message.Payload);
        data.Properties[EventTypeProperty] = message.EventType;
        await producer.SendAsync([data], cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<EventHubMessage> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var events = consumer.ReadEventsAsync(fromEarliest, cancellationToken: cancellationToken);
        await foreach (var partitionEvent in events.ConfigureAwait(false))
        {
            if (partitionEvent.Data is { } data && data.Properties.TryGetValue(EventTypeProperty, out var type) && type is string name)
            {
                yield return new EventHubMessage(name, data.EventBody.ToMemory());
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await producer.DisposeAsync().ConfigureAwait(false);
        await consumer.DisposeAsync().ConfigureAwait(false);
    }
}
