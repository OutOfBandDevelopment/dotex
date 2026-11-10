# Feature request: configurable generic TCP services in DevTerm

- **Status:** triaged
- **Requested by:** owner (answer to D6, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm`, `OoBDev.System` (`Net/Services`, `Net/Sockets`)

## Need

BinaryDataDecoders shipped `bdd-svc-host`, a console tool that started five classic TCP servers (echo, discard, daytime, time, chargen). The servers are already in `OoBDev.System`; only the host is missing. Owner decision: the host is not a separate tool. DevTerm hosts the services, configured like its other connections, so they can also serve as test peers over its other channels (loopback, TCP, serial, broker bridges).

## Proposed behavior

- A DevTerm session type (or presenter or bridge: the design settles which) runs one generic server kind (echo, discard, daytime, time, chargen) on a configured address and port.
- Settings: server kind, listen address, port (ports below 1024 need elevation on Linux, so the field is required and shown), stop on session close.
- Available in the console app's CLI mode for headless use, and in the connection editor and device manifests like other connections.
- The servers stay in `OoBDev.System`; DevTerm references them, it does not copy them.

## Acceptance criteria

- Given a session of kind echo on a free port, when a client sends bytes, then the same bytes come back and the session shows the traffic.
- Given the session is closed, then the listener stops and the port is free.
- Given a port below 1024 without rights, then the session reports the failure and does not crash.
- Given the loopback transport, then a test can use a service as the peer without hardware.

## Existing code checked

`OoBDev.System/Net/Services` (five servers) and `Net/Sockets` (`IServerBase`, `ServerBase`); DevTerm `Session`, `ITransport`, `LoopbackTransport`, `Rfc2217ServerBridge`, broker bridges; the 2026-10-03 decision that presenters may originate traffic ([architecture](../../applications/devterm/design/architecture.md)). The original source is `BinaryDataDecoders.Net.ServiceHost.Cli` (35 lines).

## Security and operations

Listening sockets: bind to loopback by default, require an explicit address to listen wider, and bound client counts and response sizes (chargen streams forever).

## Priority

- **Impact:** completes the D6 migration item; gives DevTerm a no-hardware peer for its TCP and UDP listener work.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Session type, presenter or bridge? This is a layering question for the design document and migration plan (rule `layering-and-migration-plans`) before any code.

## Triage notes

Merged into `docs/design/DevTermParity` follow-ups and `BACKLOG.md` section 1; replaces the "Net.ServiceHost CLI" migration leftover.
