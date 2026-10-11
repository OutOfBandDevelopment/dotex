using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;

namespace OoBDev.AsyncApi;

/// <summary>What an adapter knows about a configured queue.</summary>
/// <param name="ServerName">Key for the server entry.</param>
/// <param name="Server">The server.</param>
/// <param name="Address">Queue or topic name.</param>
/// <param name="Bindings">Optional channel bindings.</param>
public sealed record AsyncApiContribution(string ServerName, AsyncApiServer Server, string? Address, JsonObject? Bindings = null);
