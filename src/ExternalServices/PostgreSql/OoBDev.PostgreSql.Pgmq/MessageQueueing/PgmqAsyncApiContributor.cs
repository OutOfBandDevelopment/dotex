using Microsoft.Extensions.Configuration;
using Npgsql;
using OoBDev.AsyncApi;
using System;

namespace OoBDev.PostgreSql.Pgmq.MessageQueueing;

/// <summary>
/// Describes a pgmq queue for the AsyncAPI document; the connection string is never copied, only its host and port.
/// </summary>
public class PgmqAsyncApiContributor : IAsyncApiContributor
{
    /// <inheritdoc />
    public string ProviderKey => PgmqGlobals.MessageProviderKey;

    /// <inheritdoc />
    public AsyncApiContribution? Describe(IConfigurationSection config)
    {
        var host = "localhost";
        if (config["ConnectionString"] is { Length: > 0 } connectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                if (!string.IsNullOrEmpty(builder.Host))
                    host = builder.Port == 5432 ? builder.Host : $"{builder.Host}:{builder.Port}";
            }
            catch (ArgumentException)
            {
                // An unparsable connection string leaves the default host.
            }
        }

        return new AsyncApiContribution(
            "pgmq",
            new AsyncApiServer { Host = host, Protocol = "postgresql", Description = "PostgreSQL pgmq queue" },
            config["QueueName"],
            null);
    }
}
