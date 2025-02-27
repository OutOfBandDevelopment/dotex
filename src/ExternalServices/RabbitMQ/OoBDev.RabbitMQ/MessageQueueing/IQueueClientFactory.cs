using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace OoBDev.RabbitMQ.MessageQueueing;

/// <summary>
/// Factory for creating instances of <see cref="IConnection"/> and <see cref="IChannel"/> for Rabbit MQ Queues.
/// </summary>
public interface IQueueClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IConnection"/> and <see cref="IChannel"/> based on the provided configuration section.
    /// </summary>
    /// <param name="config">The configuration section containing connection string and queue name.</param>
    /// <returns>A new instance of <see cref="IConnection"/> and <see cref="IChannel"/> for the specified Rabbit MQ Queue.</returns>
    /// <exception cref="ApplicationException">
    /// Thrown if the required configuration values ("ConnectionString" or "QueueName") are missing.
    /// </exception>
    Task<(IConnection connection, IChannel channel, string queueName)> CreateAsync(IConfigurationSection config);
}
