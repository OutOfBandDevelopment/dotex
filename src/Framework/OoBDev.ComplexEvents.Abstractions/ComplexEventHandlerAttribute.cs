using System;

namespace OoBDev.ComplexEvents;

/// <summary>Marks a handler class for an event type and sets its run order.</summary>
/// <param name="eventType">The event type the class handles.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ComplexEventHandlerAttribute(Type eventType) : Attribute
{
    /// <summary>The event type.</summary>
    public Type EventType { get; } = eventType;

    /// <summary>Lower runs first.</summary>
    public int Order { get; init; }
}
