# OData query surface (2026-10-29)

New Extensions project `OoBDev.AspNetCore.OData` (with tests): `[OoBDevEnableQuery]` applies `$filter`, `$orderby`, `$top`, `$skip`, `$select`, `$count` to any `IQueryable<T>` action, with limits from `OoBDev:OData` (`ODataQueryLimits`: default page 50, `$top` cap 100, filter node count 100, `$expand` off). `[NotFilterable]` and `[NotSortable]` properties are rejected, bad queries are RFC 9457 `400`, `$count` returns `{ "@odata.count", "value" }`, and `ODataQueryBodyMiddleware` lets the `QUERY` verb (or the `POST` fallback) carry the options as a form body. Design: `docs/design/ODataQuery`.

- Package `Microsoft.AspNetCore.OData` 9.5.0 (targets net8.0, runs on net10.0; the 10.x line is preview only).
- Findings: OData denies filter, sort and top by default without an EDM, so registration calls `EnableQueryFeatures`-style settings; the platform validator also rejects `$top` when the global max is unset, hence `SetMaxTop(null)` with the cap enforced by the attribute.
- Tests: 18 (Unit for the middleware and options, Simulate against an in-process Kestrel host); all pass.
- Not done: the hidden-property check is a name match on the raw option text; `$apply` and `$search` stay off; GraphQL stays on demand.
