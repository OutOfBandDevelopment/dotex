using OoBDev.Query.Metadata;

namespace OoBDev.PostgreSql.Query.Tests;

[QueryKind("indexedplaces", Base = "places p", Key = "p.id", DefaultSort = "name")]
[Space("similar", SpaceMetric.Cosine, Table = "place_vectors", IdColumn = "place_id", VectorColumn = "vec", Candidates = 2)]
public sealed class IndexedPlaceRow
{
    [Facet("p.id")]
    public long Id { get; set; }

    [Facet("p.name")]
    public string Name { get; set; } = "";

    public double? Distance { get; set; }
}
