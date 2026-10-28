# Device Decoding — Testing Strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft · **Date:** 2026-10-28

## Contents

- [Approach](#approach)
- [Test layers](#test-layers)
- [Coverage](#coverage)
- [Documentation outputs](#documentation-outputs)
- [Open point](#open-point)

### List of Tables

1. Table 1 — Test layers

## Approach

Tests keep the dev-term approach (plan section 6, owner 2026-10-26): byte-exact recorded traces, scripted loopback for request and response, hardware tests that report inconclusive when the device is absent. The default run never needs hardware or Docker.

## Test layers

**Table 1 — Test layers**

| Layer | What it checks | Category |
|-------|----------------|----------|
| Segmenter | Frames split from a byte stream, resynchronisation after garbage, partial frames across reads | `Unit` |
| Decoder and encoder | Recorded frames decode to the expected record; records encode to the expected bytes; round trip | `Unit` |
| Checksums | Each algorithm against dev-term's traces, including the inner Radex One checksum | `Unit` |
| Scripted loopback | Request and reply sequences against a scripted fake (Busylight apply, Radex One reads) | `Simulate` |
| Registration | `ServiceCollectionExtensions` registers the keyed services and validates options | `Unit` |
| Real hardware | Target read from `.runsettings` run parameters, `Assert.Inconclusive` without the device | `DevLocal` |

Recorded traces are copied from the dev-term tests with their source named in a comment next to each. A merged part (M1 to M5) has its own tests, and any hypothesis (M3, M4) is labelled as such in the test name.

## Coverage

Target 80% or better line coverage per package (Framework rule, applied to the device packages), measured with the existing `XPlat Code Coverage` run. The real hardware tests are excluded from the figure.

## Documentation outputs

Specs, user guide and bench reports stay generated from tests as in dev-term (`docs/specs`, `docs/user-guide`, `docs/test`); moving the generators is part of Q2 and is not started here.

## Open point

Q2 decides where the shared test helpers (loopback transport, sample data generator, category constants) live. Until it is answered the first packages copy dev-term's helpers unchanged into their test projects.
