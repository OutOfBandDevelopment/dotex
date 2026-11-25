using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace OoBDev.WebApi.Services;

/// <summary>A bounded in-memory record of the messages the example handler received, newest first, for the receive monitor.</summary>
public class ReceivedMessageLog(TimeProvider timeProvider)
{
    /// <summary>The number of messages kept.</summary>
    public const int Capacity = 50;

    private readonly Queue<ReceivedMessageModel> _items = new();
    private readonly object _lock = new();

    /// <summary>Records a received message and drops the oldest beyond <see cref="Capacity"/>.</summary>
    /// <param name="message">The received message.</param>
    /// <param name="correlationId">The correlation id, if any.</param>
    public void Add(object message, string? correlationId)
    {
        var entry = new ReceivedMessageModel
        {
            ReceivedAt = timeProvider.GetUtcNow(),
            CorrelationId = correlationId,
            Message = message as string ?? JsonSerializer.Serialize(message),
        };
        lock (_lock)
        {
            _items.Enqueue(entry);
            while (_items.Count > Capacity)
            {
                _items.Dequeue();
            }
        }
    }

    /// <summary>Gets the recorded messages, newest first.</summary>
    /// <returns>A snapshot of the log.</returns>
    public IReadOnlyList<ReceivedMessageModel> Recent()
    {
        lock (_lock)
        {
            return [.. _items.Reverse()];
        }
    }
}
