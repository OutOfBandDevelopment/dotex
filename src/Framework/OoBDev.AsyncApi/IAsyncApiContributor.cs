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

/// <summary>What an adapter knows about a configured queue.</summary>
/// <param name="ServerName">Key for the server entry.</param>
/// <param name="Server">The server.</param>
/// <param name="Address">Queue or topic name.</param>
/// <param name="Bindings">Optional channel bindings.</param>
public sealed record AsyncApiContribution(string ServerName, AsyncApiServer Server, string? Address, JsonObject? Bindings = null);
