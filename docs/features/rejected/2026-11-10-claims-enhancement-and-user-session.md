# Feature request: claims enhancement pipeline and user session

- **Status:** declined (owner, 2026-11-19, Q21: covered by IClaimsTransformation (TryAddRoleRightMapping))
- **Requested by:** owner (SharedFramework comparison, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.Identity*` and `OoBDev.AspNetCore.*` (Epic 7 Identity)

## Need

Add claims to a signed-in user through an ordered pipeline of enhancers, expose the current user session and extended properties, and resolve rights through a provider factory. dotex has `IIdentityManager`, `IUserManagementProvider` and the role-to-rights `RoleRightClaimsTransformation`, but not the pipeline or the session.

## Proposed behavior

- Claims enhancers (attribute-registered, factory-built) running in a pipeline over a claims provider.
- `IUserSession` and its accessor, plus a claims culture accessor.
- Extended user properties and a rights provider factory.

## Source projects

Under `C:\repo\mine\proving-grounds\examples\SharedFramework` (read-only reference):

- `OoBDev.IdentityModel.Abstractions` (22 files)
- `OoBDev.IdentityModel.Extensions` (10 files: `ClaimsEnhancerPipeline`, `ClaimsProvider`, `UserSessionAccessor`, `RightsProviderFactory`)
- `OoBDev.IdentityModel.Tests`

The source also holds Azure B2C claim names (`AzB2cClaims`), which the owner dropped; leave them out.

## Acceptance criteria

- Given enhancers with priorities, then they run in order and each can add or replace claims.
- Given a request, then the user session reflects the token claims and the extended properties.
- Given no token (anonymous), then enhancers do not run.

## Existing code checked

Covered: `RoleRightClaimsTransformation`, `ApplicationRightChecker` (rights from roles in middleware), `IIdentityManager`, user create models. Missing: enhancer pipeline, `IUserSession`, extended properties. The design exists: [SharedIdentity](../../design/SharedIdentity/README.md) (`SessionManagement`, `RoleClaimsManagement`, `AccountManagement`).

## Security and operations

Services understand only JWT (owner decision); enhancers must not trust claims from outside the validated token. Never store passwords.

## Priority

- **Impact:** a common identity composition point.
- **Needed by:** no date (the epic waits for a consumer)
- **Workaround today:** the claims transformation alone

## Open questions

Is a pipeline needed beyond `IClaimsTransformation`, which the platform already provides (the owner prefers platform primitives)?

## Triage notes

Origin: SharedFramework. Design exists; compared against platform primitives 2026-11-19 in [claims-pipeline-comparison.md](../../design/SharedIdentity/claims-pipeline-comparison.md): nothing to build until a second claim source appears (Q21).

2026-11-11: triaged. The design exists in [SharedIdentity](../../design/SharedIdentity/README.md); no code until a consumer appears, and platform primitives (`IClaimsTransformation`) are compared first. Backlog: BACKLOG.md, SharedFramework.
