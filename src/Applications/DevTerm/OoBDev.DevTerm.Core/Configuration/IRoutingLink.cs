using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DevTerm.Configuration;

/// <summary>One live connection between a <see cref="MessageRouter"/> and a broker, whichever protocol; <see cref="RoutingService"/> rebuilds it after a loss.</summary>
public interface IRoutingLink : IAsyncDisposable
{
    /// <summary>The connection ended after a successful start; the argument is the failure, or null for a clean close.</summary>
    event Action<Exception?>? Lost;

    Task StartAsync(MessageRouter router, CancellationToken cancellationToken);

    Task PublishAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken);
}
