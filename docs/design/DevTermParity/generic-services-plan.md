# Generic TCP services in DevTerm: design and migration plan

**Status:** built (option A, owner decision 2026-11-11): `ServiceHostBridge`, the `ServerBase` traffic tap and `--transport service`. The Tui and Web hosts take it through the common transport option.
**Last Updated:** 2026-11-12

## Contents

- [Need](#need)
- [Current code](#current-code)
- [Options](#options)
- [Migration plan](#migration-plan)
- [Test plan](#test-plan)

## Need

The old `bdd-svc-host` tool started the echo, discard, daytime, time and chargen servers. The owner decided (D6, 2026-11-10) that DevTerm hosts them, so they double as test peers over its other channels. Request: [generic service listeners](../../features/request/2026-11-10-devterm-generic-service-listeners.md).

## Current code

| Piece | Where | Note |
|-------|-------|------|
| Five servers | `OoBDev.System/Net/Services` | derive from `ServerBase(ipAddress, port)`; `Start`, `StopAsync`; no traffic events |
| `Rfc2217ServerBridge` | `OoBDev.DevTerm.Core` | an `ISessionObserver` that shares a live session on a TCP port, loopback by default, one client |
| CLI flags | `OoBDev.DevTerm.Tui/CliMode.cs` | `--share-tcp` creates the bridge |

**Table 1 — what exists**

## Options

| Option | Shape | Shows traffic in a session | Reuses the servers |
|--------|-------|----------------------------|--------------------|
| A. Service host bridge (recommended) | `ServiceHostBridge` in DevTerm.Core next to `Rfc2217ServerBridge`: owns one `ServerBase` of the chosen kind and mirrors each exchange into a `Session` over a small traffic tap added to `ServerBase` | yes, through the tap | yes, unchanged apart from the tap |
| B. Server-side transport | a `tcp-service` `ITransport` that listens and talks to one client; the kind becomes a presenter that originates the reply | yes, natively | no: the presenters reimplement the servers |
| C. Standalone host only | a `--service echo:7007` CLI switch that runs the server with no session | no | yes |

**Table 2 — options**

Recommendation A: smallest change, matches the bridge precedent, keeps the servers in `OoBDev.System` as decided, and the tap is useful to any other server. B fits the "presenters may originate traffic" decision but duplicates the servers; C does not meet the acceptance criterion that the session shows the traffic.

## Migration plan

| Project | Current | Target | Change |
|---------|---------|--------|--------|
| `OoBDev.System` | `Net/Sockets/ServerBase` | same | add an optional traffic tap (`Action<ReadOnlyMemory<byte>, bool>`: bytes, incoming) with no behaviour change when unset |
| `OoBDev.DevTerm.Core` | no host | `ServiceHostBridge` (observer) | new class, references `OoBDev.System` already |
| `OoBDev.DevTerm.Tui` | `--share-tcp` | add `--service <kind>[:address:port]` | CLI mode; the connection editor and device manifests get a `service` kind later |

**Table 3 — projects touched**

Defaults: bind to loopback, explicit address needed to listen wider, one client at a time, chargen capped by a byte limit per connection (configurable), ports below 1024 report the failure on the session and do not throw.

## Test plan

| Test | Tier |
|------|------|
| echo returns the bytes and the session records both directions | Unit (loopback sockets, free port) |
| closing the session stops the listener and frees the port | Unit |
| a privileged port without rights reports the failure | Unit (skipped when elevated) |
| chargen stops at the byte cap | Unit |
| a loopback-transport session uses a service as its peer | Simulate |

**Table 4 — tests**
