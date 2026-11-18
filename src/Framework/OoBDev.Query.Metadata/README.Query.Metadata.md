# OoBDev.Query.Metadata

A metadata-driven query engine with no database reference. A row class declares by attributes what a client may ask; one request shape (rows, facet counts, grid buckets, keyset paging) is compiled to parameterized SQL. Design: [docs/design/MetadataQuery](../../../docs/design/MetadataQuery/README.md). The PostgreSQL adapter is [OoBDev.PostgreSql.Query](../../ExternalServices/PostgreSql/OoBDev.PostgreSql.Query/README.PostgreSql.Query.md).

## Declare a kind

```csharp
[QueryKind("photos", Title = "Photos", Base = "photos r", Joins = "JOIN albums a ON a.id = r.album_id", Key = "r.id", DefaultSort = "-taken")]
[Predicate("deleted", "hide", "r.deleted_at IS NULL")]
[SearchOperator("tag", "EXISTS (SELECT 1 FROM tags t WHERE t.photo_id = {k} AND lower(t.name) = {x})")]
public sealed class PhotoRow
{
    [Facet("r.name", Search = true, Role = "title")] public string Name { get; set; } = "";
    [Facet("r.taken", Format = "date")]              public DateTime? Taken { get; set; }
    [Facet("r.secret", Filterable = false, Sortable = false)] public string? Secret { get; set; }
}
```

Only declared SQL fragments are ever written; every request value is bound as a parameter. A facet that is undeclared, not filterable or not sortable answers 400 naming it.

## Use

```csharp
services.TryAddMetadataQuery(configuration, typeof(PhotoRow).Assembly);   // Query:Metadata:*
// an adapter registers IQueryDialect and IQueryExecutor; a host may register IQueryAuthorizer

var page = await engine.QueryAsync<PhotoRow>(new QueryRequest { From = "photos", Search = "tag:beach", Sort = ["-taken"], Top = 60 });
var next = await engine.QueryAsync<PhotoRow>(new QueryRequest { From = "photos", After = page.Next });
```

The text form `photos tag:beach by -taken top 60 count` (`QueryText.Parse`) and the JSON form give the same `QueryRequest`. `Describer.Describe(registry)` and `OpenApiBuilder.Build(registry, title, version)` are generated from the registry and never carry SQL.

## Rules

- A kind that names a `Right` answers 403 until an `IQueryAuthorizer` allows it; the check runs before compile.
- The cursor is `base64url(payload).base64url(HMAC-SHA256)`. A tampered cursor, one signed with another key, or one issued for another kind or sort answers 400. The sort always ends with the kind's key, so paging never repeats or skips a row when rows are inserted between pages.
- Only kind, shape and timing are logged, never row content or request values.

## Options (`Query:Metadata`)

| Key | Default | Meaning |
|-----|---------|---------|
| `CursorKey` | required, 32 characters or more | HMAC key that signs cursors |
| `MaxTop` | 500 (1 to 5000) | Largest page a request may ask for |

## Buckets

`shape: "buckets"` with `by: [a, b]` (two numeric facets) groups the filtered rows into grid cells of `cell` units (default 1, clamped 0.0001 to 90) and returns `Bucket` rows (`A` and `B` are the cell means, `Count`, `Sample`), largest cell first, at most 1000. `QueryKind.Sample` is an aggregate expression (for example `min(r.sha)`) that picks one representative per cell; without it the sample is null. Text form: `as buckets by lat,lon cell 0.5`. A dialect supplies `GridCell`. Buckets are not paged.

## Not built yet

Hints, relations, graph shape, snapshot sources, vector spaces (near) and the host sample; see the design document.

## Tests

`OoBDev.Query.Metadata.Tests`: golden SQL for filters, predicates, operators and negation, rejection of undeclared facets, bound values, cursor tamper, key, kind and sort checks, keyset paging through a fake executor, text and JSON equality, describer, OpenAPI, registry and registration.
