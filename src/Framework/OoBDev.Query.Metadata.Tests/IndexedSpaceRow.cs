namespace OoBDev.Query.Metadata.Tests;

[QueryKind("indexed", Base = "spots s", Key = "s.id", DefaultSort = "name")]
[Space("similar", SpaceMetric.Cosine, Table = "spot_vectors", IdColumn = "spot_id", VectorColumn = "vec", Candidates = 200)]
public sealed class IndexedSpaceRow
{
    [Facet("s.name")]
    public string Name { get; set; } = "";

    public double? Distance { get; set; }
}
