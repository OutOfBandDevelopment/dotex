using System;
using System.Collections.Generic;
using System.Reflection;

namespace OoBDev.Generations;

/// <summary>Implements an interface whose property getters return generated values (fixed on first read).</summary>
public class PropertyProxy : DispatchProxy
{
    private readonly Dictionary<string, object?> _values = new(StringComparer.Ordinal);

    internal Func<Type, string, object?>? Factory { get; set; }

    /// <inheritdoc />
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var name = targetMethod?.Name ?? string.Empty;
        if (name.StartsWith("get_", StringComparison.Ordinal))
        {
            var property = name[4..];
            if (!_values.TryGetValue(property, out var value))
                _values[property] = value = Factory?.Invoke(targetMethod!.ReturnType, property);
            return value;
        }

        if (name.StartsWith("set_", StringComparison.Ordinal) && args is { Length: 1 })
        {
            _values[name[4..]] = args[0];
            return null;
        }

        throw new NotSupportedException($"Method \"{name}\" is not a property accessor");
    }
}
