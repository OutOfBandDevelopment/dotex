# Application rights: roles to rights, one shared check

**Date:** 2026-10-22 · **Design:** [docs/design/ApplicationRights](../design/ApplicationRights/README.md)

## What changed

- `OoBDev.AspNetCore.Abstractions`: `IApplicationRightChecker`, `RightCheckResult`, default `ApplicationRightChecker` (deny by default, any one listed right, empty list needs authentication only) and `IRoleRightMapper`. The checker lives here rather than in Mvc so the filter can fall back to it without the MVC project.
- `OoBDev.AspNetCore.Mvc`: `RoleRightOptions` (`OoBDev:Authorization`), `ConfigurationRoleRightMapper`, `RoleRightClaimsTransformation` (idempotent, merges existing right claims) and `TryAddRoleRightMapping(IConfiguration)`.
- `[ApplicationRight]` now uses the checker: unauthenticated yields a challenge (401), missing right yields 403. Before, both were 403.

## Verification

`OoBDev.AspNetCore.Tests`: 9 Unit tests pass (mapping, transformation, checker, options validation, registration precedence, filter results). The CGI middleware was not changed and not re-run.

## Not done

- REQ-009: the CGI middleware still has its own check; moving it needs a reference from `OoBDev.AspNetCore.Cgi` to the Abstractions project (BACKLOG).
- REQ-010 Debug logging of granted rights is not implemented.
- The OData/`QUERY` and GraphQL surfaces are the next items in the build order.
