namespace OoBDev.Query.Metadata;

/// <summary>One grid cell of shape <c>buckets</c>: the mean position of its rows on the two axis facets, how many rows there are and one sample value.</summary>
public sealed class Bucket
{
    /// <summary>Mean of the first axis facet over the rows of the cell.</summary>
    public double A { get; set; }

    /// <summary>Mean of the second axis facet over the rows of the cell.</summary>
    public double B { get; set; }

    /// <summary>Rows in the cell.</summary>
    public long Count { get; set; }

    /// <summary>The kind's <c>Sample</c> expression evaluated over the cell (for example a content hash for a thumbnail); null when the kind declares none.</summary>
    public string? Sample { get; set; }
}
