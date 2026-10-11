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
| `TODO-migrations*.md`, `TODO-decisions.md` | Blocked on decisions | Unchanged: BinaryDataDecoders (14+ decisions), ContractParser. BulkLlm is decided (consolidate; design in [docs/design/LlmCodeGen](docs/design/LlmCodeGen/README.md), `OoBDev.LlmCodeGen.Cli` built 2026-10-09 with documentation, unit-tests, Angular to React and Angular to Vue templates; live Ollama and Groq runs still to do). The files say `Incoming/`; the folder is `Incoming/` | Blocked on the owner; fix folder name |
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
- [x] (done 2026-10-09) Implemented `OoBDev.AsyncApi`, adapter contributors, `MapAsyncApi()`, 7 Simulate tests, example wiring. [Details](docs/changes/migration-asyncapi-2026-10-09.md)
- [ ] Follow-up: describe send-only queues by type (senders are open generics, so only configured queues are listed today); check the viewer in a browser; an `azure-storage-queue` contributor if wanted. No in-process contributor: an in-process queue has no server, and the channel already shows `Provider: in-process`.

### 3. Docker test infrastructure and CI (needs the owner for the merge)
- [x] (done 2026-10-09) Port-collision hardening: every host port in `docker-compose.integration-tests.yml` is `${TEST_PORT_<NAME>:-default}` (table in `containers/testing/README.md`).
- [ ] Follow-up: `integration-up` scripts still print the default URLs, and tests read ports from `.runsettings`, so an override needs a matching settings file; consider deriving both from one `.env`.
- [x] (done 2026-10-09) Docker documentation: `docs/architecture/testing/integration/` (README with dependency matrix, docker-infrastructure with topology and startup diagrams, services with a section per stack, writing-tests). One services page replaces one page per stack. The Aspire spike is still open (owner decision); the pages double as its checklist.
- [ ] First GitHub run of `integration-tests.yml`: merge to `main`, trigger by hand, fix Linux build issues (net48 and SQL CLR projects), confirm the `validated-v{version}` tag and the 30-minute limit. Details in `TODO-testing-local-integration.md`.

### 4. Live integration (cloud) tests
- [x] Groq (`OoBDev.GroqCloud.Tests`): two `LiveIntegration` tests, template and README added; both pass against the live API (2026-10-09).
- [x] Cloud docs: [live-integration](docs/architecture/testing/live-integration/README.md) (category guide with decision diagram, credentials, cost, Groq page). Only Groq has live tests; add a page per new cloud service.
- [ ] Azure Monitor export through a collector is untested (needs the owner's subscription).
- [ ] `DevLocal` tests kept on purpose: Ollama tests (hard-coded LAN host, model pull and delete), USB HID (hardware), `PathEx`, `MergedXPathNavigator`, `ProjectTools.FixReadmes` (hard-coded `C:\Repos` paths), Markdown `TestMethod1` (writes files), DacFx `BuildPackageTest` (hard-coded path). Revisit the Ollama ones when the owner says Ollama testing can resume.

### 5. Patterns-discovery backlogs
Roslyn analyzers (owner chose `.editorconfig` plus a small custom analyzer: `OoBDev.Analyzers` added 2026-10-09 with OOB0001 `TimeProvider`, OOB0002 kebab-case keys, OOB0003 banned third-party libraries, wired into every project from `src/Directory.Build.props`; OOB0001 is a warning again (2026-10-09): production clock reads moved to `TimeProvider` (optional constructor parameter defaulting to `TimeProvider.System`; `VectorFunctions` uses `Environment.TickCount` because the SQL CLR project also targets net48), and `src/.editorconfig` exempts test projects; stock NetAnalyzers (`AnalysisMode` Recommended) and Meziantou.Analyzer are enabled for production projects in `src/Directory.Build.props` (2026-10-09); culture and ordinal rules (CA1304, CA1305, CA1309, CA1310, CA1311) are fixed, and rules with a remaining backlog are `suggestion` in `src/.editorconfig` so the build stays clean while new rules guard new code: backlog to burn down in rule-sized batches by flipping each rule back to warning: CA1710, CA1720 and CA1708 (naming rules that clash with domain names: kept as suggestion on purpose; CA1707 is enforced with the Method_Scenario test names and the SBert Nomic and `Bytes` exceptions, CA1711 and CA1716 are off because the `Ex` suffix is the extension convention and the keyword clashes only affect VB) and the other smaller CA rules (CA1000, CA1001, CA1010, CA1715, CA1510, CA1512, CA2208, CA1859, CA1860, CA1041, CA2254, CA1069 and CA1068, CA1727 and CA1725 are done), plus Meziantou (MA0004 ConfigureAwait is enforced in the library layers only: Tools, Examples and ASP.NET Core projects are exempt in `src/.editorconfig`; MA0026 TODO comments stays a suggestion because TODOs are tracked in `TODO.md` on purpose) MA0006 is switched off because `==` on strings is already ordinal; MA0002 is enforced with explicit `StringComparer.Ordinal`; MA0009, MA0011, MA0012, MA0013, MA0014, MA0015, MA0017, MA0019, MA0023, MA0048 and CA5369 are enforced (2026-10-11); MA0026 (TODO comments, 50), MA0016 (33), MA0025 (10), MA0046 (3), MA0051 and the rest listed in the editorconfig stay suggestions)), options validation modes (helper `AddValidatedOptions<T>()` exists in `OoBDev.System`; adopting it in the adapters (note: adapters reference only Abstractions projects, so they cannot call the helper in `OoBDev.System`; either inline `AddOptions().Bind().ValidateDataAnnotations()` with the Microsoft package or move the helper to an Abstractions-level project), owner chose relaxed mode and inline per adapter: done for Qdrant and OpenSearch 2026-10-09 (`[Required]` on truly required values, `ValidateDataAnnotations()`, no `ValidateOnStart`); also done for Tika, MailKit SMTP and IMAP, Mongo, Azure Blob and SBert; Ollama done; Groq, vectors hosting and the ONNX embedder have no required values (Groq ApiKey has an environment fallback) and Keycloak options are empty, so those need no annotations: rollout complete), naming consistency (provider keys done), `TimeProvider` replacements (done for the clock: `IDateTimeProvider` removed, remaining providers case by case), HTTP querying and rights middleware. Tracked in [CLAUDE.md](./CLAUDE.md#patterns-discovery-work-branch-devpatterns-discovery); several need owner answers first.

### 6. Blocked on the owner
BinaryDataDecoders (14+ decisions), ContractParser, PR #28. See [OPEN_QUESTIONS.md](./OPEN_QUESTIONS.md).
