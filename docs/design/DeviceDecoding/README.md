# Device Decoding

**Status:** Draft · **Date:** 2026-10-28 · **Plan:** [device-decoding-migration-plan.md](../../migration/device-decoding-migration-plan.md) · **Parity:** [device-parity.md](../../migration/device-parity.md)

Protocol decoders for hardware devices (DE-5000, Busylight, K8055, NMEA, Radex One, Zoom H4n, later SCPI and Z-Wave), migrated from dev-term as one package per device on the existing `OoBDev.System.IO` device model.

## Documents

| Document | Holds |
|----------|-------|
| [requirements.md](requirements.md) | Goals, non-goals, numbered requirements, constraints, open questions |
| [architecture.md](architecture.md) | Layers, data flow, package layout, the first template device |
| [api-design.md](api-design.md) | Mapping of dev-term contracts to the existing IO contracts, the gaps to close, naming |
| [testing-strategy.md](testing-strategy.md) | Test categories, byte-exact traces, hardware tests, coverage |

## Scope of this folder

Phase 1 of the plan covers the protocol libraries only. The UI framework (phases 6 and 7), manifests (phase 8) and extra transports (phase 9) get their own documents when Q3 is answered, and are listed here only where a protocol decision depends on them.
