# DevTerm parity: matrix of dev-term against this repository

**Status:** Draft, 2026-10-29 · [Migration overlap review](../../migration/overlap-review-2026-10-29.md) · [Device decoding design](../DeviceDecoding/README.md) · [Device decoding plan](../../migration/device-decoding-migration-plan.md)

Owner requirement (2026-10-29): the OoBDev version of dev-term has 100% parity with dev-term, every protocol and every feature. This is the first step, the matrix. Requirements, architecture, api-design and testing-strategy documents follow per feature group (rule: `docs/patterns-discovery/06-design-document-standard.md`).

Source: `C:\repo\oobdev\dev-term` at `8c66e74` (2026-10-28). Lines are C# lines of the project, tests are test attributes in its test project; both come from the repository files and were not built or run. Presence in this repository was checked by type name across `src/` on 2026-10-29.

## Contents

- [Summary](#summary)
- [Matrix](#matrix)
- [Open points](#open-points)
- [Next steps](#next-steps)

## Summary

Of 34 dev-term projects, the eight device protocol libraries are in this repository (cores only, without their dev-term control surface, UI definition, panel and plugin parts), `UiDefinitions` is present as `OoBDev.UiDefinitions*`, and the TCP and RFC 2217 device adapters exist in an older form. Everything else (core session engine, 11 transports, manifests, presenters, logging, configuration, observability, plugins, three shells) is absent. About 55,000 lines and 2,000 tests of dev-term are not here.

## Matrix

**Table 1 — Parity by dev-term project**

| dev-term project | Lines / tests | In this repository | Gap | Group |
|------------------|---------------|--------------------|-----|-------|
| Core | 5,892 / 166 | No (`MessageRouter`, `Session`, `PluginLoader`, `PresenterCatalog`, control server, HPGL converter all absent) | All | A Engine |
| Configuration | 9,783 / 493 | No (view models such as `ConnectionEditorViewModel`, session builder) | All | A Engine |
| Logging | 1,921 / 64 | No (session log, playback) | All | A Engine |
| Observability | 67 / 3 | No (`TelemetryExporter`); `OoBDev.OpenTelemetry` exists | Wire up | A Engine |
| Schemas | 60 / 4 | No | All | A Engine |
| Presenters.Text | 290 / 35 | No (ASCII, UTF-8, hex, decimal, octal, binary) | All | B Presenters |
| DeviceManifests | 5,825 / 154 | No (JSON/XML manifests, `.ksy` import, generic panels) | All | C Manifests |
| UiDefinitions | 3,036 / 105 | Yes, `OoBDev.UiDefinitions*` | Verify against newest dev-term (2026-10-14) | D UI model |
| Transports.Serial | 1,276 / 48 | Partly: `OoBDev.System.IO.Ports` (adapter only) | Discovery, read/write streams, platform port descriptions | E Transports |
| Transports.Hid | 577 / 11 | Partly: `OoBDev.System.IO.UsbHids` (adapter only) | Discovery, read stream | E Transports |
| Transports.Tcp | 748 / 29 | Partly: `OoBDev.System.IO.Tcp` | LXI discovery, XON/XOFF gate | E Transports |
| Transports.Rfc2217 | 1,614 / 47 | Partly: `Rfc2217DeviceAdapter` | Codec, telnet layer, server bridge (2026-10-14 is newer) | E Transports |
| Transports.Usbtmc | 1,667 / 32 | No | All | E Transports |
| Transports.Vxi11 | 552 / 7 | No | All | E Transports |
| Transports.Ble, Ble.Windows | 444 + 324 / 19 | No | All | E Transports |
| Transports.Mqtt | 455 / 14 | No | All | E Transports |
| Transports.Brokers (AMQP, STOMP) | 837 / 18 | No | All | E Transports |
| Transports.Loopback | 281 / 26 | No | All | E Transports |
| Devices.Busylight, De5000, K8055, Nmea, RadexOne, Scpi, ZWave, ZoomH4n | 5,610 / 295 | Yes (protocol cores) | Control surface, UI definition, panel contribution, plugin module per device | F Devices |
| Devices.Demo | 100 / 0 | No | Sample device for the shells | F Devices |
| Plugins.KeyValue, Plugins.Sample | 102 / 0 | No | Plugin model samples | G Plugins |
| Console | 9,678 / 328 | No (CLI and TUI) | All; becomes `OoBDev.DevTerm.Tui` and the CLI | H Shells |
| Wpf | 7,354 / 218 | Minimal: `OoBDev.DevTerm.Wpf` (286 lines, SCPI over a raw socket) | Almost all | H Shells |
| Web | 2,465 / 89 | No | All | H Shells |

## Open points

- Groups: the first five projects are the engine every other group needs, so A comes first, then B and E in parallel, then C, D, F, G and H.
- Serial, HID, TCP and RFC 2217 exist here in two forms (the older `IDeviceAdapter` adapters and dev-term's newer `ITransport` implementations); the design for group E must pick one contract. This is a design question for the architecture document, not an owner question yet.
- dev-term's test projects use their own helpers (`DevTerm.Test.Utilities`) and a coding-standards test (`DevTerm.CodingStandards.Tests`); how they map onto this repository's test categories is for the testing-strategy document.
- The `docs/` tree of dev-term (devices, protocols, specs, user guide, test sessions) is part of parity and has not been inventoried.

## Next steps

1. Inventory dev-term `docs/` and decide where each part lands (`docs/design/`, project readmes, `docs/how-tos/`).
2. Write requirements, architecture, api-design and testing-strategy for group A (engine), then E.
3. Port group by group, each with its tests, as backlog items; the backlog item "DevTerm 100% parity" links here.
