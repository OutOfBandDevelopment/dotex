using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Rows related to one anchor row, for example the other pictures of a burst. Requests name it with <c>related</c>.
/// </summary>
/// <param name="name">The relation name.</param>
/// <param name="sql">A boolean SQL condition; <c>{k}</c> is the kind's key expression and <c>{anchor}</c> the bound anchor key (text, cast it in the condition when the key is not text). Written by the application author, never by a request.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RelationAttribute(string name, string sql) : Attribute
{
    /// <summary>The relation name.</summary>
    public string Name { get; } = name;

    /// <summary>The boolean SQL condition.</summary>
    public string Sql { get; } = sql;
}
