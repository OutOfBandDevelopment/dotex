using Microsoft.Extensions.Configuration;
using OoBDev.AsyncApi;
using System.Text.Json.Nodes;

namespace OoBDev.RabbitMQ.MessageQueueing;

/// <summary>Describes RabbitMQ queues for the AsyncAPI document. User name and password are not read.</summary>
public class RabbitMQAsyncApiContributor : IAsyncApiContributor
{
    /// <inheritdoc />
    public string ProviderKey => RabbitMQGlobals.MessageProviderKey;

    /// <inheritdoc />
    public AsyncApiContribution? Describe(IConfigurationSection config)
    {
        var host = config["HostName"] ?? "localhost";
        var port = config["Port"];
        var queueName = config["QueueName"];

        var bindings = queueName == null ? null : new JsonObject
        {
            ["amqp"] = new JsonObject
            {
                ["bindingVersion"] = "0.3.0",
                ["is"] = "queue",
                ["queue"] = new JsonObject { ["name"] = queueName },
            },
        };

        return new AsyncApiContribution(
            "rabbitmq",
            new AsyncApiServer { Host = port == null ? host : $"{host}:{port}", Protocol = "amqp", Description = "RabbitMQ" },
            queueName,
            bindings);
    }
}
