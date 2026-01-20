using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.Configuration;
using System;

namespace OoBDev.Azure.StorageAccount.MessageQueueing;

/// <summary>
/// Provides functionality to map objects to and from Azure Storage Queue messages.
/// </summary>
public class AzureStorageQueueMapper : IAzureStorageQueueMapper
{
    /// <summary>
    /// Wraps the provided message and message context into a <see cref="WrappedQueueMessage"/>.
    /// </summary>
    /// <param name="message">The message object to wrap.</param>
    /// <param name="context">The message context.</param>
    /// <returns>A wrapped queue message.</returns>
    public WrappedQueueMessage Wrap(object message, IMessageContext context) =>
         new()
         {
             ContentType = "application/json;",
             PayloadType = message.GetType().AssemblyQualifiedName ?? throw new NotSupportedException(),
             CorrelationId = context.CorrelationId ?? "",
             Payload = message,
             Properties = context.Headers,
         };

    /// <summary>
    /// Ensures that the Azure Storage Queue exists based on the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration containing information about whether to ensure the queue exists.</param>
    /// <returns>True if the queue exists or should be ensured to exist; otherwise, false.</returns>
    public bool EnsureQueueExists(IConfiguration configuration) =>
        bool.TryParse(configuration["EnsureQueueExists"], out var result) && result;

    /// <summary>
    /// Retrieves the wait delay value from the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration containing the wait delay value.</param>
    /// <returns>The wait delay value, in milliseconds.</returns>
    public int WaitDelay(IConfiguration configuration) =>
        int.TryParse(configuration["WaitDelay"], out var result) ? result : 1000;
}
