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
    public Dictionary<string, AsyncApiServer> Servers { get; set; } = [];

    /// <summary>Channels by name.</summary>
    [JsonPropertyName("channels")]
    public Dictionary<string, AsyncApiChannel> Channels { get; set; } = [];

    /// <summary>Operations by name.</summary>
    [JsonPropertyName("operations")]
    public Dictionary<string, AsyncApiOperation> Operations { get; set; } = [];

    /// <summary>Reusable schemas.</summary>
    [JsonPropertyName("components")]
    public AsyncApiComponents Components { get; set; } = new();
}
