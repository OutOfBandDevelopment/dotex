# Migration plan

**Last Updated:** 2026-11-10

One plan for what is still to bring into this framework from other repositories, organised by feature, not by source. Migrated and documented features are not listed: they live in their project readmes (`README.{Project}.md`), feature designs ([docs/design](../design/README.md)) and change documents ([docs/changes](../changes/README.md)). Open work is tracked in [BACKLOG.md](../../BACKLOG.md) and [TODO.md](../../TODO.md). The per-source notes were folded into this file on 2026-11-10 (`git log -- docs/migration` has them).

## Contents

- [Rules](#rules)
- [Sources](#sources)
- [Devices](#devices)
- [Transports, DevTerm and plugins](#transports-devterm-and-plugins)
- [SharedFramework epics](#sharedframework-epics)
- [Harvest candidates](#harvest-candidates)
- [Rejected, with reasons](#rejected-with-reasons)
- [Decisions](#decisions)

### List of Tables

1. [Table 1 — Sources](#table-1)
2. [Table 2 — Device work left](#table-2)
3. [Table 3 — Transports, DevTerm and plugins](#table-3)
4. [Table 4 — SharedFramework epics](#table-4)
5. [Table 5 — Harvest candidates](#table-5)
6. [Table 6 — Rejections](#table-6)

## Rules

- Everything migrates except stubs with no implementation, obsolete platforms (Silverlight, UWP, Compact Framework) and code with no net10.0 equivalent (`CLAUDE.md`, Migration Scope). Phases are priority order, not selection.
- Best coverage (`owner-decisions`): compare every source and use the best-covered, tested implementation as the base; merge what the others add. A rejection carries its reason ([Table 6](#table-6)).
- Surroundings migrate too (guides, agent configuration, docs, scripts, plans). The dev-term and BinaryDataDecoders surroundings are migrated or rejected; what is still open is wiring `containers/devterm` into the integration stack (backlog).
- A layering change needs a confirmed plan first (rule `layering-and-migration-plans`). Copies of code inside this repository may be deleted once migrated; older repositories stay untouched (owner, 2026-11-09).

## Sources

<a id="table-1"></a>**Table 1 — Sources**

| Source | Location | State |
|--------|----------|-------|
| BinaryDataDecoders | `C:\repo\mine\binary-data-decoders` at `e590b420` (the old `Incoming/BinaryDecoders` subset copy was removed 2026-11-10) | Libraries, tools and eight devices migrated and checked; the generic TCP services continue as a DevTerm feature request ([backlog](../../BACKLOG.md)) |
| dev-term | `C:\repo\oobdev\dev-term` at `8c66e74` | Base for devices; shells and transports in progress ([parity status](../design/DevTermParity/status.md)) |
| SharedFramework | (removed from `Incoming/`; history in `git log`) | Replaced by designs ([Table 4](#table-4)) |
| proving-grounds, learning repositories, `_storage`, notes | `C:\repo\mine`, `C:\repo\oobdev` | Read-only references ([Table 5](#table-5)) |


Where two sources hold the same feature, dev-term is newer and tested, so it is the base. Migrated libraries were behaviour-checked by running the source tests against main (2026-11-03).

## Devices

Design: [DeviceDecoding](../design/DeviceDecoding/README.md) and its [migration plan](../design/DeviceDecoding/migration-plan.md) (conversion rules, naming, order of work). What is left:

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

## SharedFramework epics

Design first (owner, 2026-11-06): none is implemented until a consumer appears. Four documents per feature ([standard](../patterns-discovery/06-design-document-standard.md)); plan in [SharedFramework](../design/SharedFramework/migration-plan.md).

<a id="table-4"></a>**Table 4 — SharedFramework epics**

| Epic | Design | Open |
|------|--------|------|
| 2 Communications | [Communications](../design/Communications/README.md) | Code from the design |
| 6 Documents | [SharedFramework](../design/SharedFramework/README.md) | Code from the design |
| 7 Identity | [SharedFramework](../design/SharedFramework/README.md) | Claims, rights and sessions from the design |
| 10 Text templating | [SharedFramework](../design/SharedFramework/README.md) | Container adapter, database template source |

## Harvest candidates

Map: [harvest-candidates.md](../harvest-candidates.md). Nothing is copied without a comparison.

<a id="table-5"></a>**Table 5 — Harvest candidates**

| Source item | Decision |
|-------------|----------|
| ScoreMachine and HomeSeer | Libraries and emulators only (D8); applications rejected |
| hybridsearch, BuildFirstOnce, postsql-playground | Compared 2026-11-10: [recommendations](harvest-d7-comparison.md) await the owner (D7); postsql informs `OoBDev.DacFx` |
| Oobtainium (mocking and proxy framework, about 1,600 lines) | Not migrated; Moq and NSubstitute cover it. Reopen only with a fresh comparison against `proving-grounds/examples/OoBDev.Oobtainium` |
| `OoBDev.MessageBroker`, `Ssb.Modeling`, `SQL` | Deferred until a Service Broker feature exists |
| `event-sourcing`, `sync-mesh`, `fohjin-cqrs` | Read-only pattern references; extract knowledge only, see [Table 2](harvest-d7-comparison.md#table-2): a CQRS and event sourcing reference design, an HLC feature request. Ask before copying source |
| notes: `sqldef-evaluation`, `jws-jwt-from-scratch` | Design inputs for the matching features |

## Rejected, with reasons

<a id="table-6"></a>**Table 6 — Rejections**

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
