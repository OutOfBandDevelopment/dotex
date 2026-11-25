---
name: user-http-api-authz-preferences
description: "HTTP API and authorization preferences: REST first, OData over QUERY verb and GraphQL under evaluation, RBAC with app rights via role-to-right middleware"
metadata:
  type: user
---

REST wherever possible. Wants to evaluate OData over the HTTP QUERY verb and GraphQL alongside the existing custom search query syntax (`SearchQueryMiddleware`). Authorization: RBAC with application rights applied per endpoint (`[ApplicationRight]`), with user roles translated to application rights in middleware; claims exchange at the STS is acceptable only if token bloat does not occur.

**Why:** stated by the user as standing preferences, alongside [[user-auth-oauth-oidc-sts]] and [[user-ui-mvvm-preference]]. The user also asked to build out the remaining missing docs, changes welcome along the way.

**How to apply:** default new APIs to REST plus OpenAPI; keep the custom syntax (no breaking changes) while OData/GraphQL are evaluated; put role-to-right mapping in injected middleware, not controllers. Recorded in `docs/patterns-discovery/03-practices-and-conventions/10-http-api-practices.md` and `11-authorization-practices.md`, plus a `TODO.md` backlog.
