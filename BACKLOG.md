# BACKLOG - OoBDev (dotex) Framework

> **Full work list.** Every open, valid item lives here; [TODO.md](TODO.md) holds only what is being worked on now. Finished work lives in the change history: [docs/changes/](docs/changes/README.md).
> **Owner questions:** [OPEN-QUESTIONS.md](./OPEN-QUESTIONS.md) · **New to this project?** Read [CLAUDE.md](./CLAUDE.md) first.
> **Wish list (owner's own notes):** [docs/todo.md](docs/todo.md) · **Other repositories to harvest from:** [docs/harvest-candidates.md](docs/harvest-candidates.md)

**Last Updated:** 2026-10-22 (consolidated from the former `TODO-*.md` files; the dropped items and why are in [docs/changes/todo-consolidation-2026-10-22.md](docs/changes/todo-consolidation-2026-10-22.md))

GitHub is no longer the primary repository (local bare repo `origin`), so nothing here waits on GitHub Actions.

## 1. Test infrastructure

- [ ] Replace the legacy `.github/workflows/` with the local runner in `scripts/ci` (a script that starts the Docker stack and runs the Integration suite). Decided 2026-10-22; leave the workflows untouched until the runner is verified end to end (`scripts/ci/run-tests.ps1` has not been run end to end yet).
- [ ] Integration coverage gaps (Azure runs on emulators only: Azurite and the Service Bus emulator, no Azure licence): Azurite queues and blobs are covered (queue tests added 2026-10-22); Service Bus emulator still has no health check and few tests (not run this session); Moto is only used for SQS because no other AWS adapter exists (S3 tests wait for an S3 adapter).
- [ ] `DevLocal` tests kept on purpose: Ollama (hard-coded LAN host, model pull and delete), USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files), DacFx `BuildPackageTest` (hard-coded path). Revisit Ollama when the owner says Ollama testing can resume.
- [ ] Add a live-integration doc page per new cloud service (only Groq exists; its key was revoked 2026-10-09, so runs wait for the owner).
- Azure Monitor exporter: not built (decided 2026-10-22). Telemetry is verified with OpenTelemetry and the Aspire dashboard in the Docker test stack; OTLP reaches any collector.
- [ ] Goals: no flaky tests (10 consecutive runs pass), Integration suite under 5 minutes.
- [ ] New test project `OoBDev.DacFx.Tests`: a SQLCLR test project covering all SQLCLR features (from a module note).

## 2. APIs and documents

- [ ] AsyncAPI follow-up: describe send-only queues by type (senders are open generics, so only configured queues are listed); check the viewer in a browser; an `azure-storage-queue` contributor if wanted.
- [ ] CGI middleware (`src/Extensions/OoBDev.AspNetCore.Cgi`, [design](docs/design/CgiMiddleware/README.md); first cut built, 18 tests pass on Windows batch fixtures; design docs not validated: no Python here, see the docs validation item). Left: shell fixtures in a Linux container (Docker, or Aspire into Docker), tests for timeout, 503 cap, response limit, stderr flood and client abort, the DevLocal benchmark, run-as-another-user (decided: left as a TODO, deployers use the service account).
- [ ] Docs validation as a .NET test (replaces `scripts/docs/validate-docs.py`: PlantUML rendering through the `plantuml/plantuml-server` container, relative links, caption numbering). Decided 2026-10-22: `src/Tools/OoBDev.Docs.Tests`, Integration category with the container, link and caption checks also as Unit tests.
- [ ] HTTP querying (OData over `QUERY` with `POST` fallback, GraphQL) and the rights middleware: build order decided 2026-10-22: the rights middleware first (roles to `app__application_right` claims, one check on every API surface), then `QUERY`/OData with `POST` fallback, then GraphQL, none breaking the custom search syntax; roles map to rights in configuration (`OoBDev:Authorization:Roles:<role>`) through a claims transformation. See the [HTTP API](docs/patterns-discovery/03-practices-and-conventions/10-http-api-practices.md) and [authorization](docs/patterns-discovery/03-practices-and-conventions/11-authorization-practices.md) practices.

## 3. Code quality backlog

A clean rebuild (2026-10-22) has no analyzer warnings. Analyzers are wired from `src/Directory.Build.props` (`OoBDev.Analyzers` OOB0001 to OOB0003, NetAnalyzers Recommended, Meziantou); `Tools` and `Examples` are exempt from MA0004 and MA0048 in `src/.editorconfig`.

- [ ] (decided 2026-10-22: promote the rules that are already clean, and fix or remove the `#warning` markers) Rules still at `suggestion` in `src/.editorconfig` (flip each to warning after fixing its backlog): MA0026 (TODO comments, about 50, kept on purpose), MA0046 (3, event payloads that are not `EventArgs`), MA0051 (26, method length), MA0061 (3, the DEBUG-only required parameters), CA1710, CA1720 and CA1708 (domain names, kept on purpose) and the rest listed in the editorconfig.
- [ ] Remaining build warnings: `NU1902` MessagePack 2.5.302 (transitive via Aspire.Hosting 13.6.1 in `OoBDev.Example.AppHost`; wait for an Aspire update or pin), NU5118 and NU5128 pack warnings (Vectors.DB dacpac, `Retro/Apple2/Notes.txt`, a netstandard2.0 target without lib files), CS1030 `#warning` markers (Mailkit hosting not supported, two "is this needed" notes in queue tests).
- [ ] Remaining hand-built providers to replace with platform primitives, case by case (`TimeProvider` is done).

## 4. Migrations (blocked on the owner)

Scope rule: every BinaryDataDecoders feature is migrated; phases are priority order ([CLAUDE.md](CLAUDE.md#migration-scope)).

- [ ] **BinaryDataDecoders comparison and recommendation plan**: in `TODO.md` (requested 2026-10-22); the migration below waits for the owner's review of that plan.
- [ ] **BinaryDataDecoders** (~50,000 LOC, `Incoming/BinaryDecoders`). Phases: 1 Foundation (endianness, utilities, `BinaryPrimitives`), 2 High-value (code analysis, expression calculator, archives, drawing, barcode), 3 Protocols (NMEA and others), 4 Specialized (8 hardware devices, classic crypto with warnings, Apple II, fencing, Windows Forms, UWP), 5 Tools and CLI (4 tools). Answer the decisions on the [decision sheet](docs/migration/binarydatadecoders-decision-sheet.md) (the owner answered Q9 on 2026-10-09 by asking for the sheet; the per-line answers are still open). Compare against the copies listed in [docs/harvest-candidates.md](docs/harvest-candidates.md) first: the `mine/binary-data-decoders` repo is newer (78 projects), and `dev-term` holds newer device code.
- [ ] **ContractParser**: decided 2026-10-09 to keep as a specification; no work.
- [ ] **BulkLlm**: decided to consolidate. `OoBDev.LlmCodeGen.Cli` is built; the live Ollama and Groq runs remain ([design](docs/design/LlmCodeGen/README.md)).
- [ ] **SharedFramework** is design first (Epics 2, 6, 7, 10): four documents per feature (requirements, architecture, api-design, testing-strategy), 16 per epic. See [CLAUDE.md](CLAUDE.md#current-work-context).
- [ ] **Harvest candidates**: nothing decided; the map is in [docs/harvest-candidates.md](docs/harvest-candidates.md).
