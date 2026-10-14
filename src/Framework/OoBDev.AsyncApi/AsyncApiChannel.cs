using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.AsyncApi;

/// <summary>A queue or topic.</summary>
public sealed class AsyncApiChannel
{
    /// <summary>Queue or topic name.</summary>
    [JsonPropertyName("address")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Address { get; set; }

    /// <summary>Servers that host the channel.</summary>
    [JsonPropertyName("servers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<AsyncApiReference>? Servers { get; set; }

    /// <summary>Messages by name.</summary>
    [JsonPropertyName("messages")]
    public IDictionary<string, AsyncApiMessage> Messages { get; set; } = new Dictionary<string, AsyncApiMessage>(StringComparer.Ordinal);

    /// <summary>Provider specific bindings.</summary>
    [JsonPropertyName("bindings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonObject? Bindings { get; set; }

    /// <summary>Description.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}
