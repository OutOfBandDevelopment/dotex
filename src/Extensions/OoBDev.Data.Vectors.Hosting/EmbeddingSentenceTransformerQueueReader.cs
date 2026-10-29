using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.AI;
using OoBDev.Data.Common;
using OoBDev.System.ComponentModel.Data;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.System.Text.Xml.Serialization;
using System.Data;
using System.Data.Common;
using System.Xml.Linq;

namespace OoBDev.Data.Vectors.Hosting;

/// <summary>
/// Reads embedding requests from a SQL Service Broker queue and generates embeddings using a sentence transformer model.
/// Processes messages, generates embeddings, and sends responses back through the queue.
/// </summary>
[ConnectionStringName("EmbeddingSentenceTransformer")]
public partial class EmbeddingSentenceTransformerQueueReader : IEmbeddingSentenceTransformerQueueReader
{
    private readonly IDatabaseQuery<EmbeddingSentenceTransformerQueueReader> _database;
    private readonly IEmbeddingProvider _embedding;
    private readonly ILogger _logger;
    private readonly IXmlSerializer _xml;
    private readonly IJsonSerializer _json;
    private readonly IOptions<EmbeddingSentenceTransformerQueueReaderOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingSentenceTransformerQueueReader"/> class.
    /// </summary>
    /// <param name="database">The database query provider for queue access.</param>
    /// <param name="embedding">The embedding provider for generating sentence embeddings.</param>
    /// <param name="logger">The logger for diagnostics.</param>
    /// <param name="xml">The XML serializer for message processing.</param>
    /// <param name="json">The JSON serializer for embedding serialization.</param>
    /// <param name="options">The configuration options for queue reading.</param>
    public EmbeddingSentenceTransformerQueueReader(
        IDatabaseQuery<EmbeddingSentenceTransformerQueueReader> database,
        IEmbeddingProvider embedding,
        ILogger<EmbeddingSentenceTransformerQueueReader> logger,
        IXmlSerializer xml,
        IJsonSerializer json,
        IOptions<EmbeddingSentenceTransformerQueueReaderOptions> options
        )
    {
        _database = database;
        _embedding = embedding;
        _logger = logger;
        _xml = xml;
        _json = json;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var connection = _database.GetConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using var receiver = BuildReceiver(connection, transaction);

                var commands = new List<DbCommand>();
                using (var reader = await receiver.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var read = false;
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        read = true;
                        var conversationGroupId = (Guid)reader["conversation_group_id"];
                        var conversationHandle = (Guid)reader["conversation_handle"];
                        var messageType = (string?)reader["message_type_name"];
                        var messageBodyXml = reader["message_body_xml"] == DBNull.Value ? null : (string)reader["message_body_xml"];

                        LogMessageReceived(conversationHandle, messageType);

                        if (messageType == "oobdev://embedding/sentence-transformer/request" && messageBodyXml != null)
                        {
                            var response = await BuildResponseAsync(messageBodyXml, cancellationToken).ConfigureAwait(false);

                            commands.Add(BuildSend(connection, transaction, conversationHandle, response.ToString()));
                        }
                        else if (messageType == "http://schemas.microsoft.com/SQL/ServiceBroker/EndDialog")
                        {
                            commands.Add(BuildEnd(connection, transaction, conversationHandle));
                        }
                        else
                        {
                            throw new NotSupportedException($"{nameof(messageType)}: {messageType} is not supported");
                        }
                    }

                    if (!read)
                        LogTimeout();
                }

                foreach (var command in commands)
                {
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogFailure(ex.Message);
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                await Task.Delay((int)_options.Value.ReadWaitTimeout.TotalMilliseconds, cancellationToken).ConfigureAwait(false); 
            }
        }
    }

    private DbCommand BuildReceiver(DbConnection connection, DbTransaction transaction)
    {
        var receiver = connection.CreateCommand();
        receiver.Transaction = transaction;
        receiver.CommandText = @"
	WAITFOR (
			RECEIVE TOP (@maxRead)
				 conversation_group_id
				,conversation_handle
				,message_type_name
				,CAST(message_body AS XML) AS [message_body_xml]
				FROM [embedding].[oobdev://embedding/sentence-transformer/queue]
		), TIMEOUT @timeout;";

        AddParameter(receiver, "@maxRead", DbType.Int32, _options.Value.MaximumReadLength);
        AddParameter(receiver, "@timeout", DbType.Int32, _options.Value.ReadWaitTimeout.TotalMilliseconds);
        return receiver;
    }

    private async Task<XElement> BuildResponseAsync(string messageBodyXml, CancellationToken cancellationToken)
    {
        // oobdev://embedding/sentence-transformer
        // ﻿<st:request xmlns:st="oobdev://embedding/sentence-transformer/request" id="1" value="gsdgs" tableName="[dbo].[Names]"/>

        XNamespace st = "oobdev://embedding/sentence-transformer/request";
        var xml = XElement.Parse(messageBodyXml);

        var id = (long?)xml.Attribute("id");
        var value = (string?)xml.Attribute("value");
        var tableName = (string?)xml.Attribute("tableName");

        var embedding = string.IsNullOrWhiteSpace(value) ? null : await _embedding.GenerateEmbeddingAsync(value, default, cancellationToken).ConfigureAwait(false);
        var json = _json.Serialize(embedding.ToArray());

        XNamespace sr = "oobdev://embedding/sentence-transformer/response";
        var response = new XElement(
            sr + "response",
            id.HasValue ? new XAttribute("id", id) : null,
            !string.IsNullOrWhiteSpace(value) ? new XAttribute("value", value) : null,
            !string.IsNullOrWhiteSpace(tableName) ? new XAttribute("tableName", tableName) : null,
            !string.IsNullOrWhiteSpace(json) ? new XAttribute("embedding", json) : null
        );
        return response;
    }

    private static void AddParameter(DbCommand command, string name, DbType type, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value;
        parameter.Direction = ParameterDirection.Input;
        command.Parameters.Add(parameter);
    }

    private static DbCommand BuildSend(DbConnection connection, DbTransaction transaction, Guid conversationHandle, string message)
    {
        var send = connection.CreateCommand();
        send.Transaction = transaction;
        send.CommandText = @"
	SEND ON CONVERSATION @conversationHandle
		MESSAGE TYPE [oobdev://embedding/sentence-transformer/response]
		(@message);";
        AddParameter(send, "@conversationHandle", DbType.Guid, conversationHandle);
        AddParameter(send, "@message", DbType.String, message);
        return send;
    }

    private static DbCommand BuildEnd(DbConnection connection, DbTransaction transaction, Guid conversationHandle)
    {
        var end = connection.CreateCommand();
        end.Transaction = transaction;
        end.CommandText = "END CONVERSATION @conversationHandle;";
        AddParameter(end, "@conversationHandle", DbType.Guid, conversationHandle);
        return end;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{conversationHandle}: {messageType}")]
    private partial void LogMessageReceived(Guid conversationHandle, string? messageType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Timeout!  No messages received")]
    private partial void LogTimeout();

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "{Message}")]
    private partial void LogFailure(string message);
}
