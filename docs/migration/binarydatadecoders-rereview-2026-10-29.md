# BinaryDataDecoders — Re-review against current main

**Created:** 2026-10-29 · **Status:** replaces the BinaryDataDecoders parts of [incoming-recommendation-plan.md](incoming-recommendation-plan.md), the [decision sheet](binarydatadecoders-decision-sheet.md) and the project tables in the [migration plan](binarydatadecoders-migration-plan.md) · **Requested by:** owner, 2026-10-29 (answer to Q1: the old document is out of date)

Source: `C:\repo\mine\binary-data-decoders` at `e590b420` (78 project folders, the newest copy; `RETIRED.md` says it moved here). Main: `src/` of this repository on `dev/example-app-fixes`. Method: a script compared every public or internal type name, then every `.cs` file by name (with namespaces and `using` lines stripped and whitespace removed), then the size of the changed files. A type or file counted as present when main has it under any project. A rename inside a file would hide a match, so the table lists what each result means and the checks that remain.

## Contents

- [What changed since the first plan](#what-changed-since-the-first-plan)
- [Already in main](#already-in-main)
- [Not in main](#not-in-main)
- [Not migrated by decision](#not-migrated-by-decision)
- [Order of work](#order-of-work)
- [Checks that remain](#checks-that-remain)

### List of Tables

1. [Table 1 — Already in main](#table-1)
2. [Table 2 — Not in main](#table-2)
3. [Table 3 — Not migrated by decision](#table-3)
4. [Table 4 — Order of work](#table-4)

## What changed since the first plan

- The eight device packages (Busylight, Radex One, K8055, Zoom H4n, DE-5000, NMEA, SCPI, Z-Wave) are in main, built from dev-term and checked against this source ([device-parity.md](device-parity.md)). They were phase 4 of the old plan and are done; only the leftovers in `BACKLOG.md` remain.
- HTML templating, Markdown, YAML and JSON helpers, the DacFx project, the Windows Forms `ValidTextBox` (`OoBDev.System/Windows/Forms`), the expression calculator, archives, ISO 9660, cryptography, Apple II and the network helpers are in main, each checked by file name only (see the checks that remain).
- The source itself has not changed since the first plan (same commit), so the earlier decision questions about it (14 decisions) mostly collapse into rules the owner has already set: everything migrates except stubs and obsolete platforms, breaking changes are fine, adapters reference abstractions only.

## Already in main

<a id="table-1"></a>**Table 1 — Already in main**

| Source project | Where in main | Evidence | What is left |
|----------------|---------------|----------|--------------|
| Apple2 (+ Tests) | `OoBDev.System/Retro/Apple2` | All 11 files present, 2 identical, 9 modernised | Nothing |
| Archives, FileSystems | `OoBDev.System/Archives`, `IO/Iso9660` | 14 of 15 and 4 of 4 files present, changed files are not smaller | `SECURITY_ATTRIBUTES` (a P/Invoke struct) is not carried; confirm it is unused |
| Cryptography | `OoBDev.System/Cryptography` | 9 of 9 files | Educational warnings (checked when the project was built) |
| ExpressionCalculator | `OoBDev.System/ExpressionCalculator` | 34 of 34 files | Nothing |
| IO.Abstractions, IO.Ports, IO.Pipelines, IO.UsbHids | `OoBDev.IO.*` and `OoBDev.System.IO.Pipelines` | 46 of 47 files present | Nothing; the source bug reports `BUG-001..004` are about the IO Controller CLI (Table 3) |
| Net, Net.ServiceHost.Cli | `OoBDev.System/Net` | 24 of 24 files | Service host CLI: see Table 3 |
| Templating.*, Text.Json, Text.Markdown, Yaml | `OoBDev.System.Text.*`, `OoBDev.Extensions` | All files present | Nothing |
| ToolKit, ToolKit.Abstractions | `OoBDev.System`, `OoBDev.System.Abstractions` | 115 of 123 and 16 of 16 files | The eight missing files are the numerics in Table 2 |
| TestUtilities | `OoBDev.TestUtilities` | 12 of 12 | Nothing |
| Windows.Forms | `OoBDev.System/Windows/Forms` | `ValidTextBox` present | The decision to move it to a separate package is still open in the backlog |
| Quarta.RadexOne, Velleman.K8055, Zoom.H4n, Kuando.Busylight, Nmea | `src/HardwareDevices`, `src/Extensions/OoBDev.Nmea` | Different type names (rebuilt from dev-term), covered by [device-parity.md](device-parity.md) | The NMEA `Notes` folder: `Degree`, `Bearing`, `GeneralHeading` helpers and the WMI and HID import notes (Table 2) |

## Not in main

<a id="table-2"></a>**Table 2 — Not in main**

| Item | Source size | Contents | Proposed home | Dependencies |
|------|------------:|----------|---------------|--------------|
| Numerics | 393 LOC, 7 test classes (281 LOC) | `ByteNumber`, `EngineeringNumber`, `ScientificNumber`, `FormattableNumber`, `INumeric`, three notation formatters | `OoBDev.System` (`Numerics`) with tests in `OoBDev.System.Tests` | none |
| Code analysis | 875 LOC, tests 360 LOC | `CSharpNavigator`, `VisualBasicNavigator`, semantic and syntax navigator factories (XPath over Roslyn), `DacPacNavigator`, `StructuredLogNavigator` | `OoBDev.Extensions.CodeAnalysis` (Roslyn, MSBuild structured log) | Roslyn, `Microsoft.Build.Logging.StructuredLogger`, `OoBDev.DacFx` |
| Drawing | 704 LOC, tests 157 LOC | `Code39`, `JpegSegmentReader`, `PngPack`, `ZOrderCurveEx`, `MultiScaleTileEx` (Deep Zoom tiles), `GraphicsEx` | `OoBDev.Extensions.Drawing.*` (separately packaged, as the framework guide says) | `System.Drawing.Common` for `GraphicsEx` only |
| Fencing score machines | 451 LOC, tests 775 LOC | Favero and SG state decoders, `ScoreMachineState`, cards and lights | `OoBDev.Extensions.Hardware.Fencing` with a decoder over the existing IO abstractions | none; no device available, so Unit tests only |
| LanC | 308 LOC | Camera and VTR control command tables | `OoBDev.Extensions.Hardware.LanC` | none |
| PackMan CLI | 621 LOC | NuGet package manager command line | `src/Tools/OoBDev.PackMan.Cli` | `NuGet.*` |
| Xslt CLI | 230 LOC | Sandboxed XSLT transform command line, `SandboxedTransformer` | `src/Tools/OoBDev.Xslt.Cli` (the sandboxed transformer is reusable) | none |
| NMEA helpers | Notes folder | `Degree`, `Bearing`, `GeneralHeading` value types | `OoBDev.Nmea` or `OoBDev.System` | none |

## Not migrated by decision

<a id="table-3"></a>**Table 3 — Not migrated by decision**

| Item | Reason |
|------|--------|
| Rigol (one stub file, `Rigol.Tests` with one test) | Zero implementation (rule: stubs are deleted); `OoBDev.Scpi` covers oscilloscopes and generators through profiles |
| EByteElectronicTechnology | Tests only, no source |
| IO.Controller.Cli (`DeviceConsole`, `PortProvider`, `ZStickFactory`) | Replaced by the DevTerm tools (owner, 2026-10-28: dev-term merges entirely); the source's four bug reports describe code that is not carried over |
| Net.ServiceHost.Cli | One 36-line host; main has the worker recipe and hosting packages |
| FSharp.Samples, VisualBasic.Samples, SqlServer.Samples, WinForms.Samples | Empty or sample-only; revisit as `examples` only if the owner asks (they have no tests and no stated use) |
| `.rptproj`, `ReportList.yaml`, `build.bat` | Old reporting and build glue |

## Order of work

<a id="table-4"></a>**Table 4 — Order of work**

| Order | Item | Why this order |
|------:|------|----------------|
| 1 | Numerics | Smallest, no dependencies, has tests to port; the foundation phase |
| 2 | NMEA helpers | Small value types; check `OoBDev.Nmea` first |
| 3 | Drawing | Self-contained codecs with tests |
| 4 | Code analysis | Roslyn dependency, needs the project and packaging decision |
| 5 | Fencing, LanC | Protocol tables and decoders with Unit tests only |
| 6 | PackMan and Xslt CLIs | Tools after the libraries they would use |

Each item follows the `compare-incoming-codebase` skill: design note, port with tests (80%+ in Framework), project readme, `complete-work` tracking.

## Checks that remain

- The name and file comparison does not prove behavioural equality. Before an item in Table 1 is called finished, run the source's tests against main's types where the names match (a worthwhile one-off for `ExpressionCalculator`, `Archives` and `Net`).
- Test counts (2026-10-29, test attributes): the source has 119 across 25 test projects (ToolKit 44, ExpressionCalculator 31, the rest 6 or fewer each, the Nmea tests 0); main has 755 in `OoBDev.System.Tests` (the ToolKit, ExpressionCalculator, archives, cryptography, Apple II, templating and text packages), 72 in `OoBDev.Nmea.Tests`, and a test project per ported library, so no project has fewer tests than its source. Behavioural check, done for two projects on 2026-10-29: the six ExpressionCalculator test files match main's by test and data row (only `DataTestMethod` became `TestMethod` and null-name asserts became expression bodies), and all 95 Numerics data rows of the source ran unchanged against main's types and passed (main keeps 60 rows; the other cases are covered by the rewritten tests). A test method name check of all 24 other source test projects against `src/` finds every name except nine, all in ports that were rebuilt and renamed on purpose: `TestXPath`, `MethodInput`, `MethodVoid` (CodeAnalysis), `PngPackTest`, `TestTileCreate` (Drawing), `TestDataExtractor` (Fencing), `Simple16Test` (IO), `BuidlRequestTest` (Busylight) and `DecoderSelector` (Radex One), besides the three Numerics names that ran above. Not run against main: the source tests of those nine, which need their types remapped; each port already carries its own tests.
- `Incoming/BinaryDecoders` is superseded by the source above; deleting it is the owner's call (`BACKLOG.md`).
