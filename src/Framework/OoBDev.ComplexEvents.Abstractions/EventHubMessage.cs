using System;

namespace OoBDev.ComplexEvents;

/// <summary>One message on an event hub.</summary>
/// <param name="EventType">The event type name carried with the message.</param>
/// <param name="Payload">The serialized event.</param>
public sealed record EventHubMessage(string EventType, ReadOnlyMemory<byte> Payload);
