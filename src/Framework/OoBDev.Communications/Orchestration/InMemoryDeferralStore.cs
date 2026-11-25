using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.Orchestration;

/// <summary>Keeps held requests in memory; they are lost on restart, so use a durable <see cref="IDeferralStore"/> where that matters.</summary>
public sealed class InMemoryDeferralStore : IDeferralStore
{
    private readonly Lock _gate = new();
    private readonly List<DeferredRequest> _held = [];

    /// <inheritdoc />
    public Task AddAsync(DeferredRequest deferred, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(deferred);
        lock (_gate)
        {
            _held.Add(deferred);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<DeferredRequest>> TakeDueAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var due = _held.Where(d => d.DueAt <= now).OrderBy(d => d.DueAt).ToList();
            _held.RemoveAll(d => d.DueAt <= now);
            return Task.FromResult<IReadOnlyList<DeferredRequest>>(due);
        }
    }
}
