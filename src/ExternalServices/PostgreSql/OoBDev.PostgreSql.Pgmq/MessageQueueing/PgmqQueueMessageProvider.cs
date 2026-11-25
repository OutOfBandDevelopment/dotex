using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using OoBDev.MessageQueueing.Services;
using OoBDev.System;
using OoBDev.System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.PostgreSql.Pgmq.MessageQueueing;

/// <summary>
/// Sends and receives messages through a pgmq queue (PostgreSQL). A received message is archived after its handler
/// succeeds; when the handler throws it is not archived and is delivered again after the visibility timeout.
/// </summary>
/// <param name="serializer">The JSON serializer.</param>
/// <param name="clientFactory">The connection factory.</param>
/// <param name="logger">The logger.</param>
public partial class PgmqQueueMessageProvider(
    IJsonSerializer serializer,
    IPgmqClientFactory clientFactory,
    ILogger<PgmqQueueMessageProvider> logger
    ) : IMessageSenderProvider, IMessageReceiverProvider
{
    private IMessageHandlerProvider? _handlerProvider;

    /// <inheritdoc />
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);

        var wrapped = new WrappedQueueMessage
        {
            ContentType = "application/json;",
            PayloadType = message.GetType().AssemblyQualifiedName ?? throw new NotSupportedException(),
            CorrelationId = context.CorrelationId ?? "",
            Payload = message,
            Properties = context.Headers,
        };
        using var stream = new MemoryStream();
        await serializer.SerializeAsync(wrapped, stream, default).ConfigureAwait(false);

        var (connection, settings) = await clientFactory.CreateAsync(context.Config).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            await using var command = new NpgsqlCommand("SELECT pgmq.send(@queue, @message)", connection);
            command.Parameters.AddWithValue("queue", settings.QueueName);
            command.Parameters.Add(new NpgsqlParameter("message", NpgsqlDbType.Jsonb) { Value = Encoding.UTF8.GetString(stream.ToArray()) });
            await command.ExecuteScalarAsync().ConfigureAwait(false);
        }

        LogEnqueue(context.CorrelationId);
        return context.CorrelationId;
    }

    /// <inheritdoc />
    public IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider)
    {
        _handlerProvider = handlerProvider;
        return this;
    }

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var handler = _handlerProvider ?? throw new ConfigurationMissingException("UNKNOWN");
        var (connection, settings) = await clientFactory.CreateAsync(handler.Config, cancellationToken).ConfigureAwait(false);
        await using (connection.ConfigureAwait(false))
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var received = await ReceiveBatchAsync(connection, settings, handler, cancellationToken).ConfigureAwait(false);
                    if (received == 0)
                        await Task.Delay(settings.PollInterval, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // the host is stopping
            }
        }
    }

    private async Task<int> ReceiveBatchAsync(NpgsqlConnection connection, PgmqSettings settings, IMessageHandlerProvider handler, CancellationToken cancellationToken)
    {
        var batch = new List<(long Id, string Body, int Reads)>();
        await using (var read = new NpgsqlCommand("SELECT msg_id, message::text, read_ct FROM pgmq.read(@queue, @vt, @qty)", connection))
        {
            read.Parameters.AddWithValue("queue", settings.QueueName);
            read.Parameters.AddWithValue("vt", (int)settings.VisibilityTimeout.TotalSeconds);
            read.Parameters.AddWithValue("qty", settings.BatchSize);
            var reader = await read.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            await using (reader.ConfigureAwait(false))
            {
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    batch.Add((reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2)));
            }
        }

        foreach (var (id, body, reads) in batch)
        {
            if (settings.MaxDeliveries > 0 && reads > settings.MaxDeliveries)
            {
                await using var dead = new NpgsqlCommand("SELECT pgmq.send(@dlq, @message::jsonb)", connection);
                dead.Parameters.AddWithValue("dlq", settings.DeadLetterQueueName);
                dead.Parameters.AddWithValue("message", body);
                await dead.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                await ArchiveAsync(connection, settings, id).ConfigureAwait(false);
                LogDeadLettered(id, reads);
                continue;
            }

            try
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
                var wrapped = serializer.Deserialize<WrappedQueueMessage>(stream) ?? throw new NotSupportedException("No payload found");
                LogDequeue(wrapped.CorrelationId);
                await handler.HandleAsync(wrapped, wrapped.CorrelationId).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogHandlerFailed(ex, id);
                continue;
            }

            await ArchiveAsync(connection, settings, id).ConfigureAwait(false);
        }

        return batch.Count;
    }

    private static async Task ArchiveAsync(NpgsqlConnection connection, PgmqSettings settings, long id)
    {
        await using var archive = new NpgsqlCommand("SELECT pgmq.archive(@queue, @id)", connection);
        archive.Parameters.AddWithValue("queue", settings.QueueName);
        archive.Parameters.AddWithValue("id", id);
        // not cancellable: a message the handler finished must be archived even while the host stops
        await archive.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Enqueue: {correlationId}")]
    private partial void LogEnqueue(string? correlationId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Dequeue: {correlationId}")]
    private partial void LogDequeue(string? correlationId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Handler failed for pgmq message {messageId}; it will be delivered again after the visibility timeout")]
    private partial void LogHandlerFailed(Exception exception, long messageId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "pgmq message {messageId} was read {reads} times and moved to the dead-letter queue")]
    private partial void LogDeadLettered(long messageId, int reads);
}
