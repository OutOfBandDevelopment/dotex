# Open Questions for the Owner

**Created:** 2026-10-09 · **Branch:** `dev/allminilm-cleanup`

Questions that block or shape work in the TODO files. Each has the source it came from (backlink), a suggested answer, and an **Answer** line to fill in offline. Leave the suggestion in place; write your decision under **Answer**. Once answered, tell Claude and the matching TODO item gets updated and this entry is moved to the change document.

> **Parent:** [TODO.md](./TODO.md) · **See also:** [TODO-decisions.md](./TODO-decisions.md), [CLAUDE.md](./CLAUDE.md)

## Contents

- [1. CI and infrastructure](#1-ci-and-infrastructure)
- [2. Design questions](#2-design-questions)
- [3. Migration decisions](#3-migration-decisions)
- [4. Renames and compatibility](#4-renames-and-compatibility)
- [Summary of questions](#summary-of-questions)

## 1. CI and infrastructure

### Q1 — Enable the scheduled integration tests?

- **Source:** [TODO.md](./TODO.md) → "Backlog: CI/CD Enablement and Docker Test Infrastructure"; [TODO-testing-local-integration.md](./TODO-testing-local-integration.md); [containers/testing/STATUS.md](./containers/testing/STATUS.md)
- **Question:** Turn on `integration-tests.yml` (daily 16:00 UTC plus `workflow_dispatch`) in `.github/workflows/`? It needs a runner with Docker. The Service Bus emulator has no health check.
- **Suggestion:** Enable manual dispatch first, watch one full run, then enable the schedule.
- **Answer:** Dispatch first: run `workflow_dispatch` once and watch a full run, then enable the schedule (2026-10-09).

### Q2 — Is "Restore central package management" done?

- **Source:** [TODO.md](./TODO.md) → "Backlog: Architect Answers"; [CLAUDE.md](./CLAUDE.md) → "Recently Completed Work" (2026-10-07)
- **Question:** CLAUDE.md says it was restored, but the TODO item is still open. May I verify that versions live only in `src/Directory.Packages.props` and tick it off?
- **Suggestion:** Yes.
- **Answer:** Yes, verify and tick off (2026-10-09).

### Q3 — Live cloud tests (Groq)

- **Source:** [TODO.md](./TODO.md) → "Backlog: Live Integration (Cloud) Tests"; [TODO-testing-live-integration.md](./TODO-testing-live-integration.md)
- **Question:** I can write the `.env.liveintegration.template`, project readmes, categories and `.gitignore` entry without credentials. Running them needs your accounts. Should I do the scaffolding now and leave the live runs to you?
- **Suggestion:** Yes, scaffolding only.
- **Answer:**

### Q4 — Aspire spike before the Docker documentation

- **Source:** [TODO.md](./TODO.md) → "Backlog: CI/CD Enablement..." (last item) and "Architect Answers" (Spikes)
- **Question:** Run a short Aspire vs Docker compose spike before writing the Docker test documentation under `docs/architecture/testing/`, so the docs are not rewritten?
- **Suggestion:** Yes, spike first.
- **Answer:** Yes, run the Aspire vs Docker compose spike first (2026-10-09).

## 2. Design questions

### Q5 — `ISelectedService<T>` rough edges (resolved)

- **Source:** [TODO.md](./TODO.md) → "Backlog: Review `ISelectedService` Rough Edges"
- **Question:** Three points, all affecting how existing consumers move to the selection factory you already chose:
  1. `[ContractConfig(AllowDefault, ConfigKey)]` is declared and documented but nothing reads it. Dead, or an unfinished feature?
  2. Selection is resolved in the constructor, so it is fixed for the process. Intended, or should it follow configuration reloads?
  3. `IServiceProvider` injection (service locator) lives in the wrapper. Accepted exception until the factory lands?
- **Suggestion:** (1) dead, remove with the migration; (2) fixed at startup; (3) accepted until replaced.
- **Answer:** Resolved 2026-10-09 by the migration: `ISelectedService<T>` and `[ContractConfig]` were removed; `TryAddConfiguredKeyedService<T>` replaces them (selection per resolution, locator contained in the factory).

### Q6 — Options validation: strict default or relaxed?

- **Source:** [TODO.md](./TODO.md) → "Backlog: Options Validation Modes"
- **Question:** Adapters are config-gated and often have no configuration, so a strict default changes behavior for existing hosts. Where is the switch (per builder, global `OoBDev:Validation:Mode`, or both), and what is the rollout?
- **Suggestion:** Ship `AddValidatedOptions<T>()` as opt-in. Release with relaxed default plus a warning, flip to strict in a later release.
- **Answer:** Relaxed default (fail on first use, no startup validation). Adapters inline `AddOptions().Bind().ValidateDataAnnotations()` with the Microsoft package instead of calling the `OoBDev.System` helper (2026-10-09).

### Q7 — Is `Microsoft.Extensions.Resilience` acceptable?

- **Source:** [TODO.md](./TODO.md) → "Backlog: Architect Answers" (OpenTelemetry / no Polly); [CLAUDE.md](./CLAUDE.md) → "Rejected and Preferred Dependencies"
- **Question:** It depends on Polly internally. Your rule is "avoid Polly". Is using the Microsoft API (not Polly directly) acceptable?
- **Suggestion:** Accept, since the dependency is Microsoft's.
- **Answer:** Accept `Microsoft.Extensions.Resilience` (2026-10-09).

### Q8 — What does "async-ui" mean for API docs?

- **Source:** [TODO.md](./TODO.md) → "Backlog: Architect Answers" (Scalar)
- **Question:** An AsyncAPI viewer, or Swagger UI? Scalar is chosen either way.
- **Answer:** An AsyncAPI viewer (2026-10-09). Scalar replaced Swashbuckle first; the AsyncAPI document and viewer for the SQS, Service Bus and RabbitMQ surfaces is phase 2 ([design](docs/design/OpenApiScalar/README.md)).

## 3. Migration decisions

### Q9 — BinaryDataDecoders (39 open decisions)

- **Source:** [TODO-decisions.md](./TODO-decisions.md); [TODO-migrations-binarydatadecoders.md](./TODO-migrations-binarydatadecoders.md); [TODO.md](./TODO.md) → "Backlog: Migration Decisions"
- **Question:** The decisions cover the endianness API, `BinaryPrimitives` naming, UI collections location, CodeAnalysis use case, archive formats, ExpressionCalculator and more. Should Claude read `TODO-decisions.md` and produce a one-page list with a recommendation next to each, so you can answer yes/no per line?
- **Suggestion:** Yes.
- **Answer:** Yes, produce a one-page list with a recommendation per decision (2026-10-09).

### Q10 — BotChat

- **Source:** [TODO.md](./TODO.md) → "Backlog: Migration Decisions"; [Incoming/CHECKLIST.md](./Incoming/CHECKLIST.md)
- **Question:** Archive, enhance, or extract patterns?
- **Answer:** Extract patterns (2026-10-09).

### Q11 — ContractParser

- **Source:** [TODO.md](./TODO.md) → "Backlog: Migration Decisions"
- **Question:** Implement now, later, or keep as a specification?
- **Answer:** Keep as specification (2026-10-09).

### Q12 — Tools / BulkLlm

- **Source:** [TODO.md](./TODO.md) → "Backlog: Migration Decisions"
- **Question:** Consolidate the BulkLlm tools or archive them?
- **Answer:** Consolidate the BulkLlm tools (2026-10-09).

## 4. Renames and compatibility

### Q13 — `Retreive` to `Retrieve` rename (resolved)

- **Source:** [TODO.md](./TODO.md) → "Backlog: Caching Proxy Review (pattern 10)"
- **Question:** About 85 occurrences in 23 files, including public names (`ICachingProvider`, `ICachingManager`). CLAUDE.md says no breaking changes to existing APIs. May the old names stay as `[Obsolete]` forwarders for one release?
- **Suggestion:** Yes.
- **Answer:** Resolved 2026-10-09: the framework is unreleased, so breaking public changes are fine. `RetreiveAsync` was renamed outright to `RetrieveAsync` with no forwarders.

### Q14 — `ServiceCollectionEx` to `ServiceCollectionExtensions`

- **Source:** [TODO.md](./TODO.md) → "Backlog: Naming Consistency"
- **Question:** Resolved 2026-10-09 without needing an answer: the six classes were renamed outright. Forwarders were not possible (two classes with the same extension methods make every call ambiguous) and nothing called the old class by name.
- **Answer:** (none needed)

## Summary of questions

**Table 1 — Open questions at a glance**

| ID | Topic | Blocks | Answered |
|----|-------|--------|----------|
| Q1 | Scheduled integration tests | CI enablement | 2026-10-09 |
| Q2 | Central package management done? | TODO cleanup | 2026-10-09 |
| Q3 | Live cloud test scaffolding | Live integration backlog | |
| Q4 | Aspire spike first | Docker docs | 2026-10-09 |
| Q5 | `ISelectedService` rough edges | Selection factory migration | |
| Q6 | Options validation mode | `AddValidatedOptions<T>()` | 2026-10-09 |
| Q7 | Microsoft resilience package | Resilience practice | 2026-10-09 |
| Q8 | "async-ui" meaning | API docs | |
| Q9 | BinaryDataDecoders decisions | Migration phases | 2026-10-09 |
| Q10 | BotChat | Incoming project | 2026-10-09 |
| Q11 | ContractParser | Incoming project | 2026-10-09 |
| Q12 | Tools / BulkLlm | Incoming project | 2026-10-09 |
| Q13 | `Retrieve` rename policy | Caching rename | |
| Q14 | `ServiceCollectionEx` rename policy | Naming consistency | |

[↑ TODO.md](./TODO.md)
