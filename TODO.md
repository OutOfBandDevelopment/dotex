# TODO - OoBDev (dotex) Framework

> **Owner questions awaiting answers:** [OPEN_QUESTIONS.md](./OPEN_QUESTIONS.md)

## 🔎 ACTIVE: Patterns Discovery (branch `dev/patterns-discovery`)

**Goal:** Document how this codebase is actually put together (architecture, design, patterns & practices) so future products/frameworks can be built the same way, then produce a pros/cons comparison against common industry alternatives.
**Output location:** `docs/patterns-discovery/`

| # | Task | Status |
|---|------|--------|
| 1 | Create branch, add tracking to TODO.md | ✅ Done |
| 2 | Review existing docs (`docs/architecture/*`, CLAUDE.md) | ✅ Done |
| 3 | Survey code: solution/layers, build props, csproj conventions | ✅ Done |
| 4 | Survey code: abstractions/provider/factory/DI patterns | ✅ Done |
| 5 | Survey code: options/config, hosting, ASP.NET, data, messaging | ✅ Done |
| 6 | Survey code: testing (categories, TestContext config, Docker) | ✅ Done |
| 7 | Write `01-architecture/` (solution shape, layers, dependency rules) | ✅ Done |
| 8 | Write `02-design-patterns/` (patterns with real code refs) | ✅ Done |
| 9 | Write `03-practices-and-conventions/` (naming, build, docs, testing, warts) | ✅ Done |
| 10 | Write `04-new-project-blueprint/` (4 recipes) | ✅ Done |
| 11 | Write `05-industry-alternatives/` (30 topics, pros/cons, verdicts, owner decisions applied) | ✅ Done |
| 12 | Write `README.md` index with doc/code drift table | ✅ Done |
| 13 | Final review; validate-docs 80 files, 0 problems | ✅ Done (awaiting user review) |
| 14 | Write `06-design-document-standard.md` (how design docs are made; PlantUML/Salt rule; added to CLAUDE.md) | ✅ Done |
| 15 | Add HTTP API and authorization practices, alternatives topics 25-26, apply architect answers | ✅ Done (commit 44fb0cd) |
| 16 | Build out remaining coverage gaps (Documentation Coverage Gaps backlog) | ✅ Done (awaiting user review). [Details](docs/changes/documentation-patterns-discovery-2026-09-30.md) |
| 17 | Review all TODO files and record what is actually outstanding (Outstanding Work Review) | ✅ Done (2026-09-30) |

**Notes / findings log:**
- Code survey: 123 csproj, ~1,061 .cs, 42 test projects, 120 on net10.0 (2 netstandard2.1, 1 net48 for SQL CLR).
- Core idiom: `I{Thing}` (Abstractions project) → `{Thing}` impl → `TryAdd{Thing}Services(IServiceCollection, IConfiguration, sectionName)`; providers registered twice (default + keyed); `ISelectedService<T>` picks one via config `OoBDev::ServiceKeys::{FullTypeName}`.
- Common layer is an *aggregator* (MSBuild glob ProjectReference include/remove), not "pure interfaces" as CLAUDE.md says.
- **Drift:** `[ContractConfig]` is declared + documented but never read at runtime (SelectedService hard-codes its own key).
- **Drift:** docs say .NET 9 / README.md build-enforced; code is net10.0, files are `ReadMe.{Project}.md`, missing readme is only warning OBDPK001 (error only if PackageReadmeFile set but absent).
- **Drift:** Framework has 4 empty placeholder dirs (Generations, DataLoader, ComplexEvents, SpatialServices); arch docs reference RabbitMQ.Abstractions which does not exist.
- `Directory.Packages.props` is empty, `ManagePackageVersionsCentrally=false` -> versions inline per csproj; analyzers block is commented out in Directory.Build.props.
- Unrequested-by-user side observation: no Polly/OpenTelemetry/MediatR/FluentValidation/HybridCache usage anywhere.

---

**Last Updated:** 2026-10-09

✅ **COMPLETED:** Build Warnings Resolution - Reduced from 95+ to 8 warnings (2026-01-22)
✅ **COMPLETED:** Test Category Cleanup - DevLocal tests converted to Integration/Unit/LiveIntegration (2026-01-22)
✅ **COMPLETED:** .runsettings Documentation - Comprehensive how-to guide created (2026-01-22)
✅ **COMPLETED:** Configuration Documentation - Comprehensive CONFIGURATION_SETTINGS.md created (157+ settings)
✅ **COMPLETED:** Ollama Integration Test Setup - Automated model pulling in integration-up scripts
✅ **COMPLETED:** Docker-based Integration Testing Infrastructure (Week 1 & 2) - 14 services + Ollama
✅ **COMPLETED:** Swashbuckle 10.1.0 & .NET 10.0 breaking changes - All fixes verified working
✅ **COMPLETED:** XML Documentation generation - Swagger Summary/Description now appear
✅ **COMPLETED:** Swagger generation tested and verified working
✅ **COMPLETED:** Local Docker testing validation - All Integration tests passing (2026-01-21)

> **New to this project?** Read `/CLAUDE.md` first for a complete development guide including architecture, patterns, and migration scope.

---

## 📋 Outstanding Work Review (2026-09-30)

Every TODO file and in-code marker was checked against the repository. The other TODO files carry "Last Updated" dates from 2026-01-20 to 2026-01-24 and several statuses in them are stale. This section is the corrected picture; the epic files keep their detail.

**Table — TODO files and what is really outstanding**

| File | Stated status | Actually outstanding | Action |
|------|---------------|----------------------|--------|
| `TODO-testing-local-integration.md` | Validated, ready for CI/CD | CI workflow `.github/workflows/integration-tests.yml` is still disabled (schedule and `workflow_dispatch` commented out, only `workflow_call`); 2 of 15 services not confirmed healthy (azurinsight, servicebus); Docker stack docs under `docs/architecture/testing/` do not exist (only `README.md` and `testing-guidelines.md`) | Keep open; see backlog below |
| `TODO-testing-live-integration.md` | Week 3 migration pending | Azure B2C tests are already `LiveIntegration` (3 tests in `OoBDev.Microsoft.Azure.B2C.Tests`, not `OoBDev.Microsoft.B2C.Tests` as the file says); Application Insights tests (10) are still `DevLocal`; Groq tests have no category at all; no `.env.liveintegration.template` or per-project README exists; no cloud docs | Keep open; fix paths |
| `TODO-migrations*.md`, `TODO-decisions.md` | Blocked on decisions | Unchanged: BinaryDataDecoders (14+ decisions), BotChat (archive, enhance or extract), ContractParser, Tools/BulkLlm. The files say `Incoming/`; the folder is `Incoming/` | Blocked on the owner; fix folder name |
| `Features/Caching/TODO-migrations-caching.md`, `docs/changes/migration-message-queues-2026-01-20.md` | Complete | Nothing outstanding | Archive candidates |
| `docs/todo.md` | Wish list | 4 open wishes (below) | Folded into this file |
| `src/Framework/OoBDev.DacFx.Tests/TODO.md` | Note | Test SQLCLR project covering all SQLCLR features | Backlog |
| `src/Framework/OoBDev.System.Abstractions/ComponentModel/Data/TODO.md` | Note | Example project for the data annotations | Backlog |
| `TODO.md` (this file) | Header dated 2026-01-24 | Links to `TODO-documentation.md` and `TODO-testing-infrastructure.md`, which do not exist | Remove links or create the targets |

### Backlog: CI/CD Enablement and Docker Test Infrastructure
- [x] 2026-10-08: after a fresh start 14 services report healthy (azurinsight included); servicebus has no health check; nginx and opensearch-dashboards health checks fixed (nginx probed on 127.0.0.1, dashboards status call authenticated).
- [x] 2026-10-08 full Integration run against the Docker stack: 58 passed, 0 failed, 2 skipped (SBert tests marked `[Ignore]`). Fixed on the way: Tika 4 endpoints (`/detect`, `/tika/html|text|xml`), Ollama embedding tests now use `all-minilm`, Moto pinned to 4.4 (latest needs a licence token).
- [ ] Enable `integration-tests.yml`: uncomment `schedule` (daily 16:00 UTC) and `workflow_dispatch`, remove the temporary `workflow_call`, check the runner has Docker, trigger manually, watch the first run, confirm the `validated-v{version}` tag and the 30-minute limit.
- [ ] Azurinsight follow-ups: README service table, workflow variables, `.runsettings` Application Insights settings, nginx dashboard entry, PlantUML diagrams, stack doc.
- [ ] Docker documentation under `docs/architecture/testing/`: integration category pages (README, docker-setup, writing-tests, examples), one page per stack (SQL Server, MongoDB, RabbitMQ, OpenSearch, Qdrant, Tika, SMTP, Azurite, Moto, Keycloak, SBert, Ollama, azurinsight), a docker-infrastructure page, and network-topology and dependency-matrix diagrams (PlantUML, per the diagram rule).
- [ ] Decide whether the Aspire spike (see Architect Answers) changes any of this before the docs are written.

### Backlog: Live Integration (Cloud) Tests
- [ ] Azure B2C: add `.env.liveintegration.template` and a project README, and read settings from test properties (the category change is already done).
- [ ] Application Insights: library and tests moved to 3.x/OpenTelemetry (2026-10-07; 5 unit tests pass). Ran the 5 `DevLocal` integration tests against azurinsight on 2026-10-07: all fail because the 3.x exporter posts newline-delimited JSON envelopes to `/v2.1/track` and the emulator (image `oobdev/azurinsight`, fork `mwwhited-forks/Azurinsight`) parses one JSON document (`SyntaxError: Unexpected non-whitespace character after JSON`, body-parser). Fix the emulator to accept NDJSON (and gzip), then re-run and recategorize them (Integration against azurinsight, or LiveIntegration for the real service); add template and README.
- [ ] Groq (`OoBDev.GroqCloud.Tests`): categorize tests as `LiveIntegration`; add template and README.
- [x] Added `.env.liveintegration` to `.gitignore` (2026-10-09)
- [ ] Cloud docs: category README, cloud setup, credential and cost management, per-service pages, LiveIntegration vs Integration guide, PlantUML diagrams.
- [ ] Triage of `TestCategories.DevLocal` (mostly done 2026-10-09). Fixed and moved: Documents registry pair, `ReflectionElementNodeTest` (test utility now writes XPath navigators as XML), Html `DeeperTest` (`SimpleCopy.xslt` embedded), Bson, `AsXElement`, XPath `max`, Example service registry (Unit); document conversion via Tika and blob tests via Azurite (Integration, run against the containers).
  - Kept `DevLocal` on purpose: Ollama tests (hard-coded LAN host, model pull and delete), USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files), DacFx `BuildPackageTest` (hard-coded path).
  - [ ] Application Insights (5): being replaced by plain OpenTelemetry (see the OpenTelemetry item), so these move with it.
