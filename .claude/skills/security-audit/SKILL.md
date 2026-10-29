---
name: security-audit
description: Audit API endpoints and services for authentication, authorization and data-scoping gaps. Use when asked for a security audit, endpoint security review, or to verify a new API surface follows the secure-by-default and rights-based model.
---

# API security audit

Model to audit against (owner decisions): JWT bearer everywhere (other credentials converted at the STS or an edge adapter, never per-scheme in a service); secure by default with an explicit opt-out; RBAC through application rights (`[ApplicationRight]`) with roles translated to rights in middleware; the same checks on every surface (REST, OData/QUERY, GraphQL, SignalR, AsyncAPI consumers). See `docs/patterns-discovery/03-practices-and-conventions/09-authentication-practices.md`, `10-http-api-practices.md`, `11-authorization-practices.md`.

1. Confirm the fallback authorization policy is authenticated-by-default and find every `AllowAnonymous` or equivalent opt-out.
2. Inventory endpoints (controllers, minimal APIs, hubs, OpenAPI documents) and record auth required, rights declared, public paths, and data scoping in a table.
3. Check token validation settings (issuer, audience, signing keys, lifetime), that no passwords or secrets are stored or logged, and that errors are RFC 9457 problem details without leaking internals.
4. Check input handling, CORS, rate limits and process-launch surfaces if present (see the CGI middleware entry in `docs/todo.md`).
5. Operations check against `docs/how-tos/production-operations-practices.md`: source data is mounted or credentialed read-only (no write path into a source location, no automatic cleanup); each migration is approved by name, backed up once before the run, with no `Down` on a live database; no step kills or restarts processes it did not start; a deploy recreates only changed services and rolls back on failure; a new feature in production has an approved name, a surface, tests, a design document and a runbook; Integration tests refuse a non-test connection string (`TestInstanceGuard`).
6. Report findings by severity with file and line, and record fixes as `TODO.md` items. Do not fix or commit unrequested.

`reference.md` is the generic original procedure (its examples mix C# and TypeScript); use its search recipes and report template, but the model above wins where they differ.
