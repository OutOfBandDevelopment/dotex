using OoBDev.System;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;

namespace OoBDev.RabbitMQ.MessageQueueing;

/// <summary>
/// Factory for creating instances of <see cref="IConnection"/> and <see cref="IModel"/>> for Rabbit MQ Queues.
/// </summary>
public class QueueClientFactory : IQueueClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IConnection"/> based on the provided configuration section.
    /// </summary>
    /// <param name="config">The configuration section containing connection string and queue name.</param>
    /// <returns>A new instance of <see cref="IConnection"/> and <see cref="IModel"/> for the specified Rabbit MQ Queue.</returns>
    /// <exception cref="ApplicationException">
    /// Thrown if the required configuration values ("ConnectionString" or "QueueName") are missing.
    /// </exception>
    public (IConnection connection, IModel channel, string queueName) Create(IConfigurationSection config)
    {
        var factory = new ConnectionFactory()
        {
            HostName = config[nameof(ConnectionFactory.HostName)],
        };
        var connection = factory.CreateConnection();
        var model = connection.CreateModel();

        var queueName = config["QueueName"] ?? throw new ConfigurationMissingException($"{config.Path}:QueueName");

        return (connection, model, queueName);
    }
}
