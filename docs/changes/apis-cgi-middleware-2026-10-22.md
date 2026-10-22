# CGI middleware for Kestrel (first cut)

**Date:** 2026-10-22 · **Epic:** Web APIs · **Status:** Complete (first cut; follow-ups are in [TODO.md](../../TODO.md))

## Summary

`OoBDev.AspNetCore.Cgi` (Extensions layer, opt-in) runs scripts as CGI/1.1 programs (RFC 3875) for requests under a prefix. Design: [docs/design/CgiMiddleware](../design/CgiMiddleware/README.md). Project: `src/Extensions/OoBDev.AspNetCore.Cgi`, tests in `OoBDev.AspNetCore.Cgi.Tests`.

## What was built

**Table 1 — Delivered**

| Area | Result |
|------|--------|
| Registration | `TryAddCgiServices(configuration)` (section `OoBDev:Cgi`, validated at startup) and `UseCgi()` |
| Mapping | Prefix, script file name and `PATH_INFO` split; `Handlers` allowlist by extension; traversal, separators and links out of `ScriptRoot` refused |
| Process | No shell, cleared environment plus meta-variables, body on stdin, process-tree kill on timeout or client abort |
| Limits | Request body (413), timeout (504), concurrency cap (503 with `Retry-After`), response size, stderr size |
| Response | `Status`, `Location`, header validation (CR/LF/NUL, missing blank line, missing `Content-Type`) to 502; silent non-zero exit to 500; self-produced errors as RFC 9457 problem details |
| Authorization | Authenticated callers only unless `AllowAnonymous`; `RequiredRights` and per-script `ScriptRights` against `app__application_right` claims (401 and 403) |
| Startup checks | Missing `ScriptRoot` or a root under the static file root fails startup |

## Owner decisions (2026-10-22)

Global concurrency cap only; NPH scripts rejected; run-as-user left to the deployer; authorization built in; shell fixtures to run in a Linux container (Docker, or Aspire into Docker).

## Verification

18 tests pass on Windows (batch-file fixtures): environment and no `Proxy`, status and headers, 502 and 500 cases, 404, 403 (extension, batch files, traversal), 413, prefix fall-through, startup failure, 401 and rights checks. Batch files are off by default because `cmd.exe` re-parses environment values (found when a query string containing `&` ran a command in an early fixture).

## Follow-up

Shell fixtures in a Linux container; tests for timeout, the 503 cap, response limit, stderr flood and client abort; the benchmark; `validate-docs.py` on the design folder (no Python was available).
