namespace OoBDev.Search.Requests;

/// <summary>
/// One field condition of a <see cref="SearchRequest"/>.
/// </summary>
/// <param name="Name">The field name.</param>
/// <param name="Comparator">How the field is compared.</param>
/// <param name="Value">The value to compare with.</param>
/// <param name="Negated">Whether the condition is inverted.</param>
public sealed record FieldFilter(string Name, FilterComparator Comparator, string Value, bool Negated = false);
