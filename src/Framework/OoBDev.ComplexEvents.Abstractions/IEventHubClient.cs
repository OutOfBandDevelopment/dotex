using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Publishes to and reads from an event hub; the Azure adapter and the in-memory fake both implement it.</summary>
public interface IEventHubClient
{
    /// <summary>Publishes one message.</summary>
    /// <param name="message">The message.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task PublishAsync(EventHubMessage message, CancellationToken cancellationToken);

    /// <summary>Reads messages until cancelled.</summary>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The messages as they arrive.</returns>
    IAsyncEnumerable<EventHubMessage> ReadAsync(CancellationToken cancellationToken);
}
