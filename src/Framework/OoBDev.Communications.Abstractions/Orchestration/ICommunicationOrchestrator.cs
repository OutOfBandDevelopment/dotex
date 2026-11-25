using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions;

/// <summary>The one entry point for "tell this target about this": reads preferences, enhances the data, composes per channel and sends, or holds the request.</summary>
public interface ICommunicationOrchestrator
{
    /// <summary>Handles <paramref name="request"/> now; a normal-priority request inside quiet hours is deferred to the end of the window.</summary>
    Task<CommunicationResult> SendAsync(CommunicationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Holds <paramref name="request"/> until <paramref name="until"/>; preferences are read again when it is released.</summary>
    Task<CommunicationResult> DeferAsync(CommunicationRequest request, DateTimeOffset until, CancellationToken cancellationToken = default);

    /// <summary>Handles every held request that is due and returns how many were released (sent, deferred again or skipped).</summary>
    Task<int> ReleaseDueAsync(CancellationToken cancellationToken = default);
}
