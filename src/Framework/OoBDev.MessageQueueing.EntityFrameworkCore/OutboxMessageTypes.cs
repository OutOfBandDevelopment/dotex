using System;
using System.Collections.Concurrent;

namespace OoBDev.MessageQueueing.EntityFrameworkCore;

/// <summary>
/// The message types the outbox may store and rebuild. The outbox writes the type name beside the JSON, and reads
/// only names registered here, so a changed row cannot make the dispatcher load an arbitrary type.
/// </summary>
public sealed class OutboxMessageTypes
{
    private readonly ConcurrentDictionary<string, Type> byName = new(StringComparer.Ordinal);

    /// <summary>Allows <typeparamref name="T"/> under its full name.</summary>
    /// <returns>This registry for chaining.</returns>
    public OutboxMessageTypes Add<T>() => Add(typeof(T));

    /// <summary>Allows <paramref name="type"/> under its full name.</summary>
    /// <returns>This registry for chaining.</returns>
    public OutboxMessageTypes Add(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        byName[NameOf(type)] = type;
        return this;
    }

    /// <summary>The stored name of a type.</summary>
    public static string NameOf(Type type) => type.FullName ?? type.Name;

    internal string RequireName(object message)
    {
        var name = NameOf(message.GetType());
        return byName.ContainsKey(name)
            ? name
            : throw new InvalidOperationException($"Message type '{name}' is not registered; add it with OutboxMessageTypes.Add<T>().");
    }

    internal Type Resolve(string name) =>
        byName.TryGetValue(name, out var type)
            ? type
            : throw new InvalidOperationException($"Outbox message type '{name}' is not registered.");
}
