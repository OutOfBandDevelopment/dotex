using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Declares a property of a row type as a facet: a column that can be shown, filtered, sorted and searched. The camel-cased property name is the facet name.
/// </summary>
/// <param name="expr">SQL expression that yields the value, written against the kind's aliases.</param>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FacetAttribute(string expr) : Attribute
{
    /// <summary>The SQL expression.</summary>
    public string Expr { get; } = expr;

    /// <summary>A label for clients; defaults to the facet name.</summary>
    public string Label { get; init; } = "";

    /// <summary>How a client renders it, and how search values are read: <c>text</c>, <c>number</c>, <c>date</c>, <c>bytes</c>, <c>bool</c>, <c>path</c>.</summary>
    public string Format { get; init; } = "text";

    /// <summary>Whether a request may filter on the facet.</summary>
    public bool Filterable { get; init; } = true;

    /// <summary>Whether a request may sort by the facet.</summary>
    public bool Sortable { get; init; } = true;

    /// <summary>Whether plain search words match the facet.</summary>
    public bool Search { get; init; }

    /// <summary>The meaning to generic renderers (title, thumb, ref ...).</summary>
    public string Role { get; init; } = "";

    /// <summary>Returned in rows but left out of column lists.</summary>
    public bool Hidden { get; init; }

    /// <summary>A large value left out of rows unless the request names it in <c>select</c>.</summary>
    public bool Lazy { get; init; }
}
