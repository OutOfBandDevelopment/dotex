# OoBDev.AspNetCore.Abstractions

Contracts and attributes for rights-based authorization in ASP.NET Core (owner decision: RBAC with application rights; roles are translated to rights in middleware; see [authorization practices](../../../docs/patterns-discovery/03-practices-and-conventions/11-authorization-practices.md)). The role-to-right translation and registration are in [OoBDev.AspNetCore.Mvc](../OoBDev.AspNetCore.Mvc/README.AspNetCore.Mvc.md).

## Use

```csharp
[ApplicationRight("orders.read", "orders.admin")]     // at least one of the rights
[HttpGet]
public IActionResult List() => Ok();
```

`ApplicationRightAttribute` (namespace `OoBDev.AspNetCore.Mvc.Filters`) is a type filter that runs `ApplicationRightRequirementFilter` with the listed rights. An unauthenticated caller gets a challenge (401), an authenticated caller holding none of the rights gets a forbid (403), otherwise the request continues.

## Contracts

**Table 1 — Namespace `OoBDev.AspNetCore.Authorization`**

| Type | Role |
|------|------|
| `IApplicationRightChecker` | `Check(principal, requiredRights)` returns a `RightCheckResult`: `Allowed`, `Unauthenticated` or `Forbidden`. Every API surface uses the same checker so the rule is identical everywhere. |
| `ApplicationRightChecker` | Default: unauthenticated principal is `Unauthenticated`; no required rights is `Allowed`; otherwise `Allowed` when any `ApplicationRight` claim (`CommonClaims.ApplicationRight`) equals a required right (ordinal comparison), else `Forbidden`. |
| `IRoleRightMapper` | `MapRights(roles)` returns the set of rights for a set of roles. |

The filter resolves `IApplicationRightChecker` from the request services and falls back to the default checker when none is registered.
