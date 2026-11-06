# BACKLOG - OoBDev (dotex) Framework

> **Full work list.** Every open, valid item lives here; [TODO.md](TODO.md) holds only what is being worked on now. Finished work lives in the change history: [docs/changes/](docs/changes/README.md).
> **Owner questions:** [OPEN-QUESTIONS.md](./OPEN-QUESTIONS.md) · **New to this project?** Read [CLAUDE.md](./CLAUDE.md) first.
> **Wish list (owner's own notes):** [docs/todo.md](docs/todo.md) · **Other repositories to harvest from:** [docs/harvest-candidates.md](docs/harvest-candidates.md)

**Last Updated:** 2026-11-05

GitHub is no longer the primary repository (local bare repo `origin`), so nothing here waits on GitHub Actions. Keep items to one short line; detail lives in the linked feature doc (rule `work-tracking`).

## 1. DevTerm (merge of dev-term, 100% parity)

Owner: all protocols and features of dev-term must exist here. Done means what dev-term has today; its own future plans are in section 8. Status, slices and leftovers: [status.md](docs/design/DevTermParity/status.md); matrix: [README](docs/design/DevTermParity/README.md); restructure steps 0 to 6 are done.

- [ ] **DevTerm docs into living documents** (owner, 2026-10-30): reusable designs, proposals, features and rejected protocols are folded into `docs/design/` and `docs/protocols/` (2026-11-04, see [the map](docs/migration/devterm-docs-map.md)); device copies deleted 2026-11-04; left: the imported `source` and `bugs` history (the `changes` history is merged into the daily files, 2026-11-04; cleanup plan after every migration), then delete the map.
- The docs Unit tests `PlantUml_Blocks_AreWellFormed` and `Links_Relative_Resolve` fail on the imported DevTerm history; the planned documentation cleanup after all migrations fixes them (owner, 2026-10-30).
- [ ] Test gap: an interactive Tui run in a real console (a spike drove `conhost.exe` with `AttachConsole` and `WriteConsoleInput`; typing and screen reads worked, but the app exited at once in some runs and Ctrl+Q never ended it, so the code was dropped; retry in a spike; deferred 2026-11-05: it needs `AttachConsole` and `WriteConsoleInput` interop while the machine is still crashing for unknown reasons, so run it when the owner is present) ([list](docs/design/DevTermParity/status.md#test-gaps)).

- [ ] Example apps (WPF control example done 2026-11-05; Tui waits on OPEN-QUESTIONS Q1, Blazor on Q2, and the per-device Example panels remain) that use the UiDefinitions language (forms and panels), beyond DevTerm; decided (owner, 2026-11-04): one explicit Example panel per device family, plus a small control-example app for each control set built for Blazor, WPF, Tui or others (controls only; the existing Example app demos the framework) (placement decided: samples in each device project's tests plus `src/Examples/OoBDev.Examples.Controls.<Host>` apps) ([ui-language-later.md](docs/design/DevTermParity/ui-language-later.md)).

## 2. Devices

- [ ] Device decoding leftovers: the DE-5000 Hardware test waits on the owner fitting the BLE radio in the meter (not fitted 2026-11-05; the BLE transport itself is verified through the HC-08); phase 0 is done ([device-parity.md](docs/migration/device-parity.md) section 5; M3 needs a Radex capture); the physical passes for K8055, Busylight, Radex One, Zoom H4n and NMEA ran on 2026-11-04 and the SCPI instruments on 2026-11-05 (owner confirmed; [change log](docs/changes/2026-11-04.md)). Plan: [device-decoding-migration-plan.md](docs/migration/device-decoding-migration-plan.md).
  - Favero and Saint George score machines have no hardware pass (the owner has no machine, 2026-11-04); their decoders rest on unit tests, and Saint George priority is read from the `P` byte so it is never set (ported as-is).

## 3. Test infrastructure

- [ ] Integration coverage gaps: Azure runs on emulators only (Azurite, Service Bus emulator, no Azure licence); Moto is used only for SQS, so S3 tests wait for an S3 adapter.
- [ ] Integration suite under 5 minutes (now 7 min 54 s; the no-flaky-tests goal is met; needs the full Docker stack for repeated timed runs, so it waits for a dedicated session): [suite-timing.md](docs/architecture/testing/integration/suite-timing.md).
- [ ] `DevLocal` tests kept on purpose: Ollama (hard-coded LAN host, model pull and delete; revisit when the owner says Ollama testing can resume), USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files).
- [ ] Add a live-integration doc page per new cloud service (only Groq exists; its key was revoked 2026-10-09, so runs wait for the owner).
- [ ] Example AppHost live journeys: Keycloak wiring is built but not run live. The OTLP protocol bug is fixed and unit tested (2026-11-05); re-run the host once the stale DCP process holding port 16890 is stopped (needs an owner kill, the tool refused it), then check traces reach the dashboard and run the journeys ([apphost-live-journeys.md](docs/design/example-ui/apphost-live-journeys.md)).
- Azure Monitor exporter: not built (decided 2026-10-22); OTLP reaches any collector and is verified with the Aspire dashboard.

## 4. APIs and documents

- [ ] Stack review of the template and transform tools (owner, 2026-10-30): `Xslt.Cli` and `TemplateEngine` overlap; if TemplateEngine takes pluggable path engines (XPath navigator) and hosts the XSLT transforms, `Xslt.Cli` can go once a document explains converting its templates and scripts. Also port the BinaryDataDecoders documentation generation off `xslt.cli` onto TemplateEngine, as MSTest entry points rather than scripts around dotnet tools (a dotnet tool stays fine if its `.props` and `.targets` extensions are right); the shootout spike is written ([result](docs/design/TemplateTools/README.md)); the fold-in waits on OPEN-QUESTIONS Q3 (step 1 is done; the rest moves libraries).

- [ ] CGI middleware ([design](docs/design/CgiMiddleware/README.md)): first cut built with 23 passing tests; left is run-as-another-user (decided: stays a TODO, deployers use the service account). Shell fixtures run through `scripts/ci/run-linux-tests.ps1`.
- [ ] AllMiniLmL6V2: `AddValidatedOptions` for the embedding options (blocked by the Abstractions-only adapter rule).
- [ ] Compare the remaining project `README.{Project}.md` files with their code beyond the identifier check (behaviour review of about 100 readmes; take it project by project with its owning item) (`reconcile-designs-and-code` skill); identifier checks of Common, Framework, Extensions and ExternalServices readmes are done.
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

- [ ] **SharedFramework** (Epics 2, 6, 7, 10): the four design documents exist for every feature in `Features/Proposals` (Communications completed 2026-11-05). Implementation starts feature by feature, design first per [the standard](docs/patterns-discovery/06-design-document-standard.md); first step is moving each feature set into `docs/design/<Feature>/`: [migration plan](docs/design/SharedFramework/migration-plan.md) written 2026-11-05, waits on OPEN-QUESTIONS Q4. The Communications, Caching, Documents, Identity and MessageQueueing projects already exist in `src/Framework`; compare each with its design (`reconcile-designs-and-code`) before implementing.
  - [ ] Communications follow-ups: replacing `EmailMessageReceiverHost` with `TryAddReceivePolling` (host exists) (its `ReceivedEmailMessageModel` queue path, then delete that model), vendor processors and provider adapters, attachments in `IEmailMessage` ([design](Features/Proposals/02-Communications/README.md)). Tests for the example controllers need a test project that can reference the WebApi assembly (`OoBDev.Example.Tests` does not).
  - Epic 10 status (2026-11-05): `OoBDev.Handlebars` already has the provider, ten helpers and a compilation cache; open from the design: the Epic 11 container adapter and the database template source ([readme](src/ExternalServices/Handlebars/OoBDev.Handlebars/README.Handlebars.md)).
- [ ] **BulkLlm**: consolidate. `OoBDev.LlmCodeGen.Cli` is built; the live Groq run remains ([design](docs/design/LlmCodeGen/README.md)).
- [ ] Desired-state database tooling phase 0: bake-off of sqldef, pgschema and Atlas per `docs/design/DesiredStateDatabase/`; do it after the other backlog items (owner, 2026-10-29).
- [ ] **Consolidate the migration projects** (owner, 2026-10-29): left are `device-decoding-migration-plan.md`, `device-parity.md` and the two `incoming-*` notes, which fold into the DevTerm and BinaryDataDecoders items when those finish ([inventory](docs/migration/README.md)).
- ContractParser: keep as a specification; no work (decided 2026-10-09).

## 7. Documentation review (owner, 2026-10-28)

- [ ] Lower priority, after the migrations: review every project `.md` file for out-of-date and redundant documents (owner, 2026-10-30).
- [ ] **Review and clean up all documents**: content and coverage review against the rules `docs-and-diagrams` and `work-tracking` and `docs/patterns-discovery/06-design-document-standard.md`; the DevTerm docs come with the parity work. Numbering, validator problems and stale generated docs are done (2026-10-29); use the `write-docs` skill and `scripts/docs` validators.

## 8. Future ideas (not tied to DevTerm, nothing required now)

- [ ] An Ebyte `IDeviceConfigEditor` so the Configure device button has an editor (dev-term has only the interface and a fake; needs a fresh capture, see [the proposal](docs/protocols/ebyte-e810-dtu-config-protocol.md)).
- [ ] **Avalonia spike** for the cross-platform device UI, after the first WPF device panel works end to end: [avalonia-spike.md](docs/migration/avalonia-spike.md).
- [ ] **UiDefinitions as a generic dashboard and form language for web front ends** (later, nothing required now): [ui-language-later.md](docs/design/DevTermParity/ui-language-later.md).
- [ ] dev-term future plans, not in dev-term today and not needed to call the migration complete (owner, 2026-10-30) ([TODO and BACKLOG snapshots](docs/applications/devterm/source/BACKLOG.md)): BLE backends for Linux and macOS; UDP transport and the EByte E810-DTU config protocol (needs a fresh capture); HP-GL, PostScript and PCL presenters and SVG in the Tui; declarative SCPI control-module schema and DE-5000 bench pass; out-of-process plugins and the control channel (with the plugin sandbox below); the rest is listed in the snapshot linked above.
- [ ] DevTerm decided UX work (decided 2026-10-13, detail in the [dev-term snapshot](docs/applications/devterm/source/BACKLOG.md) and the linked designs); built status not yet checked against the code (network discovery has a start in `Configuration/Discovery`):
  - UDP listener (one session, a datagram per line with its sender) and TCP listener (a tab per accepted peer) per [Transports](docs/design/Transports/README.md).
  - Raster export as a core service every front end calls; plugin `.zip` packaging with approval at install.
  - SCPI profiles layered (generic plus device sub-types) apart from `DeviceManifest`; expression picker (parameter fields first, then channel segments).
  - Session-log playback checkpoints, skip-silence cap and note revisions; manifest editor JSON only with a convert from `.xml` panels.
  - Network device discovery (one Detect button) and in-app config editors for network bridges.
- [ ] DevTerm leftovers beyond the list above: Stream Monitor direct in-window preview of PCL, PostScript and HP-GL; a custom Roslyn analyzers project for DevTerm coding standards (the repo has `OoBDev.Analyzers`, so fold into it); Dockable MDI layout in WPF; read-write control channel and out-of-process plugins over a named pipe and a localhost web service (with the plugin sandbox item); research notes such as the BYTECC BT-UP01 bridge.
