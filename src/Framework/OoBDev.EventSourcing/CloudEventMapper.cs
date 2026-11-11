using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OoBDev.EventSourcing;

/// <summary>
/// Maps an <see cref="EventRecord"/> to and from a CloudEvents 1.0 structured JSON document
/// (<c>application/cloudevents+json</c>) for use at a boundary. The internal envelope stays <see cref="EventRecord"/>.
/// </summary>
public static class CloudEventMapper
{
    /// <summary>The media type of a structured JSON CloudEvent.</summary>
    public const string ContentType = "application/cloudevents+json";

    /// <summary>
    /// Writes the event as a CloudEvent. <c>id</c> is <c>{StreamId}:{Sequence}</c>, <c>subject</c> the stream id,
    /// and <c>sequence</c> and <c>metadata</c> are extension attributes so the mapping can be reversed.
    /// </summary>
    /// <param name="record">The event.</param>
    /// <param name="source">The CloudEvents <c>source</c>, normally the application name.</param>
    /// <returns>The CloudEvent as JSON text.</returns>
    public static string ToCloudEvent(EventRecord record, string source)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var node = new JsonObject
        {
            ["specversion"] = "1.0",
            ["id"] = $"{record.StreamId}:{record.Sequence}",
            ["source"] = source,
            ["type"] = record.Type,
            ["subject"] = record.StreamId,
            ["time"] = record.Timestamp.ToString("O", CultureInfo.InvariantCulture),
            ["datacontenttype"] = "application/json",
            ["sequence"] = record.Sequence,
            ["data"] = JsonNode.Parse(record.Payload),
        };
        if (record.Metadata is not null)
            node["metadata"] = record.Metadata;
        return node.ToJsonString();
    }

    /// <summary>
    /// Reads a CloudEvent into an <see cref="EventRecord"/>. The stream id is <c>subject</c> (or the part of <c>id</c>
    /// before the last colon) and the sequence is the <c>sequence</c> extension (or the part of <c>id</c> after it).
    /// </summary>
    /// <param name="json">The CloudEvent as JSON text.</param>
    /// <returns>The event, with position zero.</returns>
    /// <exception cref="FormatException">A required attribute is missing or the version is not 1.0.</exception>
    public static EventRecord FromCloudEvent(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var node = JsonNode.Parse(json) as JsonObject ?? throw new FormatException("A CloudEvent must be a JSON object.");
        if (Text(node, "specversion") != "1.0")
            throw new FormatException("Only CloudEvents specversion 1.0 is supported.");

        var id = Text(node, "id") ?? throw new FormatException("CloudEvent attribute 'id' is required.");
        var type = Text(node, "type") ?? throw new FormatException("CloudEvent attribute 'type' is required.");
        var split = id.LastIndexOf(':');
        var streamId = Text(node, "subject") ?? (split > 0 ? id[..split] : throw new FormatException("CloudEvent needs 'subject' or an id of the form 'stream:sequence'."));
        var sequenceText = node["sequence"]?.ToString() ?? (split > 0 ? id[(split + 1)..] : null);
        if (!long.TryParse(sequenceText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sequence))
            throw new FormatException("CloudEvent needs a numeric 'sequence' extension or an id of the form 'stream:sequence'.");
        var time = Text(node, "time") is { } t
            ? DateTimeOffset.Parse(t, CultureInfo.InvariantCulture)
            : throw new FormatException("CloudEvent attribute 'time' is required.");

        return new EventRecord(streamId, sequence, type, time, node["data"]?.ToJsonString() ?? "null", Text(node, "metadata"));
    }

    private static string? Text(JsonObject node, string name) => node[name]?.GetValueKind() == JsonValueKind.String ? node[name]!.GetValue<string>() : null;
}
