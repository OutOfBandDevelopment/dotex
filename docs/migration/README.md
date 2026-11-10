# Migration plan

**Last Updated:** 2026-11-10

One plan for everything brought into this framework from other repositories, organised by feature, not by source. Each feature names its canonical target, its state and the sources it came from or was compared with. Open work is in [BACKLOG.md](../../BACKLOG.md) and [TODO.md](../../TODO.md); finished work is in [docs/changes](../changes/README.md); designs are in [docs/design](../design/README.md). Per-source notes were folded into this file on 2026-11-10 (`git log -- docs/migration` has them).

## Contents

- [Rules](#rules)
- [Sources and best coverage](#sources-and-best-coverage)
- [Devices](#devices)
- [Transports and IO](#transports-and-io)
- [DevTerm: UI, shells and plugins](#devterm-ui-shells-and-plugins)
- [Libraries and tools from BinaryDataDecoders](#libraries-and-tools-from-binarydatadecoders)
- [Epics from SharedFramework](#epics-from-sharedframework)
- [Harvest candidates](#harvest-candidates)
- [Rejected, with reasons](#rejected-with-reasons)
- [Surroundings (guides, agent config, scripts)](#surroundings-guides-agent-config-scripts)
- [Decisions](#decisions)

### List of Tables

1. Table 1 — Sources
2. Table 2 — Devices
3. Table 3 — Device merge list
4. Table 4 — Transports and IO
5. Table 5 — DevTerm areas
6. Table 6 — BinaryDataDecoders libraries and tools
7. Table 7 — SharedFramework epics
8. Table 8 — Harvest candidates
9. Table 9 — Rejections
10. Table 10 — Surroundings
11. Table 11 — Decisions

## Rules

- Everything migrates except stubs with no implementation, obsolete platforms (Silverlight, UWP, Compact Framework) and code with no net10.0 equivalent (`CLAUDE.md`, Migration Scope). Phases are priority order, not selection.
- Best coverage (`owner-decisions`): compare every source and use the best-covered, tested implementation as the base; merge what the others add. A rejection carries its reason (Table 9).
- Surroundings migrate too: guides, agent configuration, docs, scripts, plans (Table 10).
- Documents of a migrated application live under `docs/applications/{app}/`, reusable designs under `docs/design/<Feature>/`, protocols under `docs/protocols/`. A layering change needs a confirmed plan first (rule `layering-and-migration-plans`).
- Copies of code inside this repository may be deleted once migrated; older repositories stay untouched (owner, 2026-11-09).

## Sources and best coverage

**Table 1 — Sources**

| Source | Location | State |
|--------|----------|-------|
| BinaryDataDecoders | `C:\repo\mine\binary-data-decoders` at `e590b420` (78 projects; newer than `Incoming/BinaryDecoders`, which is superseded) | Libraries migrated; leftovers in Tables 6 and 9 |
| dev-term | `C:\repo\oobdev\dev-term` at `8c66e74` | Base for devices; shells and transports in progress ([parity status](../design/DevTermParity/status.md)) |
| SharedFramework | `Incoming/SharedFramework` (52 projects) | Replaced by designs (Table 7) |
| proving-grounds, learning repositories, `_storage`, notes | `C:\repo\mine` and `C:\repo\oobdev` | Read-only references (Table 8, [harvest candidates](../harvest-candidates.md)) |

Where two sources hold the same feature, dev-term is newer and tested, so it is the base; the other source contributes only the parts the base lacks. The behaviour of migrated libraries was checked by running the source tests against main (2026-11-03): all source cases exist in main; eleven numeric rows and one checksum vector were added.

## Devices

Canonical target: one package per device (`src/HardwareDevices`, `src/Extensions`), named `OoBDev.{Vendor}.{Device}`, built on the device model in `OoBDev.System.Abstractions`. Design: [DeviceDecoding](../design/DeviceDecoding/README.md), including the conversion rules, naming and order of work in its [migration plan](../design/DeviceDecoding/migration-plan.md).

**Table 2 — Devices**

| Device | Canonical package | Base | Also compared | State |
|--------|-------------------|------|---------------|-------|
| Busylight | `OoBDev.Kuando.Busylight` | dev-term | BinaryDataDecoders, proving-grounds provider | Done; `programSequence` waits on a hardware watch |
| K8055 | `OoBDev.Velleman.K8055` | dev-term | BinaryDataDecoders | Done; digital input bit order awaits hardware |
| Radex One | `OoBDev.Quarta.RadexOne` | dev-term | BinaryDataDecoders, reverse-engineering notes | Done; serial layout and unit awaits a capture |
| Zoom H4n | `OoBDev.Zoom.H4n` | dev-term | BinaryDataDecoders, proving-grounds CLI and `ZoomH4n.md` | Done |
| NMEA 0183 | `OoBDev.Nmea` | dev-term | BinaryDataDecoders | Done |
| DE-5000 | `OoBDev.DerEE.De5000` | dev-term | `OoBDev.De5000.Ble.Cli` (dropped) | Done; Hardware test waits on the BLE radio |
| SCPI | `OoBDev.Scpi` | dev-term | BinaryDataDecoders Rigol (stub, rejected) | Done |
| Z-Wave | `OoBDev.ZWave` | dev-term | none | Done; upstream `zstick-s2` settled |
| Fencing (Favero, Saint George) | fencing package | proving-grounds ScoreMachine decoders and emulator | BinaryDataDecoders | Done, marked untested (D4) |
| LanC | LanC package | BinaryDataDecoders command tables | proving-grounds `LancMonitor` | Done |

**Table 3 — Device merge list (BinaryDataDecoders parts added to the dev-term base)**

| Id | Device | Part | State |
|----|--------|------|-------|
| M1 | Busylight | Step command structs, 16-step sequence frame | Merged (`BusylightSequence`); hardware confirmation open |
| M2 | K8055 | Named digital input flags | Merged; raw value stays until the bit order is confirmed |
| M3 | Radex One | Serial number and version layout | Open: needs a recorded reply |
| M4 | Radex One | Unit scaling (`/100` µSv/h) | Recorded as a hypothesis until a capture confirms it |
| M5 | NMEA | Fix-quality names | Merged (`NmeaFixQuality`) |

What remains is hardware-gated (physical passes for K8055 bits, Busylight program frames, Radex serial and unit, Zoom H4n handshake, NMEA BT-20) plus BLE for the DE-5000.

## Transports and IO

**Table 4 — Transports and IO**

| Feature | Canonical target | Sources | State |
|---------|------------------|---------|-------|
| Segmenters, checksums, device adapters, messages | `OoBDev.System.Abstractions`, `OoBDev.System.IO.*` | BinaryDataDecoders `IO.*` | Migrated |
| Serial, HID, pipelines | `OoBDev.System.IO.Ports`, `.UsbHids`, `.Pipelines` | BinaryDataDecoders (base), dev-term `Transports.Serial/Hid` (second implementation behind `ITransport`) | Migrated; compare dev-term's when touched. Busy-port `UnauthorizedAccessException` (source BUG-003) is requirement R9 of DeviceDecoding |
| TCP, RFC 2217, LXI discovery | `OoBDev.System.IO.Tcp` | dev-term (base for RFC 2217 and LXI) | Merge into the existing project |
| USBTMC, VXI-11, BLE, MQTT, brokers, loopback | new transport packages | dev-term only | See [Transports](../design/Transports/README.md) and the backlog |
| Device manifests and `.ksy` frame decoding | `docs/design/UiDefinitions` | dev-term | Phase 8 of the device plan |

## DevTerm: UI, shells and plugins

dev-term merges into this repository entirely (owner, 2026-10-28): shells become tools under `src/Tools`, control libraries are generic under `src/Framework`, nothing is back-ported. Status and slices: [DevTermParity](../design/DevTermParity/status.md).

**Table 5 — DevTerm areas**

| Area | Canonical target | Design |
|------|------------------|--------|
| UI model, validator, serializers | `OoBDev.UiDefinitions*` | [UiDefinitions](../design/UiDefinitions/README.md) |
| View models and renderers | `OoBDev.UiDefinitions.ViewModels`, `.Wpf`, later Avalonia, Tui, Web | [UiDefinitions](../design/UiDefinitions/README.md) (Avalonia spike included) |
| Shells | `OoBDev.DevTerm.Wpf`, `.Tui`, `.Web` | [DevTermParity](../design/DevTermParity/README.md) |
| Plugins and sandbox | Plugins design; sandbox layering proposal awaiting confirmation | [Plugins](../design/Plugins/README.md), [PluginSandbox](../design/PluginSandbox/migration-plan.md) |
| Presenters, session log, theming, connection profiles, multi-session, channel, project state, web frontend, network devices, stream tools | one folder each in `docs/design` | folded from the dev-term docs on 2026-11-04 |
| dev-term docs, bugs, changes, specs, user guide, bench reports | `docs/applications/devterm/` | kept as living docs and history; links inside `source/` stay dev-term paths |

Future dev-term plans (BLE on Linux/macOS, UDP, rendering presenters, discovery and config editors, research items) are single items in `BACKLOG.md` section 1.

## Libraries and tools from BinaryDataDecoders

**Table 6 — BinaryDataDecoders libraries and tools**

| Feature | Canonical target | State |
|---------|------------------|-------|
| Apple II, archives (Tar, Zip), ISO 9660, classic cryptography, expression calculator, network helpers, ToolKit, templating, text, YAML, test utilities | `OoBDev.System*`, `OoBDev.Extensions`, `OoBDev.TestUtilities` | In main. Incoming copies diff-reviewed and deleted 2026-11-06 (D1); only refactors and a few fixed regressions were found |
| Vigenère cipher | `OoBDev.System/Cryptography` | In main (D9); educational warning applies |
| Numerics | `OoBDev.System` | Done |
| Code analysis (Roslyn, DacFx, StructuredLog navigators) | `OoBDev.Extensions.CodeAnalysis`, next to `OoBDev.DacFx` | Done; single source |
| Drawing (Code39, JPEG, PNG pack, Z-order, DeepZoom tiles) | `OoBDev.Extensions.Drawing.*` on SkiaSharp | Done 2026-11-03; HomeSeer and `HandyClasses` copies are the same code |
| PackMan CLI, Xslt CLI | `src/Tools` | Done (D6) |
| Net.ServiceHost CLI | `src/Tools` | Migrate (D6); main has the worker recipe and hosting packages |
| Windows.Forms `ValidTextBox` | `OoBDev.Extensions.Windows.Forms` | Decision in [WindowsForms migration plan](../design/WindowsForms/migration-plan.md) |
| ContractParser (two ANTLR grammars, five sample contracts) | [docs/design/ContractParser](../design/ContractParser/README.md) | Kept as specification (D5) |

## Epics from SharedFramework

Design first (owner, 2026-11-06): none is implemented until a consumer appears. Four documents per feature ([standard](../patterns-discovery/06-design-document-standard.md)); plan in [SharedFramework](../design/SharedFramework/migration-plan.md).

**Table 7 — SharedFramework epics**

| Epic | Design | Already built in main |
|------|--------|-----------------------|
| 2 Communications | [Communications](../design/Communications/README.md) | `OoBDev.Communications`, message queueing, MailKit adapter |
| 6 Documents | [SharedFramework](../design/SharedFramework/README.md) | `OoBDev.Documents` and converters |
| 7 Identity | [SharedFramework](../design/SharedFramework/README.md) | `OoBDev.Identity`, `.Jws`, JWT bearer, Keycloak adapter |
| 10 Text templating | [SharedFramework](../design/SharedFramework/README.md) | Handlebars provider, ten helpers, compilation cache; open: container adapter, database template source |
| Queues, storage, caching | n/a | SQS, Service Bus, RabbitMQ with AsyncAPI contributors; Azure Storage, Microsoft and Redis caching |

## Harvest candidates

Map: [harvest-candidates.md](../harvest-candidates.md). Nothing is copied without a comparison.

**Table 8 — Harvest candidates**

| Source item | Decision |
|-------------|----------|
| ScoreMachine and HomeSeer | Libraries and emulators only (D8); applications (web, TCP, NetTv, Arduino) rejected |
| hybridsearch, Oobtainium, BuildFirstOnce, markdownplantuml, TotpExample, HandyClasses, postsql-playground | Evaluate later (D7); postsql informs `OoBDev.DacFx` |
| Oobtainium (mocking and proxy framework, about 1,600 lines) | Not migrated; mocking is solved by Moq and NSubstitute. Reopen only with a fresh comparison against `proving-grounds/examples/OoBDev.Oobtainium` |
| `OoBDev.MessageBroker`, `Ssb.Modeling`, `SQL` | Deferred until a Service Broker feature exists |
| `event-sourcing`, `sync-mesh`, `fohjin-cqrs` | Read-only pattern references; ask before copying |
| notes: `cgi-middleware-kestrel` (done), `sqldef-evaluation`, `jws-jwt-from-scratch` | Design inputs for the matching features |

## Rejected, with reasons

**Table 9 — Rejections**

| Item | Reason |
|------|--------|
| Rigol stub | One file with no implementation; `OoBDev.Scpi` profiles cover SCPI instruments |
| EByteElectronicTechnology | Tests only, no source anywhere |
| UWP, Silverlight, Compact Framework, .NET Framework-only code | No net10.0 equivalent |
| BinaryDataDecoders `Nmea/Notes` (about 5,000 lines of generated WMI classes, a third-party Wiimote P/Invoke sample) | Generated or scratch; HID access is HidSharp here |
| Busylight `Class1`, K8055 request and response structs, NMEA classes, Radex write-settings request, Zoom H4n request enum | Experiment code, or dev-term decodes the same offsets and codes |
| `IO.Controller.Cli`, `OoBDev.De5000.Ble.Cli` | Replaced by the DevTerm tools (D6); the source's bug reports 001, 002 and 004 describe code that is not carried over |
| `FSharp.Samples`, `VisualBasic.Samples`, `SqlServer.Samples`, `WinForms.Samples`; `.rptproj`, `ReportList.yaml`, `build.bat` | Empty, sample-only or old build glue; revisit as examples only on request |
| `WhitedUS.Libs` compression copies, `_storage` ISO reader | Older ancestor, and smaller than main's |
| proving-grounds applications, `NeedsReviewed`, `_storage` and `oobdev/containers` applications | Applications, not libraries (compare `containers` when touching the test stack) |

## Surroundings (guides, agent config, scripts)

**Table 10 — Surroundings**

| Item | Decision |
|------|----------|
| dev-term docs (423 files) | Imported whole under `docs/applications/devterm/`; device manuals also in `src/Extensions/OoBDev.Scpi/docs/` |
| dev-term project guide | Snapshot at `docs/applications/devterm/source/project-guide.md`; its constraints are in the rule `devterm-constraints` |
| dev-term TODO, BACKLOG, OPEN_QUESTIONS | Snapshots; open work carried into `BACKLOG.md`; open hardware questions wait for bench sessions |
| Skills `hardware-test`, `device-manual` | Migrated to `.claude/skills` |
| Skill `docs-sync` | Merged into rule `work-tracking`, `complete-work` and `reconcile-designs-and-code` |
| Skills `work-docs-audit`, `bug-report`; hook `merge-gate.sh` | Rejected: covered by `tracking-hygiene`, the `work-backlog` audit, `BACKLOG.md` and change documents, and the commit-time reminder |
| `devterm.runsettings` | Replaced by the Hardware section of `src/.runsettings` ([test hardware](../how-tos/test-hardware.md)) |
| `DevTerm.CodingStandards.Tests` | Ported as `OoBDev.CodingStandards.Tests` |
| `containers/`, `scripts/`, `schemas/`, `examples/` | Migrated to `containers/devterm`, `scripts/devterm`, `schemas/devterm`, `examples/devterm`; wiring into the integration stack is a backlog item |
| dev-term's "consume dotex as NuGet" plan | Superseded by the merge |
| BinaryDataDecoders `docs/bug-reports` | Migrated as history under `docs/applications/binary-data-decoders/` |
| BinaryDataDecoders README badges, `RETIRED.md`, generated `docs/*`, report XSLT templates, GitHub workflows, `act` files, tool manifest | Rejected: retired GitHub, regenerated output, or already covered by `GitVersion.yml`, `OoBDev.Docs.Tests` and manual CI (`scripts/ci`) |

## Decisions

**Table 11 — Decisions**

| Id | Decision | Answer |
|----|----------|--------|
| D1 | Diff then delete the incoming copies | A (2026-11-05); done 2026-11-06 |
| D2 | Hardware: per device, the best-covered implementation is the base | A |
| D3 | One package per device | A |
| D4 | Fencing migrates, marked untested | A |
| D5 | ContractParser moves to `docs/design/ContractParser/` | A |
| D6 | Migrate PackMan, Xslt, Net.ServiceHost; drop IO.Controller.Cli and De5000 CLIs | A |
| D7 | Deferred harvest items go to the backlog as evaluate-later | A |
| D8 | ScoreMachine and HomeSeer: libraries and emulators only | A (2026-11-09) |
| D9 | Add Vigenère | A (2026-11-09) |
