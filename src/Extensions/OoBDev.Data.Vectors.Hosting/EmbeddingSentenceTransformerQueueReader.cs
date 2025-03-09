using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using OoBDev.AI;
using OoBDev.Data.Common;
using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.System.Text.Xml.Serialization;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace OoBDev.Data.Vectors.Hosting;

[ConnectionStringName("EmbeddingSentenceTransformer")]
public class EmbeddingSentenceTransformerQueueReader : IEmbeddingSentenceTransformerQueueReader
{
    private readonly IDatabaseQuery<EmbeddingSentenceTransformerQueueReader> _database;
    private readonly IEmbeddingProvider _embedding;
    private readonly ILogger _logger;
    private readonly IXmlSerializer _xml;
    private readonly IJsonSerializer _json;

    public EmbeddingSentenceTransformerQueueReader(
        IDatabaseQuery<EmbeddingSentenceTransformerQueueReader> database,
        IEmbeddingProvider embedding,
        ILogger<EmbeddingSentenceTransformerQueueReader> logger,
        IXmlSerializer xml,
        IJsonSerializer json
        )
    {
        _database = database;
        _embedding = embedding;
        _logger = logger;
        _xml = xml;
        _json = json;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using var connection = _database.GetConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);
            var transaction = await connection.BeginTransactionAsync(cancellationToken);
            try
            {
                using var receiver = connection.CreateCommand();
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

                var maxRead = receiver.CreateParameter();
                maxRead.ParameterName = "@maxRead";
                maxRead.DbType = DbType.Int32;
                maxRead.Value = 100;
                maxRead.Direction = ParameterDirection.Input;
                receiver.Parameters.Add(maxRead);

                var timeout = receiver.CreateParameter();
                timeout.ParameterName = "@timeout";
                timeout.DbType = DbType.Int32;
                timeout.Value = 6000;
                timeout.Direction = ParameterDirection.Input;
                receiver.Parameters.Add(timeout);

                var commands = new List<DbCommand>();
                using (var reader = await receiver.ExecuteReaderAsync(cancellationToken))
                {
                    var read = false;
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        read = true;
                        var conversationGroupId = (Guid)reader["conversation_group_id"];
                        var conversationHandle = (Guid)reader["conversation_handle"];
                        var messageType = (string?)reader["message_type_name"];
                        var messageBodyXml = reader["message_body_xml"] == DBNull.Value ? null : (string)reader["message_body_xml"];

                        _logger.LogInformation($"{{{nameof(conversationHandle)}}}: {{{nameof(messageType)}}}", conversationHandle, messageType);

                        if (messageType == "oobdev://embedding/sentence-transformer/request")
                        {
                            // oobdev://embedding/sentence-transformer
                            // ﻿<st:request xmlns:st="oobdev://embedding/sentence-transformer/request" id="1" value="gsdgs" tableName="[dbo].[Names]"/>

                            XNamespace st = "oobdev://embedding/sentence-transformer/request";
                            var xml = XElement.Parse(messageBodyXml);

                            var id = (long?)xml.Attribute("id");
                            var value = (string?)xml.Attribute("value");
                            var tableName = (string?)xml.Attribute("tableName");

                            var embedding = string.IsNullOrWhiteSpace(value) ? null : await _embedding.GenerateEmbeddingAsync(value, default, cancellationToken);
                            var json = _json.Serialize(embedding.ToArray());

                            XNamespace sr = "oobdev://embedding/sentence-transformer/response";
                            var response = new XElement(
                                sr + "response",
                                id.HasValue ? new XAttribute("id", id) : null,
                                !string.IsNullOrWhiteSpace(value) ? new XAttribute("value", value) : null,
                                !string.IsNullOrWhiteSpace(tableName) ? new XAttribute("tableName", tableName) : null,
                                !string.IsNullOrWhiteSpace(json) ? new XAttribute("embedding", json) : null
                            );

                            var send = connection.CreateCommand();
                            send.Transaction = transaction;
                            send.CommandText = @"
	SEND ON CONVERSATION @conversationHandle
		MESSAGE TYPE [oobdev://embedding/sentence-transformer/response]
		(@message);";

                            var conversationHandleSend = receiver.CreateParameter();
                            conversationHandleSend.ParameterName = "@conversationHandle";
                            conversationHandleSend.DbType = DbType.Guid;
                            conversationHandleSend.Value = conversationHandle;
                            conversationHandleSend.Direction = ParameterDirection.Input;
                            send.Parameters.Add(conversationHandleSend);

                            var message = receiver.CreateParameter();
                            message.ParameterName = "@message";
                            message.DbType = DbType.String;
                            message.Value = response.ToString();
                            message.Direction = ParameterDirection.Input;
                            send.Parameters.Add(message);

                            commands.Add(send);
                        }
                        else if (messageType == "http://schemas.microsoft.com/SQL/ServiceBroker/EndDialog")
                        {
                            var end = connection.CreateCommand();
                            end.Transaction = transaction;
                            end.CommandText = "END CONVERSATION @conversationHandle;";

                            var conversationHandleSend = receiver.CreateParameter();
                            conversationHandleSend.ParameterName = "@conversationHandle";
                            conversationHandleSend.DbType = DbType.Guid;
                            conversationHandleSend.Value = conversationHandle;
                            conversationHandleSend.Direction = ParameterDirection.Input;
                            end.Parameters.Add(conversationHandleSend);

                            commands.Add(end);
                        }
                        else
                        {
                            throw new NotSupportedException($"{nameof(messageType)}: {messageType} is not supported");
                        }
                    }

                    if (!read)
                    {
                        _logger.LogWarning($"Timeout!  No messages received");
                    }
                }

                foreach (var command in commands)
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }


                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{{{nameof(ex.Message)}}}", ex.Message);
                await transaction.RollbackAsync(cancellationToken);
                await Task.Delay(1000, cancellationToken); //TODO: make this configurable
            }
        }
    }
}
