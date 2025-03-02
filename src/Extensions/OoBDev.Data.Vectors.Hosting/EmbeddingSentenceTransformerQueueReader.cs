using Microsoft.Extensions.Logging;
using OoBDev.AI;
using OoBDev.Data.Common;
using OoBDev.System.ComponentModel;
using System.Data;

namespace OoBDev.Data.Vectors.Hosting;

[ConnectionStringName("EmbeddingSentenceTransformer")]
public class EmbeddingSentenceTransformerQueueReader : IEmbeddingSentenceTransformerQueueReader
{
    private readonly IDatabaseQuery<EmbeddingSentenceTransformerQueueReader> _database;
    private readonly IEmbeddingProvider _embedding;
    private readonly ILogger _logger;

    public EmbeddingSentenceTransformerQueueReader(
        IDatabaseQuery<EmbeddingSentenceTransformerQueueReader> database,
        IEmbeddingProvider embedding,
        ILogger<EmbeddingSentenceTransformerQueueReader> logger
        )
    {
        _database = database;
        _embedding = embedding;
        _logger = logger;
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
                maxRead.Value = 5;
                maxRead.Direction = ParameterDirection.Input;
                receiver.Parameters.Add(maxRead);

                var timeout = receiver.CreateParameter();
                timeout.ParameterName = "@timeout";
                timeout.DbType = DbType.Int32;
                timeout.Value = 6000;
                timeout.Direction = ParameterDirection.Input;
                receiver.Parameters.Add(timeout);

                using (var reader = await receiver.ExecuteReaderAsync(cancellationToken))
                {
                    var read = false;
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        read = true;
                        var conversationGroupId = (Guid)reader["conversation_group_id"];
                        var conversationHandle = (Guid)reader["conversation_handle"];
                        var mesageType = (string)reader["message_type_name"];
                        var messageBodyXml = (string)reader["message_body_xml"];

                        _logger.LogInformation($"{{{nameof(conversationHandle)}}}: {{{nameof(mesageType)}}}", conversationHandle, mesageType);

                        //check message type

                        //using var requestEnd = connection.CreateCommand();
                        //requestEnd.Transaction = transaction;
                        //receiver.CommandText = @"END CONVERSATION @conversationHandle;";

                        //var conversationHandleParameter = receiver.CreateParameter();
                        //conversationHandleParameter.ParameterName = "@conversationHandle";
                        //conversationHandleParameter.DbType = DbType.Guid;
                        //conversationHandleParameter.Value = conversationHandle;
                        //conversationHandleParameter.Direction = ParameterDirection.Input;
                        //requestEnd.Parameters.Add(conversationHandleParameter);
                    }

                    if (!read)
                    {
                        _logger.LogWarning($"Timeout!  No messages received");
                    }
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
