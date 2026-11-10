# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 — 2026-11-09 — DevTerm manifest editor: how should Pick work on one channel's expression segment?

In Channels mode the picker edits the whole `id[:label[:#RRGGBB[:expression]]]; ...` list, and choosing a value appends a new channel, so it cannot also insert `{id}` into one channel's expression (the fourth segment). Parameter-field picking is already built (`ExpressionList` mode).

**Options**
- A. A second **Pick...** per channel row (recommended): the chart's Channels field stays as is; the picker gets a channel selector, and when a channel is selected the dialog switches to Expression mode for its fourth segment (functions, sample result) and writes it back. Trade-off: the most work, but it reuses Expression mode wholesale.
- B. Let Channels mode insert at the caret when the caret is inside a fourth segment, and append otherwise. Trade-off: small change, but the same double-click does two things depending on the caret.
- C. Leave channel expressions typed by hand (diagnostics already check them). Trade-off: no work, no help building them.

**Default:** C until answered (nothing is built on a guess).

**Your answer:**

### Q2 — 2026-11-09 — Raster export: confirm the layering plan

The plan in [raster-export.md](docs/design/Presenters/raster-export.md#migration-plan) splits the HP-GL converter into a scene parser and writers, with no new project.

**Options**
- A. Scene and SVG writer stay in `OoBDev.Documents.Hpgl` (Framework); the SkiaSharp rasterizer goes in `OoBDev.Extensions.Drawing` (recommended): no new project, Framework stays Skia-free, the rasterizer is reusable by any scene. Trade-off: the scene types carry an HP-GL library name though they are generic.
- B. A new Framework project `OoBDev.Drawing.Vector` for the scene, writers and parser contract. Trade-off: honest generic name, one more project to package.
- C. Keep SVG only; PNG comes from an external tool. Trade-off: no work, no in-app PNG.

**Default:** A once confirmed; nothing is built until you answer (project layout rule).

**Your answer:**

### Q3 — 2026-11-09 — WPF dockable layout: which docking library?

Sessions and the Stream Monitor are fixed tabs plus floating windows. Visual Studio style docking needs a docking library (a new third-party dependency) and a layout-persistence story; the dark templates would need covering too. Your rule on dependencies rejects third-party DI, logging and Polly, so this needs your call.

**Options**
- A. AvalonDock (MIT, the de facto WPF docking library; recommended): least work, layouts serialize to XML; trade-off: one more dependency to theme with `DarkControls.xaml`.
- B. Build a small in-house dock (tab groups, drag to float, saved layout JSON). Trade-off: no dependency, but weeks of work for less polish.
- C. Keep tabs and floating windows. Trade-off: no work, no docking.

**Default:** C until answered (nothing is built on a guess).

**Your answer:**

### Q4 — 2026-11-09 — Stream Monitor: how should PostScript and PCL be previewed in the window?

HP-GL already converts to SVG; PostScript and PCL need an interpreter to draw. No managed one exists in the repo, and writing one is a large separate project.

**Options**
- A. Call Ghostscript (and GhostPCL) as an external process when installed, and show the PNG it writes (recommended): small code, covers both languages; trade-off: an AGPL tool you do not ship, and the preview is absent when it is not installed.
- B. Write a small in-house renderer for a useful subset (text, lines, fills). Trade-off: no dependency, but weeks of work and wrong output for real jobs.
- C. Keep saving the capture and opening it in an external viewer. Trade-off: no work, no in-window preview.

**Default:** C until answered.

**Your answer:**
