# DevTerm parity: matrix of dev-term against this repository

**Status:** Draft, 2026-10-29 · [Migration overlap review](../../migration/overlap-review-2026-10-29.md) · [Device decoding design](../DeviceDecoding/README.md) · [Device decoding plan](../../migration/device-decoding-migration-plan.md)

Owner requirement (2026-10-29): the OoBDev version of dev-term has 100% parity with dev-term, every protocol and every feature. This is the first step, the matrix. Requirements, architecture, api-design and testing-strategy documents follow per feature group (rule: `docs/patterns-discovery/06-design-document-standard.md`).

Source: `C:\repo\oobdev\dev-term` at `8c66e74` (2026-10-28). Lines are C# lines of the project, tests are test attributes in its test project; both come from the repository files and were not built or run. Presence in this repository was checked by type name across `src/` on 2026-10-29.

## Contents

- [Summary](#summary)
- [Matrix](#matrix)
- [Open points](#open-points)
- [Documentation inventory](#dev-term-documentation-inventory)
- [Next steps](#next-steps)

## Summary

Of 34 dev-term projects, the eight device protocol libraries are in this repository (cores only, without their dev-term control surface, UI definition, panel and plugin parts), `UiDefinitions` is present as `OoBDev.UiDefinitions*`, and the TCP and RFC 2217 device adapters exist in an older form. Everything else (core session engine, 11 transports, manifests, presenters, logging, configuration, observability, plugins, three shells) is absent. About 55,000 lines and 2,000 tests of dev-term are not here.

## Matrix

The Gap column was written before the port started; the current state is in [status.md](status.md). The living documents moved to [docs/applications/devterm](../../applications/devterm/README.md).

**Table 1 — Parity by dev-term project**

| dev-term project | Lines / tests | In this repository | Gap | Group |
|------------------|---------------|--------------------|-----|-------|
| Core | 5,892 / 166 | No (`MessageRouter`, `Session`, `PluginLoader`, `PresenterCatalog`, control server, HPGL converter all absent) | All | A Engine |
| Configuration | 9,783 / 493 | No (view models such as `ConnectionEditorViewModel`, session builder) | All | A Engine |
| Logging | 1,921 / 64 | No (session log, playback) | All | A Engine |
| Observability | 67 / 3 | No (`TelemetryExporter`); `OoBDev.Telemetry` exists | Wire up | A Engine |
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

## Next steps

1. ~~Inventory dev-term `docs/`~~ done (Table 2); the files themselves move with their groups.
2. ~~Write requirements, api-design and testing-strategy~~ done 2026-10-29: [requirements](../../applications/devterm/requirements.md), [API design](../../applications/devterm/api-design.md), [testing strategy](../../applications/devterm/testing-strategy.md) (whole DevTerm, drafts awaiting owner review).
3. Port group by group, each with its tests, as backlog items; the backlog item "DevTerm 100% parity" links here.

## dev-term documentation inventory

Counts from `C:\repo\oobdev\dev-term\docs` (2026-10-29).

**Table 2 — dev-term documents and where they land**

| Folder | Files / lines | Content | Landing place here |
|--------|---------------|---------|--------------------|
| `design` | 51 / 8,084 | Architecture, transports, presenters, manifests, plugin model, frontends, session logging, theming, UI definitions, USBTMC, VXI-11, RFC 2217; `proposals` and `rejected` | Source for the per-group requirements and architecture documents here; rejected designs are recorded where they affect a decision |
| `specs` | 15 / 2,297 | Screen specifications (main windows, editors, stream monitor, routing, playback, Z-Wave panel) | `docs/applications/devterm/specs/` (imported) |
| `devices` | 37 / 5,642 | Per-instrument notes (scopes, DMM, supplies, Z-Wave stick, TCP232 converter, and the eight device packages) | Device readmes and `docs/design/DeviceDecoding/`; instruments with no package here stay as notes |
| `protocols` | 3 / 3,160 | USBTMC protocol notes | Transport design document |
| `user-guide` | 179 / 2,418 | Task guides with images | `docs/how-tos/devterm/` once the shells exist |
| `test` | 44 / 2,542 | Bench session records on real hardware | Kept as evidence in the device change documents; summarized, not copied |
| `changes`, `bugs` | 21 + 72 | Dated change log and bug reports | `git log` of dev-term; open bugs go to the backlog when the matching code is ported |
| `coding-standards.md` | 160 | dev-term's standards | Compared against `.claude/rules/coding-standards.md`; this repository's rule wins |

## Related documents

- [Status and leftovers](status.md), [restructure plan](restructure-plan.md), [control panel spike](control-panel-spike.md)
- [Test utilities plan](test-utilities-plan.md), [UI language (later)](ui-language-later.md)
- [Avalonia spike](../../migration/avalonia-spike.md)
