using System;

namespace OoBDev.DevTerm;

/// <summary>One routed message, stamped with the clock tick it was posted (shared by every channel).</summary>
public sealed record RoutedMessage(DateTimeOffset Timestamp, RoutingDirection Direction, string Topic, string Payload);
