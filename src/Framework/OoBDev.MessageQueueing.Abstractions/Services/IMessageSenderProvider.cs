using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Represents a provider for sending messages to a message queue.
/// </summary>
public interface IMessageSenderProvider
{
    /// <summary>
    /// Sends a message asynchronously to the message queue.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="context">The context associated with the message.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the unique identifier assigned to the sent message.</returns>
    Task<string?> SendAsync(object message, IMessageContext context);
}
