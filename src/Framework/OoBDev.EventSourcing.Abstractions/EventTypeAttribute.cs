using System;

namespace OoBDev.EventSourcing;

/// <summary>
/// Gives an event class the stable name that is stored with it, so the class can be renamed or moved later.
/// Without the attribute the class name is used.
/// </summary>
/// <param name="name">The stored event type name.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class EventTypeAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the stored event type name.
    /// </summary>
    public string Name { get; } = name;
}
