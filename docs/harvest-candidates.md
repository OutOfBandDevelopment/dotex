# Harvest candidates: other projects dotex can draw from

**Created:** 2026-10-22 · **Status:** a map, not a plan. Nothing here has been migrated or decided.

The owner keeps several other repositories and a notes repository. This page says what each one is, roughly what it does and where it lives locally, so work on dotex can take what it wants. Descriptions come from the owner's notes repository (`C:\repo\_notes\shared`, index in `projects/repository-index.md`, `projects/proving-grounds-catalog.md`, `projects/device-parity.md`), read from file lists, READMEs and a few sources. Code was not compiled, run or diffed against dotex. A name match means "probably related, not compared". Check the code before relying on any line below.

Related: [TODO](../TODO.md), [dev-term pointer in docs/todo.md](todo.md), `TODO-migrations-binarydatadecoders.md`.

## 1. Protocol and device libraries

| Where | What it is | Harvest idea |
|---|---|---|
| `C:\repo\mine\binary-data-decoders` (origin `g:/mwwhited/binary-data-decoders.git`) | 78 .NET projects: encoding/decoding, IO (serial, HID, pipelines), archives, Apple II, Fencing, Radex One, K8055, Zoom H4n, Rigol, LanC. Marked retired 2026-01-10 but committed to on 2026-09-26 (`e590b420`). | The source for `Incoming/BinaryDecoders`, which holds a 41-project renamed subset. Decide which copy is canonical before migrating. |
| `C:\repo\oobdev\dev-term` (origin `g:/oobdev/dev-term.git`, upstream https://github.com/OutOfBandDevelopment/dev-term, `899bb15`, 2026-10-14) | Modular device terminal. Projects: `Transports.*` (Serial, Tcp, Hid, Usbtmc, Vxi11, Ble, Rfc2217, Mqtt, Loopback), `Devices.*` (RadexOne, Scpi, K8055, ZoomH4n, Busylight, De5000, Nmea), plus Core, Configuration, Logging, Observability, Schemas, UI hosts (Console, Web, Wpf) and plugins. Does not reference dotex today. | Owner intends to move protocol handlers and framework-level code into dotex and rebuild dev-term on top of it. Transports probably overlap `OoBDev.System.IO.Ports`, `UsbHids` and `IO.Pipelines` (not compared). Device-profile docs and bench notes (2230 at 9600 baud with write pacing, TDS2024) are in its `docs/`. |
| Notes: `projects/device-parity.md` | Table comparing the same device across binary-data-decoders, the dotex copy and dev-term. | Starting point for choosing one implementation per device. |

## 2. Libraries and tools already built elsewhere

All under `C:\repo\mine\proving-grounds` (origin https://github.com/mwwhited/proving-grounds.git, catalogued at `caba03a`) unless stated. `examples/` and `tools/` folders:

| Folder | What it does |
|---|---|
| `examples/SharedFramework` | Shared C# framework (487 C# files) with a migration quick-reference. Likely an ancestor of dotex; compare before copying anything. |
| `examples/OoBDev.Oobtainium` | Mocking framework: dynamic interface proxies with call recording (`DispatchProxy`). Upstream repo `OutOfBandDevelopment/oobtainium`. |
| `examples/BuildFirstOnce` | `Directory.Build.props` ProjectReference trick that builds a tool before the projects that use it. Upstream `OutOfBandDevelopment/BuildFirstOnce`. |
| `examples/OoBDev.MessageBroker`, `examples/Ssb.Modeling`, `examples/SQL` | SQL Server Service Broker tooling: CLI, DB project, EF helpers, WPF designer and flow modelling. |
| `examples/hybridsearch` | Hybrid search spike (155 files). Compare `OoBDev.Search` and the embedders. |
| `examples/plugin-sandbox-poc` | Plugin sandbox proof of concept (AppContainer on Windows, bubblewrap/seccomp on Linux, signed packages, escape tests). Last commit 2026-10-08. |
| `examples/HandyClasses` | Code39, CsvWriter, IniFile (MIT, 2016). |
| `examples/TotpExample` | `WhitedUS.Totp` time-based one-time password example. |
| `examples/WindowsUnixSocket`, `examples/OobDev.Tools` | Unix-domain socket client/host and AfUnix tools. |
| `examples/UnmanagedExports` | Tests for exporting and importing unmanaged DLL entry points from .NET. |
| `examples/PostMessageProxy` | ASP.NET proxy for window `postMessage`. |
| `examples/markdownplantuml` | Markdig extension that renders PlantUML. |
| `examples/OsciRenderClient` | Client for osci-render (TCP port 51622). |
| `examples/OoBDev.ScoreMachine` | Fencing scoring system (Favero, LANC relay, HID scanner, H4n CLI, web). Its Kestrel host is one more place a CGI middleware could be tried. |
| `examples/AccountingEngine`, `examples/CarMaintenanceLog`, `examples/SecondShooter` | Applications; likely sources of reusable data-access or domain code, not libraries. |
| `design-patterns/BehaviorBlackboard` | Small blackboard pattern sample. |
| `tools/OobDev.BulkLlm.*`, `tools/OoBDev.ImageConverter.Cli` | Bulk LLM and image conversion CLIs. Already consolidated into `OoBDev.LlmCodeGen.Cli` per `TODO.md`. |
| `examples/NeedsReviewed` | 307 unsorted C# files. Unknown content. |

Catalogue with file counts and dates: notes `projects/proving-grounds-catalog.md`.

## 3. Pattern references (read-only clones from 2026-10-21)

Under `C:\repo\learning` (bare origins in `g:/learning/`):

| Repo | What it is |
|---|---|
| `event-sourcing` (`191964e`) | Open event-sourcing store: design package plus implementation. |
| `sync-mesh` (`06270e3`) | Distributed event-sourced recording and sync mesh. |
| `fohjin-cqrs` (`5585623`) | Fohjin.DDD CQRS and event-sourcing reference bank app. |
| `postsql-playground` (`f414afa`) | PgProj desired-state PostgreSQL deployment (`.pgpkg`). Relevant to `OoBDev.DacFx` and the notes `sqldef-evaluation` project. |
| `intel-8088-emu` (`3e38788`) | 8086/8088 emulator core in C# (.NET 8). |

The owner has not decided to publish the event-sourcing work; it needs sanitizing first. Ask before copying.

## 4. Applications

| Where | What it is |
|---|---|
| `C:\repo\_storage` (origin `g:/_storage.git`) | .NET 10 and Vue system that inventories, deduplicates, classifies and converts documents and programs on a NAS share. An application, not a library; reusable pieces would need extracting. |
| `C:\repo\oobdev\containers` | Central Docker services and compositions (34 services). dotex's own `containers/` may overlap. |

## 5. Design material in the notes repository

In `C:\repo\_notes\shared\projects\`:

- `cgi-middleware-kestrel/` : design for the CGI middleware requested in [todo.md](todo.md).
- `dotex-consolidation/` : the consolidation plan and candidate list.
- `sqldef-evaluation/` : bake-off plan and design spec for a dacpac-parity desired-state schema tool.
- `synchronized-telemetry-streaming-research/` : RFC and standards research with code examples.
- `plugin-sandbox-project/`, `hybrid-search-engine/`, `jws-jwt-from-scratch/`, `sql-service-broker-tools/` : design notes for the matching items above.

---

*Last updated: 2026-10-22*
