using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace OoBDev.EventSourcing;

/// <summary>
/// Compares the current schemas with a committed snapshot and reports what must not ship.
/// </summary>
public static class EventSchemaBaseline
{
    /// <summary>
    /// Finds contract violations.
    /// </summary>
    /// <param name="registry">The current schemas.</param>
    /// <param name="baselineJson">The committed snapshot written by <see cref="EventSchemaRegistry.ToSnapshot"/>.</param>
    /// <returns>One message per problem: a breaking change without a higher version, a lowered version, or a baseline type that is no longer registered. Empty when the contract holds.</returns>
    public static IReadOnlyList<string> Verify(EventSchemaRegistry registry, string baselineJson)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(baselineJson);
        var baseline = JsonNode.Parse(baselineJson) as JsonObject ?? throw new ArgumentException("The baseline must be a JSON object.", nameof(baselineJson));
        var current = registry.Describe().ToDictionary(s => s.Name, StringComparer.Ordinal);
        var problems = new List<string>();

        foreach (var (name, entry) in baseline)
        {
            if (!current.TryGetValue(name, out var schema))
            {
                problems.Add($"{name}: no longer registered; stored events of this type could not be read.");
                continue;
            }

            var oldVersion = entry!["version"]!.GetValue<int>();
            if (schema.Version < oldVersion)
                problems.Add($"{name}: version lowered from {oldVersion} to {schema.Version}.");
            var breaking = EventSchemaComparer.Compare(entry["schema"]!, schema.Schema).Where(c => c.IsBreaking).ToList();
            if (breaking.Count > 0 && schema.Version <= oldVersion)
                problems.Add($"{name}: breaking change without a version bump ({string.Join(", ", breaking.Select(c => $"{c.Property} {c.Kind}"))}); raise [EventType] Version above {oldVersion} and add an upcaster.");
        }

        return problems;
    }
}
