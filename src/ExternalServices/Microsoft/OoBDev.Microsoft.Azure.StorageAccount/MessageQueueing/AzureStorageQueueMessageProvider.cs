using OoBDev.MessageQueueing.Services;
using OoBDev.System;
using OoBDev.System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Azure.StorageAccount.MessageQueueing;

/// <summary>
/// Provides functionality for sending and receiving messages using Azure Storage Queues.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="AzureStorageQueueMessageProvider"/> class.
/// </remark>
/// <param name="serializer">The JSON serializer for message serialization and deserialization.</param>
/// <param name="clientFactory">The factory for creating Azure Storage Queue clients.</param>
/// <param name="mapper">The model mapper for Azure Storage Queues.</param>
/// <param name="logger">The logger for logging messages.</param>
public class AzureStorageQueueMessageProvider(
    IJsonSerializer serializer,
    IQueueClientFactory clientFactory,
    IAzureStorageQueueMapper mapper,
    ILogger<AzureStorageQueueMessageProvider> logger
        ) : IMessageSenderProvider, IMessageReceiverProvider
{
    private IMessageHandlerProvider? _handlerProvider;
    private readonly IAzureStorageQueueMapper _mapper = mapper;

    /// <summary>
    /// Sends a message asynchronously to an Azure Storage Queue.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">The message context containing additional information.</param>
    /// <returns>The message ID if the send operation is successful; otherwise, <c>null</c>.</returns>
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        var client = clientFactory.Create(context.Config);

        if (_mapper.EnsureQueueExists(context.Config))
            client.CreateIfNotExists();

        var wrapped = _mapper.Wrap(message, context);

        var serialized = serializer.Serialize(wrapped);
        var result = await client.SendMessageAsync(serialized);

        return result.Value.MessageId;
    }

    /// <summary>
    /// Sets the message handler provider for processing received messages.
    /// </summary>
    /// <param name="handlerProvider">The message handler provider.</param>
    /// <returns>The current instance of <see cref="AzureStorageQueueMessageProvider"/>.</returns>
    public IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider)
    {
        _handlerProvider = handlerProvider;
        return this;
    }

    /// <summary>
    /// Runs the message receiver asynchronously, continuously listening for incoming messages.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to stop the receiver.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync(
        CancellationToken cancellationToken = default
        )
    {
        var client = clientFactory.Create(_handlerProvider?.Config ?? throw new ConfigurationMissingException("UNKNOWN"));

        if (!client.Exists(cancellationToken))
        {
            if (_mapper.EnsureQueueExists(_handlerProvider.Config))
            {
                logger.LogWarning("Creating {queueName} as it does not exist", client.Name);
                client.CreateIfNotExists();
            }
            else
            {
                logger.LogError("Queue {queueName} does not exist", client.Name);
                throw new NotSupportedException($"Queue {client.Name} does not exist");
            }
        }

        var newCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken).Token;

        while (!newCancellationToken.IsCancellationRequested)
        {
            var message = await client.ReceiveMessageAsync(TimeSpan.FromMinutes(1), newCancellationToken);

            if (message?.Value == null)
            {
                logger.LogInformation($"Nothing Received waiting");
                await Task.Delay(_mapper.WaitDelay(_handlerProvider.Config), cancellationToken);
                continue;
            }

            using var stream = message.Value.Body.ToStream();
            var deserialized = serializer.Deserialize<WrappedQueueMessage>(stream)
                ?? throw new NotSupportedException($"No payload found");

            await _handlerProvider.HandleAsync(deserialized, message.Value.MessageId);

            logger.LogInformation($"Dequeue: {{{nameof(message.Value.MessageId)}}}", message.Value.MessageId);
            _ = await client.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt, newCancellationToken);
        }
    }
}
