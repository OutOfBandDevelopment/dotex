using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.AsyncApi;

/// <summary>A message carried by a channel.</summary>
public sealed class AsyncApiMessage
{
    /// <summary>Payload schema (inline or a reference).</summary>
    [JsonPropertyName("payload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonNode? Payload { get; set; }
}
