using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing;

/// <summary>
/// Factory for creating instances of <see cref="ServiceBusClient"/> and <see cref="ServiceBusSender"/> for Azure Service Bus.
/// </summary>
public interface IServiceBusSenderFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="ServiceBusClient"/> and <see cref="ServiceBusSender"/> based on the provided configuration section.
    /// </summary>
    /// <param name="config">The configuration section containing connection string and queue/topic name.</param>
    /// <returns>A new instance of <see cref="ServiceBusClient"/> and <see cref="ServiceBusSender"/> for the specified Azure Service Bus entity.</returns>
    /// <exception cref="OoBDev.System.ConfigurationMissingException">
    /// Thrown if the required configuration values ("ConnectionString" and either "QueueName" or "TopicName") are missing.
    /// </exception>
    Task<(ServiceBusClient client, ServiceBusSender sender)> CreateAsync(IConfigurationSection config);
}
