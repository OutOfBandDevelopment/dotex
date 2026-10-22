# Application Rights — Overview

**Status:** Draft · **Date:** 2026-10-22 · **Related:** [authorization practices](../../patterns-discovery/03-practices-and-conventions/11-authorization-practices.md), [HTTP API practices](../../patterns-discovery/03-practices-and-conventions/10-http-api-practices.md), [design standard](../../patterns-discovery/06-design-document-standard.md)

Role-based access control with application rights. The identity provider issues roles in the JWT; this feature translates them into `app__application_right` claims in one place, and one checker decides every endpoint's access, so REST, OData and GraphQL share the same rule. The first slice is the translation (claims transformation plus mapper) and the shared checker; the CGI middleware and the MVC filter move onto the checker.

## Contents

1. [Requirements](requirements.md) — goals, non-goals, numbered requirements
2. [Architecture](architecture.md) — layer, components, request flow, configuration, alternatives
3. [API design](api-design.md) — interfaces, options, registration, configuration shape
4. [Testing strategy](testing-strategy.md) — requirement to test map

### List of Figures

1. [Figure 1 — Request sequence](architecture.md#figure-1)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Alternatives](architecture.md#table-2)
3. [Table 3 — Options](api-design.md#table-3)
4. [Table 4 — Requirement coverage](testing-strategy.md#table-4)

## Summary

`IRoleRightMapper` turns roles into rights from configuration (`OoBDev:Authorization:Roles:<role>`). `RoleRightClaimsTransformation` (an `IClaimsTransformation`) adds the rights to the principal once per request. `IApplicationRightChecker` is the single decision: unauthenticated gives 401, no matching right gives 403, nothing is allowed by omission.

[Requirements →](requirements.md)
