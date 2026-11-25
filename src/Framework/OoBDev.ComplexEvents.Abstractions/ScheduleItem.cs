using System;

namespace OoBDev.ComplexEvents;

/// <summary>A scheduled event.</summary>
/// <param name="Id">Idempotency key passed to handlers.</param>
/// <param name="Key">Caller-chosen unique key; registering it again moves the due time.</param>
/// <param name="DueAt">When it becomes due.</param>
/// <param name="EventType">Caller-defined event type name.</param>
/// <param name="Payload">Serialized event.</param>
/// <param name="Attempts">How many times it has been leased.</param>
/// <param name="Failed">True after the attempt limit; it is no longer leased.</param>
public sealed record ScheduleItem(Guid Id, string Key, DateTimeOffset DueAt, string EventType, ReadOnlyMemory<byte> Payload, int Attempts = 0, bool Failed = false);
