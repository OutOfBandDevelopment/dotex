using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing;

/// <summary>
/// Represents a generic interface for sending messages to a message queue.
/// </summary>
public interface IMessageQueueSender
{
    /// <summary>
    /// Sends the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="messageId">The optional message identifier.</param>
    /// <param name="callerMember">Supplied by the compiler; recorded in the message context.</param>
    /// <param name="callerFile">Supplied by the compiler; recorded in the message context.</param>
    /// <param name="callerLine">Supplied by the compiler; recorded in the message context.</param>
    /// <returns>A task that represents the asynchronous sending of the message.</returns>
    Task<string> SendAsync(
        object message,
        string? messageId = default,
        [CallerMemberName] string? callerMember = default,
        [CallerFilePath] string? callerFile = default,
        [CallerLineNumber] int callerLine = default
    );
}

/// <summary>
/// Represents a generic interface for sending messages of type <typeparamref name="TChannel"/> to a message queue.
/// </summary>
/// <typeparam name="TChannel">The type of channel for the messages.</typeparam>
public interface IMessageQueueSender<TChannel> : IMessageQueueSender
{
}
