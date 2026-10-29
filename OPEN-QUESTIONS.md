# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open

### Q4 — 2026-10-28 — Documentation — Numbering of documents

Inventory: the numbered names are in `docs/patterns-discovery/` (folders `01-architecture` to `08-spikes` and `06-design-document-standard.md`, 83 numbered entries below it, added 2026-09-30 as a reading order). The rest of `docs/` (918 Markdown files) is unnumbered. Nothing else uses a numbering scheme.

**Options:** **A** (recommended): keep the numbers inside `patterns-discovery` as an explicit reading order, say so in its README, and number nothing else; this avoids breaking the many links that point at those paths. **B**: drop the numbers everywhere and rename with link fixing via `scripts/docs` (large diff, one pass). **C**: tell me which parts look wrong and I review only those.

**Default:** no renames; the full content review (coverage of dev-term and merged products) stays in BACKLOG.md.

**Your answer:** these should be rewritten into a style guide then .editorconfig, analyzers and claude hooks/rules should be updated to ensure these styles.

### Q5 — 2026-10-29 — Database tooling — Scope, wrap versus write, and stack

Request: desired-state database tooling (`docs/design/DesiredStateDatabase/`). Inventory: only SQL Server is covered today (`OoBDev.DacFx`, `OoBDev.DacPacCompiler.Cli`); nothing exists for Postgres, SQLite or Oracle.

**Options:** **A** (recommended): wrap the bake-off winner behind `IDesiredStateProvider`, schema objects only (tables, indexes, constraints) first, functions/triggers/views in a later phase, Oracle by hand-written repeatable scripts, .NET as the host (matches the rest of the repo, and wrapped tools run as external processes). **B**: write a custom catalog-diff engine per database; most control, highest cost. **C**: schema, functions, triggers and views from the start; broader but slower to a first result.

**Default:** option A. Phase 0 (the bake-off) does not depend on the answer and goes first.

**Your answer:**

### Q6 — 2026-10-29 — Plugin sandbox — Home and platforms

Request: sandboxed plugin host (`docs/design/PluginSandbox/`). Inventory: no dotex package for process isolation; `OoBDev.AspNetCore.Cgi` limits child processes but its non-goals exclude sandboxing. A proof of concept exists in proving-grounds (`examples/plugin-sandbox-poc`, last commit 2026-10-08).

**Options:** **A** (recommended): a dotex package family, Windows plus Linux only, starting with a threat model and protocol design (no isolation code until the threat model is reviewed); keeps one repo, one test stack. **B**: keep it a separate project that dotex adopts later; avoids a security boundary in the framework until proven. **C**: A plus macOS; much more host work for no stated user.

**Default:** option A, limited to phase 0 (threat model, PoC review) and phase 1 (protocol and manifest). No launcher code is written before an answer.

**Your answer:**
