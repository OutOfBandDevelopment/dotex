# CGI Middleware — Testing Strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-10-22 · **Related:** [requirements](requirements.md)

## Contents

- [Targets](#targets)
- [Requirement coverage](#requirement-coverage)
- [Fixtures](#fixtures)
- [Benchmark](#benchmark)

### List of Tables

1. [Table 4 — Requirement coverage](#table-4)
2. [Table 5 — Measured request cost](#table-5)

## Targets

Coverage 85% or more for the project. Tests run on Linux and Windows (fixtures exist for both; Linux runs in the .NET SDK container with `scripts/ci/run-linux-tests.ps1`, not WSL). No Docker service is needed, so everything is `Unit`, `Simulate` or `DevLocal`.

## Requirement coverage

<a id="table-4"></a>**Table 4 — Requirement coverage**

| Requirement | Test | Category |
|-------------|------|----------|
| REQ-001 | `Get_EnvScript_ReceivesMetaVariablesAndNoProxy` (prefix, script, `PATH_INFO`); `Get_OutsidePrefix_FallsThrough`; `Resolve_ScriptWithPathInfo_SplitsNameAndPathInfo`, `Resolve_NoScriptNamed_ThrowsNotFound` | Simulate, Unit |
| REQ-002 | `Get_EnvScript_ReceivesMetaVariablesAndNoProxy`, `Get_HostEnvironmentVariable_IsNotPassedToScript` | Simulate |
| REQ-003 | `Post_BodyOverLimit_ReturnsContentTooLarge`, `Post_Body_IsEchoedByScript` | Simulate |
| REQ-004 | `Get_StatusScript_UsesStatusAndHeaders`, `Get_NoContentType_ReturnsBadGateway`, `Get_ExitWithoutOutput_ReturnsServerError`, `Get_LocationScript_ReturnsRedirectWithLocation`, `Get_NoBlankLineAfterHeaders_ReturnsBadGateway` | Simulate |
| REQ-005 | `Get_UnlistedExtension_ReturnsForbidden` | Simulate |
| REQ-006 | `Get_QueryWithShellMetacharacters_ReachesScriptAsData` | Simulate |
| REQ-007 | `Get_Traversal_IsNotServed` (three encodings); the symlink case is Planned (BACKLOG) | Simulate |
| REQ-008 | Planned (BACKLOG): no `bad-header` fixture exists yet | Simulate |
| REQ-009 | `Get_BatchFileWithoutOptIn_ReturnsForbidden` (Windows only) | Simulate |
| REQ-010 | `Get_ProgramOverTimeout_ReturnsGatewayTimeout` (child kill is not asserted; Planned, BACKLOG) | Simulate |
| REQ-011 | `Get_AllSlotsBusy_ReturnsServiceUnavailableWithRetryAfter` (cap of one) | Simulate |
| REQ-012 | `Get_ResponseOverLimit_ReturnsBadGateway`, `Get_StderrFlood_StillAnswersAndKeepsLimit` (asserts the single Warning entry and its size cap) | Simulate |
| REQ-013 | `Get_ClientAbort_FreesTheProcessSlot` | Simulate |
| REQ-014 | `Start_MissingScriptRoot_Fails`; the static-root case is Planned (BACKLOG) | Unit |
| REQ-015 | `Benchmark_TrivialScript_ReportsLatencyAndThroughput` | DevLocal |
| REQ-016 | `Get_AnonymousByDefault_ReturnsUnauthorized`, `Get_RequiredRights_NeedsOneMatch`, `Get_ScriptRights_AreCheckedPerScript` | Simulate |
| REQ-017 | Covered by `Get_NoContentType_ReturnsBadGateway` (the same 502 path) | Simulate |

## Fixtures

Tiny scripts committed beside the tests, one per case: `env`, `echo`, `status`, `location`, `no-content-type`, `no-blank-line`, `exit-1`, `sleep`, `flood-stderr`, `bad-header`. Each exists as a shell script and a batch file, and tests pick by platform. `WebApplicationFactory` hosts the pipeline with `UseCgi`.

## Benchmark

A DevLocal benchmark measures request latency and throughput for a trivial script at several concurrency values, to replace the process-cost estimates. It is `Benchmark_TrivialScript_ReportsLatencyAndThroughput` (100 requests of the `env` script at concurrency 1, 4 and 16, results printed to the test output). Measured 2026-10-22 on the development machine (shell fixtures run by `/bin/sh` in the .NET SDK container, batch fixtures on the Windows host):

<a id="table-5"></a>**Table 5 — Measured request cost**

| Platform | Concurrency | p50 | p95 | Throughput |
|----------|-------------|-----|-----|------------|
| Windows | 1 | 30.9 ms | 160.6 ms | 16.9/s |
| Windows | 4 | 31.4 ms | 112.0 ms | 68.2/s |
| Windows | 16 | 353.7 ms | 556.7 ms | 39.8/s |
| Linux container | 1 | 0.9 ms | 1.9 ms | 936.8/s |
| Linux container | 4 | 1.9 ms | 5.4 ms | 1445.0/s |
| Linux container | 16 | 6.9 ms | 12.4 ms | 2260.2/s |

Process start dominates on Windows (`cmd.exe`, about 30 ms) and peaks near 4 concurrent processes; Linux process start is cheap. Both ran with a tiny script, so real scripts add their own run time.

[← API design](api-design.md) · [↑ Overview](README.md)
