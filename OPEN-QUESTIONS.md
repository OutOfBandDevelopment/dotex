# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q8 (2026-11-06, Examples / UiDefinitions.Tui): where do the Tui live displays live?

The Tui control example needs the indicator, bar graph and strip chart. The renderers (`CellCanvasView`, `CellCharts`) are `internal` in the application project `DevTerm.Tui`, and an Example must not reference an application.

**Options**
- A. Move them into `OoBDev.UiDefinitions.Tui` (Framework, generic names, public), and DevTerm.Tui and the example both use them (recommended: reusable code without application names is Framework, rule `layering-and-migration-plans`; mirrors the existing WPF and Blazor `LiveDisplayView`).
- B. Write a second, simpler Tui renderer in the example only (duplicates code; no layer change).
- C. Leave the Tui example without live displays.

A move needs a confirmed migration plan first (`docs/design/UiDefinitions/`).

**Default:** C until answered (nothing is moved).

**Your answer:**
