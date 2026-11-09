# Shared Identity

Design documents for the identity features of the SharedFramework (Epic 7). Each feature has four documents: requirements, architecture, API design and testing strategy.

## Contents

- [Features](#features)
- [Implementation status](#implementation-status)

## Features

**Table 1 — Shared Identity documents**

| Feature | Requirements | Architecture | API design | Testing strategy |
|---------|--------------|--------------|------------|------------------|
| Account management | [requirements](AccountManagement/requirements.md) | [architecture](AccountManagement/architecture.md) | [api-design](AccountManagement/api-design.md) | [testing-strategy](AccountManagement/testing-strategy.md) |
| Modular profile management | [requirements](ModularProfileManagement/requirements.md) | [architecture](ModularProfileManagement/architecture.md) | [api-design](ModularProfileManagement/api-design.md) | [testing-strategy](ModularProfileManagement/testing-strategy.md) |
| Role and claims management | [requirements](RoleClaimsManagement/requirements.md) | [architecture](RoleClaimsManagement/architecture.md) | [api-design](RoleClaimsManagement/api-design.md) | [testing-strategy](RoleClaimsManagement/testing-strategy.md) |
| Session management | [requirements](SessionManagement/requirements.md) | [architecture](SessionManagement/architecture.md) | [api-design](SessionManagement/api-design.md) | [testing-strategy](SessionManagement/testing-strategy.md) |

## Implementation status

Reconciled with `src/Framework/OoBDev.Identity`, `OoBDev.Identity.Abstractions` and `OoBDev.Identity.Jws` on 2026-11-06. The designs are proposals; the code is the current truth where they differ.

**Table 2 — Designed features against the code**

| Feature | Designed API | Code | State |
|---------|--------------|------|-------|
| Account management | `IAccountService`, `IAccountRepository` | `IUserManagementProvider.CreateAccountAsync` (`UserCreateModel` to `UserCreatedModel`), `IIdentityManager` (find users by email, create and remove an identity user) | partly built: account creation and the identity-provider calls only |
| Modular profile management | `IProfileService`, the module, blob, visibility, versioning, import and export services | none | not built |
| Role and claims management | `IRoleService`, `IClaimsService`, `IClaimsEnhancementService` | none in Identity; rights and role translation follow the owner decision in [authentication practices](../../patterns-discovery/03-practices-and-conventions/09-authentication-practices.md) | not built |
| Session management | `ISessionService`, `ISessionRepository`, `ITokenService` | none; `JsonWebSignature` in `OoBDev.Identity.Jws` covers token signing only ([Jws design](../Jws/README.md)) | not built |

Authentication is OAuth 2.0, OIDC and JWT bearer with the STS issuing tokens (owner decision); how much of the session, token and claims designs remains needed is not decided.
