using Microsoft.Extensions.Configuration;
using OoBDev.AsyncApi;
using System;
using System.Linq;

namespace OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing;

/// <summary>Describes Service Bus queues and topics for the AsyncAPI document. Only the endpoint host is read from the connection string.</summary>
public class ServiceBusAsyncApiContributor : IAsyncApiContributor
{
    /// <inheritdoc />
    public string ProviderKey => AzureServiceBusGlobals.MessageProviderKey;

    /// <inheritdoc />
    public AsyncApiContribution? Describe(IConfigurationSection config)
    {
        var endpoint = (config["ConnectionString"] ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2 && string.Equals(p[0], "Endpoint", StringComparison.OrdinalIgnoreCase))
            .Select(p => p[1])
            .FirstOrDefault();

        var host = Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ? uri.Authority : "servicebus";

        return new AsyncApiContribution(
            "servicebus",
            new AsyncApiServer { Host = host, Protocol = "amqp", Description = "Azure Service Bus" },
            config["QueueName"] ?? config["TopicName"]);
    }
}
