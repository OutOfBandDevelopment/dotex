using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// A search operator that is not a facet (<c>tag:trip</c>): a condition on the row where <c>{k}</c> is the row key, <c>{v}</c> a bound contains pattern and <c>{x}</c> the bound lower-case value.
/// </summary>
/// <param name="name">The operator name.</param>
/// <param name="sql">The condition.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SearchOperatorAttribute(string name, string sql) : Attribute
{
    /// <summary>The operator name.</summary>
    public string Name { get; } = name;

    /// <summary>The condition.</summary>
    public string Sql { get; } = sql;
}
