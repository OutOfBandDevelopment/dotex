# Landscape: web and API

Part of the [feature landscape](README.md). Status is "a project exists under `src`", not "verified"; see the README for the method.

## Contents

1. [Summary](#summary)
2. [ApplicationRights](#applicationrights)
3. [HttpQueryVerb and ODataQuery](#httpqueryverb-and-odataquery)
4. [OpenApiScalar and AsyncApi](#openapiscalar-and-asyncapi)
5. [CgiMiddleware](#cgimiddleware)
6. [Jws and SharedIdentity](#jws-and-sharedidentity)

**List of tables**

1. [Table 1 — Web and API features](#table-1)

## Summary

**Table 1 — Web and API features**

| Feature | Code | Standards | Main gap |
|---------|------|-----------|----------|
| ApplicationRights | `RoleRightOptions` in `OoBDev.AspNetCore.Mvc`, example API catalog and `RightsController` | RFC 6749/7519 (roles from JWT claims), RFC 9457 | status line says Draft; one right check for every surface (REST, OData, GraphQL, CGI) not proven |
| HttpQueryVerb | query-body middleware in `OoBDev.AspNetCore.OData` | IETF HTTP QUERY draft, RFC 9110 | draft standard may still change |
| ODataQuery | `OoBDev.AspNetCore.OData` with limits | OData 4.01 | limits for `$expand` depth and cost are set, per-right field filtering is not described |
| OpenApiScalar | `Microsoft.AspNetCore.OpenApi` documents with Scalar | OpenAPI 3.1, OAuth2 code with PKCE (RFC 7636) | none known |
| AsyncApi | `OoBDev.AsyncApi` plus contributors in the queue adapters | AsyncAPI 3.0 | adapters for Kafka-style brokers are not planned |
| CgiMiddleware | `OoBDev.AspNetCore.Cgi`, `.Cgi.OpenApi` | RFC 3875 | first cut; process isolation and timeouts need review |
| Jws | `OoBDev.Identity.Jws` | RFC 7515, 7517, 7518 | JWKS caching and rotation: platform handler, see the JwtAuthentication readme |
| SharedIdentity | `OoBDev.Identity`, `.Abstractions`, `OoBDev.AspNetCore.JwtAuthentication` | RFC 7519, 8693 (optional exchange), OIDC Core | token exchange is config-gated and unbuilt |

## ApplicationRights

- **Works with:** HttpQueryVerb and ODataQuery (the same right check on each surface), SharedIdentity (roles come from the token), CgiMiddleware.
- **Status:** the pieces exist; the design status is older than the code.
- **Known issues:** drift in the status line; the surface-parity rule (same checks on every API surface, owner decision) has no automated test across surfaces.
- **Practices:** secure by default, deny without a right, rights not roles in code, roles translated in middleware.
- **Built 2026-11-16:** `EndpointRightCoverageTests` (example API) fails on a new controller action with no `[ApplicationRight]`, `[Authorize]` or `[AllowAnonymous]`; 18 existing open actions are listed until Q-EXAMPLE-ANONYMOUS is answered.
- **Missing:** a rights audit log.

## HttpQueryVerb and ODataQuery

- **Works with:** ApplicationRights; the custom search syntax stays beside them (no breaking change).
- **Status:** OData host and limits exist; GraphQL is only under evaluation.
- **Known issues:** the QUERY verb is still an IETF draft, so the `POST` fallback must remain; caches and proxies often ignore QUERY.
- **Standards:** RFC 9110 (methods, safe and idempotent), OData 4.01 URL conventions, RFC 9111 (caching).
- **Missing:** a decision record on GraphQL versus OData. Built 2026-11-16: paged search results carry an RFC 8288 `Link` header (first, prev, next, last).

## OpenApiScalar and AsyncApi

- **Works with:** every REST feature (OpenAPI is the contract) and the queue adapters (AsyncAPI).
- **Status:** built; the document set and viewer are in the change log of 2026-10-09.
- **Known issues:** none recorded.
- **Standards:** OpenAPI 3.1, JSON Schema 2020-12, AsyncAPI 3.0, RFC 9457 problem details as the error schema.
- **Built 2026-11-16:** `Document_ContractBaseline_HasNoBreakingRemovals` (`OoBDev.AspNetCore.Tests`) compares the generated operations, parameters and response codes with `openapi-surface.baseline.txt`; a removal fails, an addition passes. Missing: schema-level diffing.

## CgiMiddleware

- **Works with:** ApplicationRights (CGI scripts must pass the same checks) and OpenApiScalar (`Cgi.OpenApi`).
- **Status:** first cut with options under `OoBDev:Cgi`.
- **Known issues:** running external processes from a web host needs a sandbox review (the security-audit skill); environment variables passed to the script must be an allow-list.
- **Standards:** RFC 3875 (CGI 1.1).
- **Built 2026-11-16:** documented defaults: 30 s timeout, 10 MB request, 50 MB response (`null` disables), 64 KB stderr. **Missing:** per-script timeout; FastCGI is not designed.

## Jws and SharedIdentity

- **Works with:** ApplicationRights and every authenticated surface.
- **Status:** libraries exist; the design statuses (Accepted and Design phase) lag.
- **Known issues:** the owner decision "services understand only JWT; the STS converts other credentials" is documented, but no design lists the converters (API key, HMAC, basic, Kerberos, SAML) as separate features.
- **Standards:** RFC 7515/7519 (JWS, JWT), RFC 8725 (JWT best practice), RFC 8693 (token exchange), RFC 9068 (JWT access token profile), OIDC Core and Discovery.
- **Built 2026-11-16:** key rotation and JWKS caching are the platform handler behaviour, documented in the JwtAuthentication readme and pinned by tests. **Missing:** refresh-token handling for the DevTerm and desktop clients.
