namespace OoBDev.Query.Metadata;

/// <summary>
/// One <c>name:value</c> term of a search string.
/// </summary>
/// <param name="Name">The lower-case operator name.</param>
/// <param name="Comparator"><c>&gt;</c>, <c>&gt;=</c>, <c>&lt;</c>, <c>&lt;=</c> or <c>=</c> when the value starts with one; empty otherwise.</param>
/// <param name="Value">The value without the comparator.</param>
/// <param name="Negated">Whether the term started with <c>-</c>.</param>
public sealed record SearchOperator(string Name, string Comparator, string Value, bool Negated);
