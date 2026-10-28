# Device Decoding — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Draft · **Date:** 2026-10-28 · **Related:** [migration plan](../../migration/device-decoding-migration-plan.md), [device parity](../../migration/device-parity.md)

## Contents

- [Goals](#goals)
- [Non-goals](#non-goals)
- [Requirements](#requirements)
- [Constraints](#constraints)
- [Open questions](#open-questions)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals

1. Decode and encode each supported device's wire protocol in a library that has no UI and no transport dependency.
2. Reuse the device model that already exists in `OoBDev.System.Abstractions` (segmenters, message decoders and encoders, device definitions) instead of adding parallel contracts.
3. Keep the byte-exact behaviour dev-term verified on real hardware, and say in each readme what has not been verified.
4. Let a future application shell (dev-term rebuilt on these libraries) use the packages without changes.

## Non-goals

- UI panels, view models and renderers (phases 6 and 7, waiting on Q3).
- Device manifests and `.ksy` frame decoding (phase 8) and transports dotex lacks (phase 9).
- Z-Wave (phase 5) until the upstream `dev/zstick-s2` work settles; SCPI (phase 4) is designed here only as far as the shared contracts need.
- Hardware control logic that belongs to a shell (sessions, connection profiles, plugin loading).

## Requirements

<a id="table-1"></a>

**Table 1 — Requirements**

| Id | Requirement | Source |
|----|-------------|--------|
| R1 | One package per device, named `OoBDev.{Vendor}.{Device}` (Table 6 of the plan), referencing Framework abstractions only | Plan section 3 |
| R2 | Framing is an `ISegmenter` built from an `ISegmentBuildDefinition`; decoding is `IMessageDecoder<T>`; encoding is `IMessageEncoder<T>`; a device exposes `IDeviceDefinitionReceiver<T>` and `IDeviceDefinitionTransmitter<T>` | Plan section 4 |
| R3 | Decoders are stateless per segment; anything that remembers earlier frames (change tracking, reply correlation) is a separate, explicit component | Existing `IMessageDecoder<T>` contract |
| R4 | Checksums go through `IChecksumCalculator`, with the missing algorithms added and tested against dev-term's recorded traces | Plan section 4 |
| R5 | Message values are typed records, with a text rendering that is separate from decoding | Plan section 3 |
| R6 | Registration through `ServiceCollectionExtensions` with `TryAdd*`; options validated with `AddValidatedOptions<T>()`; provider keys kebab-case in `{Vendor}Globals` | `coding-standards` rule |
| R7 | The five merges M1 to M5 of the parity note are applied where their condition is met, and the rest recorded as hypotheses | Parity note, section 3 |
| R8 | Each package has `README.{Project}.md` that lists untested-on-hardware behaviour, plus the device's `.ksy` and `known-configuration.md` moved from dev-term | Plan section 2 |
| R9 | The serial adapter reports a busy port as a failed open, not an unhandled `UnauthorizedAccessException` (dev-term BUG-003 equivalent) | Plan section 4 |
| R10 | Tests and device documentation keep the dev-term approach (plan section 6) | Owner, 2026-10-26 |

## Constraints

- net10.0, nullable enabled, `ImplicitUsings` disabled, one top-level type per file, no obsolete forwarders (the framework is unreleased).
- No third-party DI, logging or Polly. Package versions only in `src/Directory.Packages.props`.
- dev-term is a read-only reference (`C:\repo\oobdev\dev-term`, branch `dev/zstick-s2`, `d777358`); its history is not imported.
- Hardware is not available in the default test run.

## Open questions

- **Q2** where the shared test helpers and the category mapping live.
- **Q3** whether UI, manifests and transports migrate, and in what order.
- **Q4** where dev-term lives after the migration.
- Names to confirm: `OoBDev.DerEE.De5000` (vendor spelling) and `OoBDev.ZWave` (protocol, not the stick).
