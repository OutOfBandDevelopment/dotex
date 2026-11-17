using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>An in-process <see cref="IInboxStore"/>; it forgets everything on restart, so use a database store when redelivery can cross a restart.</summary>
public class InMemoryInboxStore : IInboxStore
{
    private readonly ConcurrentDictionary<(string, string), byte> _seen = new();

    /// <inheritdoc />
    public Task<bool> TryBeginAsync(string consumer, string messageId) =>
        Task.FromResult(_seen.TryAdd((consumer, messageId), 0));

    /// <inheritdoc />
    public Task ReleaseAsync(string consumer, string messageId)
    {
        _seen.TryRemove((consumer, messageId), out _);
        return Task.CompletedTask;
    }
}
