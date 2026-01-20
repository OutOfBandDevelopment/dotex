using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.Configuration;

namespace OoBDev.Azure.StorageAccount.MessageQueueing;

/// <summary>
/// Provides functionality to map objects to and from Azure Storage Queue messages.
/// </summary>
public interface IAzureStorageQueueMapper
{
    /// <summary>
    /// Ensures that the Azure Storage Queue exists based on the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration containing information about whether to ensure the queue exists.</param>
    /// <returns>True if the queue exists or should be ensured to exist; otherwise, false.</returns>
    bool EnsureQueueExists(IConfiguration configuration);

    /// <summary>
    /// Wraps the provided message and message context into a <see cref="WrappedQueueMessage"/>.
    /// </summary>
    /// <param name="message">The message object to wrap.</param>
    /// <param name="context">The message context.</param>
    /// <returns>A wrapped queue message.</returns>
    WrappedQueueMessage Wrap(object message, IMessageContext context);

    /// <summary>
    /// Retrieves the wait delay value from the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration containing the wait delay value.</param>
    /// <returns>The wait delay value, in milliseconds.</returns>
    int WaitDelay(IConfiguration configuration);
}
