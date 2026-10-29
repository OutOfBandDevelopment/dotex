# Incoming code: comparison and recommendation plan

**Created:** 2026-10-22 · **Status:** proposal for owner review; nothing here is migrated. Evidence is `grep` of type names in `src/` against the incoming sources (a name that is absent from `src/` is reported as "not in main"; a rename could hide a match, so rows marked *verify* need a second look before work starts).

Scope: everything waiting in `Incoming/` (BinaryDecoders, ContractParser, Tools), the newest source `C:\repo\mine\binary-data-decoders` (`e590b420`, 78 projects) and the reference projects in [harvest candidates](../harvest-candidates.md) (dev-term, proving-grounds, learning repos, notes). Cross-check of each feature against every harvest project: [incoming-harvest-crosscheck.md](incoming-harvest-crosscheck.md) (best source per feature). Earlier analysis this plan builds on: [feature mapping](binarydatadecoders-feature-mapping.md), [decision sheet](binarydatadecoders-decision-sheet.md), [migration plan](binarydatadecoders-migration-plan.md).

Rejecting a feature is allowed, but every rejection below carries its reason, and nothing is rejected silently. Each decision lists options with a recommendation; reply with the letter, or "approve all recommendations".

> **Superseded 2026-10-29** for BinaryDataDecoders by [binarydatadecoders-rereview-2026-10-29.md](binarydatadecoders-rereview-2026-10-29.md) (owner: the old document is out of date).

## Contents

Tables: 1 Already in main, 2 Not in main, 3 Harvest recommendations, 4 Proposed rejections, 5 Decisions.

1. [Already in main](#1-already-in-main)
2. [Not in main: BinaryDataDecoders](#2-not-in-main-binarydatadecoders)
3. [ContractParser and Tools](#3-contractparser-and-tools)
4. [Harvest candidates](#4-harvest-candidates)
5. [Rejections and reasons](#5-rejections-and-reasons)
6. [Proposed order](#6-proposed-order)
7. [Decisions for you](#7-decisions-for-you)

## 1. Already in main

**Table 1 — Already in main (no migration needed beyond a diff check)**

| Incoming project | Where in main | Remaining work |
|---|---|---|
| Apple2 | `OoBDev.System/Retro/Apple2` (Applesoft, DOS 3.3) | Diff against incoming; its notes file is `Apple2Notes.txt` so it no longer clashes with `Codecs/Notes.txt` in the package |
| ExpressionCalculator | `OoBDev.System/ExpressionCalculator` | Diff only |
| Archives (Tar, Zip) | `OoBDev.System/Archives` | Diff only |
| FileSystems (ISO 9660) | `OoBDev.System/IO/Iso9660` | Diff only |
| Cryptography (Caesar, Enigma, Lorenz, PlayFair) | `OoBDev.System/Cryptography` | Confirm the educational warnings are present |
| Net | `OoBDev.System/Net` (MAC address, protocols) | Diff only |
| ToolKit, ToolKit.Abstractions | `OoBDev.System` (BcdEx, ByteEx, Codecs, Collections, PathSegments) | Diff only |
| Windows.Forms (ValidTextBox) | decision 22: `OoBDev.Extensions.Windows.Forms` | Confirm it exists; not found by this check, *verify* |

Recommendation (**A**): run the `compare-incoming-codebase` diff for these once, record "identical" or the delta in `Incoming/CHECKLIST.md`, then delete the incoming copy. Option B: keep the copies until everything else is done (safer, but clutters the tree).

## 2. Not in main: BinaryDataDecoders

**Table 2 — Not in main, BinaryDataDecoders**

| Project | What it holds | Target in main | Phase | Parity with dev-term |
|---|---|---|---|---|
| CodeAnalysis | Roslyn semantic and syntax navigators (`CSharpNavigator`, `SemanticModelNavigatorFactory`) | `OoBDev.Extensions.CodeAnalysis` | 2 | none |
| CodeAnalysis.DacFx | `DacPacNavigator` over a dacpac | next to `OoBDev.DacFx` | 2 | none |
| CodeAnalysis.StructuredLog | `StructuredLogNavigator` (MSBuild binlog) | `OoBDev.Extensions.CodeAnalysis` | 2 | none |
| Drawing | Code39 barcode, JPEG segment reader, PNG pack, Z-order curve, DeepZoom tile helpers | `OoBDev.Extensions.Drawing.*` | 2 | none |
| Nmea | NMEA 0183 decoder (GGA, GSA, GSV, RMC) | `OoBDev.Extensions.Hardware.Nmea` | 3 | `Devices.Nmea` |
| Fencing | Favero and SG score machine decoders | `OoBDev.Extensions.Hardware.Fencing` | 4 | none; dev-term wrote it up and rejected it (no hardware) |
| Kuando.Busylight | Busylight USB commands | `OoBDev.Extensions.Hardware.Busylight` | 4 | `Devices.Busylight` |
| Quarta.RadexOne | Radex One geiger counter | `OoBDev.Extensions.Hardware.RadexOne` | 4 | `Devices.RadexOne` (newer, real-hardware findings) |
| Velleman.K8055 | USB I/O board | `OoBDev.Extensions.Hardware.K8055` | 4 | `Devices.K8055` |
| Zoom.H4n | Recorder remote | `OoBDev.Extensions.Hardware.ZoomH4n` | 4 | `Devices.ZoomH4n` |
| LanC | Camera control commands | `OoBDev.Extensions.Hardware.LanC` | 4 | none |
| PackMan.Cli | NuGet package manager CLI | `src/Tools` | 5 | none |
| Xslt.Cli | Sandboxed XSLT transform CLI | `src/Tools` | 5 | none |
| IO.Controller.Cli | Device console, Z-Stick factory | `src/Tools` | 5 | dev-term replaces it |
| Net.ServiceHost.Cli | Service host | `src/Tools` | 5 | none |
| Rigol | one stub file | none | n/a | `Devices.Scpi` supersedes |
| EByteElectronicTechnology | tests only, no source | none | n/a | proposal only |

Hardware devices: the code is byte-identical between the original repository and the incoming copy (device-parity note, 2026-10-21), so there is one implementation to bring over and a newer one in dev-term to compare.

## 3. ContractParser and Tools

- **ContractParser** (`Incoming/ContractParser/examples`): two ANTLR grammars (`ServiceContract.g4`, 66 lines; `_v2.g4`, 45 lines) and five sample contracts; no parser code. Decided 2026-10-09 to keep as a specification. Recommendation: move the folder to `docs/design/ContractParser/` as reference material so `Incoming/` empties out.
- **Tools / OoBDev.De5000.Ble.Cli**: 75-line `Program.cs` bridging a DE-5000 LCR meter over a serial BLE link; its own readme says it "needs improved". dev-term has `Devices.De5000`.

## 4. Harvest candidates

**Table 3 — Harvest recommendations**

| Source | Item | Recommendation | Why |
|---|---|---|---|
| dev-term | device modules (Radex, K8055, H4n, Busylight, Nmea, De5000, Scpi) | Use as the reference for the hardware packages: port the BinaryDataDecoders code first, then fold in dev-term's protocol fixes (Radex checksum, HID versus serial) | dev-term was checked on real hardware |
| proving-grounds | `examples/SharedFramework` | Compare before any SharedFramework design; do not copy | likely ancestor of dotex, 487 files |
| proving-grounds | `plugin-sandbox-poc`, `hybridsearch`, `OoBDev.Oobtainium`, `BuildFirstOnce`, `markdownplantuml`, `TotpExample`, `HandyClasses` (Code39, CsvWriter, IniFile) | Add to `BACKLOG.md` as "evaluate when the matching area is touched"; HandyClasses Code39 duplicates the Drawing item | one source each, no urgency |
| proving-grounds | `OoBDev.MessageBroker`, `Ssb.Modeling`, `SQL` | Defer; only relevant to a Service Broker feature | no dotex feature uses it |
| proving-grounds | `AccountingEngine`, `CarMaintenanceLog`, `SecondShooter`, `NeedsReviewed` | Reject for dotex | applications and unsorted code, not libraries |
| learning | `event-sourcing`, `sync-mesh`, `fohjin-cqrs` | Keep as read-only pattern references; ask before copying | unsanitized, publication undecided |
| learning | `postsql-playground`, `intel-8088-emu` | Defer; `postsql` only informs `OoBDev.DacFx` | out of scope today |
| `_storage`, `oobdev/containers` | applications and compositions | Reject as library sources; `containers` may overlap `containers/testing`, compare when touching the stack | applications, not libraries |
| notes | `cgi-middleware-kestrel` (done), `sqldef-evaluation`, `jws-jwt-from-scratch` | Keep as design inputs for the matching features | already tracked |

## 5. Rejections and reasons

**Table 4 — Proposed rejections**

| Item | Reason |
|---|---|
| Rigol stub | one file with no implementation; dev-term `Devices.Scpi` with JSON profiles covers SCPI |
| EByteElectronicTechnology | tests only, no source anywhere |
| UWP, Silverlight, Compact Framework, .NET Framework-only code | no net10.0 equivalent (decisions 23 to 25) |
| Fencing (option, see D4) | dev-term wrote it up and rejected it for lack of hardware; recommendation is still to migrate under the all-features rule, marked untested |
| IO.Controller.Cli (option, see D6) | needs `IO.*` projects that exist in main under other names; dev-term replaces the device console |
| proving-grounds applications, `NeedsReviewed`, `_storage` | not libraries |

## 6. Proposed order

1. Diff check of section 1 and delete the identical incoming copies.
2. Phase 2: CodeAnalysis (with DacFx and StructuredLog), then Drawing.
3. Phase 3: Nmea, as the first hardware-style package and the template for the rest.
4. Phase 4: Radex, K8055, H4n, Busylight, LanC, Fencing, each merged with the dev-term findings.
5. Phase 5: CLIs.

Every migration follows the usual rules (net10.0, nullable, README per project, Unit tests at 80%+ for Framework, separate packages for specialized features).

## 7. Decisions for you

**Table 5 — Decisions**

| # | Question | Options | Recommendation |
|---|---|---|---|
| D1 | Section 1 projects | **A** diff then delete; **B** keep copies | **A**: less clutter, git history keeps them |
| D2 | Hardware code source | **A** per device, the best-covered and tested implementation as the base (see the [cross-check](incoming-harvest-crosscheck.md)), merging the rest; **B** BinaryDataDecoders only; **C** share one core with dev-term | **A**: owner rule 2026-10-22, best feature coverage rather than first found; **C** needs dev-term owner work and is a later step |
| D8 | ScoreMachine and HomeSeer applications | see the cross-check | **A** harvest library and emulator code only |
| D9 | Vigenère from challenge-accepted | see the cross-check | **A** add it |
| D3 | Hardware package layout | **A** one package per device under `OoBDev.Extensions.Hardware.*`; **B** one combined package | **A**: users take only what they own |
| D4 | Fencing | **A** migrate, marked untested; **B** reject | **A**: the scope rule keeps niche features; **B** is reasonable if you will never use it |
| D5 | ContractParser | **A** move to `docs/design/ContractParser/`; **B** leave in `Incoming/` | **A** |
| D6 | CLI tools and De5000 | **A** migrate PackMan, Xslt, Net.ServiceHost; drop IO.Controller.Cli and De5000 (dev-term has both); **B** migrate all; **C** reject all | **A**: dev-term covers the device ones |
| D7 | Harvest items marked defer | **A** add to `BACKLOG.md` as evaluate-later; **B** drop | **A**: costs nothing |

Default until you answer: nothing migrates; only the BACKLOG additions in D7 are made.
