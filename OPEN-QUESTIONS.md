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

**Default:** A. `Terminal.Gui` and the cross-platform toolkit are not added until you accept them.

**Your answer:**

### Q4 — 2026-10-26 — Device decoding — Where dev-term lives after the shells use OoBDev libraries

The dev-term shells (Console, Wpf, Web) will be converted to use the OoBDev libraries (phase 10 of the plan). Afterwards dev-term can live in two places.

**Options:** **A** (recommended): keep it a separate repository that depends on the dotex packages over NuGet; the package boundary then proves the libraries are reusable, and dev-term keeps its own release rhythm and test-generated documentation. Cost: cross-repository changes need a package publish (a local feed works). **B**: move it under this project as a tool, or a tool framework other tools build on. Cost: a larger solution, and the tool framework needs its own design (project layering, packaging). **C**: decide after phase 7, once the first device panel runs end to end.

**Default:** A for planning; nothing in phases 0 to 9 depends on the choice.

**Your answer:**
