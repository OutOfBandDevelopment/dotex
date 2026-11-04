# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 - 2026-11-04 - Generated guides - where does the generic API guide renderer live?

`ApiGuideRenderer` (OpenAPI/AsyncAPI document to a markdown guide) sits in `src/Applications/DevTerm/OoBDev.DevTerm.TestUtilities`. The Example WebApi guide and any other application guide need it too, and it has no DevTerm-specific names.

**Options**
- A (recommended): move it to a Framework test-utility library with a generic name (for example `OoBDev.Documents.Guides`, namespace `OoBDev.Documents.Guides`), and DevTerm, Example and later apps reference it. Rule `layering-and-migration-plans`: reusable means Framework. Cost: a new Framework project with readme and tests.
- B: keep it in DevTerm.TestUtilities and let Example reference DevTerm test code. Cheapest, but a generic capability sits under an application and couples applications.
- C: copy it into the Example test project. No coupling, but two copies drift.

**Default:** none; the Example WebApi guide waits. Other items continue.

**Your answer:**
