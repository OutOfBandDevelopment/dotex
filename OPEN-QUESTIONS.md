# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 (2026-11-09, Examples and DevTerm test utilities): where does the shared Tui screen-buffer renderer live?

`TuiCapture` (Examples Tui tests) is a copy of `TuiScreenshot` (DevTerm.Console.Tests). Examples and Applications must not reference each other, so the copy cannot be removed without choosing a home.

**Options**
- A. A new Framework-level test utility project (for example `OoBDev.Terminal.TestUtilities`) holding the renderer; both test projects reference it (recommended: reusable code with no application names belongs in Framework, per `layering-and-migration-plans`).
- B. Keep the two copies and accept the drift (no work, but duplicated fixes).
- C. Move it into `OoBDev.Examples.TestUtilities` and have DevTerm reference it (breaks the no-reference rule between Applications and Examples).

**Default:** B until answered.

**Your answer:**

### Q2 (2026-11-09, Example app, Template management): may the example save edited templates?

The Templates screen lists, opens and previews templates; saving would write files into the example `Templates` folder, which is a source folder in the repository.

**Options**
- A. No persistence: preview only (recommended: nothing in the repo changes from a running demo, and the framework has no write-side template source yet).
- B. Save to a separate writable folder configured as `TemplatePath` overlay (a copy under the app data folder; the repo stays clean, but needs a second template source in the engine).
- C. Save over the repository files (simple; a demo click edits tracked files).

**Default:** A until answered.

**Your answer:**
