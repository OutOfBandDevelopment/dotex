# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open

### Q27 · 2026-11-20 · Data · Confirm the EF migration plan

Q20 decided the shape (one migrations project per database, history table per context). The rule `layering-and-migration-plans` asks for the project table to be confirmed before any project is created. The plan is [migrations-plan.md](docs/design/CqrsEventSourcing/migrations-plan.md): `OoBDev.Data.Migrations.Postgres` and `.Sqlite` in `src/Framework`, a SQL Server dacpac project, contexts right audit, suppressions and schedule in that order.

**Options**

- A. (recommended) Confirm the plan as written; work starts with the right audit store.
- B. Confirm with changes (write them under your answer, for example a different project name or layer).
- C. Hold: keep `EnsureCreated` for now and do not create migration projects.

**Default:** none; no projects are created until you confirm (creating projects is not reversible cheaply).

**Your answer:**

### Q28 · 2026-11-20 · Observability · User analytics: aggregate only, or per user?

The wish list asks for user analytics (screen usage, URLs, timeouts). The proposal in [docs/design/UserAnalytics](docs/design/UserAnalytics/README.md) counts aggregates only. Per-user usage (who opened which screen) would need consent and an erasure path.

**Options**

- A. (recommended) Aggregate counters only, tagged by route template and screen name, exported with OpenTelemetry. No personal data, so no consent or erasure work; matches the "keep cardinality low" rule.
- B. Aggregate plus per-user events stored in the audit store, with erasure through `IErasureEntryPoint`. More useful for support, but adds privacy duties.
- C. Hold: do not build analytics.

**Default:** A, built when it is next in order (reversible, no personal data).

**Your answer:**
