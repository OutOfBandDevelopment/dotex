# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q33 — 2026-11-23 — Bench-hardware items: who runs them?

**Question:** These need a person at the bench or a captured trace, which I cannot supply: DE-5000 BLE Hardware test (radio not fitted), Z-Wave include/exclude/reset pass, the silent HP 34401A and TDS2024 TCP bridges, the Ebyte and USR UDP config capture (Ebyte `IDeviceConfigEditor`), HP-GL scope and XBee bench checks, the Greaseweazle USB pass for Ser2NetWin, and the BlueZ (Linux) BLE backend, which needs a Linux machine with a radio. When you are at the bench, do you want me to run each pass through the `hardware-test` skill, or to leave them off the backlog?

**Options:**
- A (recommended): keep them listed; run them when you say the bench is ready.
- B: drop the ones with no hardware in sight and re-add on demand.

**Default:** A.

**Your answer:**


### Q34 — 2026-11-23 — Desired-state database tooling: buy with gaps or build?

**Question:** The bake-off scores ([Table 8](docs/design/DesiredStateDatabase/bake-off-result.md#scores)) put sqldef at 69 to 71% and pgschema at 60%. No tool passes all three safety gates (S5 drop gate, S6 type-change hazard, S7 plan then apply), and the plan's rule says "build" when the gates fail for every candidate. Which reading do you want?

**Options:**
- A (recommended): buy with gaps: wrap sqldef (PostgreSQL, SQL Server, SQLite), and the wrapper adds the drop gate and a type-narrowing check on the plan. Least work, sqldef already skips drops without a flag.
- B: build our own diff and apply on the catalog (strict reading of the rule). Full control of the gates, much more work.
- C: wrap pgschema for PostgreSQL only (JSON plan), wrapper refuses drop steps; sqldef for the other engines.

**Default:** A, nothing is built until you answer.

**Your answer:**


### Q35 — 2026-11-23 — Desired-state database tooling: Oracle

**Question:** The design lists Oracle beside SQL Server, Postgres and SQLite, but neither sqldef nor pgschema covers it, and Atlas needs a paid licence for it. How should Oracle be handled? (From the request's open questions; answered by default so far, never confirmed.)

**Options:**
- A (recommended): hand-written repeatable scripts for Oracle in the first release, run by the same apply step with the drop gate. Least work, and nothing is built that no consumer has asked for.
- B: a custom catalog-diff adapter for Oracle. Same experience as the other engines, much more work and a test container with a heavy image.
- C: leave Oracle out of scope until a consumer needs it, and drop it from the design.

**Default:** A; nothing is built until Q34 is answered.

**Your answer:**
