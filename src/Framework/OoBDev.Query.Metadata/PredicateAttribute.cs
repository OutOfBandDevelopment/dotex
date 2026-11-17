using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// A named filter with a few allowed values (for example <c>deleted</c> = <c>hide</c> or <c>only</c>). <c>{k}</c> in the SQL stands for the row key.
/// </summary>
/// <param name="name">The filter name.</param>
/// <param name="value">One allowed value.</param>
/// <param name="sql">The condition for that value.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class PredicateAttribute(string name, string value, string sql) : Attribute
{
    /// <summary>The filter name.</summary>
    public string Name { get; } = name;

    /// <summary>The allowed value.</summary>
    public string Value { get; } = value;

    /// <summary>The condition.</summary>
    public string Sql { get; } = sql;
}
