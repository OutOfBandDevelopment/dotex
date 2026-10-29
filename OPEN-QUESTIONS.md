# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 — 2026-10-22 — Migration — Incoming code recommendation plan

Review [docs/migration/incoming-recommendation-plan.md](docs/migration/incoming-recommendation-plan.md). It compares all incoming code and the harvest candidates with main and proposes what to migrate, defer or reject (with reasons).

**Options:** reply per decision D1 to D9 in section 7 (D2, D8 and D9 come from the [cross-check](docs/migration/incoming-harvest-crosscheck.md)) of the plan, or **A** (recommended): approve all recommendations, which starts the order in section 6; **B**: change individual decisions.

**Default:** nothing migrates; only the D7 backlog entries exist.

**Your answer:**

### Q3 — 2026-10-26 — Device decoding — UI framework, transports and manifests scope

The plan migrates the UI model (`UiDefinitions`), then adds an MVVM view-model layer and thin WPF, Terminal.Gui and Blazor renderers (about 19,500 lines of code-behind in dev-term are not reused as is). It also lists `DeviceManifests` (5,825 lines) and the transports dotex lacks (TCP, USBTMC, VXI-11, RFC 2217, BLE, brokers).

**Options:** **A** (recommended): migrate the protocol libraries and `UiDefinitions` first, build the view models with the WPF renderer, and decide on Terminal.Gui, Blazor, manifests and transports after the first device panel works end to end. **B**: migrate everything in the plan's order, accepting `Terminal.Gui` as a dependency now. **C**: protocol libraries only, UI and transports stay in dev-term.

**Cross-platform (your note, 2026-10-26):** UI libraries come in as needed, Windows-targeted (`net10.0-windows`) only where a toolkit forces it, and you want a control library that renders on Linux and macOS too. Candidates: **Avalonia** (recommended: MIT, XAML and `ICommand` binding, reuses the same view models, one codebase for Windows, Linux and macOS), **GtkSharp** (native on Linux, weak on macOS, needs GTK installed), or only the terminal and web renderers for non-Windows. Your preference (2026-10-26): the most MVVM with `ICommand` where possible, and a spike of Avalonia before the final go-ahead (backlog item added). Reply with a pick after the spike, or leave it to the phase 7 design.

**Owner answer (2026-10-28):** dev-term is merged entirely into this framework, with no back-porting of packages into dev-term (see Q4 outcome in the migration plan). That settles the scope as option B (UI, Tui, Web and the rest come in); what stays open here is only the cross-platform toolkit pick (Avalonia spike) and the transports and manifests order.

**Default:** B for scope; the toolkit pick waits for the Avalonia spike.

**Your answer:**


### Q4 — 2026-10-28 — Documentation — Numbering of documents

Inventory: the numbered names are in `docs/patterns-discovery/` (folders `01-architecture` to `08-spikes` and `06-design-document-standard.md`, 83 numbered entries below it, added 2026-09-30 as a reading order). The rest of `docs/` (918 Markdown files) is unnumbered. Nothing else uses a numbering scheme.

**Options:** **A** (recommended): keep the numbers inside `patterns-discovery` as an explicit reading order, say so in its README, and number nothing else; this avoids breaking the many links that point at those paths. **B**: drop the numbers everywhere and rename with link fixing via `scripts/docs` (large diff, one pass). **C**: tell me which parts look wrong and I review only those.

**Default:** no renames; the full content review (coverage of dev-term and merged products) stays in BACKLOG.md.

**Your answer:**

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
