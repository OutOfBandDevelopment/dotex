using Microsoft.Extensions.Configuration;
using OoBDev.AsyncApi;
using System;
using System.Collections.Generic;

namespace OoBDev.Microsoft.Azure.StorageAccount.MessageQueueing;

/// <summary>Describes Azure Storage queues for the AsyncAPI document. Only the queue endpoint host is read from the connection string; keys are never read.</summary>
public class AzureStorageQueueAsyncApiContributor : IAsyncApiContributor
{
    /// <inheritdoc />
    public string ProviderKey => AzureStorageGlobals.MessageProviderKey;

    /// <inheritdoc />
    public AsyncApiContribution? Describe(IConfigurationSection config)
    {
        var parts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in (config["ConnectionString"] ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var index = pair.IndexOf('=', StringComparison.Ordinal);
            if (index > 0)
                parts[pair[..index]] = pair[(index + 1)..];
        }

        var protocol = "https";
        string host;
        if (parts.TryGetValue("QueueEndpoint", out var endpoint) && Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            host = uri.Authority;
            protocol = uri.Scheme;
        }
        else if (parts.TryGetValue("AccountName", out var account))
        {
            host = $"{account}.queue.{(parts.TryGetValue("EndpointSuffix", out var suffix) ? suffix : "core.windows.net")}";
        }
        else
        {
            host = "queue.core.windows.net";
        }

        return new AsyncApiContribution(
            "azure-storage-queue",
            new AsyncApiServer { Host = host, Protocol = protocol, Description = "Azure Storage queue" },
            config["QueueName"]);
    }
}
