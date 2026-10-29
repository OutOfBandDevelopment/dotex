# OData Query — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-10-29

## Contents

- [Goals](#goals)
- [Non-goals](#non-goals)
- [Requirements](#requirements)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals

1. An `IQueryable<T>` action is written once and serves the custom search syntax and OData query options.
2. Hostile queries cannot cost more than a set budget (rows, expression nodes, expansion depth).
3. The same rights apply as for any other surface; OData adds no way around `[ApplicationRight]`.

## Non-goals

- A full OData service (metadata document, entity data model routing, batch, actions). Only system query options on existing actions.
- Replacing or changing the custom search syntax.
- GraphQL (a separate item, on demand).

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| ID | Priority | Requirement |
|----|----------|-------------|
| REQ-001 | Must | An action marked `[OoBDevEnableQuery]` that returns `IQueryable<T>` applies `$filter`, `$orderby`, `$top`, `$skip`, `$select` and `$count` from the query string. |
| REQ-002 | Must | `$top` is capped at `MaxTop` (default 100) and a missing `$top` is limited to `DefaultPageSize` (default 50); a request over the cap is rejected with a 400 problem details response. |
| REQ-003 | Must | `$filter` is limited by `MaxNodeCount` (default 100); `$expand` is disabled unless `AllowExpand` is set, then limited by `MaxExpansionDepth` (default 2). |
| REQ-004 | Must | Only the options in `AllowedQueryOptions` are accepted; `$apply`, `$compute` and `$search` are off by default. |
| REQ-005 | Must | Properties hidden from the custom syntax with `[NotFilterable]` and `[NotSortable]` are also rejected by OData. |
| REQ-006 | Must | A query error (bad syntax, unknown property, limit exceeded) is an RFC 9457 problem details response with status 400, not a 500. |
| REQ-007 | Must | A request rewritten by `QueryVerbMiddleware` with a form body has the `$` form fields moved into the query string before the action runs; a JSON body is left for the custom search binder. |
| REQ-008 | Must | A form field that is not an OData system query option (name not starting with `$`) is ignored, and the original query string is kept (form fields never override it). |
| REQ-009 | Should | Limits come from configuration (`OoBDev:OData`), validated at start; registration is `TryAddODataQueryServices()` and nothing happens unless it is called. |
| REQ-010 | Should | Responses carry `Accept-Query: "application/x-www-form-urlencoded"` where OData is enabled, so clients can discover `QUERY` support. |
