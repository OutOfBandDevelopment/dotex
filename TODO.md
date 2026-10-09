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

### Backlog: CI/CD Enablement and Docker Test Infrastructure
- [ ] First GitHub run of `integration-tests.yml`: merge to `main`, trigger by hand, fix Linux build issues (net48 and SQL CLR projects), confirm the `validated-v{version}` tag and the 30-minute limit. Details in `TODO-testing-local-integration.md`.
- [ ] Docker documentation under `docs/architecture/testing/`: integration category pages (README, docker-setup, writing-tests, examples), one page per stack (SQL Server, MongoDB, RabbitMQ, OpenSearch, Qdrant, Tika, SMTP, Azurite, Moto, Keycloak, SBert, Ollama, otel-lgtm), a docker-infrastructure page, and network-topology and dependency-matrix diagrams (PlantUML, per the diagram rule). Stack doc and PlantUML diagrams for the `otel-lgtm` service are part of this.
- [ ] Decide whether the Aspire spike (see Architect Answers) changes any of this before the docs are written.
- [ ] Port-collision hardening: make the host ports overridable (or move them to a private range) so other local services cannot collide, as `storage-ollama` did on 11434.

### Backlog: Live Integration (Cloud) Tests
- [ ] Azure Monitor export through a collector is untested (needs the owner's subscription).
- [ ] Groq (`OoBDev.GroqCloud.Tests`): categorize tests as `LiveIntegration`; add template and README (the project has no test sources yet).
- [ ] Cloud docs: category README, cloud setup, credential and cost management, per-service pages, LiveIntegration vs Integration guide, PlantUML diagrams.
- [ ] `DevLocal` tests kept on purpose: Ollama tests (hard-coded LAN host, model pull and delete), USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files), DacFx `BuildPackageTest` (hard-coded path). Revisit the Ollama ones when the owner says Ollama testing can resume.

### Other Open Backlogs
Open patterns-discovery backlogs (Roslyn analyzers, options validation modes, `ISelectedService` rough edges, naming consistency, `Retreive` to `Retrieve`, message context caller info, `TimeProvider` replacements, HTTP querying and rights middleware) and owner decisions are tracked in [CLAUDE.md](./CLAUDE.md#patterns-discovery-work-branch-devpatterns-discovery) and [OPEN_QUESTIONS.md](./OPEN_QUESTIONS.md).
