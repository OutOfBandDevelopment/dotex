using OoBDev.Query.Metadata;

namespace OoBDev.PostgreSql.Query.Tests;

[QueryKind("links", Base = "links l", Key = "l.id", DefaultSort = "id")]
[Edge("l.a", "l.b", "l.shared")]
public sealed class LinkRow
{
    [Facet("l.id")]
    public long Id { get; set; }

    [Facet("l.shared")]
    public long Shared { get; set; }
}
