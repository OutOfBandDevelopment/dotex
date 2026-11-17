# Feature request: configurable application-user claim and debug bypass

- **Status:** triaged
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.AspNetCore.Mvc` (authorization, builder options)

## Need

The authorization handler requires an application user id claim (`RequireApplicationUserId`, default true) and the claim is fixed to `CommonClaims.UserId`. The option has to be set by hand even when a claim mapping is configured, and a `#if DEBUG` bypass exists that cannot be used outside local development.

## Proposed behavior

The claim that carries the application user id is configurable. When no claim is configured the requirement is off; when a mapping is configured the requirement follows it, so `RequireApplicationUserId` is detected, not set. The debug bypass can be enabled by an environment variable (off by default, refused in Production).

## Acceptance criteria

- Given no user-id claim is configured, when a request is authorized, then no user id is required.
- Given a mapped claim, when the token lacks it, then authorization fails with a problem detail.
- Given the bypass variable is set outside Production, then the debug bypass applies; in Production it is ignored with a logged warning.

## Existing code checked

`UserAuthorizationHandler`, `AspNetCoreExtensionBuilder`, `AddAspNetCoreExtensions` (`requireApplicationUserId`), request [claims enhancement and user session](2026-11-10-claims-enhancement-and-user-session.md), which may absorb this.

Source comments (replaced by a reference to this request):

- `src/Framework/OoBDev.AspNetCore.Mvc/Authorization/UserAuthorizationHandler.cs`: "consider adding the ability to enable [the DEBUG bypass] with an environment variable"; "maybe this should be injected ... a configurable claim and if not configured then not required".
- `src/Framework/OoBDev.AspNetCore.Mvc/AspNetCoreExtensionBuilder.cs`: "should be able to detect if mapping claim is configured and use that instead".
- `src/Framework/OoBDev.AspNetCore.Mvc/ServiceCollectionExtensions.cs`: "fix this too" on the `requireApplicationUserId` parameter.

## Security and operations

Touches authorization: the bypass must never default on and never work in Production. Same checks on every API surface (owner decision).

## Priority

- **Impact:** services without a user-id claim must opt out in code.
- **Needed by:** no date.
- **Workaround today:** set `RequireApplicationUserId` to false.

## Open questions

Merge into the claims-enhancement request, or keep separate? Recommendation: merge at triage.

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments). Design document to follow when built.
