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

## What was not shown

- Start-suspended assignment to the job: the spike assigns after start, so a plugin has a short unlimited window. A real launcher creates the process suspended.
- Signed packages and the grant broker, per-host network grants, the lifetime modes (REQ-011) and the request context (REQ-012) are designed, not built.
- Linux and macOS. No shim was tried.
- Message rate and heartbeat enforcement.

## Consequence for the sign-off

Nothing found contradicts the protocol or the threat model's assumption that a stdio framed channel plus a job object is enough for the first Windows launcher. The unshown items stay open in the phase plan ([Table 2](architecture.md#table-2)); sign-off of REQ-010 is the owner's.
