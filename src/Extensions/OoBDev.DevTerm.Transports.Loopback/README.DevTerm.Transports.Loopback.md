# OoBDev.DevTerm.Transports.Loopback

An in-process `ITransport` that answers scripted commands, so the DevTerm shells and tests run with no hardware. Ported from dev-term's `DevTerm.Transports.Loopback`.

## Description

| Command | Response |
|---------|----------|
| `hello` | `From Loopback test` |
| `Send Stream: N, ascii` | one line of N repeating A-Z characters |
| `Send Events: N` | `Event 1` .. `Event N`, one per line |
| `MEAS?` | the next simulated sensor sample |
| `Samples: N` | the next N samples (N is capped at 1000) |
| `STATUS?` | a labelled reading with units |
| `help`, `?` | the command list |
| anything else | `? Unrecognized: <line>` |

Matching is case-insensitive. The sample counters belong to the transport instance, so each one starts at sample 0. `LoopbackTransportOptions.SampleIntervalMs` (default 0) paces the lines of `Send Events` and `Samples`.

## Usage

```csharp
services.TryAddLoopbackTransport();
```

`ITransport` is registered transient; a closed transport can be reopened.

## Verified

Unit tests cover the scripted commands, pacing, unrecognized input, reopen after close, the options validator and the registration. No hardware is involved.
