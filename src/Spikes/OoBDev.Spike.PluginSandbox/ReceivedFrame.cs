using System.Text;
using System.Text.Json;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>A verified frame with its header fields and payload bytes.</summary>
public sealed class ReceivedFrame
{
    /// <summary>The frame kind.</summary>
    public required FrameType Type { get; init; }

    /// <summary>How <see cref="Payload"/> is encoded.</summary>
    public required PayloadFormat Format { get; init; }

    /// <summary>The sender's counter for this frame.</summary>
    public required ulong Sequence { get; init; }

    /// <summary>The request sequence this frame answers, or 0.</summary>
    public required ulong Ack { get; init; }

    /// <summary>The dotted topic, or empty.</summary>
    public required string Topic { get; init; }

    /// <summary>Relay counter.</summary>
    public required byte Hops { get; init; }

    /// <summary>Request deadline in milliseconds, or 0 for none.</summary>
    public required uint TtlMs { get; init; }

    /// <summary>The payload bytes, as sent.</summary>
    public required byte[] Payload { get; init; }

    /// <summary>The frame's HMAC, empty when the session has none. A reply folds it in.</summary>
    public required byte[] Mac { get; init; }

    /// <summary>The payload as text (strict UTF-8).</summary>
    public string Text => new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(Payload);

    /// <summary>Parses the payload as JSON; the caller disposes the document.</summary>
    public JsonDocument ParseJson() => JsonDocument.Parse(Payload);
}
