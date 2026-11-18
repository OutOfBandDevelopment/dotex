using OoBDev.Query.Metadata;

namespace OoBDev.PostgreSql.Query.Tests;

[QueryKind("places", Base = "places p", Key = "p.id", DefaultSort = "name", Sample = "min(p.sha)")]
public sealed class PlaceRow
{
    [Facet("p.id")]
    public long Id { get; set; }

    [Facet("p.name")]
    public string Name { get; set; } = "";

    [Facet("p.lat")]
    public double? Lat { get; set; }

    [Facet("p.lon")]
    public double? Lon { get; set; }
}
