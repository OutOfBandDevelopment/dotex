# Open questions

Questions Claude has for the owner. Write your answer under **Your answer** (or answer in chat, which counts the same; Claude copies it here). Claude keeps working on the **Default** until you answer, then moves answered items to [Answered](#answered).

How this file is kept: see `.claude/rules/open-questions.md`.

Earlier questions from `OPEN_QUESTIONS.md` (2026-10-09, ids E1-E14) were merged in at the bottom of **Answered**.

## Open

### Q6 — HTTP querying and rights middleware
- **Asked:** 2026-10-22 · **Area:** TODO.md backlog
- **Question:** OData over `QUERY`, GraphQL, RBAC-to-rights translation: which direction should be built?
- **Default:** not started; skipped.
- **Your answer:** yes
- **Follow-up (2026-10-22):** "yes" does not pick a direction. Proposed: build all three in order, starting with the rights middleware (roles to `app__application_right` claims, same check on every API surface), then `QUERY`/OData with `POST` fallback, then GraphQL, none breaking the custom search syntax. Reply "ok" or change the order.

### Q8 — `.github/workflows/`
- **Asked:** 2026-10-22 · **Area:** CI
- **Question:** Should the workflows eventually be replaced by the local runner in `scripts/ci`?
- **Default:** left untouched.
- **Your answer:**

### Q9 — Azure Monitor export
- **Asked:** 2026-10-22 · **Area:** `OoBDev.OpenTelemetry`
- **Question:** Still wanted, given Azure coverage is emulator-only?
- **Default:** skipped.
- **Your answer:**

### Q10 — BinaryDataDecoders and ContractParser
- **Asked:** 2026-10-22 · **Area:** Incoming
- **Question:** Decisions pending (BinaryDataDecoders ~14 items; ContractParser implement now, later or keep as spec).
- **Default:** not worked on.
- **Your answer:**

### Q11 — Push and merge
- **Asked:** 2026-10-22 · **Area:** git
- **Question:** `dev/claude-config` is local only. When should it be pushed to `origin`, and merged to `main` or stacked on `dev/patterns-discovery`?
- **Default:** no push, no merge.
- **Your answer:**

### Q12 — Analyzer severities and `#warning` markers
- **Asked:** 2026-10-22 · **Area:** build
- **Question:** Promote more `suggestion` rules to `warning`, and remove the `#warning` markers (Mailkit hosting, RabbitMQ test)?
- **Default:** leave as is.
- **Your answer:**

### Q13 — Doc validation without Python
- **Asked:** 2026-10-22 · **Area:** docs
- **Question:** `validate-docs.py` cannot run here, so new design docs (`docs/design/CgiMiddleware`) are unvalidated. Will you run it, or should this stay flagged?
- **Default:** stays flagged in `TODO.md`.
- **Your answer:**

## Answered

Ids Q1-Q13 are from 2026-10-22; ids E1-E14 are the earlier questions merged in from `OPEN_QUESTIONS.md` (2026-10-09).

### Q1 — CGI: concurrency limits
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi`
- **Question:** A single global cap (2 x CPU) is in place. Do you also want per-script or per-client limits?
- **Default:** global cap only.
- **Your answer:** that's good to start

- **Resolved 2026-10-22:** global cap only; no change needed.

### Q2 — CGI: NPH scripts
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi`
- **Question:** Non-parsed-header scripts are rejected. Acceptable?
- **Default:** reject.
- **Your answer:** yes

- **Resolved 2026-10-22:** NPH stays rejected; no change needed.

### Q3 — CGI: run as another user
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi`
- **Question:** Leave this to the deployment's service account, or build it in?
- **Default:** leave to the deployer.
- **Your answer:** leave as todo

- **Resolved 2026-10-22:** kept as a TODO (`TODO.md`); nothing built.

### Q4 — CGI: Linux fixtures
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi.Tests`
- **Question:** Tests only have Windows `.cmd` fixtures (inconclusive elsewhere). Add `.sh` fixtures and require both platforms, or stay Windows-only for now?
- **Default:** add `.sh` fixtures when time allows; do not block on it.
- **Your answer:** you should be able to test with wsl on windows

- **Resolved 2026-10-22:** shell fixtures to be tested through WSL on this Windows host; tracked in `TODO.md`.

### Q5 — CGI: authorization
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi`
- **Question:** Authorization is the host's job, ahead of `UseCgi`. Do you want a built-in `[ApplicationRight]`-style check instead?
- **Default:** no built-in check.
- **Your answer:** yes

- **Resolved 2026-10-22:** built in. `CgiOptions.AllowAnonymous` (off), `RequiredRights` (at least one `CommonClaims.ApplicationRight` claim), per-script `ScriptRights`; 401 when unauthenticated, 403 when a right is missing. 18 tests pass.

### Q7 — Options validation default
- **Asked:** 2026-10-22 · **Area:** `AddValidatedOptions<T>()`
- **Question:** Strict (fail at startup) or relaxed (fail on first use) as the default?
- **Default:** strict, as coded today.
- **Your answer:**

- **Resolved 2026-10-22:** already answered as E6 (relaxed default, 2026-10-09). My assumed default (strict) was wrong; `AddValidatedOptions<T>()` stays opt-in and the CGI project uses it.

### Earlier questions (merged from OPEN_QUESTIONS.md)

#### 1. CI and infrastructure

##### E1 — Enable the scheduled integration tests?

- **Source:** [TODO.md](./TODO.md) → "Backlog: CI/CD Enablement and Docker Test Infrastructure"; [TODO.md](./TODO.md); [containers/testing/STATUS.md](./containers/testing/STATUS.md)
- **Question:** Turn on `integration-tests.yml` (daily 16:00 UTC plus `workflow_dispatch`) in `.github/workflows/`? It needs a runner with Docker. The Service Bus emulator has no health check.
- **Suggestion:** Enable manual dispatch first, watch one full run, then enable the schedule.
- **Answer:** Dispatch first: run `workflow_dispatch` once and watch a full run, then enable the schedule (2026-10-09).

##### E2 — Is "Restore central package management" done?

- **Source:** [TODO.md](./TODO.md) → "Backlog: Architect Answers"; [CLAUDE.md](./CLAUDE.md) → "Recently Completed Work" (2026-10-07)
- **Question:** CLAUDE.md says it was restored, but the TODO item is still open. May I verify that versions live only in `src/Directory.Packages.props` and tick it off?
- **Suggestion:** Yes.
- **Answer:** Yes, verify and tick off (2026-10-09). Verified: no inline `Version` attributes under `src/` or `templates/`; stale pattern docs corrected.

##### E3 — Live cloud tests (Groq)

- **Source:** [TODO.md](./TODO.md) → "Backlog: Live Integration (Cloud) Tests"; [TODO.md](./TODO.md)
- **Question:** I can write the `.env.liveintegration.template`, project readmes, categories and `.gitignore` entry without credentials. Running them needs your accounts. Should I do the scaffolding now and leave the live runs to you?
- **Suggestion:** Yes, scaffolding only.
- **Answer:**

##### E4 — Aspire spike before the Docker documentation

- **Source:** [TODO.md](./TODO.md) → "Backlog: CI/CD Enablement..." (last item) and "Architect Answers" (Spikes)
- **Question:** Run a short Aspire vs Docker compose spike before writing the Docker test documentation under `docs/architecture/testing/`, so the docs are not rewritten?
- **Suggestion:** Yes, spike first.
- **Answer:** Yes, run the Aspire vs Docker compose spike first (2026-10-09). Done (desk study): stay on Docker Compose, see [spike](docs/patterns-discovery/08-spikes/02-aspire-vs-compose.md).

#### 2. Design questions

##### E5 — `ISelectedService<T>` rough edges (resolved)

- **Source:** [TODO.md](./TODO.md) → "Backlog: Review `ISelectedService` Rough Edges"
- **Question:** Three points, all affecting how existing consumers move to the selection factory you already chose:
  1. `[ContractConfig(AllowDefault, ConfigKey)]` is declared and documented but nothing reads it. Dead, or an unfinished feature?
  2. Selection is resolved in the constructor, so it is fixed for the process. Intended, or should it follow configuration reloads?
  3. `IServiceProvider` injection (service locator) lives in the wrapper. Accepted exception until the factory lands?
- **Suggestion:** (1) dead, remove with the migration; (2) fixed at startup; (3) accepted until replaced.
- **Answer:** Resolved 2026-10-09 by the migration: `ISelectedService<T>` and `[ContractConfig]` were removed; `TryAddConfiguredKeyedService<T>` replaces them (selection per resolution, locator contained in the factory).

##### E6 — Options validation: strict default or relaxed?

- **Source:** [TODO.md](./TODO.md) → "Backlog: Options Validation Modes"
- **Question:** Adapters are config-gated and often have no configuration, so a strict default changes behavior for existing hosts. Where is the switch (per builder, global `OoBDev:Validation:Mode`, or both), and what is the rollout?
- **Suggestion:** Ship `AddValidatedOptions<T>()` as opt-in. Release with relaxed default plus a warning, flip to strict in a later release.
- **Answer:** Relaxed default (fail on first use, no startup validation). Adapters inline `AddOptions().Bind().ValidateDataAnnotations()` with the Microsoft package instead of calling the `OoBDev.System` helper (2026-10-09).

##### E7 — Is `Microsoft.Extensions.Resilience` acceptable?

- **Source:** [TODO.md](./TODO.md) → "Backlog: Architect Answers" (OpenTelemetry / no Polly); [CLAUDE.md](./CLAUDE.md) → "Rejected and Preferred Dependencies"
- **Question:** It depends on Polly internally. Your rule is "avoid Polly". Is using the Microsoft API (not Polly directly) acceptable?
- **Suggestion:** Accept, since the dependency is Microsoft's.
- **Answer:** Accept `Microsoft.Extensions.Resilience` (2026-10-09). Recorded in the [resilience practice](docs/patterns-discovery/03-practices-and-conventions/14-resilience-practices.md#implementation); OOB0003 no longer bans `Polly`.

##### E8 — What does "async-ui" mean for API docs?

- **Source:** [TODO.md](./TODO.md) → "Backlog: Architect Answers" (Scalar)
- **Question:** An AsyncAPI viewer, or Swagger UI? Scalar is chosen either way.
- **Answer:** An AsyncAPI viewer (2026-10-09). Scalar replaced Swashbuckle first; the AsyncAPI document and viewer for the SQS, Service Bus and RabbitMQ surfaces is phase 2 ([design](docs/design/OpenApiScalar/README.md)).

#### 3. Migration decisions

##### E9 — BinaryDataDecoders (39 open decisions)

- **Source:** [TODO.md](./TODO.md); [TODO.md](./TODO.md); [TODO.md](./TODO.md) → "Backlog: Migration Decisions"
- **Question:** The decisions cover the endianness API, `BinaryPrimitives` naming, UI collections location, CodeAnalysis use case, archive formats, ExpressionCalculator and more. Should Claude read the decision sheet and produce a one-page list with a recommendation next to each, so you can answer yes/no per line?
- **Suggestion:** Yes.
- **Answer:** Yes, produce a one-page list with a recommendation per decision (2026-10-09). Done: [decision sheet](docs/migration/binarydatadecoders-decision-sheet.md).

##### E10 — BotChat

- **Source:** [TODO.md](./TODO.md) → "Backlog: Migration Decisions"; [Incoming/CHECKLIST.md](./Incoming/CHECKLIST.md)
- **Question:** Archive, enhance, or extract patterns?
- **Answer:** Extract patterns (2026-10-09).

##### E11 — ContractParser

- **Source:** [TODO.md](./TODO.md) → "Backlog: Migration Decisions"
- **Question:** Implement now, later, or keep as a specification?
- **Answer:** Keep as specification (2026-10-09).

##### E12 — Tools / BulkLlm

- **Source:** [TODO.md](./TODO.md) → "Backlog: Migration Decisions"
- **Question:** Consolidate the BulkLlm tools or archive them?
- **Answer:** Consolidate the BulkLlm tools (2026-10-09).

#### 4. Renames and compatibility

##### E13 — `Retreive` to `Retrieve` rename (resolved)

- **Source:** [TODO.md](./TODO.md) → "Backlog: Caching Proxy Review (pattern 10)"
- **Question:** About 85 occurrences in 23 files, including public names (`ICachingProvider`, `ICachingManager`). CLAUDE.md says no breaking changes to existing APIs. May the old names stay as `[Obsolete]` forwarders for one release?
- **Suggestion:** Yes.
- **Answer:** Resolved 2026-10-09: the framework is unreleased, so breaking public changes are fine. `RetreiveAsync` was renamed outright to `RetrieveAsync` with no forwarders.

##### E14 — `ServiceCollectionEx` to `ServiceCollectionExtensions`

- **Source:** [TODO.md](./TODO.md) → "Backlog: Naming Consistency"
- **Question:** Resolved 2026-10-09 without needing an answer: the six classes were renamed outright. Forwarders were not possible (two classes with the same extension methods make every call ambiguous) and nothing called the old class by name.
- **Answer:** (none needed)

