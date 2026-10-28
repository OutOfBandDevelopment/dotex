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
| Real hardware | Target read from `.runsettings` run parameters, `Assert.Inconclusive` without the device | `Hardware` |

Recorded traces are copied from the dev-term tests with their source named in a comment next to each. A merged part (M1 to M5) has its own tests, and any hypothesis (M3, M4) is labelled as such in the test name.

## Coverage

Target 80% or better line coverage per package (Framework rule, applied to the device packages), measured with the existing `XPlat Code Coverage` run. The real hardware tests are excluded from the figure.

## Documentation outputs

Specs, user guide and bench reports stay generated from tests as in dev-term (`docs/specs`, `docs/user-guide`, `docs/test`); moving the generators is not started here.

## Open point

Decided by the owner (2026-10-28): every device library has its own test project (`OoBDev.{Vendor}.{Device}.Tests`). Shared helpers (loopback transport, device-present preflight, trace and sample data helpers, category constants) live in a new `OoBDev.Devices.TestUtilities` project, and `OoBDev.TestUtilities` stays unchanged. A new category `Hardware` marks tests that need a physical device; they run in the normal run and end `Assert.Inconclusive` when the device is not detected, as in dev-term. The `testing` rule lists the category.
