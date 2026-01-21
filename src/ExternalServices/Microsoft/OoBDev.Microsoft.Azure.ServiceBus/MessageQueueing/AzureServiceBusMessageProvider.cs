using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using OoBDev.MessageQueueing.Services;
using OoBDev.System.Text.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing;

/// <summary>
/// Provides functionality for sending messages using Azure Service Bus Queues and Topics.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="AzureServiceBusMessageProvider"/> class.
/// </remark>
/// <param name="serializer">The JSON serializer for message serialization.</param>
/// <param name="senderFactory">The factory for creating Azure Service Bus senders.</param>
/// <param name="logger">The logger for logging messages.</param>
public class AzureServiceBusMessageProvider(
    IJsonSerializer serializer,
    IServiceBusSenderFactory senderFactory,
    ILogger<AzureServiceBusMessageProvider> logger
        ) : IMessageSenderProvider
{
    /// <summary>
    /// Sends a message asynchronously to an Azure Service Bus Queue or Topic.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">The message context containing additional information.</param>
    /// <returns>The correlation ID if the send operation is successful; otherwise, <c>null</c>.</returns>
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        var (client, sender) = await senderFactory.CreateAsync(context.Config);

        try
        {
            // Wrap message (standard pattern)
            var wrapped = new WrappedQueueMessage
            {
                ContentType = "application/json",
                PayloadType = message.GetType().AssemblyQualifiedName
                    ?? throw new NotSupportedException(),
                CorrelationId = context.CorrelationId ?? Guid.NewGuid().ToString(),
                Payload = message,
                Properties = context.Headers,
            };

            // Serialize to JSON
            using var stream = new MemoryStream();
            await serializer.SerializeAsync(wrapped, stream, default);

            // Create Service Bus message
            var busMessage = new ServiceBusMessage(stream.ToArray())
            {
                CorrelationId = context.CorrelationId ?? Guid.NewGuid().ToString(),
                ContentType = "application/json",
            };

            // Add optional Service Bus-specific settings
            var sessionId = context.Config["SessionId"];
            if (!string.IsNullOrEmpty(sessionId))
                busMessage.SessionId = sessionId;

            // Convert context headers to ApplicationProperties
            foreach (var header in context.Headers.Where(h => h.Value != null))
            {
                busMessage.ApplicationProperties[header.Key] = header.Value;
            }

            // Send message
            await sender.SendMessageAsync(busMessage);

            logger.LogInformation(
                "Sent Service Bus message: CorrelationId: {CorrelationId}",
                busMessage.CorrelationId
            );

            return busMessage.CorrelationId;
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
