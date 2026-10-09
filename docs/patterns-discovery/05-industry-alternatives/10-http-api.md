# HTTP API and Querying

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Authentication Approaches](./09-authentication.md) · [Authorization Approaches →](./11-authorization.md)
<!-- nav -->

## 25. HTTP API style and query syntax

**Today:** MVC controllers with a custom search query syntax (`SearchQueryMiddleware`, `IQueryable<T>` result filters) and Swagger generation. The stated preference is REST wherever possible, with OData over the HTTP `QUERY` verb and GraphQL under consideration ([HTTP API practices](../03-practices-and-conventions/10-http-api-practices.md)).

**Table 28 — API styles**

| Style | Pros | Cons |
|-------|------|------|
| REST with OpenAPI | Universal tooling, caching, simple clients, matches the preference | Over- and under-fetching; many round trips for related data |
| GraphQL | Clients choose the shape; one round trip; strong schema | Cost and depth control needed; caching is harder; per-field authorization; new operational surface |
| gRPC | Fast, typed, streaming | Poor browser fit without gRPC-Web; not resource-oriented |
| RPC over JSON (JSON-RPC and similar) | Simple for commands | Loses HTTP semantics and caching; weaker tooling |
| Backend for frontend | Shapes APIs per client | More services to maintain |

**Table 29 — Query syntax options for collections**

| Option | Pros | Cons |
|--------|------|------|
| Current custom search syntax | Already built and tested; fits `IQueryable<T>`; no dependency | Private dialect; no client tooling or generated SDK support |
| OData query options on `GET` (`$filter`, `$orderby`, `$top`, `$skip`, `$select`, `$count`) | Widely known standard; tooling and client libraries exist; maps to `IQueryable<T>` | Long URLs; can expose expensive queries without limits; library churn between versions; full OData conventions are heavy |
| OData query options in the body of an HTTP `QUERY` request | Removes URL length limits; safe and idempotent by definition; cacheable in principle; keeps REST semantics | Newer specification; verify framework, gateway and proxy support; OpenAPI and tooling coverage is thin; needs a `POST` fallback |
| `POST` search endpoint with a query body | Works everywhere today | Not safe or idempotent by definition; not cacheable; hides intent from intermediaries |
| GraphQL queries | Flexible shapes across relations; strong typing | Separate surface to secure and monitor; overlaps with OData for filtering |
| Simple fixed filters per resource | Easiest to secure and cache | Every new filter is a code change |

**Table 30 — Combination strategies**

| Strategy | Notes |
|----------|-------|
| Keep custom syntax, add OData on the same seam | Lowest risk; existing clients keep working while new ones use the standard |
| OData (`GET`, then `QUERY`) for collections, GraphQL for aggregated views | Matches the stated preference; two surfaces to secure, both behind the same rights middleware |
| Replace the custom syntax outright | Least code long term; breaking change for existing clients, which conflicts with the no-breaking-change rule |

Statements about the `QUERY` verb, OData and GraphQL library maturity reflect general knowledge and must be re-checked before a decision.

**Verdict: Keep REST as the default; Consider OData (with `QUERY` and a `POST` fallback) and GraphQL as additional surfaces behind the existing query seam.** Keep the custom syntax until an ADR chooses otherwise, and require limits (maximum page size, allowed properties, depth and cost for GraphQL) and shared authorization on every surface.

**Owner decision:** the current `IQueryable<T>` middleware supports search, filter, sort and page only at entity level, not nested. OData or GraphQL should be supported for all `IQueryable<T>` endpoints, selected by content header or URL convention.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Authentication Approaches](./09-authentication.md) · [Authorization Approaches →](./11-authorization.md)
<!-- nav -->
