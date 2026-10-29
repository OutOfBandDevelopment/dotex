using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OoBDev.AsyncApi;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;

/// <summary>
/// Describes a Service Broker queue for the AsyncAPI document; the connection string is never copied, only its server.
/// </summary>
public class ServiceBrokerAsyncApiContributor : IAsyncApiContributor
{
    /// <inheritdoc />
    public string ProviderKey => ServiceBrokerGlobals.MessageProviderKey;

    /// <inheritdoc />
    public AsyncApiContribution? Describe(IConfigurationSection config)
    {
        var host = "localhost";
        if (config["ConnectionString"] is { Length: > 0 } connectionString)
        {
            try
            {
                var dataSource = new SqlConnectionStringBuilder(connectionString).DataSource;
                if (!string.IsNullOrEmpty(dataSource))
                {
                    host = dataSource;
                }
            }
            catch (global::System.ArgumentException)
            {
                // An unparsable connection string leaves the default host.
            }
        }

        return new AsyncApiContribution(
            "sql-service-broker",
            new AsyncApiServer { Host = host, Protocol = "mssql", Description = "SQL Server Service Broker" },
            config["QueueName"],
            null);
    }
}
