# Claims enhancement pipeline against the platform primitives

Status: **comparison written 2026-11-19; recommendation: build nothing until a consumer appears.** Open question Q21 in `OPEN-QUESTIONS.md` (assumed answer A for now). Request: [claims enhancement and user session](../../features/rejected/2026-11-10-claims-enhancement-and-user-session.md).

## Contents

1. [Question](#question)
2. [What the request wants against what exists](#what-the-request-wants-against-what-exists)
3. [Where a pipeline would still pay](#where-a-pipeline-would-still-pay)
4. [Recommendation](#recommendation)
5. [Shape if a consumer appears](#shape-if-a-consumer-appears)

## Question

The SharedFramework source has an ordered claims enhancer pipeline, a user session with extended properties and a rights provider factory. The owner prefers platform primitives ([coding standards](../../../.claude/rules/coding-standards.md)). Does dotex need those types, or do the platform and the existing code cover the need?

## What the request wants against what exists

**Table 1 — Request items against the platform and dotex**

| Request item | Platform primitive | Already in dotex | Gap |
|--------------|--------------------|------------------|-----|
| Add claims after sign-in | `IClaimsTransformation` runs once per authenticated request | `RoleRightClaimsTransformation` (roles to `ApplicationRight` claims, idempotent, tested in `RoleRightTests`) | none for one source of claims |
| Ordered pipeline of enhancers | none: the platform resolves a single `IClaimsTransformation` (the last registered wins) | `TryAddRoleRightMapping` removes the no-op default and registers its own | several independent sources cannot coexist without a composite |
| Current user and session | `HttpContext.User`, `IHttpContextAccessor` | `ICurrentUserAccessor` (`HttpContextUserAccessor`), `IPrincipal`, `IIdentity` and `ClaimsPrincipal` registered by `TryAddAspNetCoreExtensions` | none for identity; no session store |
| Extended user properties | claims on the principal | `CommonClaims` | properties that are not claims have no home |
| Rights provider factory | `IAuthorizationHandler`, policies | `IRoleRightMapper`, `IApplicationRightChecker`, `[ApplicationRight]` | none: rights are resolved in one mapper |
| Anonymous requests skip enhancers | `TransformAsync` receives an unauthenticated principal | `RoleRightClaimsTransformation` returns the same principal for it (`Transform_Unauthenticated_ReturnsSamePrincipalAsync`) | none |

*Table 1 shows that every item except the ordered, multi-source pipeline is covered.*

## Where a pipeline would still pay

- Two or more independent claim sources, such as roles to rights plus tenant or licence claims from a store, each owned by a different library. Today the last registration silently wins, so a second source must wrap the first by hand.
- A transformation that must run more than once per request (the platform may call it repeatedly) needs every enhancer to be idempotent; a pipeline is the place to say that once.

Neither case exists in the repository today: there is one transformation.

## Recommendation

Do not build `ClaimsEnhancerPipeline`, `IUserSession` or the rights provider factory now. The services understand only JWT (owner decision), so enhancement is a mapping from validated token claims, and one `IClaimsTransformation` covers it. Reopen when a second claim source appears in a real consumer. The request stays `triaged`; if Q21 is answered A, it closes as covered by the platform and `RoleRightClaimsTransformation`.

## Shape if a consumer appears

The smallest design that keeps the platform primitive as the entry point:

- `IClaimsEnhancer` with `Order` and `EnhanceAsync(ClaimsPrincipal, CancellationToken)`, registered with `TryAddEnumerable`.
- One `CompositeClaimsTransformation : IClaimsTransformation` that runs enhancers in order on authenticated principals only, and skips a claim already present (idempotent).
- `RoleRightClaimsTransformation` becomes one enhancer instead of a transformation.
- Enhancers read only the validated principal and never claims from outside the token (the request's security note).
- Unit tests: order, idempotent second pass, anonymous skip, duplicate claims not added.

No session store: a session that is more than the token's claims needs its own request and owner decision.
