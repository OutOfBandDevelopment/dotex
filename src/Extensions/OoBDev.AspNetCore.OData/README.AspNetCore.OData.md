# OoBDev.AspNetCore.OData

OData system query options (`$filter`, `$orderby`, `$top`, `$skip`, `$select`, `$count`) on any `IQueryable<T>` action, with cost limits, the search annotations and problem details errors.

## Description

`[OoBDevEnableQuery]` is `[EnableQuery]` with limits from `ODataQueryLimits` (`OoBDev:OData`): page size, `$top` cap, filter node count, `$expand` off by default. Properties marked `[NotFilterable]` or `[NotSortable]` are rejected in `$filter` and `$orderby` (a name match on the raw option text, so it over-rejects rather than under-rejects). A bad query is an RFC 9457 `400`. `ODataQueryBodyMiddleware` lets the HTTP `QUERY` verb carry the options as a form body. The endpoint and its `[ApplicationRight]` are unchanged. Design: `docs/design/ODataQuery`.

## Usage

```csharp
builder.Services.TryAddODataQueryServices(builder.Configuration);
// ...
app.UseHttpQueryVerb();
app.UseODataQueryBody();
app.UseRouting();
```

```csharp
[HttpGet, ApplicationRight("orders.read"), OoBDevEnableQuery]
public IQueryable<Order> Get() => _orders.AsQueryable();
```

```json
{ "OoBDev": { "OData": { "DefaultPageSize": 50, "MaxTop": 100, "MaxNodeCount": 100, "AllowExpand": false, "MaxExpansionDepth": 2 } } }
```

`Microsoft.AspNetCore.OData` 9.5.0 targets net8.0 and runs on net10.0.
