using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Reads an event hub and dispatches each message to the handlers of its event type. Unknown event types are skipped.</summary>
/// <param name="client">The hub client.</param>
/// <param name="dispatcher">The dispatcher.</param>
/// <param name="eventTypes">The event types this consumer accepts.</param>
public sealed class EventHubConsumer(IEventHubClient client, IComplexEventDispatcher dispatcher, IEnumerable<Type> eventTypes)
{
    private static readonly MethodInfo Dispatch = typeof(IComplexEventDispatcher).GetMethod(nameof(IComplexEventDispatcher.DispatchAsync))!;
    private readonly Dictionary<string, Type> types = BuildMap(eventTypes);

    /// <summary>Messages skipped because their type is unknown or the payload did not deserialize.</summary>
    public int Skipped { get; private set; }

    /// <summary>Consumes until cancelled; a cancelled token ends the loop without throwing.</summary>
    /// <param name="cancellationToken">Cancellation.</param>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in client.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var @event = Deserialize(message);
                if (@event is null)
                {
                    Skipped++;
                    continue;
                }

                var task = (Task)Dispatch.MakeGenericMethod(@event.GetType()).Invoke(dispatcher, [@event, cancellationToken])!;
                await task.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static Dictionary<string, Type> BuildMap(IEnumerable<Type> eventTypes)
    {
        ArgumentNullException.ThrowIfNull(eventTypes);
        var map = new Dictionary<string, Type>(StringComparer.Ordinal);
        foreach (var type in eventTypes)
        {
            map[type.FullName ?? type.Name] = type;
        }

        return map;
    }

    private object? Deserialize(EventHubMessage message)
    {
        if (!types.TryGetValue(message.EventType, out var type))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(message.Payload.Span, type);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
