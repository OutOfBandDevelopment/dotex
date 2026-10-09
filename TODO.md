# TODO - OoBDev (dotex) Framework

> **Owner questions awaiting answers:** [OPEN_QUESTIONS.md](./OPEN_QUESTIONS.md)
> **New to this project?** Read [CLAUDE.md](./CLAUDE.md) first.
> **Completed work** lives in the change history: [docs/changes/](docs/changes/README.md). This file lists open work only.

**Last Updated:** 2026-10-09

## Outstanding Work Review

**Table — TODO files and what is really outstanding**

| File | Stated status | Actually outstanding | Action |
|------|---------------|----------------------|--------|
| `TODO-testing-local-integration.md` | Validated, ready for CI/CD | CI workflow `.github/workflows/integration-tests.yml` is enabled (2026-10-09, ubuntu) but has not run on GitHub yet; Docker stack docs under `docs/architecture/testing/` do not exist (only `README.md` and `testing-guidelines.md`) | Keep open; see backlog below |
| `TODO-testing-live-integration.md` | Week 3 migration pending | Groq tests have no category at all; no `.env.liveintegration.template` or per-project README exists; no cloud docs | Keep open; fix paths |
| `TODO-migrations*.md`, `TODO-decisions.md` | Blocked on decisions | Unchanged: BinaryDataDecoders (14+ decisions), BotChat (archive, enhance or extract), ContractParser, Tools/BulkLlm. The files say `Incoming/`; the folder is `Incoming/` | Blocked on the owner; fix folder name |
| `Features/Caching/TODO-migrations-caching.md`, `docs/changes/migration-message-queues-2026-01-20.md` | Complete | Nothing outstanding | Archive candidates |
| `docs/todo.md` | Wish list | 4 open wishes | Read `docs/todo.md` |
| `src/Framework/OoBDev.DacFx.Tests/TODO.md` | Note | Test SQLCLR project covering all SQLCLR features | Backlog |
| `src/Framework/OoBDev.System.Abstractions/ComponentModel/Data/TODO.md` | Note | Example project for the data annotations | Backlog |
| `TODO.md` (this file) | Header dated 2026-01-24 | Links removed; the file now only links to `OPEN_QUESTIONS.md` and `CLAUDE.md` | Done (2026-10-09) |

## Work Order

Order of work, with the reason for the position. Start at the top; tick items off and move finished ones to the change history.

### 1. Fix what the OpenAPI migration left behind (small, unblocks the rest)
- [x] (done 2026-10-09) `scripts/templates/verify-templates.ps1` failed at "test capability" with NU1008: generated projects carry `Version` on `PackageReference` while central package management is on. Remove the versions from the templates and add any missing `PackageVersion` entries; re-run until all three templates build.
- [x] (done 2026-10-09) Removed or annotated the stale Swashbuckle pages under `docs/Libraries/OoBDev.AspNetCore.*.md` and `docs/generated/Framework/OoBDev.AspNetCore.*/SwaggerGen`; update `FEATURE_INVENTORY.md` rows that list the removed Swagger classes; rename `.claude/protocols/software/template-swagger-documentation.md` references (`/swagger/all/swagger.json` is now `/openapi/all.json`).

### 2. AsyncAPI document and viewer (phase 2 of [OpenApiScalar](docs/design/OpenApiScalar/README.md); owner request)
- [x] Design set `docs/design/AsyncApi/` (requirements, architecture, api-design, testing-strategy): evaluate `Saunter` and `LEGO.AsyncAPI` against a thin first-party model; channels and messages for SQS, Service Bus and RabbitMQ.
- [ ] Implement the model and builder, adapter contributions, `/asyncapi/{name}.json` and the viewer page; Simulate tests; example app wiring; docs and change record.

### 3. Docker test infrastructure and CI (needs the owner for the merge)
- [ ] Port-collision hardening: make host ports overridable (or move them to a private range) so other local services cannot collide, as `storage-ollama` did on 11434.
- [ ] Docker documentation under `docs/architecture/testing/`: integration category pages (README, docker-setup, writing-tests, examples), one page per stack (SQL Server, MongoDB, RabbitMQ, OpenSearch, Qdrant, Tika, SMTP, Azurite, Moto, Keycloak, SBert, Ollama, otel-lgtm), a docker-infrastructure page, and network-topology and dependency-matrix diagrams (PlantUML). Decide first whether the Aspire spike changes this.
- [ ] First GitHub run of `integration-tests.yml`: merge to `main`, trigger by hand, fix Linux build issues (net48 and SQL CLR projects), confirm the `validated-v{version}` tag and the 30-minute limit. Details in `TODO-testing-local-integration.md`.

### 4. Live integration (cloud) tests
- [ ] Groq (`OoBDev.GroqCloud.Tests`): categorize tests as `LiveIntegration`; add template and README (the project has no test sources yet).
- [ ] Cloud docs: category README, cloud setup, credential and cost management, per-service pages, LiveIntegration vs Integration guide, PlantUML diagrams.
- [ ] Azure Monitor export through a collector is untested (needs the owner's subscription).
- [ ] `DevLocal` tests kept on purpose: Ollama tests (hard-coded LAN host, model pull and delete), USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files), DacFx `BuildPackageTest` (hard-coded path). Revisit the Ollama ones when the owner says Ollama testing can resume.

### 5. Patterns-discovery backlogs
Roslyn analyzers, options validation modes, `ISelectedService` rough edges, naming consistency, `Retreive` to `Retrieve`, message context caller info, `TimeProvider` replacements, HTTP querying and rights middleware. Tracked in [CLAUDE.md](./CLAUDE.md#patterns-discovery-work-branch-devpatterns-discovery); several need owner answers first.

### 6. Blocked on the owner
BinaryDataDecoders (14+ decisions), BotChat, ContractParser, Tools/BulkLlm, PR #28. See [OPEN_QUESTIONS.md](./OPEN_QUESTIONS.md).
