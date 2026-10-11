using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.AsyncApi;

/// <summary>Document title and version.</summary>
public sealed class AsyncApiInfo
{
    /// <summary>Title.</summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = "Messaging";

    /// <summary>Version.</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}
