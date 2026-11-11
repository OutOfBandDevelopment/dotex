using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.PostgreSql.Pgmq.MessageQueueing;

/// <summary>
/// Opens connections to the database that hosts a pgmq queue.
/// </summary>
public interface IPgmqClientFactory
{
    /// <summary>
    /// Opens a connection and makes sure the queue exists.
    /// </summary>
    /// <param name="config">The configuration section of the queue.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The open connection (the caller disposes it) and the queue settings.</returns>
    Task<(NpgsqlConnection Connection, PgmqSettings Settings)> CreateAsync(IConfigurationSection config, CancellationToken cancellationToken = default);
}
