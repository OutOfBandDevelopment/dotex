# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open


### Q1 · 2026-11-11 · UiDefinitions · Where do the Avalonia and WinForms renderers live?

The backlog item "UiDefinitions renderers for Avalonia and WinForms, each with an example app" needs a home before any project is created (rule `layering-and-migration-plans`). The Wpf, Tui and Blazor renderers already exist as `OoBDev.UiDefinitions.*`.

**Options**
- A. (recommended) Same layer and naming as the existing renderers (`OoBDev.UiDefinitions.Avalonia`, `OoBDev.UiDefinitions.WinForms`), each in its own package, example apps under `src/Examples`. Consistent and each UI toolkit dependency stays in its own package.
- B. Extensions layer, because Avalonia and WinForms add toolkit dependencies. Keeps Framework lean but splits the renderer family across layers.
- C. Spike first under `src/Spikes` (Avalonia already has findings in `docs/design/UiDefinitions/avalonia-spike-findings.md`), then decide. Slowest, safest.

**Default:** A for WinForms (the control library exists), C for Avalonia. Nothing is created until you answer.

**Your answer:**
