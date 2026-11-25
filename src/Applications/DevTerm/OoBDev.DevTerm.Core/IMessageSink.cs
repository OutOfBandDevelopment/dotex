using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DevTerm;

/// <summary>Where a <see cref="MessageRouter"/> publishes device messages: a broker connection, or a fake in tests.</summary>
public interface IMessageSink
{
    Task PublishAsync(string topic, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken = default);
}
