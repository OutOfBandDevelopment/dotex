using Microsoft.Data.SqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;

/// <inheritdoc />
public class BrokerConnectionFactory : IBrokerConnectionFactory
{
    /// <inheritdoc />
    public async Task<SqlConnection> OpenAsync(ServiceBrokerOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var connection = new SqlConnection(options.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using (var check = new SqlCommand(BrokerScripts.BrokerEnabledQuery, connection))
            {
                var enabled = await check.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                if (enabled is not true)
                {
                    throw new InvalidOperationException($"Service Broker is not enabled for database '{connection.Database}'. Run ALTER DATABASE ... SET ENABLE_BROKER.");
                }
            }

            if (options.EnsureObjects)
            {
                using var ensure = new SqlCommand(BrokerScripts.Ensure(options), connection);
                await ensure.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            return connection;
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
