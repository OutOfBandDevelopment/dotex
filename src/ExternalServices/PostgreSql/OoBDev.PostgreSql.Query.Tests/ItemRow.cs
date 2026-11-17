using System;
using OoBDev.Query.Metadata;

namespace OoBDev.PostgreSql.Query.Tests;

[QueryKind("items", Base = "items r", Key = "r.id", DefaultSort = "-created")]
[Predicate("live", "yes", "r.removed IS FALSE")]
[SearchOperator("tag", "{x} = ANY(r.tags)")]
public sealed class ItemRow
{
    [Facet("r.id")]
    public long Id { get; set; }

    [Facet("r.title", Search = true)]
    public string Title { get; set; } = "";

    [Facet("r.created", Format = "date")]
    public DateTime Created { get; set; }

    [Facet("r.tags")]
    public string[]? Tags { get; set; }

    [Facet("r.score")]
    public int Score { get; set; }
}
