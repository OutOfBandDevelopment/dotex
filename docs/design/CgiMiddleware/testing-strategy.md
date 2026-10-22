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

## Targets

Coverage 85% or more for the project. Tests run on Linux and Windows (fixtures exist for both). No Docker service is needed, so everything is `Unit`, `Simulate` or `DevLocal`.

## Requirement coverage

<a id="table-4"></a>**Table 4 — Requirement coverage**

| Requirement | Test | Category |
|-------------|------|----------|
| REQ-001 | Prefix, script and `PATH_INFO` split for several paths | Unit |
| REQ-002 | Script dumps its environment: cleared, meta-variables present, `Proxy` absent | Simulate |
| REQ-003 | Echo script round-trips a body; an 11 MB body gets 413 and no process starts | Simulate |
| REQ-004 | Fixtures for `Status`, `Location`, no `Content-Type`, no blank line, non-zero exit | Simulate |
| REQ-005 | Unlisted extension gets 403 | Simulate |
| REQ-006 | Query string with shell metacharacters reaches the script only through the environment | Simulate |
| REQ-007 | `..`, encoded traversal and a symlink out of the root are rejected | Unit, Simulate |
| REQ-008 | Script emitting CR, LF or NUL in a header gets 502 | Simulate |
| REQ-009 | `.cmd` rejected unless `AllowBatchFiles` | Unit |
| REQ-010 | Sleeping script is killed with its child | Simulate |
| REQ-011 | Third parallel request with a cap of two gets 503 and `Retry-After` | Simulate |
| REQ-012 | Large output stops at the limit; stderr flood is bounded and logged | Simulate |
| REQ-013 | Client abort kills the process | Simulate |
| REQ-014 | Missing root or root under static files fails startup | Unit |
| REQ-015 | Benchmark (see below) | DevLocal |
| REQ-016 | Anonymous gets 401 by default; `RequiredRights` needs one match (403 otherwise); `ScriptRights` checked per script | Simulate |
| REQ-017 | NPH output has no `Status`/`Content-Type` and is rejected with 502 | Simulate |

## Fixtures

Tiny scripts committed beside the tests, one per case: `env`, `echo`, `status`, `location`, `no-content-type`, `no-blank-line`, `exit-1`, `sleep`, `flood-stderr`, `bad-header`. Each exists as a shell script and a batch file, and tests pick by platform. `WebApplicationFactory` hosts the pipeline with `UseCgi`.

## Benchmark

A DevLocal benchmark measures request latency and throughput for a trivial script at several concurrency values, to replace the process-cost estimates.

[← API design](api-design.md) · [↑ Overview](README.md)
