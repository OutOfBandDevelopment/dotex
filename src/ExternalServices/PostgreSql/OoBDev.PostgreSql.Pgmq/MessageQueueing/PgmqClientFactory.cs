using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.PostgreSql.Pgmq.MessageQueueing;

/// <summary>
/// Opens Npgsql connections for pgmq queues; pooling is left to Npgsql.
/// </summary>
public class PgmqClientFactory : IPgmqClientFactory
{
    /// <inheritdoc />
    public async Task<(NpgsqlConnection Connection, PgmqSettings Settings)> CreateAsync(IConfigurationSection config, CancellationToken cancellationToken = default)
    {
        var settings = PgmqSettings.From(config);
        var connection = new NpgsqlConnection(settings.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            // pgmq.create does nothing when the queue already exists
            await using var create = new NpgsqlCommand("SELECT pgmq.create(@queue)", connection);
            create.Parameters.AddWithValue("queue", settings.QueueName);
            await create.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            return (connection, settings);
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
