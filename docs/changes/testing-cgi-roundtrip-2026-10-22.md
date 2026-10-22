# CGI middleware: round-trip tests

**Date:** 2026-10-22 · **Design:** [CgiMiddleware](../design/CgiMiddleware/README.md)

- New fixtures `echo`, `location` and `no-blank-line` (`.cmd` and `.sh`).
- New tests: `Post_Body_IsEchoedByScript` (REQ-003), `Get_LocationScript_ReturnsRedirectWithLocation` and `Get_NoBlankLineAfterHeaders_ReturnsBadGateway` (REQ-004), `Get_QueryWithShellMetacharacters_ReachesScriptAsData` (REQ-006), and `CgiPathResolverTests` (REQ-001).
- `Get_StderrFlood_StillAnswersAndKeepsLimit` now asserts the stderr Warning entry and its size cap (REQ-012), through a `CapturingLoggerProvider`.
- `Get_HostEnvironmentVariable_IsNotPassedToScript` sets `OOB_HOST_SECRET` in the host and asserts the script does not see it (REQ-002).
- `CgiOptionsValidatorTests` cover REQ-014. They found a bug: a static root path with a trailing separator was never matched, so a script root under it passed validation. `CgiOptionsValidator` now trims trailing separators from both paths.
- Verified: 41/41 CGI tests pass on Windows. The `.sh` fixtures were not run on Linux. The remaining gaps stay in BACKLOG.
