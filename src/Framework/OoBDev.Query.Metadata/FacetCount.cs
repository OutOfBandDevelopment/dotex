namespace OoBDev.Query.Metadata;

/// <summary>
/// One value of a facet and how many rows have it (shape <c>facets</c>).
/// </summary>
public sealed class FacetCount
{
    /// <summary>The facet value.</summary>
    public object? Value { get; set; }

    /// <summary>The number of rows with the value.</summary>
    public long Count { get; set; }
}
