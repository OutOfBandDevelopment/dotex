# CGI middleware uses the shared right checker

**Date:** 2026-10-22 · **Design:** [CgiMiddleware](../design/CgiMiddleware/README.md)

- `CgiMiddleware.Authorize` asks `IApplicationRightChecker` (registered service, else `ApplicationRightChecker`) for the 401/403 decision; the anonymous opt-in still short-circuits when no rights are required.
- `OoBDev.AspNetCore.Cgi` now references `OoBDev.AspNetCore.Abstractions`.
- New Simulate test `Get_RegisteredChecker_DecidesTheOutcome`; 27/27 CGI tests pass. Debug logging of granted rights and the config docs stay in BACKLOG.
