using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Provides functionality for receiving messages from a message queue.
/// </summary>
public interface IMessageReceiverProvider
{
    /// <summary>
    /// Sets the handler provider for processing received messages.
    /// </summary>
    /// <param name="handlerProvider">The message handler provider.</param>
    /// <returns>The current instance of the message receiver provider.</returns>
    IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider);

    /// <summary>
    /// Runs the message receiver asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync(CancellationToken cancellationToken = default);
}
