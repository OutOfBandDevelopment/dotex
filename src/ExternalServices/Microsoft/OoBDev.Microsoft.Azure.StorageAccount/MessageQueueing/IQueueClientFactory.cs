using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using System;

namespace OoBDev.Azure.StorageAccount.MessageQueueing;

/// <summary>
/// Factory for creating instances of <see cref="QueueClient"/> for Azure Storage Queues.
/// </summary>
public interface IQueueClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="QueueClient"/> based on the provided configuration section.
    /// </summary>
    /// <param name="config">The configuration section containing connection string and queue name.</param>
    /// <returns>A new instance of <see cref="QueueClient"/> for the specified Azure Storage Queue.</returns>
    /// <exception cref="ApplicationException">
    /// Thrown if the required configuration values ("ConnectionString" or "QueueName") are missing.
    /// </exception>
    QueueClient Create(IConfigurationSection config);
}
