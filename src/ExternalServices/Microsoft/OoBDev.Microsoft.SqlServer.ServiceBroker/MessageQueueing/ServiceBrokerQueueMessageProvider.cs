using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OoBDev.MessageQueueing.Services;
using OoBDev.System;
using OoBDev.System.Text.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;

/// <summary>
/// Sends and receives messages through a SQL Server Service Broker queue.
/// </summary>
/// <param name="serializer">The JSON serializer for the message envelope.</param>
/// <param name="connectionFactory">The factory that opens connections.</param>
/// <param name="logger">The logger.</param>
public partial class ServiceBrokerQueueMessageProvider(
    IJsonSerializer serializer,
    IBrokerConnectionFactory connectionFactory,
    ILogger<ServiceBrokerQueueMessageProvider> logger
        ) : IMessageSenderProvider, IMessageReceiverProvider
{
    private const string EndDialogType = "http://schemas.microsoft.com/SQL/ServiceBroker/EndDialog";
    private const string ErrorType = "http://schemas.microsoft.com/SQL/ServiceBroker/Error";

    private readonly ConcurrentDictionary<(Guid Conversation, long Order), int> _failures = new();
    private IMessageHandlerProvider? _handlerProvider;

    /// <inheritdoc />
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(context);

        var options = ServiceBrokerOptions.From(context.Config);

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

        using var connection = await connectionFactory.OpenAsync(options).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction();
        using var command = new SqlCommand(BrokerScripts.Send, connection, transaction);
        command.Parameters.Add("@from", SqlDbType.NVarChar, 256).Value = options.ServiceName;
        command.Parameters.Add("@to", SqlDbType.NVarChar, 256).Value = options.TargetServiceName;
        command.Parameters.Add("@contract", SqlDbType.NVarChar, 256).Value = options.ContractName;
        command.Parameters.Add("@body", SqlDbType.VarBinary, -1).Value = stream.ToArray();
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        await transaction.CommitAsync().ConfigureAwait(false);

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
        var handlerProvider = _handlerProvider ?? throw new ConfigurationMissingException("UNKNOWN");
        var options = ServiceBrokerOptions.From(handlerProvider.Config);

        try
        {
            using var connection = await connectionFactory.OpenAsync(options, cancellationToken).ConfigureAwait(false);
            var receive = BrokerScripts.Receive(options);

            while (!cancellationToken.IsCancellationRequested)
            {
                await ReceiveOneAsync(connection, receive, options, handlerProvider, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested && ex is OperationCanceledException or SqlException)
        {
            // Stopping is the normal way out of the loop; cancelling a WAITFOR surfaces as a SqlException.
        }
    }

    private async Task ReceiveOneAsync(
        SqlConnection connection,
        string receive,
        ServiceBrokerOptions options,
        IMessageHandlerProvider handlerProvider,
        CancellationToken cancellationToken)
    {
        using var transaction = connection.BeginTransaction();

        Guid conversation = Guid.Empty;
        long order = 0;
        string messageType = "";
        byte[]? body = null;
        var found = false;

        using (var command = new SqlCommand(receive, connection, transaction))
        {
            command.CommandTimeout = options.ReceiveTimeoutSeconds + 30;
            command.Parameters.Add("@timeout", SqlDbType.Int).Value = options.ReceiveTimeoutSeconds * 1000;
            using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            found = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (found)
            {
                order = reader.GetInt64(0);
                conversation = reader.GetGuid(1);
                messageType = reader.GetString(2);
                body = reader.IsDBNull(3) ? null : reader.GetSqlBinary(3).Value;
            }
        }

        if (!found)
        {
            await transaction.CommitAsync(CancellationToken.None).ConfigureAwait(false);
            return;
        }

        if (messageType is EndDialogType or ErrorType)
        {
            await EndConversationAsync(connection, transaction, conversation, error: null).ConfigureAwait(false);
            await transaction.CommitAsync(CancellationToken.None).ConfigureAwait(false);
            return;
        }

        try
        {
            using var stream = new MemoryStream(body ?? []);
            var message = serializer.Deserialize<WrappedQueueMessage>(stream)
                ?? throw new NotSupportedException("No payload found");

            LogDequeue(message.CorrelationId);
            await handlerProvider.HandleAsync(message, message.CorrelationId).ConfigureAwait(false);

            _failures.TryRemove((conversation, order), out _);
            await transaction.CommitAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var failures = _failures.AddOrUpdate((conversation, order), 1, (_, count) => count + 1);
            if (failures >= options.MaxRetries)
            {
                LogPoison(ex, conversation, failures);
                _failures.TryRemove((conversation, order), out _);
                await EndConversationAsync(connection, transaction, conversation, error: $"Handler failed {failures} times: {ex.Message}").ConfigureAwait(false);
                await transaction.CommitAsync(CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                LogRetry(ex, conversation, failures);
                await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
            }
        }
    }

    private static async Task EndConversationAsync(SqlConnection connection, SqlTransaction transaction, Guid conversation, string? error)
    {
        var sql = error is null
            ? "END CONVERSATION @h;"
            : "END CONVERSATION @h WITH ERROR = 50000 DESCRIPTION = @description;";

        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.Add("@h", SqlDbType.UniqueIdentifier).Value = conversation;
        if (error is not null)
        {
            command.Parameters.Add("@description", SqlDbType.NVarChar, 3000).Value = error.Length > 3000 ? error[..3000] : error;
        }

        try
        {
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        catch (SqlException ex) when (ex.Number == 8429)
        {
            // The conversation was already ended by the other side.
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Enqueue: {correlationId}")]
    private partial void LogEnqueue(string? correlationId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Dequeue: {correlationId}")]
    private partial void LogDequeue(string? correlationId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Handler failed for conversation {conversation} (attempt {attempt}); the message returns to the queue")]
    private partial void LogRetry(Exception exception, Guid conversation, int attempt);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Handler failed for conversation {conversation} {attempts} times; the conversation is ended with an error")]
    private partial void LogPoison(Exception exception, Guid conversation, int attempts);
}
