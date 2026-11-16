using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace OoBDev.EventSourcing;

/// <summary>What erasure does to one event type: the subject property and the placeholder for each personal field.</summary>
/// <param name="SubjectProperty">The property that holds the subject id.</param>
/// <param name="Placeholders">The replacement value by field name (null for a nullable field).</param>
public sealed record PersonalDataRule(string SubjectProperty, IReadOnlyDictionary<string, JsonNode?> Placeholders);
