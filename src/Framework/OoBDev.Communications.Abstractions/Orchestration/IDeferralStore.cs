using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>Holds requests until they are due; replace the in-memory default with a durable store.</summary>
public interface IDeferralStore
{
    /// <summary>Holds <paramref name="deferred"/>.</summary>
    Task AddAsync(DeferredRequest deferred, CancellationToken cancellationToken = default);

    /// <summary>Removes and returns every request due at or before <paramref name="now"/>, earliest first.</summary>
    Task<IReadOnlyList<DeferredRequest>> TakeDueAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
}
