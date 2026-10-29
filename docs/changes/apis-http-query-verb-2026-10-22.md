# HTTP QUERY verb

**Date:** 2026-10-22 · **Design:** [docs/design/HttpQueryVerb](../design/HttpQueryVerb/README.md)

## What changed

- `QueryVerbMiddleware`, `IQueryVerbFeature` and `UseHttpQueryVerb()` in `OoBDev.AspNetCore.Mvc`. A `QUERY` request, or a `POST` with `X-HTTP-Method-Override: QUERY`, is routed as `GET` with its JSON or form body kept, so `IQueryable<T>` endpoints, the search query binder and `[ApplicationRight]` need no change. Other verbs carrying the header are not rewritten; a `QUERY` without a JSON or form body gets 415.
- Call `UseHttpQueryVerb()` before routing.

## Verification

`OoBDev.AspNetCore.Tests`: 15 Unit tests pass in total, 4 test methods (6 cases) new for the middleware, driven with `DefaultHttpContext`. No end-to-end host test was run, so the binder reading a body on the rewritten `GET` is covered by design reasoning, not a test.

## Not done

`Accept-Query` header and a registration test (BACKLOG). OData shipped later (apis-odata-query-2026-10-29.md); GraphQL stays on demand.
