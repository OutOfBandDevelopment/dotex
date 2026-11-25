# Harvest candidates: other projects dotex can draw from

**Created:** 2026-10-22 · **Status:** reference map, not a plan; maintained by Claude (owner handed it over 2026-10-22). Only what is still worth taking is listed; what dotex already has was removed. Use it to find reference projects when working on a feature, and update a row when you compare or take code from it.


**Surroundings rule (owner, 2026-10-30, applied 2026-11-04):** these repositories are read-only references. When a project is actually harvested, review its guide, `.claude`, docs, scripts and plans first and record the result in `docs/migration/{project}-surroundings-review-{date}.md`.
The owner keeps several other repositories and a notes repository. This page says what each one is, roughly what it does and where it lives locally, so work on dotex can take what it wants. Descriptions come from the owner's notes repository (`C:\repo\_notes\shared`, index in `projects/repository-index.md`, `projects/proving-grounds-catalog.md`, `projects/device-parity.md`), read from file lists, READMEs and a few sources. Code was not compiled, run or diffed against dotex. A name match means "probably related, not compared". Check the code before relying on any line below.

Related: [TODO](../TODO.md), [migration plan](migration/README.md).

## 1. Libraries and tools to draw from

All under `C:\repo\mine\proving-grounds` (origin https://github.com/mwwhited/proving-grounds.git) unless stated. Only what dotex does not already have is listed; migrated, covered and rejected items were removed (see [the migration plan](migration/README.md) and [the comparison](migration/harvest-d7-comparison.md)).

| Folder | What it does | Worth taking |
|---|---|---|
| `examples/BuildFirstOnce` | `Directory.Build.targets` tasks `DeepClean` and `CleanPaths` | The deep-clean targets for the 112-project solution; the "build a tool first" trick only as a documented technique |
| `examples/hybridsearch` | Hybrid search spike | The Azure and Neo4j variants as optional later adapters; the rest is covered by `OoBDev.Search` |
| `examples/OsciRenderClient` | Client for osci-render (TCP port 51622): frame, object and stroke model, plus fractal and curve generators | Not taken as a client (owner, 2026-11-25: a one-off demo; the HP-GL and SVG to stereo WAV sink in [the request](features/request/2026-11-10-hpgl-vector-display-sink.md) replaces its purpose). Its Hilbert, Koch and fractal generators may become `VectorScene` sources when that sink is built |
| `examples/AccountingEngine` | Billing, amortization, clearing and dunning engines behind interfaces (28 files) | A domain reference design; compare before any extraction |
| `design-patterns/BehaviorBlackboard` | Blackboard pattern sample (8 files) | A worked example for the patterns docs |
| `examples/OoBDev.MessageBroker`, `examples/Ssb.Modeling`, `examples/SQL` | SQL Server Service Broker tooling | Deferred until a Service Broker feature exists ([design](design/SqlServiceBroker/README.md)) |
| `examples/OoBDev.ScoreMachine` | Fencing scoring system (Favero, LANC relay, HID scanner, H4n CLI, web) | Libraries and emulators only (D8); its Kestrel host is one more place the CGI middleware could be tried |
| `examples/CarMaintenanceLog`, `examples/SecondShooter` | Applications | Reference applications; extract only when a feature needs the same data-access or domain code |
| `examples/NeedsReviewed` | 307 unsorted C# files | Triage once; unknown content |

Removed because dotex already has or rejects them: SharedFramework (replaced by designs), Oobtainium (Moq and NSubstitute cover it), plugin-sandbox-poc (`docs/design/PluginSandbox` and the spike), the Unix-domain socket samples (.NET has `UnixDomainSocketEndPoint`), UnmanagedExports (`UnmanagedCallersOnly` and Native AOT), PostMessageProxy (old ASP.NET MVC), the BulkLlm and ImageConverter tools (`OoBDev.LlmCodeGen.Cli`), and binary-data-decoders and dev-term (migrated).

## 2. Pattern references (read-only clones from 2026-10-21)

Under `C:\repo\learning` (bare origins in `g:/learning/`):

| Repo | What it is | Worth taking |
|---|---|---|
| `event-sourcing` (`191964e`) | Open event-sourcing store: design package plus implementation | The write and read side design, for the CQRS and event sourcing reference design |
| `fohjin-cqrs` (`5585623`) | CQRS and event-sourcing reference bank app | The pattern explanations, as the teaching baseline |
| `sync-mesh` (`06270e3`) | Distributed event-sourced recording and sync mesh | The hybrid logical clock and two ADRs |
| `postsql-playground` (`f414afa`) | PgProj desired-state PostgreSQL deployment (`.pgpkg`) | Design input for `OoBDev.DacFx` and [DesiredStateDatabase](design/DesiredStateDatabase/README.md) |
| `intel-8088-emu` (`3e38788`) | 8086/8088 emulator core in C# (.NET 8) | Reference only |

The owner has not decided to publish the event-sourcing work; it needs sanitizing first. Ask before copying. The extraction plan for the first three is in [the comparison](migration/harvest-d7-comparison.md#table-2).

## 3. Applications

| Where | What it is | Worth taking |
|---|---|---|
| `C:\repo\_storage` (origin `g:/_storage.git`) | .NET 10 and Vue system that inventories, deduplicates, classifies and converts documents and programs on a NAS share | An application; extract reusable pieces only when a feature needs them |
| `C:\repo\oobdev\containers` | Central Docker services and compositions (34 services) | Compare with `containers/testing` for services the test stack lacks |

## 4. Design material in the notes repository

In the notes repository (projects folder), only what is still an input:

- `dotex-consolidation/` : the consolidation plan and candidate list.
- `sqldef-evaluation/` : bake-off plan and design spec for a dacpac-parity desired-state schema tool (input for [DesiredStateDatabase](design/DesiredStateDatabase/README.md)).
- `synchronized-telemetry-streaming-research/` : RFC and standards research with code examples.

---

*Last updated: 2026-11-10*
