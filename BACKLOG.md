# BACKLOG - OoBDev (dotex) Framework

> **Full work list.** Every open, valid item lives here; [TODO.md](TODO.md) holds only what is being worked on now. Finished work lives in the change history: [docs/changes/](docs/changes/README.md).
> **Owner questions:** [OPEN-QUESTIONS.md](./OPEN-QUESTIONS.md) · **New to this project?** Read [CLAUDE.md](./CLAUDE.md) first.
> **Other repositories to harvest from:** [docs/harvest-candidates.md](docs/harvest-candidates.md)

**Last Updated:** 2026-11-10

GitHub is no longer the primary repository (local bare repo `origin`), so nothing here waits on GitHub Actions. Keep items to one short line; detail lives in the linked feature doc (rule `work-tracking`).

## 1. DevTerm (merge of dev-term, 100% parity)

Owner: all protocols and features of dev-term must exist here. Done means what dev-term has today; its own future plans are in section 8. Status, slices and leftovers: [status.md](docs/design/DevTermParity/status.md); matrix: [README](docs/design/DevTermParity/README.md); restructure steps 0 to 6 are done.

- [ ] **Generic TCP services in DevTerm** (owner, 2026-11-10, answer to D6; replaces the Net.ServiceHost CLI tool): [request](docs/features/request/2026-11-10-devterm-generic-service-listeners.md). Write the design and migration plan first (session type, presenter or bridge), then build; the servers stay in `OoBDev.System`.
- [ ] **DevTerm docs into living documents** (owner, 2026-10-30): reusable designs, proposals, features and rejected protocols are folded into `docs/design/` and `docs/protocols/` (2026-11-04, see [the map](docs/migration/README.md#devterm-ui-shells-and-plugins)); device copies deleted 2026-11-04; left: the imported `source` and `bugs` history (the `changes` history is merged into the daily files, 2026-11-04; cleanup plan after every migration), then delete the map.
- The docs Unit tests `PlantUml_Blocks_AreWellFormed` and `Links_Relative_Resolve` fail on the imported DevTerm history; the planned documentation cleanup after all migrations fixes them (owner, 2026-10-30).
- [ ] Test gap: an interactive Tui run in a real console (a spike drove `conhost.exe` with `AttachConsole` and `WriteConsoleInput`; typing and screen reads worked, but the app exited at once in some runs and Ctrl+Q never ended it, so the code was dropped; retry in a spike; deferred 2026-11-05: it needs `AttachConsole` and `WriteConsoleInput` interop while the machine is still crashing for unknown reasons, so run it when the owner is present) ([list](docs/design/DevTermParity/status.md#test-gaps)).


## 2. Devices

- [ ] Device decoding leftovers: the DE-5000 Hardware test waits on the owner fitting the BLE radio in the meter (not fitted 2026-11-05; the BLE transport itself is verified through the HC-08); phase 0 is done ([device merge list](docs/migration/README.md#devices); M3 needs a Radex capture); the physical passes for K8055, Busylight, Radex One, Zoom H4n and NMEA ran on 2026-11-04 and the SCPI instruments on 2026-11-05 (owner confirmed; [change log](docs/changes/2026-11-04.md)). Plan: [migration-plan.md](docs/design/DeviceDecoding/migration-plan.md).
  - Favero and Saint George score machines have no hardware pass (the owner has no machine, 2026-11-04); their decoders rest on unit tests, and Saint George priority is read from the `P` byte so it is never set (ported as-is).

## 3. Test infrastructure

- [ ] Integration coverage gaps: Azure runs on emulators only (Azurite, Service Bus emulator, no Azure licence); Moto is used only for SQS, so S3 tests wait for an S3 adapter.
- [ ] Integration suite under 5 minutes (now 7 min 54 s; the no-flaky-tests goal is met; needs the full Docker stack for repeated timed runs, so it waits for a dedicated session): [suite-timing.md](docs/architecture/testing/integration/suite-timing.md).
- [ ] `DevLocal` tests kept on purpose: USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files).
- [ ] Add a live-integration doc page per new cloud service (only Groq exists; the live tests passed again on 2026-11-09 with the key in the user environment variable `GROQ_API_KEY`).
- [ ] Example AppHost live journeys: Keycloak wiring is built but not run live. The OTLP protocol bug is fixed and unit tested (2026-11-05); re-run the host once the stale DCP process holding port 16890 is stopped (needs an owner kill, the tool refused it), then check traces reach the dashboard and run the journeys ([apphost-live-journeys.md](docs/design/example-ui/apphost-live-journeys.md)).
- Azure Monitor exporter: not built (decided 2026-10-22); OTLP reaches any collector and is verified with the Aspire dashboard.

## 4. APIs and documents


- [ ] CGI middleware ([design](docs/design/CgiMiddleware/README.md)): first cut built with 23 passing tests; left is run-as-another-user (decided: stays a TODO, deployers use the service account). Shell fixtures run through `scripts/ci/run-linux-tests.ps1`.
- [ ] AllMiniLmL6V2: `AddValidatedOptions` for the embedding options (blocked by the Abstractions-only adapter rule).
- [ ] Compare the remaining project `README.{Project}.md` files with their code beyond the identifier check (behaviour review of about 100 readmes; take it project by project with its owning item) (`reconcile-designs-and-code` skill); identifier checks of Common, Framework, Extensions and ExternalServices readmes are done.
- [ ] GraphQL surface (HotChocolate, on demand), without breaking the custom search syntax; adopt `[OoBDevEnableQuery]` in the example API when it gets an `IQueryable` endpoint. See the [HTTP API](docs/patterns-discovery/03-practices-and-conventions/10-http-api-practices.md) and [authorization](docs/patterns-discovery/03-practices-and-conventions/11-authorization-practices.md) practices.
- [ ] Example app missing screens ([list](docs/design/example-ui/missing-screens.md)); owner, 2026-11-09, [recommended order](docs/design/example-ui/missing-screens.md#recommended-order) steps 1 to 3 are built (Search, Template management, queue monitor and blob browser); the rest stay unplanned.
  - [ ] Template save waits for OPEN-QUESTIONS Q2; the queue receive monitor shows nothing until a channel has its receiver enabled (all example channels set `DisableReceiver`).
- [ ] Performance gate for database-backed (non-vector) endpoints: no such endpoint exists yet, so add the gate with the first one.

## 5. Code quality backlog

A clean rebuild after the 2026-11-06 pass reports no analyzer warnings (218 before). Analyzers are wired from `src/Directory.Build.props` (`OoBDev.Analyzers` OOB0001 to OOB0003, NetAnalyzers Recommended, Meziantou); `Tools`, `Examples` and `Applications` are exempt from MA0004, and `Tools` and `Examples` from MA0048, in `src/.editorconfig`.

- [ ] Rules kept at `suggestion` on purpose in `src/.editorconfig` (nothing to flip unless the owner decides otherwise):: MA0026 (about 50 TODO comments), MA0046 (3, `EventHandler<TMessage>` on the device interfaces), MA0061 (3, DEBUG-only required parameters), CA1710, CA1720, CA1708 (domain names).
- [ ] Bug-review the migrated projects that the source review never covered (device IO, toolkit and text, code analysis and archives, apps and tooling); the source split was dropped with `REVIEW-STATUS.md` (history keeps it)
- [ ] ExpressionCalculator optional reductions, never built (the source only listed them as a "bonus round"): `B^-1` to `1/B`, `B/N` to `B*(1/N)`, `B*B*...` to `B^T`, `B+B+...` to `B*T`; add them to `IdentityExpressionOptimizer` or a new optimizer with tests, or decline

## 6. Migrations

Scope rule: every BinaryDataDecoders feature is migrated; phases are priority order ([CLAUDE.md](CLAUDE.md#migration-scope)).

### Waiting on the owner

- [ ] **Migration leftovers** ([plan](docs/migration/README.md)): nothing left to decide; everything from BinaryDataDecoders, dev-term and the incoming copies is migrated, rejected with a reason, or tracked in the plan.
- [ ] **Harvest candidates** (D7, evaluate later): hybridsearch, Oobtainium, BuildFirstOnce, markdownplantuml, TotpExample, HandyClasses, postsql-playground; map in [harvest-candidates.md](docs/harvest-candidates.md), decisions in [the plan](docs/migration/README.md#harvest-candidates).
- [ ] Plugin sandbox: the [threat model](docs/design/PluginSandbox/threat-model.md) and [protocol and manifest design](docs/design/PluginSandbox/protocol-and-manifest.md) wait for the owner sign-off of REQ-010, and the [layering proposal](docs/design/PluginSandbox/migration-plan.md) for its confirmation; spike result: [spike-result.md](docs/design/PluginSandbox/spike-result.md).
  - Sandbox follow-ups (owner, 2026-11-10: macOS deferred until later): message rate and heartbeat enforcement; Survive named-pipe transport; request context tied to a token validator; Linux equivalents of the caller-death and pool lifetime tests; AppContainer profile and ACL cleanup; wiring the persisted `GrantStore` path into `PluginHost` use and the docs. Deferred: `CrossPlatformEscapeTests` on a Mac, macOS limits and PID isolation.

### Decided, waiting its turn

- [ ] **SharedFramework** (Epics 2, 6, 7, 10): the four design documents exist for every feature in `docs/design` ([migration plan](docs/design/SharedFramework/migration-plan.md)) and all four epics are compared with the existing projects (`reconcile-designs-and-code`). Owner, 2026-11-06 (Q1): none is implemented for now; the designs stay designs until a consumer appears. Implementation, when it starts, is design first per [the standard](docs/patterns-discovery/06-design-document-standard.md).
  - Epic 10 status (2026-11-05): `OoBDev.Handlebars` already has the provider, ten helpers and a compilation cache; open from the design: the Epic 11 container adapter and the database template source ([readme](src/ExternalServices/Handlebars/OoBDev.Handlebars/README.Handlebars.md)).
- [ ] Desired-state database tooling phase 0: bake-off of sqldef, pgschema and Atlas per `docs/design/DesiredStateDatabase/`; do it after the other backlog items (owner, 2026-10-29). Test plan written 2026-11-09 ([bake-off-test-plan.md](docs/design/DesiredStateDatabase/bake-off-test-plan.md)); run it after the owner reviews it.

## 7. Documentation review (owner, 2026-10-28)


- [ ] Lower priority, after the migrations: review every project `.md` file for out-of-date and redundant documents (owner, 2026-10-30).
- [ ] **Review and clean up all documents**: content and coverage review against the rules `docs-and-diagrams` and `work-tracking` and `docs/patterns-discovery/06-design-document-standard.md`; the DevTerm docs come with the parity work. Numbering, validator problems and stale generated docs are done (2026-10-29); use the `write-docs` skill and `scripts/docs` validators.

## 8. Future ideas (not tied to DevTerm, nothing required now)

- [ ] Owner, 2026-11-09: after the first WPF device panel works end to end. An Ebyte `IDeviceConfigEditor` so the Configure device button has an editor (dev-term has only the interface and a fake; needs a fresh capture, see [the proposal](docs/protocols/ebyte-e810-dtu-config-protocol.md)).
- [ ] Owner wish list (rolled in from `docs/todo.md`, 2026-11-10): turn MS Project into planning control; an OAuth token-exchange example that handles several client ids in one set of services and front ends; user analytics (screen usage, URLs, timeouts); error-handling middleware with correlation that returns the original error location (see [observability](docs/patterns-discovery/03-practices-and-conventions/13-observability-practices.md)).
- [ ] **UiDefinitions as a generic dashboard and form language for web front ends** (later, nothing required now): [ui-language-later.md](docs/design/DevTermParity/ui-language-later.md).
- [ ] **dev-term suggested features, one request each** (owner, 2026-11-10, so dev-term can be retired): [UDP and listeners](docs/features/request/2026-11-10-devterm-udp-transport.md), [network device editors and probes](docs/features/request/2026-11-10-devterm-network-device-config-editors.md), [BLE Linux and macOS](docs/features/request/2026-11-10-devterm-ble-linux-macos.md), [rendering presenters and raster export](docs/features/request/2026-11-10-devterm-rendering-presenters-and-raster-export.md), [SCPI control-module schema](docs/features/request/2026-11-10-devterm-scpi-control-module-schema.md), [out-of-process plugins and control channel](docs/features/request/2026-11-10-devterm-out-of-process-plugins-and-control-channel.md), [playback checkpoints](docs/features/request/2026-11-10-devterm-session-playback-checkpoints.md), [dockable layout and Blazor](docs/features/request/2026-11-10-devterm-dockable-layout-and-blazor-frontend.md), [BYTECC bridge](docs/features/request/2026-11-10-devterm-bytecc-usb-bridge.md). The two lines below stay until these are triaged.
- [ ] dev-term future plans, not in dev-term today and not needed to call the migration complete (owner, 2026-10-30) ([TODO and BACKLOG snapshots](docs/applications/devterm/source/BACKLOG.md)): BLE backends for Linux and macOS; UDP transport and the EByte E810-DTU config protocol (needs a fresh capture); HP-GL, PostScript and PCL presenters and SVG in the Tui; declarative SCPI control-module schema and DE-5000 bench pass; out-of-process plugins and the control channel (with the plugin sandbox below); the rest is listed in the snapshot linked above.
- [ ] DevTerm decided UX work (decided 2026-10-13, detail in the [dev-term snapshot](docs/applications/devterm/source/BACKLOG.md) and the linked designs); built status not yet checked against the code (network discovery has a start in `Configuration/Discovery`):
  - expression picker per channel in the Tui Manifest Editor: the Tui's whole-list Channels picker already edits each channel's expression segment with validation, so a per-row Pick would need dynamic fields in the generic `OoBDev.UiDefinitions.Tui` form renderer; build it only if the owner wants it (WPF has it since 2026-11-09; see [expression-picker.md](docs/applications/devterm/specs/expression-picker.md)).
  - Network device discovery: the Detect button and one row per device with badges are built; the EByte and USR UDP probes need a fresh capture. In-app config editors for network bridges ([proposal](docs/design/NetworkDevices/network-device-config-editors.md)).
- [ ] DevTerm leftovers beyond the list above:
  - Stream Monitor in-window preview of HP-GL, PostScript and PCL in the Tui monitor (a terminal cannot show a picture; decide whether to open the converted file externally) (the WPF and Web monitors draw HP-GL themselves and PostScript/PCL through an installed Ghostscript/GhostPCL since 2026-11-09).
  - A DevTerm rule in `OoBDev.Analyzers` only when one is needed (decided 2026-11-09: the checkable DevTerm constraints are legitimate in most uses or need data flow the analyzer cannot see, so a rule now would be noise; add OOB0004 when a repeatable mistake appears).
  - Read-write control channel and out-of-process plugins over a named pipe and a localhost web service (with the plugin sandbox item); research notes such as the BYTECC BT-UP01 bridge.
