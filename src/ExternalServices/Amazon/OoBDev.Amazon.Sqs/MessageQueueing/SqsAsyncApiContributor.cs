using Microsoft.Extensions.Configuration;
using OoBDev.AsyncApi;
using System;
using System.Text.Json.Nodes;

namespace OoBDev.Amazon.Sqs.MessageQueueing;

/// <summary>Describes SQS queues for the AsyncAPI document. Credentials are not read.</summary>
public class SqsAsyncApiContributor : IAsyncApiContributor
{
    /// <inheritdoc />
    public string ProviderKey => AwsSqsGlobals.MessageProviderKey;

    /// <inheritdoc />
    public AsyncApiContribution? Describe(IConfigurationSection config)
    {
        var queueName = config["QueueName"];
        var queueUrl = config["QueueUrl"];
        if (queueName == null && Uri.TryCreate(queueUrl, UriKind.Absolute, out var url))
        {
            queueName = url.Segments[^1].Trim('/');
        }

        var host = Uri.TryCreate(config["ServiceUrl"], UriKind.Absolute, out var service)
            ? service.Authority
            : $"sqs.{config["Region"] ?? "us-east-1"}.amazonaws.com";

        var bindings = queueName == null ? null : new JsonObject
        {
            ["sqs"] = new JsonObject
            {
                ["bindingVersion"] = "0.2.0",
                ["queue"] = new JsonObject
                {
                    ["name"] = queueName,
                    ["fifoQueue"] = queueName.EndsWith(".fifo", StringComparison.OrdinalIgnoreCase),
                },
            },
        };

        return new AsyncApiContribution(
            "sqs",
            new AsyncApiServer { Host = host, Protocol = "sqs", Description = "Amazon SQS" },
            queueName,
            bindings);
    }
}
