# TODO.md - Completed Items (archived)

**Date:** 2026-10-09
**Epic:** Documentation
**Status:** ✅ COMPLETE
**Impact:** Completed items moved out of `TODO.md`; the TODO now lists only open work

---

## Patterns Discovery (branch `dev/patterns-discovery`)

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
| 16 | Build out remaining coverage gaps (Documentation Coverage Gaps backlog) | ✅ Done (awaiting user review). [Details](documentation-patterns-discovery-2026-09-30.md) |
| 17 | Review all TODO files and record what is actually outstanding (Outstanding Work Review) | ✅ Done (2026-09-30) |

**Notes / findings log:**
- Code survey: 123 csproj, ~1,061 .cs, 42 test projects, 120 on net10.0 (2 netstandard2.1, 1 net48 for SQL CLR).
- Core idiom: `I{Thing}` (Abstractions project) → `{Thing}` impl → `TryAdd{Thing}Services(IServiceCollection, IConfiguration, sectionName)`; providers registered twice (default + keyed); `ISelectedService<T>` picks one via config `OoBDev::ServiceKeys::{FullTypeName}`.
- Common layer is an *aggregator* (MSBuild glob ProjectReference include/remove), not "pure interfaces" as CLAUDE.md says.
- **Drift:** `[ContractConfig]` is declared + documented but never read at runtime (SelectedService hard-codes its own key).
- **Drift:** docs say .NET 9 / README.md build-enforced; code is net10.0, files were `ReadMe.{Project}.md` (normalized to `README.{Project}.md`), missing readme is only warning OBDPK001 (error only if PackageReadmeFile set but absent).
- **Drift:** Framework has 4 empty placeholder dirs (Generations, DataLoader, ComplexEvents, SpatialServices); arch docs reference RabbitMQ.Abstractions which does not exist.
- Superseded 2026-10-07: central package management is on (versions only in `src/Directory.Packages.props`). Analyzers block is still commented out in Directory.Build.props (see backlog).
- Side observation: no Polly/MediatR/FluentValidation/HybridCache usage anywhere. OpenTelemetry was added 2026-10-09 (`OoBDev.OpenTelemetry`).

---

## Earlier Completed Banners

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

---

## Completed Backlog Items

- [x] 2026-10-08: after a fresh start 14 services report healthy (azurinsight included); servicebus has no health check; nginx and opensearch-dashboards health checks fixed (nginx probed on 127.0.0.1, dashboards status call authenticated).
- [x] 2026-10-08 full Integration run against the Docker stack: 58 passed, 0 failed, 2 skipped (SBert tests marked `[Ignore]`). Fixed on the way: Tika 4 endpoints (`/detect`, `/tika/html|text|xml`), Ollama embedding tests now use `all-minilm`, Moto pinned to 4.4 (latest needs a licence token).
- [x] Azurinsight replaced by the `otel-lgtm` service (2026-10-09): compose, env, `.runsettings`, workflow variables, scripts, nginx card and stack readme updated. [ ] Remaining: stack doc and PlantUML diagrams under `docs/architecture/testing/`.
- [x] Azure B2C dropped (2026-10-09): project, tests, `IdentityProviders.AzureB2C` and the example host profile removed; Keycloak is the identity provider.
- [x] Application Insights replaced by plain OpenTelemetry (2026-10-09, `OoBDev.OpenTelemetry`); Integration tests read spans and logs back from the `otel-lgtm` container. [ ] Azure Monitor export through a collector is untested (needs the owner's subscription).
- [x] Added `.env.liveintegration` to `.gitignore` (2026-10-09)
  - [x] Application Insights (5): superseded by the OpenTelemetry tests.
