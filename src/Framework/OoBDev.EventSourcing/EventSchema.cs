using System.Text.Json.Nodes;

namespace OoBDev.EventSourcing;

/// <summary>
/// The versioned payload contract of one event type.
/// </summary>
/// <param name="Name">The stored event type name.</param>
/// <param name="Version">The contract version.</param>
/// <param name="Schema">The JSON Schema of the payload.</param>
public sealed record EventSchema(string Name, int Version, JsonNode Schema);
