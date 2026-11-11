using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace OoBDev.EventSourcing;

/// <summary>
/// Maps stored event type names to event classes and back.
/// </summary>
public class EventTypeRegistry
{
    private readonly ConcurrentDictionary<string, Type> _byName = new(StringComparer.Ordinal);

    /// <summary>
    /// Registers an event class under its <see cref="EventTypeAttribute"/> name, or its class name.
    /// </summary>
    /// <typeparam name="T">The event class.</typeparam>
    /// <returns>The registry for chaining.</returns>
    public EventTypeRegistry Register<T>() => Register(typeof(T));

    /// <summary>
    /// Registers an event class under its <see cref="EventTypeAttribute"/> name, or its class name.
    /// </summary>
    /// <param name="type">The event class.</param>
    /// <returns>The registry for chaining.</returns>
    /// <exception cref="InvalidOperationException">Another class already uses the name.</exception>
    public EventTypeRegistry Register(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var name = NameOf(type);
        var existing = _byName.GetOrAdd(name, type);
        if (existing != type)
            throw new InvalidOperationException($"Event type name '{name}' is used by both {existing} and {type}.");
        return this;
    }

    /// <summary>
    /// Registers every class of the assembly that has an <see cref="EventTypeAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>The registry for chaining.</returns>
    public EventTypeRegistry RegisterAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        foreach (var type in assembly.GetTypes())
            if (type.GetCustomAttribute<EventTypeAttribute>() is not null)
                Register(type);
        return this;
    }

    /// <summary>
    /// Gets the stored name of an event class.
    /// </summary>
    /// <param name="type">The event class.</param>
    /// <returns>The attribute name or the class name.</returns>
    public static string NameOf(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.GetCustomAttribute<EventTypeAttribute>()?.Name ?? type.Name;
    }

    /// <summary>
    /// Finds the class for a stored name.
    /// </summary>
    /// <param name="name">The stored event type name.</param>
    /// <returns>The class.</returns>
    /// <exception cref="InvalidOperationException">The name is not registered.</exception>
    public Type Resolve(string name)
        => _byName.TryGetValue(name, out var type) ? type : throw new InvalidOperationException($"Event type '{name}' is not registered.");
}
