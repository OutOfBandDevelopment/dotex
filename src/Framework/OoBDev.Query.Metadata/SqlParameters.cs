using System.Collections.Generic;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Collects bound values and hands out their names. Every request value reaches the database through here.
/// </summary>
public sealed class SqlParameters
{
    private readonly Dictionary<string, object?> _values = [];

    /// <summary>The bound values by name, without the leading <c>@</c>.</summary>
    public IReadOnlyDictionary<string, object?> Values => _values;

    /// <summary>Binds a value.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The placeholder to write into the SQL.</returns>
    public string Add(object? value)
    {
        var name = "p" + _values.Count;
        _values[name] = value;
        return "@" + name;
    }
}
