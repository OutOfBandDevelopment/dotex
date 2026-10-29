# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q2 · 2026-10-29 · DevTerm parity · Schema `$id` host

The generated JSON Schemas carry an `$id` URL (`SchemaGenerator.BaseUrl`). The ported default is dev-term's GitHub raw URL, but GitHub is no longer the primary repository. Where should the schemas be served from?

**Options**

- A. Keep the dev-term GitHub raw URL for now (recommended: reversible by changing one constant and regenerating; editors keep working while the schemas stay hosted there).
- B. A URL on a domain you control (stable, but I need the address).
- C. Relative `$id`s, no hosting (no editor auto-fetch).

**Default:** A.

**Your answer:**

