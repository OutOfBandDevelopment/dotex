# Rights: Debug logging and configuration docs

**Date:** 2026-10-22 · **Design:** [ApplicationRights](../design/ApplicationRights/README.md)

- `RoleRightClaimsTransformation` logs `Granted {RightCount} application rights from {RoleCount} roles` at Debug (counts only, REQ-010). The constructor takes an `ILogger` (breaking, unreleased).
- `OoBDev:Authorization` is documented in `README.AspNetCore.Mvc.md` and `CONFIGURATION_SETTINGS.md`.
- Verified: `OoBDev.AspNetCore.Tests` 32/32 pass.
