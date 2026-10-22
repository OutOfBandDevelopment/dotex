# CGI Middleware — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-10-22 · **Related:** [docs/todo.md](../../todo.md)

## Contents

- [Goals](#goals)
- [Non-goals](#non-goals)
- [Requirements](#requirements)
- [Constraints](#constraints)
- [Open questions](#open-questions)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals

1. Run CGI/1.1 programs from a Kestrel pipeline with no change to Kestrel.
2. Be safe by default, so enabling the middleware does not make arbitrary code execution reachable from a URL.
3. Stay a normal middleware: authentication and authorization are ordinary middleware placed ahead of `UseCgi`.

## Non-goals

- FastCGI or SCGI, persistent workers, and a process pool.
- Sandboxing beyond OS user and resource limits.
- Serving the script root as static files (the root must never be under a static file provider).

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| ID | Priority | Requirement |
|----|----------|-------------|
| REQ-001 | Must | Map a URL prefix (default `/cgi-bin`) to one script root. The first path segment after the prefix is the script, the rest is `PATH_INFO`. |
| REQ-002 | Must | Start the child with a cleared environment, then set the CGI meta-variables and `HTTP_*` request headers. Never forward a `Proxy` header as `HTTP_PROXY`. |
| REQ-003 | Must | Stream the request body to stdin. The limit defaults to 10 MB and over-limit requests get 413 before the process starts. |
| REQ-004 | Must | Parse script headers (`Status`, `Location`, others) and stream stdout to `Response.Body`. No `Content-Type` or no blank line gives 502; a non-zero exit before any output gives 500. |
| REQ-005 | Must | Run only extensions listed in `Handlers`; an unknown extension gets 403. |
| REQ-006 | Must | No shell. Request data goes only into the environment and stdin, never into arguments. |
| REQ-007 | Must | Reject any resolved path or symlink target outside the script root. |
| REQ-008 | Must | Reject CR, LF and NUL in script header names and values. |
| REQ-009 | Must | `.cmd` and `.bat` handlers are opt-in only. |
| REQ-010 | Must | Timeout (default 30 s) with a process-tree kill. |
| REQ-011 | Must | `MaxConcurrentProcesses` (default 2 x CPU count); beyond it, answer 503 with `Retry-After`. |
| REQ-012 | Should | Response size limit; bounded stderr drain logged at Warning. |
| REQ-013 | Should | Kill the process tree when the client disconnects. |
| REQ-014 | Should | Fail startup when the script root is missing or under a static file root. |
| REQ-015 | Could | A benchmark that replaces the process-cost estimates. |
| REQ-016 | Must | Callers must be authenticated unless `AllowAnonymous` is set; `RequiredRights` (any one `app__application_right` claim) and per-script `ScriptRights` are checked before the program starts: 401 unauthenticated, 403 missing right. |
| REQ-017 | Won't | Non-parsed-header (NPH) scripts are rejected. |

## Constraints

- net10.0, nullable on, implicit usings off; Linux and Windows.
- Follows the options pattern (`AddValidatedOptions<T>()`), `TimeProvider` and `[LoggerMessage]`.
- No third-party libraries.

## Decisions

Owner decisions, 2026-10-22:

- Concurrency: a global cap only (no per-script cap) to start.
- NPH scripts: rejected (REQ-017).
- Run-as-user: left to the deployer's service account; a helper is a TODO in `TODO.md`.
- Authorization: built into the middleware (REQ-016) with application rights, instead of relying only on middleware ahead of `UseCgi`.
- Linux coverage: shell fixtures run in a Linux container (Docker, or Aspire into Docker), not WSL (open item in `TODO.md`).

[↑ Overview](README.md) · [Architecture →](architecture.md)
