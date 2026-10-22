# CGI middleware: round-trip tests

**Date:** 2026-10-22 · **Design:** [CgiMiddleware](../design/CgiMiddleware/README.md)

- New fixtures `echo`, `location` and `no-blank-line` (`.cmd` and `.sh`).
- New tests: `Post_Body_IsEchoedByScript` (REQ-003), `Get_LocationScript_ReturnsRedirectWithLocation` and `Get_NoBlankLineAfterHeaders_ReturnsBadGateway` (REQ-004), `Get_QueryWithShellMetacharacters_ReachesScriptAsData` (REQ-006), and `CgiPathResolverTests` (REQ-001).
- Verified: 35/35 CGI tests pass on Windows. The `.sh` fixtures were not run on Linux. The remaining gaps stay in BACKLOG.
