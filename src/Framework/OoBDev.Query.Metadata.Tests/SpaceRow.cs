namespace OoBDev.Query.Metadata.Tests;

[QueryKind("spots", Base = "spots s", Key = "s.id", DefaultSort = "name")]
[Space("geo", SpaceMetric.Haversine, A = "lat", B = "lon")]
[Space("similar", SpaceMetric.Cosine, Table = "spot_vectors", IdColumn = "spot_id", VectorColumn = "vec", DefaultMax = 0.5)]
public sealed class SpaceRow
{
    [Facet("s.name")]
    public string Name { get; set; } = "";

    [Facet("s.lat")]
    public double? Lat { get; set; }

    [Facet("s.lon")]
    public double? Lon { get; set; }

    public double? Distance { get; set; }
}
