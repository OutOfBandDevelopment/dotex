# Application Rights — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-10-22 · **Related:** [authorization practices](../../patterns-discovery/03-practices-and-conventions/11-authorization-practices.md)

## Contents

- [Goals](#goals)
- [Non-goals](#non-goals)
- [Requirements](#requirements)
- [Constraints](#constraints)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals

1. Applications code against rights (`orders.read`), never role names.
2. The role-to-right mapping lives in one place and changes without reissuing tokens.
3. One decision for every API surface, so a right means the same thing in REST, OData and GraphQL.

## Non-goals

- Assigning roles to users (the identity provider owns that).
- Resource-level or data-scoping rules (a separate concern).
- The claims-exchange variant at the STS (documented in the practices page; this feature is the middleware variant).

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| ID | Priority | Requirement |
|----|----------|-------------|
| REQ-001 | Must | `IRoleRightMapper` maps a set of role names to a set of application rights. |
| REQ-002 | Must | The default mapper reads `OoBDev:Authorization:Roles:<role>` (a list of rights) from configuration. Role names compare case-insensitively. |
| REQ-003 | Must | `RoleRightClaimsTransformation` adds one `app__application_right` claim per mapped right to an authenticated principal, once per principal (re-running adds nothing). Unauthenticated principals are returned unchanged. |
| REQ-004 | Must | Roles are read from the claim types listed in `RoleClaimTypes` (default `ClaimTypes.Role`, `roles`, `role`). |
| REQ-005 | Must | Rights already present as `app__application_right` claims (for example from an STS exchange) are kept and merged with mapped rights. |
| REQ-006 | Must | `IApplicationRightChecker.Check` returns `Unauthenticated`, `Forbidden` or `Allowed`: unauthenticated callers never pass; with a non-empty required list the caller needs at least one listed right; an empty list requires only authentication. |
| REQ-007 | Must | An unmapped role grants nothing (deny by default). |
| REQ-008 | Must | `[ApplicationRight]` uses the checker: `Unauthenticated` yields a challenge (401), `Forbidden` yields 403. |
| REQ-009 | Should | The CGI middleware decides `RequiredRights` and `ScriptRights` through the checker. |
| REQ-010 | Should | Registration is `TryAdd*`, options are validated at start, and (Planned) a right granted is logged at Debug with the role count only (no token contents). |
| REQ-011 | Should | The mapper and checker are replaceable by registering another implementation first. |

## Constraints

- net10.0, nullable on, implicit usings off.
- Interfaces live in `OoBDev.AspNetCore.Abstractions`; implementations in `OoBDev.AspNetCore.Mvc` (Framework layer, depends only on Common and below).
- Follows `AddValidatedOptions<T>()` and `[LoggerMessage]`; no third-party libraries.

[Architecture →](architecture.md)
