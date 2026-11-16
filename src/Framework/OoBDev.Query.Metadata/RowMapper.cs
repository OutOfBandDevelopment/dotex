using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Copies a row dictionary into an instance of the kind's row class; property names match column names ignoring the first letter's case.
/// </summary>
internal static class RowMapper
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> Setters = new();

    public static T Map<T>(IReadOnlyDictionary<string, object?> row)
        where T : new()
    {
        var setters = Setters.GetOrAdd(typeof(T), t => t.GetProperties().Where(p => p.CanWrite)
            .ToDictionary(p => char.ToLowerInvariant(p.Name[0]) + p.Name[1..], StringComparer.OrdinalIgnoreCase));
        var result = new T();
        foreach (var (name, value) in row)
            if (setters.TryGetValue(name, out var property))
                property.SetValue(result, Convert(value, property.PropertyType));
        return result;
    }

    private static object? Convert(object? value, Type target)
    {
        if (value is null)
            return null;
        var type = Nullable.GetUnderlyingType(target) ?? target;
        if (type.IsInstanceOfType(value) || type == typeof(object))
            return value;
        if (type == typeof(DateTimeOffset) && value is DateTime dt)
            return new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc));
        return global::System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
    }
}
