using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using OoBDev.System;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing;

/// <summary>
/// Factory for creating instances of <see cref="ServiceBusClient"/> and <see cref="ServiceBusSender"/> for Azure Service Bus.
/// </summary>
public class ServiceBusSenderFactory : IServiceBusSenderFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="ServiceBusClient"/> and <see cref="ServiceBusSender"/> based on the provided configuration section.
    /// </summary>
    /// <param name="config">The configuration section containing connection string and queue/topic name.</param>
    /// <returns>A new instance of <see cref="ServiceBusClient"/> and <see cref="ServiceBusSender"/> for the specified Azure Service Bus entity.</returns>
    /// <exception cref="ConfigurationMissingException">
    /// Thrown if the required configuration values ("ConnectionString" and either "QueueName" or "TopicName") are missing.
    /// </exception>
    public async Task<(ServiceBusClient client, ServiceBusSender sender)> CreateAsync(IConfigurationSection config)
    {
        var connectionString = config["ConnectionString"]
            ?? throw new ConfigurationMissingException($"{config.Path}:ConnectionString");

        var client = new ServiceBusClient(connectionString);

        // Support both Queue and Topic
        var queueName = config["QueueName"];
        var topicName = config["TopicName"];

        if (string.IsNullOrEmpty(queueName) && string.IsNullOrEmpty(topicName))
            throw new ConfigurationMissingException($"{config.Path}:QueueName or TopicName");

        var entityName = queueName ?? topicName!;
        var sender = client.CreateSender(entityName);

        return await Task.FromResult((client, sender));
    }
}
