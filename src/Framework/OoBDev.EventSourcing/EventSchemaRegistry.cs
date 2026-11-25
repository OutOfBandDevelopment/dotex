using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace OoBDev.EventSourcing;

/// <summary>
/// Derives the versioned JSON Schema of every event class in an <see cref="EventTypeRegistry"/> and validates payloads against it.
/// </summary>
/// <param name="types">The registered event classes.</param>
public class EventSchemaRegistry(EventTypeRegistry types)
{
    private static readonly JsonSerializerOptions Strict = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true,
    };

    /// <summary>
    /// Gets the schema of every registered event type, ordered by name.
    /// </summary>
    /// <returns>The schemas.</returns>
    public IReadOnlyList<EventSchema> Describe()
        => [.. types.Types.OrderBy(t => t.Key, StringComparer.Ordinal).Select(t => Describe(t.Key, t.Value))];

    /// <summary>
    /// Gets the schema of one event type.
    /// </summary>
    /// <param name="name">The stored event type name.</param>
    /// <returns>The schema.</returns>
    /// <exception cref="InvalidOperationException">The name is not registered.</exception>
    public EventSchema Describe(string name) => Describe(name, types.Resolve(name));

    /// <summary>
    /// Checks a stored or incoming payload against the current class.
    /// </summary>
    /// <param name="name">The stored event type name.</param>
    /// <param name="payload">The JSON payload.</param>
    /// <returns>The problems; empty when the payload fits.</returns>
    public IReadOnlyList<string> Validate(string name, string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var type = types.Resolve(name);
        try
        {
            if (JsonSerializer.Deserialize(payload, type, Strict) is null)
                return ["The payload is null."];
            var present = ((JsonObject)JsonNode.Parse(payload)!).Select(p => p.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var required = JsonSchemaExporter.GetJsonSchemaAsNode(Strict, type)["required"] as JsonArray;
            return [.. (required ?? []).Select(n => n!.GetValue<string>()).Where(n => !present.Contains(n)).Select(n => $"Required property '{n}' is missing.")];
        }
        catch (JsonException ex)
        {
            return [ex.Message];
        }
    }

    /// <summary>
    /// Writes every schema with its version, in the form <see cref="EventSchemaBaseline"/> reads back.
    /// </summary>
    /// <returns>An object keyed by event type name.</returns>
    public JsonObject ToSnapshot()
    {
        var snapshot = new JsonObject();
        foreach (var schema in Describe())
            snapshot[schema.Name] = new JsonObject { ["version"] = schema.Version, ["schema"] = schema.Schema.DeepClone() };
        return snapshot;
    }

    private static EventSchema Describe(string name, Type type)
        => new(name, type.GetCustomAttribute<EventTypeAttribute>()?.Version ?? 1,
            JsonSchemaExporter.GetJsonSchemaAsNode(Strict, type));
}
