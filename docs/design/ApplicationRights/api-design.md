# Application Rights — API Design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Draft · **Date:** 2026-10-22 · **Related:** [architecture](architecture.md)

## Contents

- [Contracts](#contracts)
- [Options](#options)
- [Registration](#registration)

### List of Tables

1. [Table 3 — Options](#table-3)

## Contracts

```csharp
namespace OoBDev.AspNetCore.Authorization;

public interface IRoleRightMapper
{
    IReadOnlySet<string> MapRights(IEnumerable<string> roles);
}

public enum RightCheckResult { Allowed, Unauthenticated, Forbidden }

public interface IApplicationRightChecker
{
    RightCheckResult Check(ClaimsPrincipal principal, IReadOnlyCollection<string> requiredRights);
}
```

`Check` with an empty `requiredRights` requires authentication only. `RoleRightClaimsTransformation` implements `IClaimsTransformation`.

## Options

<a id="table-3"></a>**Table 3 — Options (`RoleRightOptions`, section `OoBDev:Authorization`)**

| Property | Default | Notes |
|----------|---------|-------|
| `RoleClaimTypes` | `ClaimTypes.Role`, `roles`, `role` | Claim types read as roles; at least one entry |
| `Roles` | empty | Role name to list of rights; names and rights must not be blank |

## Registration

`TryAddRoleRightMapping(this IServiceCollection, IConfiguration)` in `ServiceCollectionExtensions`: validated options, `IRoleRightMapper` (configuration mapper), `IClaimsTransformation` (the transformation) and `IApplicationRightChecker`, all with `TryAdd*`.

[Testing strategy →](testing-strategy.md)
