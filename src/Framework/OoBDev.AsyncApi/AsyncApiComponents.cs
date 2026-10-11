using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.AsyncApi;

/// <summary>Reusable parts.</summary>
public sealed class AsyncApiComponents
{
    /// <summary>Schemas by name.</summary>
    [JsonPropertyName("schemas")]
    public Dictionary<string, JsonNode?> Schemas { get; set; } = [];
}
