using System.Globalization;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using OoBDev.MessageQueueing.Services;
using OoBDev.System.Text.Json.Serialization;
using System;
using System.IO;
using System.Threading;
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
public partial class AmazonSqsMessageProvider(
    IJsonSerializer serializer,
    ISqsClientFactory clientFactory,
    ILogger<AmazonSqsMessageProvider> logger
        ) : IMessageSenderProvider, IMessageReceiverProvider
{
    private IMessageHandlerProvider? _handlerProvider;

    /// <summary>
    /// Sends a message asynchronously to an AWS SQS Queue.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">The message context containing additional information.</param>
    /// <returns>The message ID if the send operation is successful; otherwise, <c>null</c>.</returns>
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        var (client, queueUrl) = await clientFactory.CreateAsync(context.Config).ConfigureAwait(false);

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
        await serializer.SerializeAsync(wrapped, stream, default).ConfigureAwait(false);
        var messageBody = Encoding.UTF8.GetString(stream.ToArray());

        // Build SQS request
        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = messageBody,
        };

        // Add optional SQS-specific settings
        if (int.TryParse(context.Config["DelaySeconds"], CultureInfo.InvariantCulture, out var delay))
            request.DelaySeconds = delay;

        var messageGroupId = context.Config["MessageGroupId"];
        if (!string.IsNullOrEmpty(messageGroupId))
            request.MessageGroupId = messageGroupId;

        // Convert context headers to message attributes (the SDK leaves the dictionary null until assigned)
        request.MessageAttributes ??= [];
        foreach (var header in context.Headers.Where(h => h.Value != null))
        {
            request.MessageAttributes[header.Key] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = header.Value?.ToString() ?? ""
            };
        }

        // Send message
        var response = await client.SendMessageAsync(request).ConfigureAwait(false);

        LogSent(response.MessageId, context.CorrelationId);

        return response.MessageId;
    }

    /// <summary>Sets the handler provider that processes received messages.</summary>
    /// <param name="handlerProvider">The message handler provider.</param>
    /// <returns>The current instance.</returns>
    public IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider)
    {
        _handlerProvider = handlerProvider;
        return this;
    }

    /// <summary>
    /// Long-polls the queue until cancelled. A message is deleted only after its handler returns; a handler that throws leaves it on the queue, so SQS redelivers it after the visibility timeout.
    /// Config: <c>WaitSeconds</c> (long-poll wait, 0 to 20, default 20) and <c>MaxMessages</c> (1 to 10, default 10).
    /// </summary>
    /// <param name="cancellationToken">Stops the receiver.</param>
    /// <returns>A task that completes when the receiver is cancelled.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var handlerProvider = _handlerProvider ?? throw new InvalidOperationException("SetHandlerProvider must be called before RunAsync.");
        var config = handlerProvider.Config;
        var waitSeconds = Math.Clamp(int.TryParse(config["WaitSeconds"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var wait) ? wait : 20, 0, 20);
        var maxMessages = Math.Clamp(int.TryParse(config["MaxMessages"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var max) ? max : 10, 1, 10);
        var (client, queueUrl) = await clientFactory.CreateAsync(config).ConfigureAwait(false);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await client.ReceiveMessageAsync(
                    new ReceiveMessageRequest { QueueUrl = queueUrl, WaitTimeSeconds = waitSeconds, MaxNumberOfMessages = maxMessages },
                    cancellationToken).ConfigureAwait(false);

                foreach (var received in response.Messages ?? [])
                {
                    await HandleAsync(client, queueUrl, handlerProvider, received, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task HandleAsync(global::Amazon.SQS.IAmazonSQS client, string queueUrl, IMessageHandlerProvider handlerProvider, Message received, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(received.Body));
            var wrapped = serializer.Deserialize<WrappedQueueMessage>(stream)
                ?? throw new NotSupportedException("No payload found");
            await handlerProvider.HandleAsync(wrapped, received.MessageId).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogFailed(ex, received.MessageId);
            return;
        }

        await client.DeleteMessageAsync(queueUrl, received.ReceiptHandle, cancellationToken).ConfigureAwait(false);
        LogReceived(received.MessageId);
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Received and deleted SQS message: {MessageId}")]
    private partial void LogReceived(string? messageId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "SQS message {MessageId} failed and stays on the queue")]
    private partial void LogFailed(Exception exception, string? messageId);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Sent SQS message: {MessageId}, CorrelationId: {CorrelationId}")]
    private partial void LogSent(string? messageId, string? correlationId);
}
