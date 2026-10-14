using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;

namespace OoBDev.AsyncApi;

/// <summary>
/// Implemented by a message queue adapter to describe its server and channel from configuration.
/// Implementations must never return credentials.
/// </summary>
public interface IAsyncApiContributor
{
    /// <summary>The provider key the adapter is selected by (for example <c>sqs</c>).</summary>
    string ProviderKey { get; }

    /// <summary>Describes the queue configured in <paramref name="config"/>; null when it cannot be described.</summary>
    AsyncApiContribution? Describe(IConfigurationSection config);
}

