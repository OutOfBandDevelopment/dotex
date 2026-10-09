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

/// <summary>A broker the application talks to. Never carries credentials.</summary>
public sealed class AsyncApiServer
{
    /// <summary>Host (and port when not the default).</summary>
    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;

    /// <summary>Protocol such as <c>amqp</c> or <c>sqs</c>.</summary>
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = string.Empty;

    /// <summary>Description.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}

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
    public List<AsyncApiReference>? Servers { get; set; }

    /// <summary>Messages by name.</summary>
    [JsonPropertyName("messages")]
    public Dictionary<string, AsyncApiMessage> Messages { get; set; } = [];

    /// <summary>Provider specific bindings.</summary>
    [JsonPropertyName("bindings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonObject? Bindings { get; set; }

    /// <summary>Description.</summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }
}

/// <summary>A message carried by a channel.</summary>
public sealed class AsyncApiMessage
{
    /// <summary>Payload schema (inline or a reference).</summary>
    [JsonPropertyName("payload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonNode? Payload { get; set; }
}

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

/// <summary>A <c>$ref</c>.</summary>
public sealed class AsyncApiReference
{
    /// <summary>Reference target.</summary>
    [JsonPropertyName("$ref")]
    public string Ref { get; set; } = string.Empty;
}

/// <summary>Reusable parts.</summary>
public sealed class AsyncApiComponents
{
    /// <summary>Schemas by name.</summary>
    [JsonPropertyName("schemas")]
    public Dictionary<string, JsonNode?> Schemas { get; set; } = [];
}
