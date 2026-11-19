# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 (2026-11-18, Security, OAuth client): confirm the OAuthClient project layout

You chose a Framework library `OoBDev.Security.OAuthClient` with a Windows DPAPI leaf (Q2, 2026-11-17). The rule `layering-and-migration-plans` needs the table in [the migration plan](docs/design/OAuthClient/migration-plan.md) confirmed before any project is created: abstractions in Common, the library in Framework, `OoBDev.Security.OAuthClient.Windows` in Extensions, and tests beside each.

**Options**
- A. Confirm as written (recommended: it follows your Q2 answer and the Abstractions-only rule; the work then builds the first slice from [the design](docs/design/OAuthClient/README.md)).
- B. One Framework project without the Common abstractions split. Fewer projects, but adapters and clients then reference the implementation.
- C. Change the layout (say what).

**Default:** A is assumed for the design only; no project is created until you answer.

**Your answer:**
