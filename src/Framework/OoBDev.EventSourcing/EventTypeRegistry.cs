using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Nodes;

namespace OoBDev.EventSourcing;

/// <summary>
/// Maps stored event type names to event classes and back.
/// </summary>
public class EventTypeRegistry
{
    private readonly ConcurrentDictionary<string, Type> _byName = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, PersonalDataRule> _rules = new(StringComparer.Ordinal);

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
        if (type.GetCustomAttribute<PersonalDataAttribute>() is { } marks)
            _rules[name] = BuildRule(type, marks);
        return this;
    }

    /// <summary>
    /// Gets what erasure does to an event type.
    /// </summary>
    /// <param name="name">The stored event type name.</param>
    /// <returns>The rule, or <see langword="null"/> when the type carries no personal data.</returns>
    public PersonalDataRule? RuleFor(string name) => _rules.TryGetValue(name, out var rule) ? rule : null;

    private static PersonalDataRule BuildRule(Type type, PersonalDataAttribute marks)
    {
        PropertyInfo Find(string property) => type.GetProperty(property, BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"{type} marks '{property}' as personal data but has no such property.");

        Find(marks.SubjectProperty);
        var placeholders = new Dictionary<string, JsonNode?>(StringComparer.OrdinalIgnoreCase);
        var nullability = new NullabilityInfoContext();
        foreach (var field in marks.Fields)
        {
            var property = Find(field);
            var nullable = Nullable.GetUnderlyingType(property.PropertyType) is not null
                || (!property.PropertyType.IsValueType && nullability.Create(property).WriteState == NullabilityState.Nullable);
            placeholders[field] = nullable ? null
                : property.PropertyType == typeof(string) ? JsonValue.Create(ErasedText)
                : property.PropertyType == typeof(bool) ? JsonValue.Create(false)
                : property.PropertyType.IsPrimitive || property.PropertyType == typeof(decimal) ? JsonValue.Create(0)
                : null;
        }

        return new PersonalDataRule(marks.SubjectProperty, placeholders);
    }

    /// <summary>The placeholder stored in a non-nullable text field after erasure.</summary>
    public const string ErasedText = "[erased]";

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
