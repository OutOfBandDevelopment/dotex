# OData Query — Overview

**Status:** Draft · **Date:** 2026-10-29 · **Related:** [HTTP QUERY verb](../HttpQueryVerb/README.md), [application rights](../ApplicationRights/README.md), [HTTP API practices](../../patterns-discovery/03-practices-and-conventions/10-http-api-practices.md)

Lets any `IQueryable<T>` endpoint answer the OData system query options (`$filter`, `$orderby`, `$top`, `$skip`, `$select`, `$count`) beside the custom search syntax, on `GET` and, through the HTTP `QUERY` verb, with the options in a form body. Decided 2026-10-22: OData through `Microsoft.AspNetCore.OData` on the `IQueryable<T>` seam; GraphQL (HotChocolate) later on demand; the custom syntax is unchanged.

## Contents

1. [Requirements](requirements.md) — goals, non-goals, numbered requirements
2. [Architecture](architecture.md) — request flow, limits, alternatives
3. [API design](api-design.md) — types, options, registration
4. [Testing strategy](testing-strategy.md) — requirement to test map

### List of Figures

1. [Figure 1 — Request flow](architecture.md#figure-1)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Alternatives](architecture.md#table-2)
3. [Table 3 — Requirement coverage](testing-strategy.md#table-3)

## Summary

`OoBDev.AspNetCore.OData` (Extensions layer) holds three small pieces: `OoBDevEnableQueryAttribute` (the platform `[EnableQuery]` with safe limits taken from `ODataQueryLimits`), `ODataQueryBodyMiddleware` (moves form fields into the query string for a request that `QueryVerbMiddleware` rewrote) and `TryAddODataQueryServices()`. The endpoint, its `[ApplicationRight]` and the rights check are unchanged, so every surface enforces the same rights.

[Requirements →](requirements.md)
