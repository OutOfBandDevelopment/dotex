using Microsoft.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;

/// <summary>
/// Opens connections to the database that holds the queue.
/// </summary>
public interface IBrokerConnectionFactory
{
    /// <summary>
    /// Opens a connection, checks that Service Broker is enabled and, when configured, creates the broker objects.
    /// </summary>
    /// <param name="options">The queue settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An open connection that the caller disposes.</returns>
    /// <exception cref="global::System.InvalidOperationException">Service Broker is not enabled for the database.</exception>
    Task<SqlConnection> OpenAsync(ServiceBrokerOptions options, CancellationToken cancellationToken = default);
}
