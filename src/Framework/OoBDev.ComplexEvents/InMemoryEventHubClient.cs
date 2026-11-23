using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>An in-process <see cref="IEventHubClient"/> for tests and single-process use.</summary>
public sealed class InMemoryEventHubClient : IEventHubClient
{
    private readonly Channel<EventHubMessage> channel = Channel.CreateUnbounded<EventHubMessage>();

    /// <inheritdoc />
    public async Task PublishAsync(EventHubMessage message, CancellationToken cancellationToken)
        => await channel.Writer.WriteAsync(message, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public IAsyncEnumerable<EventHubMessage> ReadAsync(CancellationToken cancellationToken)
        => channel.Reader.ReadAllAsync(cancellationToken);
}
