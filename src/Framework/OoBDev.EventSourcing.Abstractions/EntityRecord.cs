namespace OoBDev.EventSourcing;

/// <summary>
/// A stored entity (read model, checkpoint).
/// </summary>
/// <param name="Type">The entity type name.</param>
/// <param name="Key">The key inside the type.</param>
/// <param name="Version">The version used as the concurrency token; starts at one.</param>
/// <param name="Value">The value, as JSON text.</param>
public sealed record EntityRecord(string Type, string Key, long Version, string Value);
