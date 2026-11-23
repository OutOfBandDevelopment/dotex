# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q31 — 2026-11-23 — DevTerm plugins: sandbox the `process` plugins?

**Question:** DevTerm `process` plugins (Python, Java, Go over JSON lines) run with the user's full rights after approval. The sandbox library can limit them, but its launchers start only their own plugin host and speak a different protocol ([design](docs/design/Plugins/sandboxed-launch.md)). Should DevTerm plugins run through the sandbox?

**Options:**
- A (recommended): add a command-line mode to the sandbox launchers and keep the JSON-lines protocol; `plugin.json` gets an optional `permissions` block, and a plugin with it is refused where no launcher exists. Existing examples keep working and the isolation is real.
- B: move presenter plugins to the sandbox frame protocol. One protocol, but every language needs a frame codec and presenters gain nothing from the broker.
- C: stay unsandboxed with approval only. No work, no isolation.

**Default:** A, built only after your answer (it changes the sandbox library API); until then the design is the deliverable and nothing is built.

**Your answer:**


### Q32 — 2026-11-23 — DevTerm Tui: per-channel expression picker?

**Question:** The WPF manifest editor has an expression picker on each channel row. The Tui's whole-list Channels picker already edits each channel's expression with validation. A per-row picker needs dynamic fields in the shared `OoBDev.UiDefinitions.Tui` form renderer. Build it?

**Options:**
- A (recommended): no. The existing editor covers the job, and the renderer change touches every Tui form.
- B: build the per-row picker in the renderer, with tests and a screenshot.

**Default:** A.

**Your answer:**


### Q33 — 2026-11-23 — Bench-hardware items: who runs them?

**Question:** These need a person at the bench or a captured trace, which I cannot supply: DE-5000 BLE Hardware test (radio not fitted), Z-Wave include/exclude/reset pass, the silent HP 34401A and TDS2024 TCP bridges, the Ebyte and USR UDP config capture (Ebyte `IDeviceConfigEditor`), HP-GL scope and XBee bench checks. When you are at the bench, do you want me to run each pass through the `hardware-test` skill, or to leave them off the backlog?

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
