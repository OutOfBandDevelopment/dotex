using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.AsyncApi;

/// <summary>Root of an AsyncAPI 3.0 document.</summary>
public sealed class AsyncApiDocument
{
    /// <summary>AsyncAPI specification version.</summary>
    [JsonPropertyName("asyncapi")]
    public string AsyncApi { get; set; } = "3.0.0";

    /// <summary>Document information.</summary>
    [JsonPropertyName("info")]
    public AsyncApiInfo Info { get; set; } = new();

    /// <summary>Servers by name.</summary>
    [JsonPropertyName("servers")]
    public IDictionary<string, AsyncApiServer> Servers { get; set; } = new Dictionary<string, AsyncApiServer>(StringComparer.Ordinal);

    /// <summary>Channels by name.</summary>
    [JsonPropertyName("channels")]
    public IDictionary<string, AsyncApiChannel> Channels { get; set; } = new Dictionary<string, AsyncApiChannel>(StringComparer.Ordinal);

    /// <summary>Operations by name.</summary>
    [JsonPropertyName("operations")]
    public IDictionary<string, AsyncApiOperation> Operations { get; set; } = new Dictionary<string, AsyncApiOperation>(StringComparer.Ordinal);

    /// <summary>Reusable schemas.</summary>
    [JsonPropertyName("components")]
    public AsyncApiComponents Components { get; set; } = new();
}
