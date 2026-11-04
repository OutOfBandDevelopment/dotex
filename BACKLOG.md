# BACKLOG - OoBDev (dotex) Framework

> **Full work list.** Every open, valid item lives here; [TODO.md](TODO.md) holds only what is being worked on now. Finished work lives in the change history: [docs/changes/](docs/changes/README.md).
> **Owner questions:** [OPEN-QUESTIONS.md](./OPEN-QUESTIONS.md) · **New to this project?** Read [CLAUDE.md](./CLAUDE.md) first.
> **Wish list (owner's own notes):** [docs/todo.md](docs/todo.md) · **Other repositories to harvest from:** [docs/harvest-candidates.md](docs/harvest-candidates.md)

**Last Updated:** 2026-11-04

GitHub is no longer the primary repository (local bare repo `origin`), so nothing here waits on GitHub Actions. Keep items to one short line; detail lives in the linked feature doc (rule `work-tracking`).

## 1. DevTerm (merge of dev-term, 100% parity)

Owner: all protocols and features of dev-term must exist here. Done means what dev-term has today; its own future plans are in section 8. Status, slices and leftovers: [status.md](docs/design/DevTermParity/status.md); matrix: [README](docs/design/DevTermParity/README.md); restructure steps 0 to 6 are done.

- [ ] **Surroundings review of the other migrated projects** (owner, 2026-10-30; dev-term is done, see [the review](docs/migration/devterm-surroundings-review-2026-10-30.md)): BinaryDataDecoders is done, see [the review](docs/migration/binarydatadecoders-surroundings-review-2026-11-04.md); repeat for each harvest project: guide, `.claude` configuration, docs, open questions, TODO and BACKLOG, UIs, tests, scripts and plans, each migrated or rejected with a reason; application docs go to `docs/applications/{app-name}/`.
- [ ] **Generated guides** (owner, 2026-10-30): [design](docs/design/GeneratedGuides/README.md); journey-driven user guides and screenshots for Tui, WPF and Web written straight into `docs/applications/{app}/user-guide`, API guides from OpenAPI, AsyncAPI and ksy files by MSTest scripts, JavaScript projects as `.esproj` with `npm test`.
- Restore of the whole `src/OoBDev.slnx` fails with NU1201 (found 2026-11-04 while moving the DevTerm projects; unrelated to the move): find the project and fix its target framework or reference.
- The docs Unit tests `PlantUml_Blocks_AreWellFormed` and `Links_Relative_Resolve` fail on the imported DevTerm history; the planned documentation cleanup after all migrations fixes them (owner, 2026-10-30).
- Generated user guides for the `src/Tools` command-line tools (journeys, captured output, screenshots where there is a UI); later, after the application guides; see [GeneratedGuides](docs/design/GeneratedGuides/README.md).
- [ ] Test gaps: Playwright DevLocal flows, an interactive Tui run, serial real-hardware tests ([list](docs/design/DevTermParity/status.md#test-gaps)).

- [ ] Example apps that use the UiDefinitions language (forms and panels), beyond DevTerm; open spike question: whether the device projects need coverage in the example framework at all, since DevTerm covers them ([ui-language-later.md](docs/design/DevTermParity/ui-language-later.md)).

## 2. Devices

- [ ] Run the new bench Hardware tests once with the bench on (Korad supplies on COM6 and COM7, network instruments online) and fix what the real replies show ([change](docs/changes/test-bench-hardware-2026-11-04.md)).
- [ ] Device decoding leftovers: the DE-5000 Hardware test needs a BLE transport; phase 0 is done (merge status in [device-parity.md](docs/migration/device-parity.md) section 5; M3 needs a Radex capture); a physical pass for K8055 input bit mapping, Busylight program frames, Radex settings and reset, Zoom H4n handshake, NMEA BT-20 framing and a real SCPI instrument. Plan: [device-decoding-migration-plan.md](docs/migration/device-decoding-migration-plan.md).
- [ ] Saint George score machine: priority is read from the `P` byte itself, so it is never set (ported as-is); settle the real byte from a captured frame, and add a Hardware test for Favero and Saint George when a machine is available.

## 3. Test infrastructure

- [ ] Integration coverage gaps: Azure runs on emulators only (Azurite, Service Bus emulator, no Azure licence); Moto is used only for SQS, so S3 tests wait for an S3 adapter.
- [ ] Integration suite under 5 minutes (now 7 min 54 s; the no-flaky-tests goal is met): [suite-timing.md](docs/architecture/testing/integration/suite-timing.md).
- [ ] `DevLocal` tests kept on purpose: Ollama (hard-coded LAN host, model pull and delete; revisit when the owner says Ollama testing can resume), USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files).
- [ ] Add a live-integration doc page per new cloud service (only Groq exists; its key was revoked 2026-10-09, so runs wait for the owner).
- [ ] Example AppHost live journeys fail (Keycloak runs anonymously, telemetry does not reach the dashboard): [apphost-live-journeys.md](docs/design/example-ui/apphost-live-journeys.md).
- Azure Monitor exporter: not built (decided 2026-10-22); OTLP reaches any collector and is verified with the Aspire dashboard.

## 4. APIs and documents

- [ ] Stack review of the template and transform tools (owner, 2026-10-30): `Xslt.Cli` and `TemplateEngine` overlap; if TemplateEngine takes pluggable path engines (XPath navigator) and hosts the XSLT transforms, `Xslt.Cli` can go once a document explains converting its templates and scripts. Also port the BinaryDataDecoders documentation generation off `xslt.cli` onto TemplateEngine, as MSTest entry points rather than scripts around dotnet tools (a dotnet tool stays fine if its `.props` and `.targets` extensions are right); start with a shootout spike.
- [ ] Make the DevTerm Ghostscript conversion for PCL and PostScript generic and part of the document conversion framework, and add HP-GL to SVG.

- [ ] CGI middleware ([design](docs/design/CgiMiddleware/README.md)): first cut built with 23 passing tests; left is run-as-another-user (decided: stays a TODO, deployers use the service account). Shell fixtures run through `scripts/ci/run-linux-tests.ps1`.
- [ ] AllMiniLmL6V2: `AddValidatedOptions` for the embedding options (blocked by the Abstractions-only adapter rule).
- [ ] Compare the remaining project `README.{Project}.md` files with their code beyond the identifier check (`reconcile-designs-and-code` skill); identifier checks of Common, Framework, Extensions and ExternalServices readmes are done.
- [ ] GraphQL surface (HotChocolate, on demand), without breaking the custom search syntax; adopt `[OoBDevEnableQuery]` in the example API when it gets an `IQueryable` endpoint. See the [HTTP API](docs/patterns-discovery/03-practices-and-conventions/10-http-api-practices.md) and [authorization](docs/patterns-discovery/03-practices-and-conventions/11-authorization-practices.md) practices.
- [ ] Example app missing screens ([list](docs/design/example-ui/missing-screens.md)); the owner picks which of the unplanned areas deserve a screen.
- [ ] Performance gate for database-backed (non-vector) endpoints: no such endpoint exists yet, so add the gate with the first one.

## 5. Code quality backlog

A clean rebuild (2026-10-22) has no analyzer warnings. Analyzers are wired from `src/Directory.Build.props` (`OoBDev.Analyzers` OOB0001 to OOB0003, NetAnalyzers Recommended, Meziantou); `Tools` and `Examples` are exempt from MA0004 and MA0048 in `src/.editorconfig`.

- [ ] Rules still at `suggestion` in `src/.editorconfig`: flip each to warning after fixing its backlog. Kept on purpose: MA0026 (about 50 TODO comments), MA0046 (3, `EventHandler<TMessage>` on the device interfaces), MA0061 (3, DEBUG-only required parameters), CA1710, CA1720, CA1708 (domain names).

## 6. Migrations

Scope rule: every BinaryDataDecoders feature is migrated; phases are priority order ([CLAUDE.md](CLAUDE.md#migration-scope)).

### Waiting on the owner

- [ ] **Incoming code recommendation plan** ([plan](docs/migration/incoming-recommendation-plan.md)): the migration waits for the owner to answer D1 to D7.
- [ ] **BinaryDataDecoders** (~50,000 LOC, source `C:\repo\mine\binary-data-decoders`, newer than `Incoming/BinaryDecoders`, which is superseded; removing it is the owner's call). Done: Numerics, NMEA helpers, Drawing, Code analysis, Fencing, LanC, the Xslt CLI, the PackMan CLI and the eight hardware devices (from dev-term). Next is the order in Table 4 of [the re-review](docs/migration/binarydatadecoders-rereview-2026-10-29.md); compare against [harvest candidates](docs/harvest-candidates.md) first.
- [ ] **Harvest candidates**: nothing decided; map in [docs/harvest-candidates.md](docs/harvest-candidates.md). Evaluate later (decision D7): hybridsearch, Oobtainium, BuildFirstOnce, markdownplantuml, TotpExample, HandyClasses, postsql-playground (informs `OoBDev.DacFx`).
- [ ] Plugin sandbox: the [threat model](docs/design/PluginSandbox/threat-model.md) and [protocol and manifest design](docs/design/PluginSandbox/protocol-and-manifest.md) wait for the owner sign-off of REQ-010; a proof-of-concept spike comes before scheduling (owner, 2026-10-29).

### Decided, waiting its turn

- [ ] **SharedFramework** is design first (Epics 2, 6, 7, 10): four documents per feature (requirements, architecture, api-design, testing-strategy). See [CLAUDE.md](CLAUDE.md#current-work-context).
- [ ] **BulkLlm**: consolidate. `OoBDev.LlmCodeGen.Cli` is built; the live Groq run remains ([design](docs/design/LlmCodeGen/README.md)).
- [ ] Desired-state database tooling phase 0: bake-off of sqldef, pgschema and Atlas per `docs/design/DesiredStateDatabase/`; do it after the other backlog items (owner, 2026-10-29).
- [ ] **Consolidate the migration projects** (owner, 2026-10-29): left are `device-decoding-migration-plan.md`, `device-parity.md` and the two `incoming-*` notes, which fold into the DevTerm and BinaryDataDecoders items when those finish ([inventory](docs/migration/README.md)).
- ContractParser: keep as a specification; no work (decided 2026-10-09).

## 7. Documentation review (owner, 2026-10-28)

- [ ] Lower priority, after the migrations: review every project `.md` file for out-of-date and redundant documents (owner, 2026-10-30).
- [ ] **Review and clean up all documents**: content and coverage review against the rules `docs-and-diagrams` and `work-tracking` and `docs/patterns-discovery/06-design-document-standard.md`; the DevTerm docs come with the parity work. Numbering, validator problems and stale generated docs are done (2026-10-29); use the `write-docs` skill and `scripts/docs` validators.

## 8. Future ideas (not tied to DevTerm, nothing required now)

- [ ] An Ebyte `IDeviceConfigEditor` so the Configure device button has an editor (dev-term has only the interface and a fake; needs a fresh capture, see [the proposal](docs/applications/devterm/design/proposals/ebyte-e810-dtu-config-protocol.md)).
- [ ] **Avalonia spike** for the cross-platform device UI, after the first WPF device panel works end to end: [avalonia-spike.md](docs/migration/avalonia-spike.md).
- [ ] **UiDefinitions as a generic dashboard and form language for web front ends** (later, nothing required now): [ui-language-later.md](docs/design/DevTermParity/ui-language-later.md).
- [ ] dev-term future plans, not in dev-term today and not needed to call the migration complete (owner, 2026-10-30) ([TODO and BACKLOG snapshots](docs/applications/devterm/source/BACKLOG.md)): BLE backends for Linux and macOS; UDP transport and the EByte E810-DTU config protocol (needs a fresh capture); HP-GL, PostScript and PCL presenters and SVG in the Tui; declarative SCPI control-module schema and DE-5000 bench pass; out-of-process plugins and the control channel (with the plugin sandbox below); the rest is listed in the snapshot linked above.
