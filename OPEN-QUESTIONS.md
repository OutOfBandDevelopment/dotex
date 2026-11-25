# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q33 — 2026-11-23 — Bench-hardware items: who runs them?

**Question:** These need a person at the bench or a captured trace, which I cannot supply: DE-5000 BLE Hardware test (radio not fitted), Z-Wave include/exclude/reset pass, the silent HP 34401A and TDS2024 TCP bridges, the Ebyte and USR UDP config capture (Ebyte `IDeviceConfigEditor`), HP-GL scope and XBee bench checks, the Greaseweazle USB pass for Ser2NetWin. When you are at the bench, do you want me to run each pass through the `hardware-test` skill, or to leave them off the backlog?

**Options:**
- A (recommended): keep them listed; run them when you say the bench is ready.
- B: drop the ones with no hardware in sight and re-add on demand.

**Default:** A.

**Your answer:**


### Q37 — 2026-11-24 — Retire the EF migration projects for final-state scripts?

**Question:** You prefer final (desired) state over migrations. The [plan](docs/design/DesiredStateDatabase/final-state-plan.md) replaces the generated EF migrations of PostgreSQL and SQLite (`OoBDev.Data.Migrations.Postgres`, `.Sqlite`, and the event store provider migrations) with generated final-state scripts applied through `OoBDev.Data.DesiredState`, as SQL Server already is. Do you confirm the project table in the plan, and how should data changes be handled?

**Options:**
- A (recommended): confirm the table; remove the migrations projects once the scripts and tests exist; data changes are hand-written `*.once.sql` ensure scripts. Cost: renames need an annotation for one release, and no per-change history except `git log`.
- B: confirm, but keep the event store providers on EF migrations (their contract is separate) and convert only the three stores. Cost: two schema mechanisms stay.
- C: change the plan (say what).

**Default:** A is planned but nothing is removed until you answer; the PostgreSQL and SQLite scripts and their tests (which remove nothing) are built meanwhile.

**Your answer:**


### Q38 — 2026-11-24 — Where does the EF Core hook live?

**Question:** `EfDesiredStateDeployer` (replaces `Database.MigrateAsync()` with plan, gates and apply) needs `Microsoft.EntityFrameworkCore.Relational`. I put it in `OoBDev.Data.DesiredState`, which now references EF Core Relational. Keep it there, or split a project?

**Options:**
- A (recommended): keep in place. One fewer project; EF Relational is already a Framework dependency (Communications.EntityFrameworkCore). Cost: callers without EF still pull the package.
- B: new `OoBDev.Data.DesiredState.EntityFrameworkCore`. Clean dependency edge; needs a confirmed migration plan (layering rule) and a project more.

**Default:** A.

**Your answer:**

### Q39 — 2026-11-24 — Build the native differ spike (plan phase G)?

**Question:** Phases A to D show sqldef plus the package deployer covers the scenarios tried (create, add, rename with refactor log, gated drop and narrowing, repeat is empty) on PostgreSQL and SQLite. Phase G would add a spike project `src/Spikes/...` (native inspector and typed plan steps). It creates a project, so it needs your confirmed migration plan first.

**Options:**
- A (recommended): skip for now; stay on sqldef (the plan's own default: "stay on sqldef until phase D shows a gap", and none showed). Revisit if a scenario fails.
- B: build the spike anyway to compare. Costs a project and a written result; no production effect.

**Default:** A.

**Your answer:**
