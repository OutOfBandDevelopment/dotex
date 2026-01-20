using OoBDev.System;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace OoBDev.RabbitMQ.MessageQueueing;

/// <summary>
/// Factory for creating instances of <see cref="IConnection"/> and <see cref="IChannel"/>> for Rabbit MQ Queues.
/// </summary>
public class QueueClientFactory : IQueueClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IConnection"/> based on the provided configuration section.
    /// </summary>
    /// <param name="config">The configuration section containing connection string and queue name.</param>
    /// <returns>A new instance of <see cref="IConnection"/> and <see cref="IChannel"/> for the specified Rabbit MQ Queue.</returns>
    /// <exception cref="ApplicationException">
    /// Thrown if the required configuration values ("ConnectionString" or "QueueName") are missing.
    /// </exception>
    public async Task<(IConnection connection, IChannel channel, string queueName)> CreateAsync(IConfigurationSection config)
    {
        var factory = new ConnectionFactory()
        {
            HostName = config[nameof(ConnectionFactory.HostName)] ?? throw new ConfigurationMissingException($"{config.Path}:{nameof(ConnectionFactory.HostName)}"),
        };

        // Optional: Port (default 5672)
        if (int.TryParse(config[nameof(ConnectionFactory.Port)], out var port))
        {
            factory.Port = port;
        }

        // Optional: Username (default "guest")
        if (config[nameof(ConnectionFactory.UserName)] is { } userName)
        {
            factory.UserName = userName;
        }

        // Optional: Password (default "guest")
        if (config[nameof(ConnectionFactory.Password)] is { } password)
        {
            factory.Password = password;
        }

        // Optional: Connection timeout (default 30 seconds) - set to shorter for tests
        if (int.TryParse(config[nameof(ConnectionFactory.RequestedConnectionTimeout)], out var connectionTimeout))
        {
            factory.RequestedConnectionTimeout = TimeSpan.FromMilliseconds(connectionTimeout);
        }

        // Optional: Handshake timeout (default 10 seconds)
        if (int.TryParse(config[nameof(ConnectionFactory.HandshakeContinuationTimeout)], out var handshakeTimeout))
        {
            factory.HandshakeContinuationTimeout = TimeSpan.FromMilliseconds(handshakeTimeout);
        }

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var queueName = config["QueueName"] ?? throw new ConfigurationMissingException($"{config.Path}:QueueName");

        return (connection, channel, queueName);
    }
}
