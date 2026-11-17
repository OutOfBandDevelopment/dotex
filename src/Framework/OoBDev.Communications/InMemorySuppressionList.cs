using OoBDev.Communications.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications;

/// <summary>An in-process <see cref="ISuppressionList"/>; it forgets everything on restart, so production hosts supply a durable one.</summary>
public class InMemorySuppressionList : ISuppressionList
{
    private readonly ConcurrentDictionary<string, string> _reasons = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<bool> IsSuppressedAsync(string address, CancellationToken cancellationToken = default) =>
        Task.FromResult(_reasons.ContainsKey(address));

    /// <inheritdoc />
    public Task AddAsync(string address, string reason, CancellationToken cancellationToken = default)
    {
        _reasons.TryAdd(address, reason);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string address, CancellationToken cancellationToken = default)
    {
        _reasons.TryRemove(address, out _);
        return Task.CompletedTask;
    }
}
