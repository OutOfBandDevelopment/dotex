using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.AsyncApi;

/// <summary>A <c>$ref</c>.</summary>
public sealed class AsyncApiReference
{
    /// <summary>Reference target.</summary>
    [JsonPropertyName("$ref")]
    public string Ref { get; set; } = string.Empty;
}
