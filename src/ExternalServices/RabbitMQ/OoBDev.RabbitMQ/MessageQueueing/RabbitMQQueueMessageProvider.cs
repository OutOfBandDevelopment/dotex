using OoBDev.MessageQueueing.Services;
using OoBDev.System;
using OoBDev.System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.RabbitMQ.MessageQueueing;

/// <summary>
/// Provides functionality for sending and receiving messages using Rabbit MQ Queues.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="RabbitMQQueueMessageProvider"/> class.
/// </remark>
/// <param name="serializer">The JSON serializer for message serialization and deserialization.</param>
/// <param name="clientFactory">The factory for creating Rabbit MQ Queue clients.</param>
/// <param name="logger">The logger for logging messages.</param>
public class RabbitMQQueueMessageProvider(
    IJsonSerializer serializer,
    IQueueClientFactory clientFactory,
    ILogger<RabbitMQQueueMessageProvider> logger
        ) : IMessageSenderProvider, IMessageReceiverProvider
{
    private IMessageHandlerProvider? _handlerProvider;

    /// <summary>
    /// Sends a message asynchronously to an Rabbit MQ Queue.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="context">The message context containing additional information.</param>
    /// <returns>The message ID if the send operation is successful; otherwise, <c>null</c>.</returns>
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        var (connection, channel, queueName) = clientFactory.Create(context.Config);

        var wrapped = new WrappedQueueMessage
        {
            ContentType = "application/json;",
            PayloadType = message.GetType().AssemblyQualifiedName ?? throw new NotSupportedException(),
            CorrelationId = context.CorrelationId ?? "",
            Payload = message,
            Properties = context.Headers,
        };

        using var stream = new MemoryStream();
        await serializer.SerializeAsync(wrapped, stream, default);
        var body = stream.ToArray();

        using (connection)
        using (channel)
        {
            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: queueName,
                                 basicProperties: null,
                                 mandatory: true,
                                 body: body);

            return context.CorrelationId;
        }
    }

    /// <summary>
    /// Sets the message handler provider for processing received messages.
    /// </summary>
    /// <param name="handlerProvider">The message handler provider.</param>
    /// <returns>The current instance of <see cref="RabbitMQQueueMessageProvider"/>.</returns>
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
        // https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html

        var (connection, channel, queueName) = clientFactory.Create(_handlerProvider?.Config ?? throw new ConfigurationMissingException("UNKNOWN"));
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var newCancellationToken = cts.Token;

        using (connection)
        using (channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                using var stream = new MemoryStream(body);
                var deserialized = serializer.Deserialize<WrappedQueueMessage>(stream)
                    ?? throw new NotSupportedException($"No payload found");

                var message = Encoding.UTF8.GetString(body);
                logger.LogInformation($"Dequeue: {{{nameof(deserialized.CorrelationId)}}}", deserialized.CorrelationId);

                await _handlerProvider.HandleAsync(deserialized, deserialized.CorrelationId);

                logger.LogInformation($"Dequeue: {{{nameof(deserialized.CorrelationId)}}}", deserialized.CorrelationId);

            };

            channel.QueueDeclare(queue: queueName, exclusive: false);
            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            while (!newCancellationToken.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }
    }
}
