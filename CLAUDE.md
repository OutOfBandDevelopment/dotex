# OoBDev (dotex) Framework - Claude Development Guide

**Last Updated:** 2026-10-22
**Framework:** OoBDev (dotex) - Enterprise .NET Library Suite
**Target:** net10.0
**Current Work:** Patterns discovery (branch `dev/patterns-discovery`, see "Patterns Discovery Work" below) | Design-first approach for SharedFramework migrations | Integration test stack ready for CI/CD

---

## How Claude is configured in this repo

Working agreements are enforced by files, not by this guide alone:

| Mechanism | Location | What it holds |
|-----------|----------|---------------|
| Rules (always loaded, or loaded for matching files) | `.claude/rules/` | `git-workflow`, `owner-decisions`, `open-questions` (always); `coding-standards` (C#), `testing` (tests, runsettings, containers), `docs-and-diagrams` (docs, markdown) |
| Skills (loaded when the task matches) | `.claude/skills/` | `add-integration-service`, `archive-completed-work`, `document-configuration`, `compare-incoming-codebase`, `architectural-analysis`, `security-audit`, `write-docs`, `complete-work`, `reconcile-designs-and-code`, `work-backlog`, `owner-interview` |
| Hook (deterministic guard) | `.claude/hooks/guard-git.mjs`, registered in `.claude/settings.json` | blocks AI attribution in commit messages, force pushes, `gh` usage and pushes to `upstream` |
| Memory | `~/.claude/projects/.../memory/` | owner preferences and feedback |

The former `.claude/protocols/` set was converted into these on 2026-10-22. Workflow, DataGrid, schema, template and rewrite protocols were retired (they assumed branch-per-task PRs or a TypeScript stack); `git log` has them. Keep rules short and testable; put procedures in skills; put anything that must never happen in a hook.

---

## Overview

OoBDev is a collection of .NET framework extensions at several utility levels: binary data and device communication, code analysis and expression evaluation, file I/O, networking and protocols, template engines and document generation, database access, UI components (Windows Forms, WPF), specialized hardware and legacy systems, and desktop/server applications.

112+ projects in 4 layers, net10.0, MSTest, 80%+ coverage for the Framework layer.

---

## Before Starting Any Work

Read first:
- `/TODO.md` (active work) and `/BACKLOG.md` (full work list); finished work goes to `docs/changes/` (rule `work-tracking`, skill `complete-work`); owner questions are in `/OPEN-QUESTIONS.md`
- `/docs/patterns-discovery/README.md` - how this codebase is built (patterns, conventions, blueprint, alternatives)
- `/docs/architecture/README.md` and `/docs/architecture/architectural-standards.md`

Key principles:
- ALL features from BinaryDataDecoders migrate (phases = priority order, not selection); the newest source is `C:\repo\mine\binary-data-decoders`, and `Incoming/BinaryDecoders` can be behind
- Provider/factory pattern for extensibility; DI with `TryAdd*`
- `README.{Project}.md` (upper-case `README`) per project; `PackageReadmeFile` derives from it (`src/Directory.Build.props`), and a missing file is only warning OBDPK001
- Nullable enabled, ImplicitUsings disabled
- Report outcomes faithfully: failing tests, skipped steps and unverified changes are stated plainly

Incoming projects pending owner decisions (BotChat was extracted 2026-10-09 and removed): BinaryDataDecoders (~50,000 LOC, 14+ decisions; Phase 1 foundation work is blocked on them), ContractParser (implement now, later, or keep as spec), Tools (consolidate BulkLlm or archive).

---

## Architecture Layers

- **Common** (6 projects): foundation abstractions and contracts. The aggregator uses MSBuild globs to include/remove ProjectReferences (not purely interfaces).
- **Framework** (39): core implementations; depends only on Common; 80%+ coverage; readme expected.
- **Extensions** (6): optional enhancements; depends on Framework; packaged separately.
- **ExternalServices** (40+): Azure, AWS, Google Cloud, database providers, external APIs.

Core patterns: provider/factory (`IService` -> `IServiceProvider` -> `IServiceProviderFactory`), `TryAdd*` DI, options (`AddValidatedOptions<T>()`), handler (`IHandler<TRequest, TResponse>`). Details and naming are in `.claude/rules/coding-standards.md`.

Packages: core `OoBDev.System`, `OoBDev.Extensions`, `OoBDev.IO.*`; specialized and separately packaged `OoBDev.Security.Cryptography.Classic` (educational, with warnings), `OoBDev.Extensions.Hardware.*`, `OoBDev.Retro.Apple2`, `OoBDev.Extensions.Windows.Forms`, `OoBDev.Extensions.Drawing.*`.

---

## Migration Scope

Everything from BinaryDataDecoders migrates: core, specialized (ISO 9660, hardware), niche (Apple II, fencing equipment), educational (classic cryptography with warnings), incomplete (migrate as-is, track TODOs) and UI features. Only stubs with zero implementation (for example Rigol), obsolete-platform code (Silverlight) and features with no net10.0 equivalent are deleted. Specialized features are packaged separately, and all code follows the architectural standards.

> "We maintain ALL features, even highly specialized ones, to preserve the complete functionality of BinaryDataDecoders. Phases indicate priority order for migration work, not a selection of which features to keep."

Procedure: the `compare-incoming-codebase` skill.

---

## Common Tasks

| Task | Do this |
|------|---------|
| Add a Docker service or test parameter | `add-integration-service` skill |
| "clean up your task list", TODO over ~400 lines | `archive-completed-work` skill |
| "find all my configurations" | `document-configuration` skill |
| Compare incoming code with main | `compare-incoming-codebase` skill |
| Document or review architecture | `architectural-analysis` skill |
| Audit API security | `security-audit` skill |
| Write or restructure docs | `write-docs` skill and the `docs-and-diagrams` rule |
| New migration phase, bug fix or feature | read `TODO.md`, check the layer and similar projects, implement with tests (80%+ Framework), update the project readme and `TODO.md` |

When in doubt: check `TODO.md`, read the architecture docs, look at a similar project, and ask the owner if requirements are unclear.

---

## Testing Quick Reference

Categories (details in `.claude/rules/testing.md`): `Unit`, `Simulate`, `Integration` (Docker; run locally), `DevLocal` (manual), `LiveIntegration` (cloud, manual).

```bash
cd containers/testing && ./scripts/integration-up.sh --wait      # start the 15-service stack
cd ../../src && dotnet test --filter "TestCategory=Integration"
cd ../containers/testing && ./scripts/integration-down.sh --clean
```

Services: Apache Tika, SMTP4Dev, MongoDB, SQL Server, RabbitMQ, Redis, OpenSearch, Qdrant, Azurite, Moto (AWS), Azure Service Bus emulator, Keycloak, SBert (CPU), Ollama (CPU, phi3), Aspire dashboard (OTLP, traces, logs, metrics, telemetry API). Every host port is overridable with `TEST_PORT_<NAME>`.

See `TEST_VARIABLES.md` (30+ properties), `containers/testing/README.md`, `containers/testing/TESTING-CHECKLIST.md`, `docs/architecture/testing/integration/`.

Build and test commands (the legacy GitHub pipeline triggered on `src/**/*.cs` and used `src/.runsettings`):

```bash
dotnet build src/
dotnet test src/ --filter TestCategory=Unit
dotnet test src/ --collect:"XPlat Code Coverage"
```

---

## Important Files

- `/TODO.md`, `/BACKLOG.md`, `/Incoming/CHECKLIST.md` - work tracking and incoming project status
- `/docs/architecture/`, `/docs/how-tos/` ([.runsettings variables](docs/how-tos/runsettings-variables-and-configuration.md)), `/docs/migration/`, `/docs/changes/`
- Git: the primary repo is the local bare repo `origin` (`G:/oobdev/dotex.git`); GitHub (`upstream`) is no longer primary, so no `gh`, PRs or GitHub CI waits. Commit as you work on a branch, never on `main` (`.claude/rules/git-workflow.md`)
- `/GitVersion.yml` (manual major, minor per merge into main, patch = commits past the last main tag on branches), `/src/.runsettings`, `/.github/workflows/dotnet.yml`

---

## Patterns Discovery Work (branch `dev/patterns-discovery`)

**Resume here.** Goal: document how the codebase is built so future products follow the same patterns, then compare with industry alternatives.

- **Docs:** `docs/patterns-discovery/` (01 architecture, 02 design patterns, 03 practices, 04 new-project blueprint, 05 industry alternatives, 06 design-document standard; README has the doc/code drift table). Patterns are the owner's preferences: record them, don't "correct" them; rough edges go to `TODO.md` backlogs.
- **Templates:** `templates/` (`dotnet new` pack: `oobdev-capability`, `oobdev-adapter`, `oobdev-webapp`); verify with `scripts/templates/verify-templates.ps1` (builds from `src/Framework` cwd because `Directory.Build.props` computes `SolutionDir` from the cwd, then cleans up).
- **Scripts:** `scripts/docs/` (see its README). `python scripts/docs/validate-docs.py docs/patterns-discovery` should report 80 files and 0 problems; the whole `docs/` tree has ~566 old problems (for example `docs/sbom`).
- **Decisions made:** `#if DEBUG` required builder parameters are intentional (forces child builders to be forwarded); options were deliberately unvalidated (strict/relaxed mode under analysis).
- **Open backlogs in `TODO.md`:** naming consistency, Roslyn analyzers, options validation modes, message context caller info, replace hand-built providers with platform primitives, HTTP querying and rights middleware, documentation coverage gaps (build in the listed order).
- **Branch state:** history was rewritten to remove AI trailers; the branch matched `origin/dev/patterns-discovery` as of 3f54bfe. Never force-push without explicit approval (the hook also blocks it).

---

## Recently Completed Work

Older entries (before 2026-10) live in `docs/changes/` (index: `docs/changes/README.md`).

### 2026-10-22
- **Claude configuration rebuilt** - protocols converted to `.claude/rules`, `.claude/skills` and a git guard hook (see top of this file)
- **Aspire dashboard replaces Grafana LGTM** in the test stack; telemetry is read back through its API; telemetry page lists recent traces

### 2026-10-09
- **Framework cleanup (unreleased, breaking)** - `ISelectedService<T>` replaced by `TryAddConfiguredKeyedService<T>` (path `OoBDev:CachingProvider:Type`), `IDateTimeProvider` removed for `TimeProvider`, `Retreive` renamed `Retrieve`, hash/serializer/HMAC provider keys kebab-case, message caller info via caller attributes, `AddValidatedOptions<T>()` added (adapter adoption blocked by the Abstractions-only reference rule, see TODO.md), Groq live tests on `openai/gpt-oss-20b`
- **Integration test stack** - every host port overridable via `TEST_PORT_<NAME>`; Docker docs under `docs/architecture/testing/integration/`
- **AsyncAPI document and viewer** - `OoBDev.AsyncApi`, `IAsyncApiContributor` in the SQS, Service Bus and RabbitMQ adapters, `MapAsyncApi()`. [Details](docs/changes/migration-asyncapi-2026-10-09.md)
- **Swashbuckle replaced by OpenAPI transformers and Scalar** - `Microsoft.AspNetCore.OpenApi` documents, Scalar reference with OAuth2 code and PKCE. [Details](docs/changes/migration-openapi-scalar-2026-10-09.md)
- **Azure B2C dropped** - Keycloak is the identity provider
- **OpenTelemetry replaces Application Insights** - `OoBDev.OpenTelemetry` (config-gated, OTLP export). [Details](docs/changes/migration-opentelemetry-2026-10-09.md)
- **Test Ollama on host port 11435** - avoids the clash with a local Ollama on 11434

### 2026-10-08
- **Image embeddings** - in-process ONNX runner (DINOv2-small, ViT-base, CLIP ViT-B/32). [Details](docs/changes/migration-image-embeddings-2026-10-08.md)
- **Embedding presets and version policy** - MPNet and Nomic presets, `GitVersion.yml` policy. [Details](docs/changes/migration-embedding-presets-2026-10-08.md)
- **AllMiniLmL6V2 replaces the fork** - first-party ONNX embedder and tokenizer. [Details](docs/changes/migration-allminilml6v2-embedder-2026-10-08.md)
- **NULL-safe vectors, CI order, Moto replaces LocalStack** - [Details](docs/changes/testing-vectors-sqs-moto-ci-2026-10-08.md)

### 2026-10-07
- **Dependencies and build health** - central package management restored, packages current, about 12 distinct warnings left (see TODO.md)

### 2026-09-30
- **Patterns Discovery** - architecture/patterns/practices/blueprint/alternatives docs, `dotnet new` templates, doc scripts. [Details](docs/changes/documentation-patterns-discovery-2026-09-30.md)

---

## Current Work Context

**Active priorities:**
1. **Integration testing** - run the Docker stack and Integration suite locally; `.github/workflows/` (including the daily `integration-tests.yml`) are legacy GitHub Actions that no longer gate work, pending a decision on a local replacement
2. **SharedFramework** - design-first (Epics 2 Communications, 6 Documents, 7 Identity, 10 Text Templating): four documents per feature (requirements, architecture, api-design, testing-strategy; 85-90% coverage targets) instead of code migration, per `docs/patterns-discovery/06-design-document-standard.md`
3. **Incoming projects** - all investigated, decisions pending
4. **Backlog** - single backlog in `TODO.md`; CGI middleware and related-work pointers are in `docs/todo.md` and `docs/harvest-candidates.md`
