# Ser2NetWin migration plan

Status: **plan confirmed by the owner 2026-11-04 (option A: Framework bridge core, application CLI, the three packages added)** | Source: `C:\repo\mine\proving-grounds\tools\Ser2NetWin` (git, version 0.0.4, about 2,300 lines in `src`, 10 test classes) | Rule: `layering-and-migration-plans`

Ser2NetWin bridges Windows COM ports to TCP (raw or RFC 2217, optionally advertised over mDNS). It hosts the Aeotec Z-Stick for the Z-Wave Hardware tests and is a Windows-native stand-in for Linux `ser2net`.

## What the source holds

| Part | Contents |
|------|----------|
| `OoBDev.Ser2NetWin.Core` | config model, store and validator; `BridgeWorker` (serial to TCP pump); `Rfc2217Session`, `Rfc2217Codes`; `MdnsAdvertiser`; `SystemSerialLink` and the serial factory behind `ISerialLink`, `ISerialControl`, `ISerialPortFactory`; `BridgeHostedService` |
| `OoBDev.Ser2NetWin.Cli` | `ser2net-win.exe`: `run`, `config` (Spectre.Console editor), `list`, `ports`, `install`, `uninstall`, `start`, `stop`, `status`; `ServiceInstaller` (`sc.exe`); Windows Service hosting |
| `tests/OoBDev.Ser2NetWin.Tests` | MSTest: bridge worker, client connection, CLI, config store and validator, mDNS plan, RFC 2217 session and bridge; `HardwareConnectionTests` against a real COM port; fake serial devices in `TestKit/` |
| Surroundings | `README.md` (usage, settings, mDNS), `docs/bugs/single-client-silent-reject.md` (fixed in 0.0.4), `scripts/publish.ps1` (single-file self-contained publish), `Directory.Build.props`, solution file. No `.claude`, TODO, BACKLOG or plans folder |

The single-client bug in [the issue](../../issues/ser2netwin-single-client-silent-reject.md) is already fixed in 0.0.4 of the source (reason line written to the rejected client, keep-alive, write timeout, reconnect grace, `ReplaceExistingClient`). The migration brings the fix; no new code is needed for it.

## Plan table

| Project | Current location | Target location (recommended) | Namespace | Package or deployment unit | References that change |
|---------|------------------|-------------------------------|-----------|----------------------------|------------------------|
| Bridge core (config, worker, RFC 2217 server session, hosted service, mDNS advertiser) | `tools/Ser2NetWin/src/OoBDev.Ser2NetWin.Core` | `src/Framework/OoBDev.System.IO.Ports.Bridge` (generic name; Framework, no ASP.NET) | `OoBDev.System.IO.Ports.Bridge` | NuGet library | `Microsoft.Extensions.Hosting.Abstractions`, `System.IO.Ports` (central), `Makaretu.Dns.Multicast.New` added to `Directory.Packages.props` (see Q1); the mDNS part may split into `OoBDev.Net.Mdns` if other code advertises services |
| Command line, TUI editor, service installer | `tools/Ser2NetWin/src/OoBDev.Ser2NetWin.Cli` | `src/Applications/Ser2NetWin/OoBDev.Ser2NetWin.Cli` (assembly name `ser2net-win` kept) | `OoBDev.Ser2NetWin.Cli` | Single-file self-contained exe (`scripts/publish.ps1`) | project reference to the bridge library; `Spectre.Console` and `Microsoft.Extensions.Hosting.WindowsServices` added to central packages (Q1) |
| Tests | `tools/Ser2NetWin/tests/OoBDev.Ser2NetWin.Tests` | `src/Framework/OoBDev.System.IO.Ports.Bridge.Tests` (library tests) and `src/Applications/Ser2NetWin/OoBDev.Ser2NetWin.Cli.Tests` (CLI tests) | matching | test projects | tier categories per test (`Unit`, `Simulate`, `Hardware` for `HardwareConnectionTests`); fake serial devices stay in the library tests and are shared |
| Docs | `README.md`, `docs/bugs/...` | `docs/applications/ser2netwin/` (README, requirements, architecture, API/CLI design, testing strategy; bug report kept as history) and `README.System.IO.Ports.Bridge.md`, `README.Ser2NetWin.Cli.md` | - | - | links from `docs/how-tos/test-hardware.md` and the Z-Wave design |
| Scripts | `scripts/publish.ps1` | `scripts/ser2netwin/publish.ps1` | - | - | repo-relative paths |
| Solution | `Ser2NetWin.sln`, `Directory.Build.props` | dropped; entries in `OoBDev.slnx` | - | - | repository props apply |
| Transient map | - | `docs/migration/ser2netwin-map.md`, deleted when the move is complete | - | - | - |

## Layer consequences

- The bridge core is reusable and has no application names once renamed, so it is Framework (rule: reusable means Framework). It needs only the Hosting abstractions, so the Abstractions-only reference rule is met except for the mDNS and serial packages; the question asks whether those go behind a small interface so the core keeps to Abstractions.
- The CLI is an application: nothing references it.
- DevTerm can later use the RFC 2217 session code for its own RFC 2217 transport tests instead of a private fake (backlog, not part of the move).

## Order

1. Owner confirmed the layering and the packages (2026-11-04).
2. Import docs whole under `docs/applications/ser2netwin/` with the transient map.
3. Move the library and its tests, rename namespaces, register in `OoBDev.slnx`, fix analyzer warnings, run the tests.
4. Move the CLI, its tests and the publish script; verify `ser2net-win list` and `ports`.
5. Generate the CLI usage guide with the tool-guide scripts (`docs/design/GeneratedGuides/`).
6. Change document, remove the BACKLOG item and the issue file (its final detail moves into the design spec).

Not decided here: whether the Hardware test against a real COM port runs on this machine (the bench decides; the owner deferred hardware passes).
