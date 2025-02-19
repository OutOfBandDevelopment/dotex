using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>
/// Provides a mechanism for handling queue messages.
/// </summary>
public interface IMessageHandlerProvider
{
    /// <summary>
    /// Handles the specified queue message with the given message identifier.
    /// </summary>
    /// <param name="message">The queue message to handle.</param>
    /// <param name="messageId">The identifier associated with the message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(IQueueMessage message, string messageId);

    /// <summary>
    /// Gets the configuration section associated with the message handler provider.
    /// </summary>
    IConfigurationSection Config { get; }
}
