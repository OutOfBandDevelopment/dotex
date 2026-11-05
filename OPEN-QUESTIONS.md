# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 (2026-11-05, UiDefinitions) Where does the Terminal.Gui form renderer live?

The Tui control example needs `FormRenderer` (form definition to Terminal.Gui views), which today sits in `src/Applications/DevTerm/OoBDev.DevTerm.Tui`. An example under `src/Examples` cannot reference an Applications project (layer rule), and the renderer has no DevTerm-specific names.

**Options**
- A (recommended): move `FormRenderer` and its tests into a new Framework library `OoBDev.UiDefinitions.Tui` (mirrors `OoBDev.UiDefinitions.Wpf`), DevTerm.Tui references it, and the Tui control example uses it. Reusable means Framework (rule `layering-and-migration-plans`); cost: one migration plan and a Terminal.Gui reference in Framework.
- B: keep it in DevTerm.Tui and skip a Tui control example. No move, but the Tui control set has no example.
- C: put the example inside DevTerm (Applications). Works today, but is not a sample for other apps.

**Default:** A, after a migration plan in `docs/design/example-ui/`; nothing moves until you confirm.

**Your answer:**
