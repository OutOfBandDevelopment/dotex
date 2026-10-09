using Amazon;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using OoBDev.System;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.MessageQueueing;

/// <summary>
/// Factory for creating instances of <see cref="IAmazonSQS"/> for AWS SQS Queues.
/// </summary>
public class SqsClientFactory : ISqsClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="IAmazonSQS"/> based on the provided configuration section.
    /// </summary>
    /// <param name="config">The configuration section containing queue URL or queue name and AWS credentials.</param>
    /// <returns>A new instance of <see cref="IAmazonSQS"/> and the queue URL for the specified SQS Queue.</returns>
    /// <exception cref="ConfigurationMissingException">
    /// Thrown if the required configuration values (either "QueueUrl" or "QueueName") are missing.
    /// </exception>
    public async Task<(IAmazonSQS client, string queueUrl)> CreateAsync(IConfigurationSection config)
    {
        // Read configuration with validation
        var region = RegionEndpoint.GetBySystemName(config["Region"] ?? "us-east-1");
        var accessKeyId = config["AccessKeyId"];
        var secretAccessKey = config["SecretAccessKey"];

        // Optional endpoint override for emulators such as Moto (leave unset for real AWS)
        var clientConfig = new AmazonSQSConfig { RegionEndpoint = region };
        var serviceUrl = config["ServiceUrl"];
        if (!string.IsNullOrEmpty(serviceUrl))
        {
            clientConfig.ServiceURL = serviceUrl;
            clientConfig.AuthenticationRegion = region.SystemName;
        }

        // Create client (uses AWS credential chain if keys not provided)
        IAmazonSQS client = string.IsNullOrEmpty(accessKeyId)
            ? new AmazonSQSClient(clientConfig)
            : new AmazonSQSClient(accessKeyId, secretAccessKey, clientConfig);

        // Get queue URL
        var queueUrl = config["QueueUrl"];
        if (string.IsNullOrEmpty(queueUrl))
        {
            var queueName = config["QueueName"]
                ?? throw new ConfigurationMissingException($"{config.Path}:QueueName");
            var response = await client.GetQueueUrlAsync(queueName);
            queueUrl = response.QueueUrl;
        }

        return (client, queueUrl);
    }
}
