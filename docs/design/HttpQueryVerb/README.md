# HTTP QUERY Verb — Overview

**Status:** Draft · **Date:** 2026-10-22 · **Related:** [HTTP API practices](../../patterns-discovery/03-practices-and-conventions/10-http-api-practices.md), [HTTP API alternatives](../../patterns-discovery/05-industry-alternatives/10-http-api.md), [application rights](../ApplicationRights/README.md), [design standard](../../patterns-discovery/06-design-document-standard.md)

Lets a client send a query as a request body with the HTTP `QUERY` verb (safe, idempotent), with a `POST` fallback for clients and proxies that cannot send `QUERY`. The first slice carries the existing custom search syntax; an OData query language rides on the same seam through `[OoBDevEnableQuery]` (see [ODataQuery](../ODataQuery/README.md)).

## Contents

1. [Requirements](requirements.md) — goals, non-goals, numbered requirements
2. [Architecture](architecture.md) — middleware, request flow, alternatives
3. [API design](api-design.md) — types, headers, registration
4. [Testing strategy](testing-strategy.md) — requirement to test map

### List of Figures

1. [Figure 1 — Request flow](architecture.md#figure-1)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Alternatives](architecture.md#table-2)
3. [Table 3 — Requirement coverage](testing-strategy.md#table-3)

## Summary

`QueryVerbMiddleware` runs before routing. A `QUERY` request, or a `POST` that carries `X-HTTP-Method-Override: QUERY`, is routed as `GET` to the endpoint that serves the collection, with the body kept for the search model binder. The endpoint, its `[ApplicationRight]` and the rights check are unchanged, so one endpoint serves `GET`, `QUERY` and the `POST` fallback with the same authorization.

[Requirements →](requirements.md)
