# Telemetry library and user accessor (2026-10-29)

Step 0 of the [DevTerm restructure plan](../design/DevTermParity/restructure-plan.md).

- `OoBDev.OpenTelemetry` (ExternalServices) became `OoBDev.Telemetry` in `src/Framework` (tests moved with it). It no longer references `Microsoft.AspNetCore.App`, so command line and desktop applications can use it.
- `ICurrentUserAccessor` (`OoBDev.System.Abstractions`) gained `ClaimsPrincipal? User`; `UserName` defaults to its identity name.
- `EnvironmentUserAccessor` returns the Windows identity on Windows (environment user name elsewhere); `HttpContextUserAccessor` wraps `IHttpContextAccessor`.
- The user telemetry and log processors read `ICurrentUserAccessor` instead of `IHttpContextAccessor`.
- `OoBDev.Common.Extensions` references the Framework library explicitly (it was found by the ExternalServices glob).

Verified: `OoBDev.Telemetry.Tests` Unit 6 passed; `OoBDev.System.Tests` Unit 1868 passed; `OoBDev.Common.Extensions`, `OoBDev.AspNetCore.Mvc` and `OoBDev.Example.WebApi` build. Not run: the Telemetry Integration tests (need the Aspire container) and `AspNetCore.Mvc` tests (none for the accessors).
