using System;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.Brokers;

namespace OoBDev.DevTerm.Configuration;

internal sealed class BrokerRoutingLink(BrokerRouterBridge bridge) : IRoutingLink
{
    public event Action<Exception?>? Lost
    {
        add => bridge.Lost += value;
        remove => bridge.Lost -= value;
    }

    public Task StartAsync(MessageRouter router, CancellationToken cancellationToken) => bridge.StartAsync(router, cancellationToken);

    public Task PublishAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) => bridge.PublishAsync(topic, payload, cancellationToken);

    public ValueTask DisposeAsync() => bridge.DisposeAsync();
}
