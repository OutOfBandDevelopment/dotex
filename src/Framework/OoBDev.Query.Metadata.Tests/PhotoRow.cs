using System;
using OoBDev.Query.Metadata;

namespace OoBDev.Query.Metadata.Tests;

[QueryKind("photos", Title = "Photos", Base = "photos r", Joins = "JOIN albums a ON a.id = r.album_id", Key = "r.id", DefaultSort = "-taken")]
[Predicate("deleted", "hide", "r.deleted_at IS NULL")]
[Predicate("deleted", "only", "r.deleted_at IS NOT NULL")]
[SearchOperator("tag", "EXISTS (SELECT 1 FROM tags t WHERE t.photo_id = {k} AND lower(t.name) = {x})")]
public sealed class PhotoRow
{
    [Facet("r.name", Search = true, Role = "title")]
    public string Name { get; set; } = "";

    [Facet("a.title", Label = "Album")]
    public string? Album { get; set; }

    [Facet("r.size", Format = "bytes")]
    public long Size { get; set; }

    [Facet("r.taken", Format = "date")]
    public DateTime? Taken { get; set; }

    [Facet("r.rating")]
    public int Rating { get; set; }

    [Facet("r.starred")]
    public bool Starred { get; set; }

    [Facet("r.labels")]
    public string[]? Labels { get; set; }

    [Facet("r.secret", Filterable = false, Sortable = false)]
    public string? Secret { get; set; }

    [Facet("r.body", Lazy = true, Search = true)]
    public string? Body { get; set; }
}

[QueryKind("admin", Base = "admins r", Key = "r.id", Right = "admin.read")]
public sealed class AdminRow
{
    [Facet("r.name")]
    public string Name { get; set; } = "";
}
