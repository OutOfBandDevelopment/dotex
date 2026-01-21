using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using OoBDev.MessageQueueing.Services;
using OoBDev.System.Text.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.MessageQueueing;

/// <summary>
/// Provides functionality for sending messages using AWS SQS Queues.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="AmazonSqsMessageProvider"/> class.
/// </remark>
/// <param name="serializer">The JSON serializer for message serialization.</param>
/// <param name="clientFactory">The factory for creating AWS SQS clients.</param>
/// <param name="logger">The logger for logging messages.</param>
public class AmazonSqsMessageProvider(
    IJsonSerializer serializer,
    ISqsClientFactory clientFactory,
    ILogger<AmazonSqsMessageProvider> logger
        ) : IMessageSenderProvider
{
    /// <summary>
    /// Sends a message asynchronously to an AWS SQS Queue.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">The message context containing additional information.</param>
    /// <returns>The message ID if the send operation is successful; otherwise, <c>null</c>.</returns>
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        var (client, queueUrl) = await clientFactory.CreateAsync(context.Config);

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
        var messageBody = Encoding.UTF8.GetString(stream.ToArray());

        // Build SQS request
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = messageBody,
        };

        // Add optional SQS-specific settings
        if (int.TryParse(context.Config["DelaySeconds"], out var delay))
            request.DelaySeconds = delay;

        var messageGroupId = context.Config["MessageGroupId"];
        if (!string.IsNullOrEmpty(messageGroupId))
            request.MessageGroupId = messageGroupId;

        // Convert context headers to message attributes
        foreach (var header in context.Headers.Where(h => h.Value != null))
        {
            request.MessageAttributes[header.Key] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = header.Value?.ToString() ?? ""
            };
        }

        // Send message
        var response = await client.SendMessageAsync(request);

        logger.LogInformation(
            "Sent SQS message: {MessageId}, CorrelationId: {CorrelationId}",
            response.MessageId,
            context.CorrelationId
        );

        return response.MessageId;
    }
}
