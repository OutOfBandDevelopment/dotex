using Microsoft.Extensions.DependencyInjection;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using System;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Queue handler that accepts a CloudEvents 1.0 JSON message and appends it to the event store, so an external
/// producer can feed a stream through any queue provider. The event keeps the stream and sequence it names.
/// </summary>
/// <param name="store">The event store.</param>
public sealed class CloudEventsIngressHandler([FromKeyedServices(EventSourcingGlobals.EventStoreSelectedKey)] IEventStore store)
    : IMessageQueueHandler<EventsChannel, string>
{
    /// <inheritdoc />
    public Task HandleAsync(object message, IMessageContext context)
        => HandleAsync(message as string ?? throw new ArgumentException("A CloudEvents message must be a JSON string.", nameof(message)), context);

    /// <inheritdoc />
    public async Task HandleAsync(string message, IMessageContext context)
    {
        var record = CloudEventMapper.FromCloudEvent(message);
        var result = await store.AppendAsync(record.StreamId, record.Sequence - 1, [new NewEvent(record.Type, record.Payload, record.Metadata)]);
        if (!result.Succeeded)
            throw new InvalidOperationException($"CloudEvent {record.StreamId}:{record.Sequence} does not follow the stream (last sequence {result.LastSequence}).");
    }
}
