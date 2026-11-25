using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace OoBDev.EventSourcing;

/// <summary>
/// Compares two payload schemas property by property (top level).
/// </summary>
public static class EventSchemaComparer
{
    /// <summary>
    /// Lists what changed from the old schema to the current one.
    /// </summary>
    /// <param name="old">The baseline schema.</param>
    /// <param name="current">The current schema.</param>
    /// <returns>The changes, empty when the shapes are equal.</returns>
    public static IReadOnlyList<EventSchemaChange> Compare(JsonNode old, JsonNode current)
    {
        ArgumentNullException.ThrowIfNull(old);
        ArgumentNullException.ThrowIfNull(current);
        var before = Properties(old);
        var after = Properties(current);
        var oldRequired = Required(old);
        var newRequired = Required(current);
        var changes = new List<EventSchemaChange>();

        foreach (var (name, type) in after.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            if (!before.TryGetValue(name, out var previous))
                changes.Add(new(name, newRequired.Contains(name) ? ChangeKind.AddedRequired : ChangeKind.AddedOptional));
            else if (previous != type)
                changes.Add(new(name, ChangeKind.TypeChanged));
            else if (newRequired.Contains(name) && !oldRequired.Contains(name))
                changes.Add(new(name, ChangeKind.BecameRequired));
        }

        changes.AddRange(before.Keys.Where(name => !after.ContainsKey(name)).OrderBy(n => n, StringComparer.Ordinal).Select(name => new EventSchemaChange(name, ChangeKind.Removed)));
        return changes;
    }

    private static Dictionary<string, string> Properties(JsonNode schema)
        => (schema["properties"] as JsonObject)?.ToDictionary(p => p.Key, p => p.Value?.ToJsonString() ?? "null", StringComparer.Ordinal)
           ?? [];

    private static HashSet<string> Required(JsonNode schema)
        => (schema["required"] as JsonArray)?.Select(n => n!.GetValue<string>()).ToHashSet(StringComparer.Ordinal) ?? [];
}
