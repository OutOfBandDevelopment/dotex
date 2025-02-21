using OoBDev.MessageQueueing.Services;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing;

/// <summary>
/// Represents a generic interface for handling messages from a message queue.
/// </summary>
public interface IMessageQueueHandler
{
    /// <summary>
    /// Handles the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="context">The context associated with the message.</param>
    /// <returns>A task that represents the asynchronous handling of the message.</returns>
    Task HandleAsync(object message, IMessageContext context);
}

/// <summary>
/// Represents a generic interface for handling messages of type <typeparamref name="TChannel"/> from a message queue.
/// </summary>
/// <typeparam name="TChannel">The type of channel for the messages.</typeparam>
public interface IMessageQueueHandler<TChannel> : IMessageQueueHandler
{
}

/// <summary>
/// Represents a generic interface for handling messages of type <typeparamref name="TMessage"/>
/// from a message queue associated with a specific channel of type <typeparamref name="TChannel"/>.
/// </summary>
/// <typeparam name="TChannel">The type of channel for the messages.</typeparam>
/// <typeparam name="TMessage">The type of messages to handle.</typeparam>
public interface IMessageQueueHandler<TChannel, TMessage> : IMessageQueueHandler<TChannel>
{
    /// <summary>
    /// Handles the specified message asynchronously.
    /// </summary>
    /// <param name="message">The message to handle.</param>
    /// <param name="context">The context associated with the message.</param>
    /// <returns>A task that represents the asynchronous handling of the message.</returns>
    Task HandleAsync(TMessage message, IMessageContext context);
}
