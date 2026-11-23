using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Serializes events as JSON and publishes them to an event hub, tagged with the event type name.</summary>
/// <param name="client">The hub client.</param>
public sealed class EventHubPublisher(IEventHubClient client)
{
    /// <summary>Publishes an event.</summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="event">The event.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(@event);
        return client.PublishAsync(new EventHubMessage(typeof(TEvent).FullName ?? typeof(TEvent).Name, JsonSerializer.SerializeToUtf8Bytes(@event)), cancellationToken);
    }
}
