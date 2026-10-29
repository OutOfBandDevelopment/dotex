# BACKLOG - OoBDev (dotex) Framework

> **Full work list.** Every open, valid item lives here; [TODO.md](TODO.md) holds only what is being worked on now. Finished work lives in the change history: [docs/changes/](docs/changes/README.md).
> **Owner questions:** [OPEN-QUESTIONS.md](./OPEN-QUESTIONS.md) · **New to this project?** Read [CLAUDE.md](./CLAUDE.md) first.
> **Wish list (owner's own notes):** [docs/todo.md](docs/todo.md) · **Other repositories to harvest from:** [docs/harvest-candidates.md](docs/harvest-candidates.md)

**Last Updated:** 2026-10-28

GitHub is no longer the primary repository (local bare repo `origin`), so nothing here waits on GitHub Actions.

## 1. Test infrastructure

- [ ] Replace the legacy `.github/workflows/` with the local runner in `scripts/ci` (a script that starts the Docker stack and runs the Integration suite). Decided 2026-10-22; leave the workflows untouched until the runner is verified end to end (`scripts/ci/run-tests.ps1` has not been run end to end yet).
- [ ] Integration coverage gaps (Azure runs on emulators only: Azurite and the Service Bus emulator, no Azure licence): Azurite queues and blobs are covered (queue tests added 2026-10-22); Service Bus emulator still has no health check and few tests (not run this session); Moto is only used for SQS because no other AWS adapter exists (S3 tests wait for an S3 adapter).
- [ ] `DevLocal` tests kept on purpose: Ollama (hard-coded LAN host, model pull and delete), USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files), DacFx `BuildPackageTest` (hard-coded path). Revisit Ollama when the owner says Ollama testing can resume.
- [ ] Add a live-integration doc page per new cloud service (only Groq exists; its key was revoked 2026-10-09, so runs wait for the owner).
- Azure Monitor exporter: not built (decided 2026-10-22). Telemetry is verified with OpenTelemetry and the Aspire dashboard in the Docker test stack; OTLP reaches any collector.
- [ ] Goals: no flaky tests (10 consecutive runs pass), Integration suite under 5 minutes.

## 2. APIs and documents

- [ ] AsyncAPI follow-up: check the viewer in a browser; an `azure-storage-queue` contributor if wanted.
- [ ] CGI middleware (`src/Extensions/OoBDev.AspNetCore.Cgi`, [design](docs/design/CgiMiddleware/README.md); first cut built, 23 tests pass on Windows batch fixtures, including timeout, 503 cap, response limit, stderr flood and client abort). Shell fixtures run in the .NET SDK container (`scripts/ci/run-linux-tests.ps1`). The DevLocal benchmark is done (see the testing strategy). Left: run-as-another-user (decided: left as a TODO, deployers use the service account).
- [ ] AllMiniLmL6V2 design items the code lacks: keyed `IEmbeddingGenerator` registration through `EmbeddingGeneratorBuilder`, `AddValidatedOptions` for the embedding options (blocked by the Abstractions-only adapter rule, see TODO backlog).
- [ ] LlmCodeGen tests the design promises (`docs/design/LlmCodeGen/testing-strategy.md`): Also `<template>.response.json` provider metadata (needs a richer completion contract).
- [ ] Compare the remaining project `README.{Project}.md` files with their code beyond the identifier check (`reconcile-designs-and-code` skill). The identifier check of the Common, Framework and Extensions readmes was done 2026-10-22; ExternalServices readmes are not yet checked.
- [ ] CGI tests still missing against the design ([testing strategy](docs/design/CgiMiddleware/testing-strategy.md)): symlink out of the root (REQ-007; needs link privilege, Linux container).
- [ ] OData and GraphQL surfaces (rights middleware and the `QUERY` verb are done; decided 2026-10-22: OData now through `Microsoft.AspNetCore.OData` on the `IQueryable<T>` seam, GraphQL (HotChocolate) later on demand), none breaking the custom search syntax; roles map to rights in configuration (`OoBDev:Authorization:Roles:<role>`) through a claims transformation. See the [HTTP API](docs/patterns-discovery/03-practices-and-conventions/10-http-api-practices.md) and [authorization](docs/patterns-discovery/03-practices-and-conventions/11-authorization-practices.md) practices.

## 3. Code quality backlog

A clean rebuild (2026-10-22) has no analyzer warnings. Analyzers are wired from `src/Directory.Build.props` (`OoBDev.Analyzers` OOB0001 to OOB0003, NetAnalyzers Recommended, Meziantou); `Tools` and `Examples` are exempt from MA0004 and MA0048 in `src/.editorconfig`.

- [ ] (decided 2026-10-22: promote the rules that are already clean, and the `#warning` markers became TODO comments, intent in Q4) Rules still at `suggestion` in `src/.editorconfig` (flip each to warning after fixing its backlog): MA0026 (TODO comments, about 50, kept on purpose), MA0046 (3, event payloads that are not `EventArgs`), MA0051 (26, method length), MA0061 (3, the DEBUG-only required parameters), CA1710, CA1720 and CA1708 (domain names, kept on purpose) and the rest listed in the editorconfig.
- [ ] Remaining build warnings: `NU1902` MessagePack 2.5.302 (transitive via Aspire.Hosting 13.6.1 in `OoBDev.Example.AppHost`; wait for an Aspire update or pin), CS1030 `#warning` markers (Mailkit hosting not supported, two "is this needed" notes in queue tests).
- [ ] Remaining hand-built providers to replace with platform primitives, case by case (`TimeProvider` is done).

## 4. Migrations (blocked on the owner)

Scope rule: every BinaryDataDecoders feature is migrated; phases are priority order ([CLAUDE.md](CLAUDE.md#migration-scope)).

- [ ] **Incoming code recommendation plan** (BinaryDataDecoders, ContractParser, Tools, harvest candidates): written 2026-10-22 in [docs/migration/incoming-recommendation-plan.md](docs/migration/incoming-recommendation-plan.md), rejections carry their reasons; the migration below waits for the owner to answer D1 to D7.
- [ ] **BinaryDataDecoders** (~50,000 LOC). The source is `C:\repo\mine\binary-data-decoders` (owner, 2026-10-26); it replaces the earlier pull-in at `Incoming/BinaryDecoders`, which is superseded (removing it is the owner's call). Phases: 1 Foundation (endianness, utilities, `BinaryPrimitives`), 2 High-value (code analysis, expression calculator, archives, drawing, barcode), 3 Protocols (NMEA and others), 4 Specialized (8 hardware devices, classic crypto with warnings, Apple II, fencing, Windows Forms, UWP), 5 Tools and CLI (4 tools). Answer the decisions on the [decision sheet](docs/migration/binarydatadecoders-decision-sheet.md) (the owner answered Q9 on 2026-10-09 by asking for the sheet; the per-line answers are still open). Compare against the copies listed in [docs/harvest-candidates.md](docs/harvest-candidates.md) first: the `mine/binary-data-decoders` repo is newer (78 projects), and `dev-term` holds newer device code.
- [ ] **Device decoding from dev-term** (branch `dev/device-decoding`): the eight device packages are done and recorded in `docs/changes/devices-*-2026-10-28.md`. Remaining: the reusable UI framework (`UiDefinitions`, MVVM view models, WPF/TUI/Web renderers) and the dev-term shells rebuilt on the OoBDev libraries (phase 10); dev-term is merged entirely into this framework (owner, 2026-10-28): shells become `OoBDev.DevTerm.{Wpf|Tui|Web}` tools, controls become generic per-platform libraries under Framework, nothing is back-ported. Plan: [docs/migration/device-decoding-migration-plan.md](docs/migration/device-decoding-migration-plan.md); parity checks: [docs/migration/device-parity.md](docs/migration/device-parity.md) section 4.
- [ ] **Avalonia spike for the device UI** (task, owner, 2026-10-26; part of the device decoding work, blocks the final go-ahead for the cross-platform renderer in Q3). Preference: the most MVVM with real `ICommand` types where possible. Time-boxed spike on a throwaway branch: render sample `UiDefinition` panels (toggle, slider, choice, numeric, chart, form; fixtures with no device, transport or hardware reference, since the UI is separate from the devices) through the view-model layer in an Avalonia window, with every command a real `ICommand` class with injected dependencies (no lambdas, no code-behind). This is a rendering test: WSL with WSLg is enough for the manual Linux check (`DevLocal`); automated checks use Avalonia headless and a Linux container with a virtual display. Check: .NET 10 support and license, runs on Windows and Linux (Linux in a container per the testing rule) and macOS where reachable, headless rendering for tests, package size, how closely the view models map to Avalonia bindings, and what an Eto.Forms or GTK renderer would need by comparison. Output: a short findings note in `docs/migration/` with a recommendation; the owner decides, then Q3 is answered. Plan: phase 7 of [docs/migration/device-decoding-migration-plan.md](docs/migration/device-decoding-migration-plan.md).
- [ ] **ContractParser**: decided 2026-10-09 to keep as a specification; no work.
- [ ] **BulkLlm**: decided to consolidate. `OoBDev.LlmCodeGen.Cli` is built; the live Groq run remains (Ollama live run is an Integration test) ([design](docs/design/LlmCodeGen/README.md)).
- [ ] **SharedFramework** is design first (Epics 2, 6, 7, 10): four documents per feature (requirements, architecture, api-design, testing-strategy), 16 per epic. See [CLAUDE.md](CLAUDE.md#current-work-context).
- [ ] **Harvest candidates**: nothing decided; the map is in [docs/harvest-candidates.md](docs/harvest-candidates.md).

- [ ] Harvest candidates marked evaluate-later in the plan (decision D7): proving-grounds plugin-sandbox-poc, hybridsearch, Oobtainium, BuildFirstOnce, markdownplantuml, TotpExample, HandyClasses; Service Broker tools; postsql-playground (informs `OoBDev.DacFx`).
- [ ] Production-copy rehearsal test: restore a sanitized dump into the SQL Server container, apply the DacFx or EF migration, run the Integration suite (DevLocal first, Integration when stable).
- [ ] Performance gates (log over 500 ms, fail over 750 ms) for the search endpoints (the CGI middleware gate is done).
- [ ] Add an operations check (read-only sources, migration approval, rollback) to the `security-audit` skill.

## Documentation review (owner, 2026-10-28)

- [ ] **Review and clean up all documents.** The numbering of the documents (for example the numbered files and folders under `docs/`) does not look right to the owner and its origin is unclear. Review every document: fix or remove the numbering, align each with the rest of the framework (rules `docs-and-diagrams` and `work-tracking`, `docs/patterns-discovery/06-design-document-standard.md`), make sure the docs cover the features of dev-term and the other merged products, and check that they make sense. Use the `write-docs` skill and the `scripts/docs` validators; start with an inventory of the numbering scheme and where it came from.
- [ ] **Device decoding leftovers** (found 2026-10-28): Z-Wave product configuration table (`product-configs.json`, 2.2 MB) and an automatic ACK responder over a session observer; SCPI definite-length block reply framing (`#<n><length>` image dumps) and the generated control panel, reply tracker and auto-detect flow (DevTerm merge); DE-5000 Hardware test needs a BLE transport; physical pass for K8055 input bit mapping, Busylight program frames, Radex settings and reset, Zoom H4n handshake, NMEA BT-20 framing and a real SCPI instrument.
