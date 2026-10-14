using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.AsyncApi;

/// <summary>A send or receive operation on a channel.</summary>
public sealed class AsyncApiOperation
{
    /// <summary><c>send</c> or <c>receive</c>.</summary>
    [JsonPropertyName("action")]
    public string Action { get; set; } = "receive";

    /// <summary>Channel reference.</summary>
    [JsonPropertyName("channel")]
    public AsyncApiReference Channel { get; set; } = new();
}
