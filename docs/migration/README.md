# Migration plan

**Last Updated:** 2026-11-10

One plan for what is still to bring into this framework from other repositories, organised by feature, not by source. Migrated and documented features are not listed: they live in their project readmes (`README.{Project}.md`), feature designs ([docs/design](../design/README.md)) and change documents ([docs/changes](../changes/README.md)). Open work is tracked in [BACKLOG.md](../../BACKLOG.md) and [TODO.md](../../TODO.md). The per-source notes were folded into this file on 2026-11-10 (`git log -- docs/migration` has them).

## Contents

- [Rules](#rules)
- [Sources](#sources)
- [Devices](#devices)
- [Transports, DevTerm and plugins](#transports-devterm-and-plugins)
- [BinaryDataDecoders leftovers](#binarydatadecoders-leftovers)
- [SharedFramework epics](#sharedframework-epics)
- [Harvest candidates](#harvest-candidates)
- [Rejected, with reasons](#rejected-with-reasons)
- [Decisions](#decisions)

### List of Tables

1. [Table 1 — Sources](#table-1)
2. [Table 2 — Device work left](#table-2)
3. [Table 3 — Transports, DevTerm and plugins](#table-3)
4. [Table 4 — BinaryDataDecoders leftovers](#table-4)
5. [Table 5 — SharedFramework epics](#table-5)
6. [Table 6 — Harvest candidates](#table-6)
7. [Table 7 — Rejections](#table-7)

## Rules

- Everything migrates except stubs with no implementation, obsolete platforms (Silverlight, UWP, Compact Framework) and code with no net10.0 equivalent (`CLAUDE.md`, Migration Scope). Phases are priority order, not selection.
- Best coverage (`owner-decisions`): compare every source and use the best-covered, tested implementation as the base; merge what the others add. A rejection carries its reason ([Table 7](#table-7)).
- Surroundings migrate too (guides, agent configuration, docs, scripts, plans). The dev-term and BinaryDataDecoders surroundings are migrated or rejected; what is still open is wiring `containers/devterm` into the integration stack (backlog).
- A layering change needs a confirmed plan first (rule `layering-and-migration-plans`). Copies of code inside this repository may be deleted once migrated; older repositories stay untouched (owner, 2026-11-09).

## Sources

<a id="table-1"></a>**Table 1 — Sources**

| Source | Location | State |
|--------|----------|-------|
| BinaryDataDecoders | `C:\repo\mine\binary-data-decoders` at `e590b420` (the old `Incoming/BinaryDecoders` subset copy was removed 2026-11-10) | Libraries, tools and eight devices migrated and checked; leftovers in [Table 4](#table-4) |
| dev-term | `C:\repo\oobdev\dev-term` at `8c66e74` | Base for devices; shells and transports in progress ([parity status](../design/DevTermParity/status.md)) |
| SharedFramework | (removed from `Incoming/`; history in `git log`) | Replaced by designs ([Table 5](#table-5)) |
| proving-grounds, learning repositories, `_storage`, notes | `C:\repo\mine`, `C:\repo\oobdev` | Read-only references ([Table 6](#table-6)) |

The former `Incoming/CHECKLIST.md` status table is folded in: dotnet-lib (95% identical to main, bug fix applied) and Framework (vector library already in main) were deleted, Oobtainium moved to proving-grounds, BotChat was extracted 2026-10-09, SharedFramework was replaced by designs, and BinaryDecoders, ContractParser and Tools are covered below.

Where two sources hold the same feature, dev-term is newer and tested, so it is the base. Migrated libraries were behaviour-checked by running the source tests against main (2026-11-03).

## Devices

Design: [DeviceDecoding](../design/DeviceDecoding/README.md) and its [migration plan](../design/DeviceDecoding/migration-plan.md) (conversion rules, naming, order of work). Busylight, K8055, Radex One, Zoom H4n, NMEA, DE-5000, SCPI, Z-Wave, Fencing and LanC are migrated; merges M1, M2 and M5 from BinaryDataDecoders are in. What is left:

<a id="table-2"></a>**Table 2 — Device work left**

| Item | Needs |
|------|-------|
| M3: Radex One serial number and version layout | A recorded reply from the device |
| M4: Radex One unit scaling (`/100` µSv/h) | A capture; stays a documented hypothesis |
| M2: K8055 digital input bit order (raw value kept) | A bench pass |
| M1: Busylight `programSequence` (the frame is sent) | Watching the light cycle |
| Zoom H4n handshake, NMEA BT-20 | A bench pass |
| DE-5000 Hardware test | The BLE radio fitted in the meter |

## Transports, DevTerm and plugins

dev-term merges into this repository entirely (owner, 2026-10-28): shells become applications under `src/Applications/DevTerm`, control libraries are generic under `src/Framework`, nothing is back-ported. Slices and status: [DevTermParity](../design/DevTermParity/status.md).

<a id="table-3"></a>**Table 3 — Transports, DevTerm and plugins**

| Feature | Target | Design |
|---------|--------|--------|
| Transports not yet in Framework (merge dev-term's RFC 2217 and LXI discovery into `OoBDev.System.IO.Tcp`; VXI-11; BLE backends) | `OoBDev.System.IO.*` | [Transports](../design/Transports/README.md) |
| Device manifests and `.ksy` frame decoding (phase 8) | `docs/design/UiDefinitions` | [UiDefinitions](../design/UiDefinitions/README.md) |
| Renderers beyond WPF (Avalonia, Tui, Web) | `OoBDev.UiDefinitions.*` | [UiDefinitions](../design/UiDefinitions/README.md) |
| Plugins and sandbox; the sandbox layering proposal awaits confirmation | `OoBDev.System.Sandbox*` | [Plugins](../design/Plugins/README.md), [PluginSandbox](../design/PluginSandbox/migration-plan.md) |
| dev-term future plans (UDP, rendering presenters, discovery and config editors, research items) | one backlog item each | `BACKLOG.md` section 1 |

## BinaryDataDecoders leftovers

<a id="table-4"></a>**Table 4 — BinaryDataDecoders leftovers**

| Item | State |
|------|-------|
| Net.ServiceHost CLI | Decided (D6, owner 2026-11-10): no standalone tool; DevTerm hosts the generic TCP services, see the [feature request](../features/request/2026-11-10-devterm-generic-service-listeners.md) |
| Windows.Forms `ValidTextBox` | Commented out in main; decision in the [WindowsForms plan](../design/WindowsForms/migration-plan.md) |
| `Incoming/BinaryDecoders` | Removed 2026-11-10 (the source repository is clean at `e590b420`). Its comparison, bug and issue reports were reviewed against `src` and removed; the open gaps are in [the feature request](../features/request/2026-11-10-binary-data-decoders-open-issues.md) |
| `Incoming/Tools` (De5000 BLE scan CLI) | Removed 2026-11-10: scanning is `BleDeviceScanner`, GATT enumeration is `IBleGattProfileExplorer`, decoding is `OoBDev.DerEE.De5000` (D6) |

## SharedFramework epics

Design first (owner, 2026-11-06): none is implemented until a consumer appears. Four documents per feature ([standard](../patterns-discovery/06-design-document-standard.md)); plan in [SharedFramework](../design/SharedFramework/migration-plan.md). Queues, storage, caching, identity, documents and the MailKit adapter already exist in main.

<a id="table-5"></a>**Table 5 — SharedFramework epics**

| Epic | Design | Open |
|------|--------|------|
| 2 Communications | [Communications](../design/Communications/README.md) | Code from the design |
| 6 Documents | [SharedFramework](../design/SharedFramework/README.md) | Code from the design |
| 7 Identity | [SharedFramework](../design/SharedFramework/README.md) | Claims, rights and sessions from the design |
| 10 Text templating | [SharedFramework](../design/SharedFramework/README.md) | Container adapter, database template source |

## Harvest candidates

Map: [harvest-candidates.md](../harvest-candidates.md). Nothing is copied without a comparison.

<a id="table-6"></a>**Table 6 — Harvest candidates**

| Source item | Decision |
|-------------|----------|
| ScoreMachine and HomeSeer | Libraries and emulators only (D8); applications rejected |
| hybridsearch, Oobtainium, BuildFirstOnce, markdownplantuml, TotpExample, HandyClasses, postsql-playground | Evaluate later (D7); postsql informs `OoBDev.DacFx` |
| Oobtainium (mocking and proxy framework, about 1,600 lines) | Not migrated; Moq and NSubstitute cover it. Reopen only with a fresh comparison against `proving-grounds/examples/OoBDev.Oobtainium` |
| `OoBDev.MessageBroker`, `Ssb.Modeling`, `SQL` | Deferred until a Service Broker feature exists |
| `event-sourcing`, `sync-mesh`, `fohjin-cqrs` | Read-only pattern references; ask before copying |
| notes: `sqldef-evaluation`, `jws-jwt-from-scratch` | Design inputs for the matching features |

## Rejected, with reasons

<a id="table-7"></a>**Table 7 — Rejections**

| Item | Reason |
|------|--------|
| Rigol stub | One file with no implementation; `OoBDev.Scpi` profiles cover SCPI instruments |
| EByteElectronicTechnology | Tests only, no source anywhere |
| UWP, Silverlight, Compact Framework, .NET Framework-only code | No net10.0 equivalent |
| BinaryDataDecoders `Nmea/Notes` (about 5,000 lines of generated WMI classes, a Wiimote P/Invoke sample) | Generated or scratch; HID access is HidSharp here |
| Busylight `Class1`, K8055 request and response structs, NMEA classes, Radex write-settings request, Zoom H4n request enum | Experiment code, or dev-term decodes the same offsets and codes |
| `IO.Controller.Cli`, `OoBDev.De5000.Ble.Cli` | Replaced by the DevTerm tools (D6); source bug reports 001, 002 and 004 describe code that is not carried over |
| `*.Samples` projects, `.rptproj`, `ReportList.yaml`, `build.bat` | Empty, sample-only or old build glue; revisit as examples only on request |
| `WhitedUS.Libs` compression copies, `_storage` ISO reader | Older ancestor, and smaller than main's |
| proving-grounds applications, `NeedsReviewed`, `_storage` and `oobdev/containers` applications | Applications, not libraries (compare `containers` when touching the test stack) |
| dev-term skills `work-docs-audit`, `bug-report`, hook `merge-gate.sh` | Covered by `tracking-hygiene`, the `work-backlog` audit, `BACKLOG.md`, change documents and the commit-time reminder |
| BinaryDataDecoders badges, `RETIRED.md`, generated `docs/*`, report XSLT templates, GitHub workflows, `act` files | Retired GitHub or regenerated output; covered by `GitVersion.yml`, `OoBDev.Docs.Tests` and manual CI (`scripts/ci`) |
| dev-term's "consume dotex as NuGet" plan | Superseded by the merge |

## Decisions

D1 to D9 were all answered with the recommendation (2026-11-05 and 2026-11-09): diff then delete the incoming copies (done), the best-covered implementation per device as base, one package per device, Fencing migrated untested, ContractParser kept as a specification under `docs/design/ContractParser/`, PackMan and Xslt migrated and the IO controller and De5000 CLIs dropped, deferred harvest items to the backlog, ScoreMachine and HomeSeer libraries only, Vigenère added.
