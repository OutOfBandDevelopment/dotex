# Sandboxed Plugin Host — Spike result

[↑ Overview](README.md)

**Status:** Evidence for the REQ-010 sign-off · **Date:** 2026-11-09

The owner asked for a proof of concept in `src/Spikes` before signing off the [threat model](threat-model.md) and the [protocol](protocol-and-manifest.md). `src/Spikes/OoBDev.Spike.PluginSandbox` is that spike; its three Integration tests pass on Windows.

## What was shown

- **Framing and flow.** A child process (this assembly in `echo` mode) sends `lifecycle.ready`, answers a `Request` with a `Response` carrying the `correlationId`, and exits 0 after `Shutdown`. The 4-byte little-endian length plus UTF-8 JSON works unchanged over redirected stdin and stdout.
- **Violation reaction.** A frame length over the 1 MiB cap makes the host's reader throw `FrameViolation`, kill the process tree and report it exited (Table 8, first row). Invalid UTF-8 and invalid JSON take the same path in code; only the length case has a test.
- **Resource limit.** A Windows job object with a per-process memory limit and kill-on-close ends a plugin that allocates without bound (exit code not 0), and closing the job kills the plugin.

## Update 2026-11-10

The frame is now a packed binary structure (versioned header, type, format, sequence, ack, topic, payload, optional CRC-32 and HMAC, counter-signed replies), covered by 19 Unit tests. The Windows AppContainer launcher now runs under test: `EscapeTests` (9 Integration tests, 31 in the project) show, each with a positive control, that inside the container

- a loopback TCP connect is refused (it works unsandboxed), so no network by default (REQ-007 default; per-host grants are still not built);
- a read outside the granted folders and a write outside or into a read-only grant are refused, a read and a write inside the grants work (REQ-001 files);
- a host environment variable is absent (present unsandboxed), because the child gets an explicit environment block (REQ-001 environment);
- a child process is refused with `ActiveProcessLimit = 1` and allowed without it (REQ-005 process count);
- a CPU spinner and a sleeper are ended by the host watcher and reported as `LimitHit.Cpu` and `LimitHit.Wall`, a memory hog dies and is reported as `LimitHit.Memory` (REQ-005); a well-behaved plugin reports `None`.

Findings from the first real run: `CreateProcessW` with an explicit environment block fails with `ERROR_ENVVAR_NOT_FOUND` (203) unless the block contains `LOCALAPPDATA` (point it at the plugin scratch folder, not the user profile); the block must be sorted; `Process.GetProcessById` cannot read the exit code of a process it did not start, so the launcher keeps its own handle; the .NET runtime sets a GC heap limit at 75% of the job memory limit, so peak memory at the kill is about 73%, and the attribution uses two thirds; coverage instrumentation writes a lock file to `TEMP` at exit and the container correctly denies it, so exit code 0 is not asserted for an instrumented build. CPU and wall limits are enforced by a host watcher thread, not by the job object. Linux (REQ-008) is not built in the spike.

## Built since, with tests (Windows 110 passed, Linux container 107 passed)

- Host chain: `PluginHost` ties package verification (REQ-006), grant and manifest check, the cross-process named mutex, `PluginPool` Bound and Shared (REQ-011), the sandbox and `BrokeredPlugin` (brokered file read and write and fetch, each checked against the grant and the caller's request-context rights, REQ-002/003/004/007/012).
- Grants and audit survive a restart (JSON file plus append-only audit lines, `GrantStoreTests`).
- Link-aware file check: a link inside a granted folder that points outside it is refused (`LinkResolver`, `LinkCheckTests`; the three symlink tests run in the Linux container and skip on this Windows machine because creating links needs privilege).
- Linux under bubblewrap (REQ-008): no network, read-only system, granted folders only, plus limits: memory through the .NET GC hard limit (a managed-heap cap, not an OS cap), CPU through `ulimit -t`, wall time through a kill timer, process count through `LinuxProcessWatcher` polling `/proc`; `LimitHit` reports which one ended the plugin.

## Must-row audit (REQ-001 to 008 and 010)

| Requirement | State | Gap |
|-------------|-------|-----|
| REQ-001 isolation (files, environment) | Shown on Windows and Linux | Links closed; none known (start-suspended assignment is in) |
| REQ-002 approved permissions, stored and auditable | Shown, persisted | Grant files are not encrypted or access-protected |
| REQ-003 brokered file access | Shown | none known |
| REQ-004 brokered network | Shown for `broker.fetch` | none known |
| REQ-005 limits | Windows job object and watcher; Linux as above | Linux memory is a managed-heap cap only |
| REQ-006 signed packages | Shown | key distribution is not designed |
| REQ-007 network off by default | Shown on both | none known |
| REQ-008 Linux | Shown | macOS deferred |
| REQ-010 sign-off | Owner decision | done 2026-11-20 |

## Still not built

- AppContainer profile deletion (the Windows API calls are hand-written; `AppContainerLauncher.Revoke` removes the folder rules, 2026-11-20). The Windows launcher already creates the process suspended, assigns it to the job and then resumes it (`AppContainerLauncher`), so there is no unlimited window.
- A Survive transport (a named pipe that outlives the caller; the reconnect registry has the token rules only) and Linux versions of the caller-death and pool lifetime tests.
- Message rate and heartbeat enforcement.
- The request context is built from a validated principal (`RequestContext.FromPrincipal`, 2026-11-20); RFC 8693 exchange stays with the STS.
- macOS (deferred by the owner): `SeatbeltPlugin` and its profile text are covered by Unit tests on every OS, but the escape tests have never run on a Mac; `sandbox-exec` is deprecated, and there is no PID namespace or resource limit.
- Library placement: [migration plan](migration-plan.md), confirmed 2026-11-20. The spikes stay in `src/OoBDev.Spikes.slnx` until it is confirmed and executed (owner, 2026-11-10).

## Consequence for the sign-off

Nothing found contradicts the protocol or the threat model's assumption that a stdio framed channel plus a job object (Windows) or bubblewrap (Linux) is enough for the first launchers. The unshown items stay open in the phase plan ([Table 2](architecture.md#table-2)); sign-off of REQ-010 is the owner's.
