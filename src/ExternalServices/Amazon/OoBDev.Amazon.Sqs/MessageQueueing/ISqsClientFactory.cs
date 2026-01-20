using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.MessageQueueing;

/// <summary>
/// Factory for creating instances of <see cref="IAmazonSQS"/> for AWS SQS Queues.
/// </summary>
public interface ISqsClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IAmazonSQS"/> based on the provided configuration section.
    /// </summary>
    /// <param name="config">The configuration section containing queue URL or queue name and AWS credentials.</param>
    /// <returns>A new instance of <see cref="IAmazonSQS"/> and the queue URL for the specified SQS Queue.</returns>
    /// <exception cref="OoBDev.System.ConfigurationMissingException">
    /// Thrown if the required configuration values (either "QueueUrl" or "QueueName") are missing.
    /// </exception>
    Task<(IAmazonSQS client, string queueUrl)> CreateAsync(IConfigurationSection config);
}
