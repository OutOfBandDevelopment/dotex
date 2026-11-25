using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// A flag that tells the client a second lookup exists (a burst, copies, a location). Computed for the rows of one page only, and only when the request selects <c>has</c>.
/// </summary>
/// <param name="name">The hint name, a property of the row's hints class.</param>
/// <param name="sql">A boolean SQL expression; <c>{k}</c> is replaced by the kind's key expression. Written by the application author, never by a request.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class HintAttribute(string name, string sql) : Attribute
{
    /// <summary>The hint name.</summary>
    public string Name { get; } = name;

    /// <summary>The boolean SQL expression.</summary>
    public string Sql { get; } = sql;
}
